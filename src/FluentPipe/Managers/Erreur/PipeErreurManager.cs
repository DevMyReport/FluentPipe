using FluentPipe.Runner;

namespace FluentPipe.Managers.Erreur;

public sealed class PipeErreurManager : IPipeErreurManager<object>
{
    private readonly List<object> _errorMessage = new();
    private readonly List<object> _warning = new();

    public bool IsCancelled { get; internal set; }

    public void Cancel()
    {
        IsCancelled = true;
    }

    public void AddError(object error)
    {
        _errorMessage.Add(error);
    }

    public void AddError(IEnumerable<object> errors)
    {
        _errorMessage.AddRange(errors);
    }

    public bool HasError => IsCancelled || _errorMessage.Count != 0;

    public void AddWarning(object warning)
    {
        _warning.Add(warning);
    }

    public void AddWarning(IEnumerable<object> warnings)
    {
        _warning.AddRange(warnings);
    }

    public bool HasWarning => _warning.Count != 0;

    public IList<object> GetErrors()
        => _errorMessage;

    public IList<object> GetWarnings()
        => _warning;

    IEnumerable<object> IPipeErreurManager.GetErrorsObject()
        => GetErrors();

    IEnumerable<object> IPipeErreurManager.GetWarningsObject()
        => GetWarnings();
}