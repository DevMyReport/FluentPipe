﻿using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Blocks.Readers;
using FluentPipe.Builder;
using FluentPipe.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Tests.Explain;

[TestClass]
public sealed class PipeExplainTests : BasePipeInit
{
    private IPipeRunner _CurrentService => GetContainer().GetService<IPipeRunner>()!;

    [TestMethod]
    public async Task Explain_Simple_Chaining()
    {
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>()
            .Next<ToIntBlock, int>();


        var plan = builder.GetDetails();

        var result = await _CurrentService.ExplainAsync(plan);

        Assert.AreEqual(2, result.Blocks.Count);
    }

    [TestMethod]
    public async Task Explain_Parallel_Mono_Block()
    {
        //describe all the workflow
        //chain method  .Next<MethodToCall,MethodOutputType>
        var builder = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<LongReadEnumBlock, int>()
                .NextMonoParallel<AddOneBlock, int>(3)
                .Next<SommeBlock, int>()
                .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption { Format = "TOTO:" })
            ;

        var plan = builder.GetDetails();

        var result = await _CurrentService.ExplainAsync(plan);

        Assert.AreEqual(4, result.Blocks.Count);
    }

    [TestMethod]
    public async Task Explain_Parallel_Multi_Block()
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

        var plan = builder.GetDetails();

        var result = await _CurrentService.ExplainAsync(plan);

        Assert.AreEqual(6, result.Blocks.Count);
    }

    [TestMethod]
    public async Task Chain_Imbrique()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .Next<AddThreeBlock, int, AddThreeOption>(new AddThreeOption(3));

        var result = await _CurrentService.ExplainAsync(builder.GetDetails());

        Assert.IsNotNull(result);

        Assert.AreEqual(4, result.Blocks.Count);
        Assert.AreEqual(typeof(AddThreeBlock).FullName, result.Blocks[0].Key);
        Assert.IsNotNull(result.Blocks[0].Description);
        Assert.AreEqual(1, result.Blocks[0].Description!.Count);
        Assert.AreEqual("3", result.Blocks[0].Description!["Increment"]);

        Assert.AreEqual(typeof(AddOneBlock).FullName, result.Blocks[1].Key);
        Assert.IsNull(result.Blocks[1].Description);

        Assert.AreEqual(typeof(AddOneBlock).FullName, result.Blocks[2].Key);
        Assert.AreEqual(typeof(AddOneBlock).FullName, result.Blocks[3].Key);
    }
}