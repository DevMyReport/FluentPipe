using System.Diagnostics.CodeAnalysis;
using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Entity;
using FluentPipe.Managers.Erreur;
using FluentPipe.Managers.Etat;

namespace FluentPipe.Runner;

//default runner
public interface IPipeRunner : IPipeRunner<PipeErreurManager>;

public interface IPipeRunnerStateProgress : IPipeRunner<PipeErreurManager, PipeEtatManager, EnumEtapeEtat,
    EnumEtapeDeclancheur, PipeProgressionManager>;

public sealed record SortieRunner<TOut, TError>(
    TOut? Sortie,
    IReadOnlyList<ProcessStep> Etapes,
    TError ErrorManager,
    TimeSpan DureeComplete);

public interface
    IPipeRunner<TErrorManager, in TStateManager, TState, TTrigger, in TProgressionManager> : IPipeRunner<TErrorManager>
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

public interface IPipeRunner<TErrorManager> where TErrorManager : IPipeErreurManager
{
    public Task<SortieRunner<TOut, TErrorManager>> RunAsync<TIn, TOut>(
        PipeBuilderDetails<TIn, TOut> detail,
        [DisallowNull] TIn input,
        CancellationToken cancellationToken = default);

    public Task<SortieRunner<TOut, TErrorManager>> ExplainAsync<TIn, TOut>(
        PipeBuilderDetails<TIn, TOut> detail,
        CancellationToken cancellationToken = default);
}