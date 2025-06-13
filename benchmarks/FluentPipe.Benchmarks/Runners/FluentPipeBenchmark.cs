using BenchmarkDotNet.Attributes;
using FluentPipe.Blocks;
using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Blocks.Readers;
using FluentPipe.Builder;
using FluentPipe.Builder.Entity;
using FluentPipe.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Benchmarks.Runners;

[SimpleJob(warmupCount: 1, iterationCount: 1)]
[MemoryDiagnoser]
public sealed class FluentPipeInlineBenchmark
{
    private PipeBuilderDetails<int, string> _plan = null!;
    private ServiceProvider _provider = null!;
    private IPipeRunner _serviceInline = null!;

    [GlobalSetup]
    public void Setup()
    {
        //container
        IServiceCollection services = new ServiceCollection();
        services.RegisterRunner();
        services.RegisterBlocks();

        //services
        _provider = services.BuildServiceProvider();
        _serviceInline = _provider.GetRequiredService<IPipeRunner>();

        //service
        var builder = PipeBuilderFactory.Creer<int>()
                .Next_ReadBlock()
                .Next<SommeBlock, int>()
                .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption
                { Format = "TOTO:" })
            ;


        _plan = builder.GetDetails();
    }


    [Benchmark(Baseline = true)]
    public async Task RunManuelle()
    {
        var reader = new ReadBlock();
        var somme = new SommeBlock();
        var toSring = new ToStringBlock();
        var token = new CancellationToken();

        var r1 = await reader.ProcessAsync(100, token);
        var r2 = await somme.ProcessAsync(r1.TypedValue, token);
        var result = await toSring.ProcessAsync(r2.TypedValue, new ToStringBlockOption(), token);
    }

    [Benchmark]
    public async Task RunInlineSansPlan()
    {
        var result = await _serviceInline.RunAsync(_plan, 100);
    }

    [Benchmark]
    public async Task RunInlineAvecPlan()
    {
        var pipe = PipeBuilderFactory.Creer<int>()
                //.Next<ReadBlock, int[]>()
                .Next_ReadBlock()
                .Next<SommeBlock, int>()
                .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption
                { Format = "TOTO:" })
            ;
        var plan = pipe.GetDetails();
        var result = await _serviceInline.RunAsync(plan, 100);
    }
}