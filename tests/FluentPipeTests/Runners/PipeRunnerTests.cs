using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Blocks.Readers;
using FluentPipe.Builder;
using FluentPipe.Builder.Contracts;
using FluentPipe.Builder.Entity;
using FluentPipe.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Tests.Runners;

[TestClass]
public class PipeRunnerTests : BasePipeInit
{
    private IPipeRunner _CurrentService => GetContainer().GetRequiredService<IPipeRunner>();

    [TestMethod]
    public async Task Next_Simple_Chaining()
    {
        IPipeBuilder<string, int> builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>()
            .Next<ToIntBlock, int>();

        PipeBuilderDetails<string, int> plan = builder.GetDetails();

        var result = await _CurrentService.RunAsync(plan, "1234");

        Assert.AreEqual(4321, result.Sortie);
        Assert.AreEqual(2, result.Blocks.Count);
    }

    [TestMethod]
    public async Task Next_Stop_With_Null_Value()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .NextEnumerable<NullReadBlock, int>()
            .Next<SommeBlock, int>();

        var plan = builder.GetDetails();

        var result = await _CurrentService.RunAsync(plan, 1234);

        Assert.AreEqual(default, result.Sortie);
        Assert.AreEqual(1, result.Blocks.Count);
    }

    [TestMethod]
    public async Task Next_Simple_Option()
    {
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>()
            .Next<ToIntBlock, int>()
            .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption
            { Format = "TOTO:" });

        var plan = builder.GetDetails();

        var result = await _CurrentService.RunAsync(plan, "1234");

        Assert.AreEqual("TOTO:4321", result.Sortie);
        Assert.AreEqual(3, result.Blocks.Count);
    }

    [TestMethod]
    public async Task Next_Condition()
    {
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>(false)
            .Next<ToIntBlock, int>();

        var plan = builder.GetDetails();

        var result = await _CurrentService.RunAsync(plan, "1234");

        Assert.AreEqual(1234, result.Sortie);
        Assert.AreEqual(1, result.Blocks.Count);
    }

    [TestMethod]
    public void Next_Condition_And_Options()
    {
        var builder = PipeBuilderFactory.Creer<string>()
                .Next<ReverseBlock, string>(false)
                .Next<ToIntBlock, int>()
                .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption())
                .Next<ReverseBlock, string>(false)
            ;

        var expected = new List<BlockInfo>
        {
            new(typeof(ToIntBlock)),
            new(typeof(ToStringBlock), new ToStringBlockOption())
        }.ToArray();

        var values = builder.GetDetails();

        Assert.AreEqual(expected.Length, values.Blocks.Count);

        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i].TypeDuBlock, ((BlockInfo)values.Blocks[i]).TypeDuBlock);
    }

    [TestMethod]
    public async Task Can_Be_Cancelled_Simple_Chain()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .ParallelStart<IEnumerable<int>>()
            .With<LongReadBlock>()
            .With<LongReadBlock>()
            .With<LongReadBlock>()
            .With<LongReadBlock>()
            .ParallelEnd()
            .Next<IntAggregatorBlock, int[]>()
            .Next<SommeBlock, int>();

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        var result =
            await _CurrentService.RunAsync(builder.GetDetails(), 100, cts.Token);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.ErrorManager.IsCancelled);
        Assert.IsTrue(result.ErrorManager.HasError);
    }

    [TestMethod]
    public async Task Simple_Chain_With_Error()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .NextEnumerable<ErrorReadBlock, int>()
            .Next<SommeBlock, int>();

        var result =
            await _CurrentService.RunAsync(builder.GetDetails(), 100);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.ErrorManager.HasError);
    }

    [TestMethod]
    public async Task Simple_Chain_With_Warning()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .NextEnumerable<WarningReadBlock, int>()
            .Next<SommeBlock, int>();

        var result =
            await _CurrentService.RunAsync(builder.GetDetails(), 100);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.ErrorManager.HasWarning);
    }

    [TestMethod]
    public async Task Chain_Imbrique()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .Next<AddThreeBlock, int, AddThreeOption>(new AddThreeOption(3));

        var result = await _CurrentService.RunAsync(builder.GetDetails(), 100);

        Assert.IsNotNull(result);
        Assert.AreEqual(103, result.Sortie);
        Assert.AreEqual(4, result.Blocks.Count);
        Assert.AreEqual(typeof(AddThreeBlock).FullName, result.Blocks[0].Key);
        Assert.AreEqual(typeof(AddOneBlock).FullName, result.Blocks[1].Key);
        Assert.AreEqual(typeof(AddOneBlock).FullName, result.Blocks[2].Key);
        Assert.AreEqual(typeof(AddOneBlock).FullName, result.Blocks[3].Key);
    }
}