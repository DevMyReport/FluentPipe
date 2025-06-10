using FluentPipe.Blocks.Contracts;
using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Builder;
using FluentPipe.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Tests.ErreurManager;

[TestClass]
public class PipeErreurManagerTests : BasePipeInit
{
    private IRunner DefaultService => GetContainer().GetRequiredService<IRunner>();

    private IMyRunner MyService => GetContainer().GetRequiredService<IMyRunner>();

    protected override IServiceCollection SetServices(IServiceCollection services)
    {
        services.AddTransient<IMyRunner, MyRunner>();
        return base.SetServices(services);
    }

    [TestMethod]
    public async Task Can_Use_Default_Error_Service()
    {
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>()
            .Next<ToIntBlock, int>();

        var plan = builder.GetDetails();

        var result = await DefaultService.RunAsync(plan, "1234");

        Assert.AreEqual(typeof(PipeErreurManager), result.ErrorManager.GetType());

        Assert.AreEqual(4321, result.Sortie);
        Assert.AreEqual(2, result.Etapes.Count);
    }

    [TestMethod]
    public async Task Can_Use_Custom_Error_Service()
    {
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>()
            .Next<ToIntBlock, int>();

        var plan = builder.GetDetails();

        var result = await MyService.RunAsync(plan, "1234");

        Assert.AreEqual(typeof(ErreurManager.PipeErreurManagerPersonalisedPourTests), result.ErrorManager.GetType());

        Assert.AreEqual(4321, result.Sortie);
        Assert.AreEqual(2, result.Etapes.Count);
    }


    [TestMethod]
    public async Task Can_Use_ComputeResult_FromResult()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .Next<AddThreeBlock, int, AddThreeOption>(new AddThreeOption(3));

        var plan = builder.GetDetails();

        SortieRunner<int, ErreurManager.PipeErreurManagerPersonalisedPourTests> result = await MyService.RunAsync(plan, 2);

        var compute = ComputeResult.FromResult(result);

        Assert.AreEqual(typeof(ErreurManager.PipeErreurManagerPersonalisedPourTests), result.ErrorManager.GetType());

        Assert.AreEqual(5, compute.TypedValue);
        Assert.AreEqual(4, result.Etapes.Count);
    }
}