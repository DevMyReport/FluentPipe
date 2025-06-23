using System.Linq;
using FluentPipe.Blocks;
using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Builder;
using FluentPipe.Runner;
using FluentPipe.Managers.Erreur;
using FluentPipe.Managers.Etat;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Tests.Runners;

[TestClass]
public class PipeRunnerCompletTests : BasePipeInit
{    
    private PipeRunnerComplet _pipeRunner ;
    
    [ClassInitialize]
    public static void MyClassInitialize(TestContext testContext)
    {
    }

    [TestInitialize]
    public void MyTestInitialize()
    {
        _pipeRunner = GetRequiredServicePipeRunner();
    }

    [TestCleanup]
    public void MyTestCleanup()
    {
        _pipeRunner = null;
    }
    
    private PipeRunnerComplet GetRequiredServicePipeRunner()
    {
        return (PipeRunnerComplet)
            GetContainer()
                .GetRequiredService<IPipeRunner>();
    }

    [TestMethod]
    public void Default_Service_Is_PipeRunnerComplet()
    {
        Assert.IsInstanceOfType(GetContainer()
            .GetRequiredService<IPipeRunner>(), typeof(PipeRunnerComplet));
    }

    [TestMethod]
    public async Task RunAsync_Notifies_Progression()
    {
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>()
            .Next<ToIntBlock, int>();

        var plan = builder.GetDetails();

        var blockProgressEvent = new List<ProgressionEvent>();
        _pipeRunner.ProgressionManager.BlockProgressionChanged += (s, e) => { blockProgressEvent.Add(e); };
        var pipeProgressEvent = new List<PipeProgressionEvent>();
        _pipeRunner.ProgressionManager.PipeProgressionChanged += (s, e) => { pipeProgressEvent.Add(e); };

        var result = await _pipeRunner.RunAsync(plan, "1234");

        Assert.AreEqual(4321, result.Sortie);
        CollectionAssert.AreEqual(new[] {1L, 2L, 3L, 4L}, blockProgressEvent.Select(c => c.Fait).ToList());
        // Un événement de progression par block traité au niveau du Pipe
        CollectionAssert.AreEqual(new[] {0L, 1L, 2L}, pipeProgressEvent.Where(e=>e.DernierBlockProgressionEvent is null).Select(c => c.Fait).ToList());
    }

    [TestMethod]
    public async Task ExplainAsync_Notifies_Progression()
    {
        var pipeRunner = GetRequiredServicePipeRunner();
        
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>()
            .Next<ToIntBlock, int>();

        var plan = builder.GetDetails();

        var pipeProgressEvent = new List<PipeProgressionEvent>();
        _pipeRunner.ProgressionManager.PipeProgressionChanged += (s, e) => { pipeProgressEvent.Add(e); };
        
        var result = await _pipeRunner.ExplainAsync(plan);

        Assert.AreEqual(2, result.Blocks.Count);
        // pas de progression de block
        Assert.AreEqual(0 , pipeProgressEvent.Count(e => e.DernierBlockProgressionEvent is not null));
        // On ne doit avoir que 3 événements : debut de la progression à 0 puis +1 par block
        CollectionAssert.AreEqual(new[] {0L, 1L, 2L}, pipeProgressEvent.Select(c => c.Fait).ToList());
    }

    private sealed class TestProgressManager : PipeProgressionManager
    {
        public List<(string Id, double Percentage)> Calls { get; } = new();

        public void NotifierProgression(string etapeId, double percentage)
        {
            Calls.Add((etapeId, percentage));
        }

        public double GetProgressionEtape(string etapeId)
        {
            return Calls.LastOrDefault(c => c.Id == etapeId).Percentage;
        }
    }
}