namespace FluentPipe.Runner;

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