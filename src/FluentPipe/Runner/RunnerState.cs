using FluentPipe.Blocks.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Runner;

internal sealed record RunnerState(
    bool IsExplain,
    IServiceProvider Provider,
    IPipeErreurManager ErreurManager,
    CancellationTokenSource CancellationTokenSource) : IDisposable
{
    public IServiceScope Scope { get; set; } = Provider.CreateScope();

    public void Dispose()
    {
        Scope.Dispose();
        CancellationTokenSource.Dispose();
    }

    public void ResetScope()
    {
        Scope.Dispose();
        Scope = Provider.CreateScope();
    }

    /// <summary>
    ///     Allow to add warning if detected
    /// </summary>
    /// <param name="result"></param>
    public void SetStateWarning(ComputeResult result)
    {
        ErreurManager.AddWarning(result.Warnings);
    }

    /// <summary>
    ///     Allow to add warning if detected
    /// </summary>
    /// <param name="result"></param>
    public void SetStateWarning((ProcessStep, ComputeResult)[] result)
    {
        var warnings = result.Select(s => s.Item2).ToList();
        if (!warnings.Any()) return;

        var warns = warnings.SelectMany(s => s.Warnings).ToList();
        if (warns.Any())
            ErreurManager.AddWarning(warns);
    }

    /// <summary>
    ///     Allow to cancel the Token if an error is detected
    /// </summary>
    /// <param name="result"></param>
    public void SetStateOnError(ComputeResult result)
    {
        if (result.IsSuccess) return;

        CancellationTokenSource.Cancel();
        if (result.Error != null)
            ErreurManager.AddError(result.Error);
    }

    /// <summary>
    ///     Allow to cancel the Token if an error is detected
    /// </summary>
    /// <param name="result"></param>
    public void SetStateOnError((ProcessStep, ComputeResult)[] result)
    {
        var errors = result.Select(s => s.Item2).Where(w => !w.IsSuccess).ToList();
        if (!errors.Any()) return;

        CancellationTokenSource.Cancel();
        var errs = errors.Where(w => w.Error != null).Select(s => s.Error).ToList();
        if (errs.Any())
            ErreurManager.AddError(errs!);
    }
}