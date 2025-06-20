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
    private IPipeRunner<PipeErreurManager, PipeEtatManager, EnumEtapeEtat, EnumEtapeDeclancheur, PipeProgressionManager>
        CurrentService =>
        (IPipeRunner<PipeErreurManager, PipeEtatManager, EnumEtapeEtat, EnumEtapeDeclancheur, PipeProgressionManager>)
        GetContainer()
            .GetRequiredService<IPipeRunner>();

    [TestMethod]
    public void Default_Service_Is_PipeRunnerComplet()
    {
        Assert.IsInstanceOfType(CurrentService, typeof(PipeRunnerComplet));
    }

    [TestMethod]
    public async Task RunAsync_Notifies_Progression()
    {
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>()
            .Next<ToIntBlock, int>();

        var plan = builder.GetDetails();

        var progressEvent = new List<ProgressionEvent>();
        CurrentService.ProgressionManager.BlockProgressionChanged += (s, e) => { progressEvent.Add(e); };
        var pipeProgressEvent = new List<PipeProgressionEvent>();
        CurrentService.ProgressionManager.PipeProgressionChanged += (s, e) => { pipeProgressEvent.Add(e); };

        var result = await CurrentService.RunAsync(plan, "1234");

        Assert.AreEqual(4321, result.Sortie);
        CollectionAssert.AreEqual(new[] {0L, 1L, 2L}, pipeProgressEvent.Select(c => c.Fait).ToList());
    }

    [TestMethod]
    public async Task ExplainAsync_Notifies_Progression()
    {
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>()
            .Next<ToIntBlock, int>();

        var plan = builder.GetDetails();
        var progress = new TestProgressManager();

        var result = await CurrentService.ExplainAsync(plan);

        Assert.AreEqual(2, result.Etapes.Count);
        CollectionAssert.AreEqual(new[] {0d, 100d}, progress.Calls.Select(c => c.Percentage).ToList());
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