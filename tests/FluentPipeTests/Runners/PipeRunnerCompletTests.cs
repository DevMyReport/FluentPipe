using System.Linq;
using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Builder;
using FluentPipe.Managers.Etat;
using FluentPipe.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Tests.Runners;

[TestClass]
public class PipeRunnerCompletTests : BasePipeInit
{
    private IPipeRunnerAvecEtatEtProgression PipeRunnerComplet => (IPipeRunnerAvecEtatEtProgression)GetContainer().GetRequiredService<IPipeRunner>();

    [TestMethod]
    public void Default_Service_Is_PipeRunnerComplet()
    {
        var svc = GetContainer().GetRequiredService<IPipeRunner>();
        Assert.IsInstanceOfType(svc, typeof(PipeRunnerComplet));
    }

    [TestMethod]
    public async Task RunAsync_Notifies_Progression()
    {
        var pipeRunnerComplet = GetContainer().GetRequiredService<PipeRunnerComplet>();
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>()
            .Next<ToIntBlock, int>();

        var plan = builder.GetDetails();
        var progress =PipeRunnerComplet.ProgressionManager;

        var result = await PipeRunnerComplet.RunAsync(plan, "1234");

        Assert.AreEqual(4321, result.Sortie);
        CollectionAssert.AreEqual(new[] { 0d, 100d }, progress.Calls.Select(c => c.Percentage).ToList());
    }

    [TestMethod]
    public async Task ExplainAsync_Notifies_Progression()
    {
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>()
            .Next<ToIntBlock, int>();

        var plan = builder.GetDetails();
        var progress = new TestProgressManager();

        var result = await PipeRunnerComplet.ExplainAsync(plan);
        Assert.AreEqual(2, result.Etapes.Count);
        CollectionAssert.AreEqual(new[] { 0d, 100d }, progress.Calls.Select(c => c.Percentage).ToList());
    }

    private sealed class TestProgressManager : IPipeProgressionManager
    {
        public List<(string Id, double Percentage)> Calls { get; } = new();

        public void NotifierProgressionRunner(string etapeId, double percentage)
        {
            Calls.Add((etapeId, percentage));
        }

        public double GetProgressionEtape(string blockId)
        {
            return Calls.LastOrDefault(c => c.Id == blockId).Percentage;
        }
    }
}
