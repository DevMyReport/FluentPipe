namespace FluentPipe.Runner;

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

public interface IPipeErreurManager<T> : IPipeErreurManager
{
    public IList<T> GetErrors();

    public IList<T> GetWarnings();
}

public interface IPipeErreurManager
{
    /// <summary>
    /// Informe que le pipeline a été cancel
    /// </summary>
    public void Cancel();

    /// <summary>
    /// Ajoute une erreur
    /// </summary>
    /// <param name="error"></param>
    public void AddError(object error);

    /// <summary>
    /// Ajoute une liste d'erreur
    /// </summary>
    /// <param name="errors"></param>
    public void AddError(IEnumerable<object> errors);

    /// <summary>
    /// Indique Si la pipeline à une erreur ou est cancel
    /// </summary>
    public bool HasError { get; }

    /// <summary>
    /// Ajoute un warning
    /// </summary>
    /// <param name="warning"></param>
    public void AddWarning(object warning);

    /// <summary>
    /// Ajoute une liste de warning
    /// </summary>
    /// <param name="warnings"></param>
    public void AddWarning(IEnumerable<object> warnings);

    /// <summary>
    /// Indique Si la pipeline à un warning ou est cancel
    /// </summary>
    public bool HasWarning { get; }

    bool IsCancelled { get; }

    public IEnumerable<object> GetErrorsObject();

    public IEnumerable<object> GetWarningsObject();
}