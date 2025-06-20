using FluentPipe.Runner;

namespace FluentPipe.Blocks.Contracts;

public record ComputeResult
{
    private readonly object? _result;

    internal ComputeResult(bool isSuccess, object result)
    {
        IsSuccess = isSuccess;
        _result = result;
        Warnings = new List<object>();
    }

    internal ComputeResult(bool isSuccess, object result, IReadOnlyList<ProcessBlock> sousBlocs)
     : this(isSuccess, result)
    {
        SousBlocs = sousBlocs;
    }

    public object? Error => IsSuccess ? null : _result;

    public object? Value => IsSuccess ? _result : null;

    public bool IsSuccess { get; }

    public IReadOnlyList<ProcessBlock> SousBlocs { get; } = new List<ProcessBlock>();

    public List<object> Warnings { get; }

    public static ComputeResult Success(object value)
    {
        return new ComputeResult(true, value);
    }

    public static ComputeResult<T> Success<T>(T value)
    {
        return new ComputeResult<T>(true, value);
    }

    public static ComputeResult<T> Failure<T>(object error)
    {
        return new ComputeResult<T>(false, error);
    }

    public static ComputeResult<T> Failure<T>(object error, IReadOnlyList<ProcessBlock> etapes)
    {
        return new ComputeResult<T>(false, error, etapes);
    }

    public static ComputeResult<T> SuccessWithWarnings<T>(T value, IEnumerable<object> warnings)
    {
        return new ComputeResult<T>(true, value, warnings);
    }

    public static ComputeResult<T> SuccessWithWarnings<T>(T value, IEnumerable<object> warnings, IReadOnlyList<ProcessBlock> sousEtapes)
    {
        return new ComputeResult<T>(true, value, warnings, sousEtapes);
    }

    public static ComputeResult<TSortie> FromResult<TSortie, TErrorManager>(SortieRunner<TSortie, TErrorManager> sortie)
        where TErrorManager : IPipeErreurManager
    {
        if (sortie.ErrorManager.HasError)
            return Failure<TSortie>(sortie.ErrorManager.GetErrorsObject(), sortie.Blocks);

        return SuccessWithWarnings(sortie.Sortie, sortie.ErrorManager.GetWarningsObject(), sortie.Blocks);
    }
}

public sealed record ComputeResult<T> : ComputeResult
{
    internal ComputeResult(bool isSuccess, object result)
        : this(isSuccess, result, Enumerable.Empty<object>(), new List<ProcessBlock>())
    { }

    internal ComputeResult(bool isSuccess, object result, IReadOnlyList<ProcessBlock> sousBlocs)
        : this(isSuccess, result, Enumerable.Empty<object>(), sousBlocs)
    { }

    internal ComputeResult(bool isSuccess, object result, IEnumerable<object> warnings)
        : this(isSuccess, result, warnings, new List<ProcessBlock>())
    { }

    internal ComputeResult(bool isSuccess, object result, IEnumerable<object> warnings, IReadOnlyList<ProcessBlock> sousBlocs)
        : base(isSuccess, result, sousBlocs)
    {
        Warnings.AddRange(warnings);
    }

    public T? TypedValue => IsSuccess && Value != null ? (T)Value : default;

    public Task<ComputeResult<T>> ToTask()
    {
        return Task.FromResult(this);
    }
}