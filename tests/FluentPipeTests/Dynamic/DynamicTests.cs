using FluentPipe.Blocks.Dynamic;
using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Builder;
using FluentPipe.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Tests.Dynamic;

[TestClass]
public class DynamicTests : BasePipeInit
{
    private IPipeRunner _CurrentService => GetContainer().GetRequiredService<IPipeRunner>();

    [TestMethod]
    public async Task OneIn_Inject_Add_Step_In_Runner()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .Next<AddOneBlock, int>()
            .OneIn<ModuloDynamicBlock, string>()
            .Next<ToIntBlock, int>();

        var plan = builder.GetDetails();

        var result1 = await _CurrentService.RunAsync(plan, 0);

        Assert.AreEqual(1, result1.Sortie);
        Assert.AreEqual(3, result1.Blocks.Count);

        var result2 = await _CurrentService.RunAsync(plan, 1);

        Assert.AreEqual(3, result2.Sortie);
        Assert.AreEqual(4, result2.Blocks.Count);
    }

    [TestMethod]
    public async Task OneIn_Inject_Add_Step_In_Explain()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .Next<AddOneBlock, int>()
            .OneIn<ModuloDynamicBlock, string>()
            .Next<ToIntBlock, int>();

        var plan = builder.GetDetails();

        var result1 = await _CurrentService.ExplainAsync(plan);

        Assert.AreEqual(4, result1.Blocks.Count);
    }

    [TestMethod]
    public async Task OneIn_Inject_Option_Step_In_Runner()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .Next<AddOneBlock, int>()
            .OneIn<ModuleOptionBlock, string, ModuleOption>(new ModuleOption());

        var plan = builder.GetDetails();

        var result1 = await _CurrentService.RunAsync(plan, 0);

        Assert.AreEqual("1", result1.Sortie);
        Assert.AreEqual(2, result1.Blocks.Count);

        var result2 = await _CurrentService.RunAsync(plan, 1);

        Assert.AreEqual("3", result2.Sortie);
        Assert.AreEqual(3, result2.Blocks.Count);
    }
}