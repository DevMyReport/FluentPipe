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
public sealed class FluentPipeParallelBenchmark
{
    private PipeBuilderDetails<int, int> _plan = null!;
    private ServiceProvider _provider = null!;
    private IPipeRunner _serviceDefault = null!;
    private IPipeRunner _serviceWithTask = null!;

    [Params(100, 10_000)]
    public int Input;

    [GlobalSetup]
    public void Setup()
    {
        //container
        IServiceCollection services = new ServiceCollection();
        services.RegisterRunner();
        services.RegisterBlocks();

        //services
        _provider = services.BuildServiceProvider();
        _serviceDefault = _provider.GetRequiredService<IPipeRunner>();
        _serviceWithTask = _provider.GetRequiredService<IPipeRunner>();

        var builder = PipeBuilderFactory.Creer<int>()
                .NextEnumerable<LongReadEnumBlock, int>()
                .NextMonoParallel<AddOneBlock, int>(3)
                .Next<SommeBlock, int>()
            ;

        _plan = builder.GetDetails();
    }


    [Benchmark(Baseline = true)]
    public async Task<int> RunParallelForeach()
    {
        var result = await _serviceDefault.RunAsync(_plan, Input);
        Console.WriteLine($"RESULT:{result}");
        return result.Sortie;
    }


    [Benchmark]
    public async Task<int> RunInsideTaskRun()
    {
        var result = await _serviceWithTask.RunAsync(_plan, Input);
        Console.WriteLine($"RESULT:{result}");
        return result.Sortie;
    }
}