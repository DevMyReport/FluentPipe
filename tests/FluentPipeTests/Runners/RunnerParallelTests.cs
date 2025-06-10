using System.Diagnostics;
using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Blocks.Readers;
using FluentPipe.Builder;
using FluentPipe.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Tests.Runners;

[TestClass]
public class RunnerParallelTests : BasePipeInit
{
    private IRunner CurrentService => GetContainer().GetRequiredService<IRunner>();

    [TestMethod]
    public async Task NextInParallel_Simple_Chain()
    {
        //describe all the workflow
        //chain method  .Next<MethodToCall,MethodOutputType>
        var builder = PipeBuilderFactory.Creer<int>()
                .ParallelStart<IEnumerable<int>>()
                .With<LongReadBlock>()
                .With<LongReadBlock>()
                .With<LongReadBlock>()
                .With<LongReadBlock>()
                .ParallelEnd()
                .Next<IntAggregatorBlock, int[]>()
                .Next<SommeBlock, int>()
            ;

        //get runner service

        //start runner with expected input
        var result = await CurrentService.RunAsync(builder.GetDetails(), 100);
        var val = 5050 * 4;

        Assert.AreEqual(val, result.Sortie);
        AssertTime(result);

    }

    [TestMethod]
    public async Task NextInParallel_Simple_Chain_With_Start_End()
    {
        //describe all the workflow
        //chain method  .Next<MethodToCall,MethodOutputType>
        var builder = PipeBuilderFactory.Creer<int>()
                .ParallelStart<IEnumerable<int>>() // debut mode parallele
                                                   //simple
                .With<LongReadBlock>()
                //avec condition(il est optionel
                .With<LongReadBlock>(false)
                //avec une option
                .With<LongReadOptBlock, LongReadOption>(new LongReadOption())
                //condition & option
                .With<LongReadOptBlock, LongReadOption>(new LongReadOption(), (_) => true)
                //binding de l'option avec la config
                .With<LongReadOptBlock, LongReadOption>(() => new LongReadOption())
                .With<LongReadOptBlock, LongReadOption>(() => new LongReadOption())
                .With<LongReadOptBlock, LongReadOption>(() => new LongReadOption(), (_) => true)
                .With<LongReadOptBlock, LongReadOption>(() => new LongReadOption())
                .ParallelEnd() // fin mode parallel
                .Next<IntAggregatorBlock, int[]>()
                .Next<SommeBlock, int>()
            ;

        //get runner service
        //start runner with expected input
        var result = await CurrentService.RunAsync(builder.GetDetails(), 100);

        var expected = 5050 * 7;
        Assert.AreEqual(expected, result.Sortie);
        AssertTime(result);
    }

    [TestMethod]
    public async Task NextInParallel_With_Multiple_Block()
    {
        //describe all the workflow
        //chain method  .Next<MethodToCall,MethodOutputType>
        var builder = PipeBuilderFactory.Creer<int>()
                .ParallelStart<int>()
                .With<LongMonoReadBlock>()
                .With<LongMonoReadBlock>()
                .With<LongMonoReadBlock>()
                .With<LongMonoReadBlock>()
                .ParallelEnd()
                .Next<SommeBlock, int>()
                .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption
                { Format = "TOTO:" })
            ;

        //get runner service
        //start runner with expected input
        var result = await CurrentService.RunAsync(builder.GetDetails(), 100);

        var val = 500 * 4;

        Assert.AreEqual($"TOTO:{val}", result.Sortie);

        AssertTime(result);
    }

    [TestMethod]
    public async Task NextInParallel_With_One_Block_Different_Output_Type()
    {
        //describe all the workflow
        //chain method  .Next<MethodToCall,MethodOutputType>
        var builder = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<LongReadEnumBlock, int>()
                .NextMonoParallel<AddOneToStringBlock, string>(3)
                .Next<ConcatBlock, string>()
            ;

        //get runner service
        //start runner with expected input
        var result = await CurrentService.RunAsync(builder.GetDetails(), 10);

        Assert.AreEqual(12, result.Sortie.Length);
        AssertTime(result, 1500);
    }

    [TestMethod]
    public async Task NextInParallel_With_One_Block_Same_Output_Type()
    {
        //build container

        //describe all the workflow
        //chain method  .Next<MethodToCall,MethodOutputType>
        var builder = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<LongReadEnumBlock, int>()
                .NextMonoParallel<AddOneBlock, int>(3)
                .Next<SommeBlock, int>()
                .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption
                { Format = "TOTO:" })
            ;

        //get runner service
        //start runner with expected input
        var result = await CurrentService.RunAsync(builder.GetDetails(), 100);

        Assert.AreEqual("TOTO:5150", result.Sortie);
        AssertTime(result, 1500);
    }

    [TestMethod]
    public async Task NextInParallel_CanCell_With_One_Block_Same_Output_Type()
    {
        //describe all the workflow
        var builder = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<LongReadEnumBlock, int>()
                .NextMonoParallel<AddOneLongBlock, int>(2)
                .Next<SommeBlock, int>()
            ;

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        //get runner service
        //start runner with expected input
        var result = await CurrentService.RunAsync(builder.GetDetails(), 100, cts.Token);
        Assert.IsTrue(result.ErrorManager.IsCancelled);
        Assert.IsTrue(result.Etapes.Count <= 3);

        var temps = result.Etapes.Select(s => s.Duree.TotalMilliseconds).Sum();
        Console.WriteLine($"Etapes Temps total:{temps} ms");
    }

    [TestMethod]
    public async Task NextInParallel_MaxParallelism_With_One_Block_Have_Impact()
    {
        //build container

        //describe all the workflow
        var builder1 = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<LongReadEnumBlock, int>()
                .NextMonoParallel<AddOneLongBlock, int>(5)
                .Next<SommeBlock, int>()
            ;

        var builder2 = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<LongReadEnumBlock, int>()
                .NextMonoParallel<AddOneLongBlock, int>(20)
                .Next<SommeBlock, int>()
            ;

        //get runner service
        //start runner with expected input
        var sw1 = Stopwatch.StartNew();
        var result1 = await CurrentService.RunAsync(builder1.GetDetails(), 20);
        sw1.Stop();

        var sw2 = Stopwatch.StartNew();
        var result2 = await CurrentService.RunAsync(builder2.GetDetails(), 20);
        sw2.Stop();

        var temps1 = result1.Etapes.Select(s => s.Duree.TotalMilliseconds).Sum();
        var temps2 = result2.Etapes.Select(s => s.Duree.TotalMilliseconds).Sum();
        Console.WriteLine($"Etapes Temps total:{sw1.ElapsedMilliseconds}/{temps1} ms ");
        Console.WriteLine($"Etapes Temps total:{sw2.ElapsedMilliseconds}/{temps2} ms");

        Assert.IsTrue(sw2.ElapsedMilliseconds < sw1.ElapsedMilliseconds);
    }

    [TestMethod]
    public async Task NextInParallel_With_Option()
    {
        //describe all the workflow
        var builder = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<ReadBlock, int>()
                .NextMonoParallel<AddOneToStringOptionBlock, string, AddOneToStringOption>(new(1), 3)
                .Next<ConcatBlock, string>()
            ;

        //get runner service
        //start runner with expected input
        var result = await CurrentService.RunAsync(builder.GetDetails(), 10);

        Assert.AreEqual(12, result.Sortie.Length);
        AssertTime(result, 1500);
    }

    [TestMethod]
    public async Task Can_Be_Cancelled_With_Parallel_OnError()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .ParallelStart<IEnumerable<int>>()
            .With<ErrorReadBlock>()
            .With<LongReadBlock>()
            .ParallelEnd()
            .Next<IntAggregatorBlock, int[]>()
            .Next<SommeBlock, int>();

        var result = await CurrentService.RunAsync(builder.GetDetails(), 100);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.ErrorManager.HasError);
        Assert.AreEqual(ErrorReadBlock.ErrorMessage, result.ErrorManager.GetErrors().First());
        AssertTime(result, 1500);
    }

    [TestMethod]
    public async Task Chain_With_Parallel_Warning()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .ParallelStart<IEnumerable<int>>()
            .With<WarningReadBlock>()
            .With<WarningReadBlock>()
            .With<WarningReadBlock>()
            .ParallelEnd()
            .Next<IntAggregatorBlock, int[]>()
            .Next<SommeBlock, int>();

        var result = await CurrentService.RunAsync(builder.GetDetails(), 100);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.ErrorManager.HasWarning);
        Assert.AreEqual(result.ErrorManager.GetWarnings().Count, 3);

        Assert.AreEqual(result.Sortie, 300);
    }

    [TestMethod]
    public async Task NextPipeInParallel_Enabled()
    {
        //describe all the workflow
        var underBuilder = PipeBuilderFactory.Creer<int>()
            .Next<AddOneBlock, int>()
            .Next<ToStringBlock, string, ToStringBlockOption>(new());

        var builder = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<ReadBlock, int>()
                .NextPipeMonoParallel(underBuilder, 3)
                .Next<ConcatBlock, string>()
            ;

        //get runner service
        //start runner with expected input
        var result = await CurrentService.RunAsync(builder.GetDetails(), 10);

        Assert.AreEqual(12, result.Sortie.Length);
        AssertTime(result, 1500);
    }

    [TestMethod]
    public async Task NextPipeInParallel_Disabled()
    {
        //describe all the workflow
        var underBuilder = PipeBuilderFactory.Creer<int>()
            .Next<AddOneBlock, int>();

        var builder = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<ReadBlock, int>()
                .NextPipeMonoParallel(underBuilder, 3, false)
            ;

        //get runner service
        //start runner with expected input
        var result = await CurrentService.RunAsync(builder.GetDetails(), 10);
        var list = result.Sortie.ToList();

        Assert.AreEqual(1, result.Etapes.Count);
        Assert.AreEqual(10, list.Count);
        AssertTime(result, 1500);
    }
}