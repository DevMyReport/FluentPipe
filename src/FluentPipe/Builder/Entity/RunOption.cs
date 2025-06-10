namespace FluentPipe.Builder.Entity;

/// <summary>
///     Centralise les options disponible lors du run
/// </summary>
public sealed class RunOption
{
    public RunOption()
    {

    }

    /// <summary>
    /// Option pour plusieurs connecteurs en parallel
    /// </summary>
    /// <param name="multiBlockParallelOption"></param>
    public RunOption(RunMultiBlockParallelOption multiBlockParallelOption)
    {
        MultiBlockParallelOption = multiBlockParallelOption;
    }

    /// <summary>
    /// Option pour un connecteur en parallel
    /// </summary>
    /// <param name="monoBlockParallelOption"></param>
    public RunOption(RunMonoBlockParallelOption monoBlockParallelOption)
    {
        MonoBlockParallelOption = monoBlockParallelOption;
    }

    public bool HasParallelism => MultiBlockParallelOption != null || MonoBlockParallelOption != null;

    public RunMultiBlockParallelOption? MultiBlockParallelOption { get; }

    public RunMonoBlockParallelOption? MonoBlockParallelOption { get; }
    public bool UseSubscope { get; set; }

    public string Description()
    {
        var groupe = string.Empty;
        if (MultiBlockParallelOption?.Id != null)
            groupe = $"id:{MultiBlockParallelOption.Id}";
        else if (MonoBlockParallelOption?.MaxDegreeOfParallelism != null)
            groupe = $"max:[{MonoBlockParallelOption.MaxDegreeOfParallelism}]";

        return groupe;
    }
}

/// <summary>
///     Option pour paralleliser plusieurs connecteur homogene
/// </summary>
public sealed class RunMultiBlockParallelOption
{
    /// <summary>
    ///     Option pour paralleliser plusieurs connecteur homogene
    /// </summary>
    /// <param name="parallelismPairing">Guid pour determiner le groupe</param>
    public RunMultiBlockParallelOption(string? parallelismPairing = null)
    {
        Id = parallelismPairing ?? Guid.NewGuid().ToString();
    }

    /// <summary>
    ///     guid du group à paralleliser
    /// </summary>
    public string Id { get; init; }
}

/// <summary>
///     Option pour paralleliser un connecteur en particulier
/// </summary>
public sealed class RunMonoBlockParallelOption
{
    /// <summary>
    ///     Option pour paralleliser un connecteur en particulier
    /// </summary>
    /// <param name="maxDegreeOfParallelism">Nombre maximum de parallelism possible</param>
    public RunMonoBlockParallelOption(int maxDegreeOfParallelism)
    {
        MaxDegreeOfParallelism = maxDegreeOfParallelism;
    }

    /// <summary>
    ///     Nombre maximum de parallelism possible
    /// </summary>
    public int MaxDegreeOfParallelism { get; init; }
}