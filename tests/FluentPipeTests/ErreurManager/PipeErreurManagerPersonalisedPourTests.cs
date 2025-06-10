using FluentPipe.Runner;

namespace FluentPipe.Tests.ErreurManager;

public sealed class MyRunner(IServiceProvider provider) : PipeRunnerBase<PipeErreurManagerPersonalisedPourTests>(provider), IMyRunner;

public interface IMyRunner : IRunner<PipeErreurManagerPersonalisedPourTests>;

public sealed class PipeErreurManagerPersonalisedPourTests : IPipeErreurManager<string>
{
    private readonly List<string> _errors = new();
    private readonly List<string> _warnings = new();

    public bool IsCancelled { get; internal set; }

    public void Cancel()
    {
        IsCancelled = true;
    }

    public IList<string> GetErrors()
        => _errors;

    public IList<string> GetWarnings()
        => _warnings;

    IEnumerable<object> IPipeErreurManager.GetErrorsObject()
        => _errors;

    IEnumerable<object> IPipeErreurManager.GetWarningsObject()
        => _warnings;

    public void AddError(object error)
    {
        _errors.Add((string)error);
    }

    public void AddError(IEnumerable<object> errors)
    {
        _errors.AddRange(errors.Cast<string>());
    }

    public bool HasError => IsCancelled || _errors.Count != 0;

    public void AddWarning(object warning)
    {
        _warnings.Add((string)warning);
    }

    public void AddWarning(IEnumerable<object> warnings)
    {
        _warnings.AddRange(warnings.Cast<string>());
    }

    public bool HasWarning => _warnings.Count != 0;
}