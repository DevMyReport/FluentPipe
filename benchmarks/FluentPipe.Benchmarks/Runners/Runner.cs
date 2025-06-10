using FluentPipe.Runner;

namespace FluentPipe.Benchmarks.Runners;

public sealed class RunnerParallelTask : PipeRunnerInLine
{
    public RunnerParallelTask(IServiceProvider provider) : base(provider)
    {
    }

    //protected override async Task<(EtapeTermine, ComputeResult)[]> RunInParallel(IEnumerable<Etape> steps, object input, CancellationTokenSource cts)
    //{
    //    var values = new ConcurrentBag<(EtapeTermine, ComputeResult)>();
    //    var option = new ParallelOptions();

    //    await Task.Run(() => Parallel.ForEachAsync(steps.TakeWhile(_ => !cts.IsCancellationRequested),
    //        option,
    //        async (etape, ct) =>
    //        {
    //            values.Add(await ComputeStepAsync(etape, input, cts));
    //        })
    //    );

    //    //TakeWhile permet d'arreter le parallel sans généré de cancellationException


    //    return values.ToArray();
    //}

    //protected override async Task<(EtapeTermine, ComputeResult)[]> RunInParallel(Etape etape, IEnumerable<object> inputs, int maxDegreeOfParallelism, CancellationTokenSource cts)
    //{
    //    var values = new ConcurrentBag<(EtapeTermine, ComputeResult)>();
    //    var option = new ParallelOptions
    //    {
    //        MaxDegreeOfParallelism = maxDegreeOfParallelism == 0 ? Environment.ProcessorCount : maxDegreeOfParallelism,
    //    };


    //    await Task.Run(() => Parallel.ForEachAsync(inputs.TakeWhile(_ => !cts.IsCancellationRequested),
    //            option,
    //            async (input, ct) =>
    //            {
    //                values.Add(await ComputeStepAsync(etape, input, cts));
    //            })
    //    );

    //    //TakeWhile permet d'arreter le parallel sans généré de cancellationException


    //    return values.ToArray();
    //}
}