using System.Diagnostics;
using FluentPipe.Blocks.Dynamic;
using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Blocks.Readers;
using FluentPipe.Builder;
using FluentPipe.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Tests.Dynamic;

[TestClass]
public class DynamicParallelTests : BasePipeInit
{
    private IRunner _CurrentService => GetContainer().GetRequiredService<IRunner>();

    [TestMethod]
    public async Task NextDynamicParallel_With_One_Block_Same_Output_Type()
    {
        //build container

        //describe all the workflow
        var builder = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<LongReadEnumBlock, int>()
                .NextDynamicParallel<ModuloDynamicBlock, string>(3)
            ;

        //get runner service
        //start runner with expected input
        var result = await _CurrentService.RunAsync(builder.GetDetails(), 3);

        Assert.AreEqual(3, result.Sortie.Count());
        AssertTime(result, 1500);
    }

    [TestMethod]
    public async Task NextDynamicParallel_Dynamic_In_Dynamic()
    {
        //build container

        //describe all the workflow
        var builder = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<LongReadEnumBlock, int>()
                .NextDynamicParallel<DynamicToDynamicBlock, string>(3)
            ;

        //get runner service
        //start runner with expected input
        var result = await _CurrentService.RunAsync(builder.GetDetails(), 3);

        Assert.AreEqual(3, result.Sortie.Count());
        AssertTime(result, 1500);
    }

    [TestMethod]
    public async Task NextDynamicParallel_Dynamic_In_DynamicParallel()
    {
        //build container

        //describe all the workflow
        var builder =
                PipeBuilderFactory.CreerEnumerable<List<int>>()
                .NextDynamicParallel<DynamicToDynamicParallelBlock, IEnumerable<string>>(3)
            ;

        //get runner service
        //start runner with expected input
        var result = await _CurrentService.RunAsync(builder.GetDetails(), [[1], [2], [3]]);

        Assert.AreEqual(3, result.Sortie.Count());
        AssertTime(result, 1500);
    }

    [TestMethod]
    public async Task NextDynamicParallel_MaxParallelism_With_One_Block_Have_Impact()
    {
        //build container

        //describe all the workflow
        var builder1 = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<LongReadEnumBlock, int>()
                .NextDynamicParallel<AddOneLongDynamicBlock, int>(5)
                .Next<SommeBlock, int>()
            ;

        var builder2 = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<LongReadEnumBlock, int>()
                .NextDynamicParallel<AddOneLongDynamicBlock, int>(20)
                .Next<SommeBlock, int>()
            ;

        //get runner service
        //start runner with expected input
        var sw1 = Stopwatch.StartNew();
        var result1 = await _CurrentService.RunAsync(builder1.GetDetails(), 20);
        sw1.Stop();

        var sw2 = Stopwatch.StartNew();
        var result2 = await _CurrentService.RunAsync(builder2.GetDetails(), 20);
        sw2.Stop();

        var temps1 = result1.Etapes.Select(s => s.Duree.TotalMilliseconds).Sum();
        var temps2 = result2.Etapes.Select(s => s.Duree.TotalMilliseconds).Sum();
        Console.WriteLine($"Etapes Temps total:{sw1.ElapsedMilliseconds}/{temps1} ms ");
        Console.WriteLine($"Etapes Temps total:{sw2.ElapsedMilliseconds}/{temps2} ms");

        Assert.IsTrue(sw2.ElapsedMilliseconds < sw1.ElapsedMilliseconds);
    }

    [TestMethod]
    public async Task NextDynamicParallel_With_Option()
    {
        //describe all the workflow
        var builder = PipeBuilderFactory.Creer<int>()
                .Next_ReadBlock()
                .NextDynamicParallel<ModuleOptionBlock, string, ModuleOption>(new(), 3)
                .Next<ConcatBlock, string>()
            ;

        //get runner service
        //start runner with expected input
        var result = await _CurrentService.RunAsync(builder.GetDetails(), 3);

        Assert.AreEqual(3, result.Sortie.Length);
        Assert.AreEqual(6, result.Etapes.Count);
        AssertTime(result, 1500);
    }

    [TestMethod]
    public async Task NextDynamicParallel_With_SubScope()
    {
        //describe all the workflow
        var builderWithoutScope = PipeBuilderFactory.Creer<int>()
                .Next_ReadBlock()
                .NextDynamicParallel<AddMultiScopedOptionBlock, string, ModuleOption>(new(), 3)
                .Next<ConcatBlock, string>()
            ;

        var builderWithScope = PipeBuilderFactory.Creer<int>()
                .Next_ReadBlock()
                .NextDynamicParallel<AddMultiScopedOptionBlock, string, ModuleOption>(new(), 3, true)
                .Next<ConcatBlock, string>()
            ;

        //get runner service
        //start runner with expected input
        var result = await _CurrentService.RunAsync(builderWithoutScope.GetDetails(), 3);
        var resultScoped = await _CurrentService.RunAsync(builderWithScope.GetDetails(), 3);

        Assert.AreEqual(11, result.Etapes.Count);
        Assert.AreEqual(8, resultScoped.Etapes.Count);
        AssertTime(result, 1500);
    }
}