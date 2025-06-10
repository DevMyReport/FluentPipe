using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using FluentPipe.Builder.Entity;

namespace FluentPipe.Runner;

//default runner
public interface IRunner : IRunner<PipeErreurManager>;

public interface IRunnerStateProgress : IRunner<PipeErreurManager, PipeEtatManagerStateless, EnumEtapeEtat,
    EnumEtapeTrigger, PipeProgressionManager>;

/// <summary>
///   Un seul manager métier qui gère à la fois
///   - les erreurs (IPipeErreurManager)
///   - l’état de chaque étape (IPipeEtatManager)
///   - la progression de chaque étape (IPipeProgressionManager)
/// </summary>
public class PipeProgressionManager : IPipeProgressionManager
{
    // 3) Pour la progression, on conserve un pourcentage par étape
    private readonly ConcurrentDictionary<string, double> _stepProgress
        = new ConcurrentDictionary<string, double>();

    public void ReportProgress(string stepId, double percentage)
    {
        // On se force à plafonner entre 0 et 100
        var pct = Math.Max(0, Math.Min(100, percentage));
        _stepProgress.AddOrUpdate(stepId, pct, (_, __) => pct);
    }

    public double GetProgress(string stepId)
    {
        return _stepProgress.TryGetValue(stepId, out var pct)
            ? pct
            : 0.0;
    }

    public IDictionary<string, double> GetAllProgress()
    {
        return _stepProgress.ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}

public interface
    IRunner<TErrorManager, in TStateManager, TState, TTrigger, in TProgressionManager> : IRunner<TErrorManager>
    where TErrorManager : IPipeErreurManager
    where TStateManager : IPipeEtatManager<TState, TTrigger>
    where TProgressionManager : IPipeProgressionManager
{
    public Task<SortieRunner<TOut, TErrorManager>> RunAsync<TIn, TOut>(
        PipeBuilderDetails<TIn, TOut> detail,
        [DisallowNull] TIn input,
        TStateManager etatManager = default,
        TProgressionManager progressionManager = default,
        CancellationToken cancellationToken = default);

    public Task<SortieRunner<TOut, TErrorManager>> ExplainAsync<TIn, TOut>(
        PipeBuilderDetails<TIn, TOut> detail,
        TStateManager etatManager = default,
        TProgressionManager progressManager = default,
        CancellationToken cancellationToken = default);
}

public interface IRunner<TErrorManager> where TErrorManager : IPipeErreurManager
{
    public Task<SortieRunner<TOut, TErrorManager>> RunAsync<TIn, TOut>(
        PipeBuilderDetails<TIn, TOut> detail,
        [DisallowNull] TIn input,
        CancellationToken cancellationToken = default);

    public Task<SortieRunner<TOut, TErrorManager>> ExplainAsync<TIn, TOut>(
        PipeBuilderDetails<TIn, TOut> detail,
        CancellationToken cancellationToken = default);
}