using System.Diagnostics.CodeAnalysis;
using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Entity;
using FluentPipe.Managers.Erreur;
using FluentPipe.Managers.Etat;

namespace FluentPipe.Runner;

//default runner
public interface IPipeRunner : IPipeRunner<PipeErreurManager>;

public interface IPipeRunnerAvecEtatEtProgression : IPipeRunner<PipeErreurManager, PipeEtatManager, EnumMachineEtat,
    EnumMachineDeclancheur, PipeProgressionManager>;

public sealed record SortieRunner<TOut, TError>(
    TOut? Sortie,
    IReadOnlyList<ProcessBlock> Blocks,
    TError ErrorManager,
    TimeSpan DureeComplete);

public interface IPipeRunner<TErrorManager, out TEtatManager, TState, TTrigger, out TProgressionManager> : IPipeRunner<TErrorManager>
    where TErrorManager : IPipeErreurManager
    where TEtatManager : IPipeEtatManager<TState, TTrigger>
    where TProgressionManager : IPipeProgressionManager
{
    public TEtatManager EtatManager { get; } 
    
    public TProgressionManager ProgressionManager { get; } 
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