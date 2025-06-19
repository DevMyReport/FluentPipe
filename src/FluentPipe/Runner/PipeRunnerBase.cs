using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Contracts;
using FluentPipe.Builder.Entity;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Runner;

/// <summary>
///     PipeRunnnerBase qui prend en charge les erreurs.
///     Il existe une base dérivée qui prend en plus en charge les états et la progression
/// </summary>
/// <param name="provider"></param>
/// <typeparam name="TErreurManager"></typeparam>
public abstract class PipeRunnerBase<TErreurManager>(IServiceProvider provider) : IPipeRunner<TErreurManager>
    where TErreurManager : IPipeErreurManager, new()
{
    private TErreurManager _ErreurManager { get; } = new TErreurManager();

    /// <summary>
    ///     Lance l'exécution des étapes
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="detail"></param>
    /// <param name="input"></param>
    /// <param name="errorManager"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<SortieRunner<TOut, TErreurManager>> RunAsync<TIn, TOut>(
        PipeBuilderDetails<TIn, TOut> detail,
        [DisallowNull] TIn input,
        CancellationToken cancellationToken = default)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var stopwatch = Stopwatch.StartNew();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var state = new RunnerState(false, provider, _ErreurManager, cts);

        var (listEtapes, data) = await RunInMixed(detail.Etapes, input, state);

        if (cancellationToken.IsCancellationRequested)
            InternalAnnuler();

        if (_ErreurManager.HasError || data == null)
            data = default(TOut);

        stopwatch.Stop();

        return new SortieRunner<TOut, TErreurManager>((TOut) data!, listEtapes, _ErreurManager, stopwatch.Elapsed);
    }

    /// <summary>
    ///     Lance les étapes en fonction de leurs configurations
    /// </summary>
    /// <param name="steps"></param>
    /// <param name="input"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private async Task<(List<ProcessStep>, object?)> RunInMixed(IReadOnlyList<IEtape> steps, object input,
        RunnerState state)
    {
        var data = input;
        var listEtapes = new List<ProcessStep>();

        foreach (var stepGen in steps)
        {
            if (state.CancellationTokenSource.IsCancellationRequested)
                break;

            if (data == null)
                break;

            switch (stepGen)
            {
                case ScopeEtape:
                    state.ResetScope();
                    break;

                case DynamicEtape dynamicStep when dynamicStep.Etape.RunOpt.MonoBlockParallelOption is { } optionMono:

                    //il faut lancer le meme connecteur autant de foi qu'il y'a de valeur dans la list
                    var dynamicValues = (data as IEnumerable)?.Cast<object>();
                    if (dynamicValues == null)
                        throw new InvalidCastException($"Failed to cast Etape {dynamicStep.Etape.TypeDuBlock.Name}");

                    (var listSteps, data) = await RunInDynamicParallel(dynamicStep, dynamicValues,
                        optionMono.MaxDegreeOfParallelism, state);

                    listEtapes.AddRange(listSteps);
                    break;

                case DynamicEtape dynamicStep:
                    //un block qui rajoute des étapes en fonction de la valeur
                    var (extraSteps, _) = GetDynamicEtapes(dynamicStep.Etape, data, state);
                    var (s, o) = await RunInMixed(extraSteps, data, state);

                    listEtapes.AddRange(s);
                    data = o;
                    break;

                case ParallelEtape stepMulti:
                {
                    //plusieurs connecteur a lancé en parallele
                    var subList = stepMulti.Etapes;
                    (var listStepsMulti, data) = await RunInParallel(subList, data, state);
                    listEtapes.AddRange(listStepsMulti);
                    break;
                }

                case Etape stepMono when stepMono.RunOpt.MonoBlockParallelOption is { } optionMono:
                {
                    //il faut lancer le meme connecteur autant de foi qu'il y'a de valeur dans la list
                    var values = (data as IEnumerable)?.Cast<object>();
                    if (values == null)
                        throw new InvalidCastException($"Failed to cast Etape {stepMono.TypeDuBlock.Name}");

                    (var listStepsMono, data) =
                        await RunInParallel(stepMono, values, optionMono.MaxDegreeOfParallelism, state);

                    listEtapes.AddRange(listStepsMono);
                    break;
                }

                case Etape stepMono:
                    var (fini, step, _) = await ComputeStepAsync(stepMono, data, state);
                    data = step.Value;
                    listEtapes.AddRange(GetAllProcessSteps(fini, step));
                    state.SetStateOnError(step);
                    state.SetStateWarning(step);
                    break;
            }
        }

        return (listEtapes, data);
    }

    /// <summary>
    ///     Lance toutes les etapes en parallele
    /// </summary>
    /// <param name="steps"></param>
    /// <param name="input"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private async Task<(IEnumerable<ProcessStep>, object?)> RunInParallel(IEnumerable<Etape> steps,
        object input, RunnerState state)
    {
        var values = new ConcurrentBag<(ProcessStep, ComputeResult)>();
        var option = new ParallelOptions();
        IPipeProcessBlock? converter = null;

        //TakeWhile permet d'arreter le parallel sans généré de cancellationException
        await Parallel.ForEachAsync(steps.TakeWhile(_ => !state.CancellationTokenSource.IsCancellationRequested),
            option,
            async (step, _) =>
            {
                var (e, c, f) = await ComputeStepAsync(step, input, state);
                converter ??= f;
                values.Add((e, c));
            });

        var runSteps = values.ToArray();

        var ret = runSteps.Where(w => w.Item2.IsSuccess)
            .Select(s => s.Item2.Value).ToList();

        state.SetStateOnError(runSteps);
        state.SetStateWarning(runSteps);

        var completedSteps = runSteps.SelectMany(s => GetAllProcessSteps(s.Item1, s.Item2));
        var data = converter?.ToTypedEnumerableConverter(ret) ?? null;
        return (completedSteps, data);
    }

    /// <summary>
    ///     Lance toutes les etapes en parallele, caqueune avec une valeur differente
    /// </summary>
    /// <param name="inputs"></param>
    /// <param name="maxDegreeOfParallelism"></param>
    /// <param name="step"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private async Task<(IEnumerable<ProcessStep>, object?)> RunInParallel(Etape step,
        IEnumerable<object> inputs, int maxDegreeOfParallelism, RunnerState state)
    {
        var values = new ConcurrentBag<(ProcessStep, ComputeResult)>();
        var option = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism == 0 ? Environment.ProcessorCount : maxDegreeOfParallelism
        };

        //Le converter permet de corriger le type de l'objet en sortie, pour ne pas avoir d'erreur de cast dans le block suivant/Sortie
        IPipeProcessBlock? svc = null;

        //TakeWhile permet d'arreter le parallel sans générer de cancellationException
        await Parallel.ForEachAsync(inputs.TakeWhile(_ => !state.CancellationTokenSource.IsCancellationRequested),
            option,
            async (input, _) =>
            {
                var (e, c, f) = await ComputeStepAsync(step, input, state);
                svc ??= f;
                values.Add((e, c));
            });

        var runSteps = values.ToArray();

        var ret = runSteps.Where(w => w.Item2.IsSuccess)
            .Select(s => s.Item2.Value).ToList();

        state.SetStateOnError(runSteps);
        state.SetStateWarning(runSteps);

        var completedSteps = runSteps.SelectMany(s => GetAllProcessSteps(s.Item1, s.Item2));
        var data = svc?.ToTypedEnumerableConverter(ret) ?? null;
        return (completedSteps, data);
    }

    /// <summary>
    ///     Lance toutes les etapes en parallele, caqueune avec une valeur differente
    /// </summary>
    /// <param name="inputs"></param>
    /// <param name="maxDegreeOfParallelism"></param>
    /// <param name="step"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private async Task<(List<ProcessStep>, object?)> RunInDynamicParallel(DynamicEtape step,
        IEnumerable<object> inputs, int maxDegreeOfParallelism, RunnerState state)
    {
        var values = new ConcurrentBag<(List<ProcessStep> etapes, object? data)>();
        var option = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism == 0 ? Environment.ProcessorCount : maxDegreeOfParallelism
        };

        //Le converter permet de corriger le type de l'objet en sortie, pour ne pas avoir d'erreur de cast dans le block suivant/Sortie
        IProcessConverter? svc = null;

        //TakeWhile permet d'arreter le parallel sans générer de cancellationException
        await Parallel.ForEachAsync(inputs.TakeWhile(_ => !state.CancellationTokenSource.IsCancellationRequested),
            option,
            async (input, _) =>
            {
                IServiceScope? subScop = null;
                if (step.Etape.RunOpt.UseSubscope)
                    subScop = state.Provider.CreateScope();
                var (listEtapes, f) = GetDynamicEtapes(step.Etape, input, state, subScop);
                var (etapesTermine, o) = await RunInMixed(listEtapes, input, state);
                svc ??= f;
                values.Add((etapesTermine, o));
                subScop?.Dispose();
            });

        var allEtapes = values.SelectMany(e => e.etapes).ToList();
        var allData = values.Select(e => e.Item2);

        return (allEtapes, svc!.ToTypedEnumerableConverter(allData));
    }

    private IEnumerable<ProcessStep> GetAllProcessSteps(ProcessStep process, ComputeResult compute)
    {
        yield return process;
        foreach (var etape in compute.SousEtapes)
            yield return etape;
    }

    /// <summary>
    ///     Execute le connecteur avec options
    /// </summary>
    /// <param name="step"></param>
    /// <param name="input"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private async Task<(ProcessStep, ComputeResult, IPipeProcessBlock)> ComputeStepAsync(Etape step,
        object input, RunnerState state)
    {
        InternalPreEtape(step);
        var sw = Stopwatch.StartNew();

        var svc = state.Scope.ServiceProvider.GetRequiredKeyedService<IPipeProcessBlock>(step.TypeDuBlock);
        var result = await svc.ComputeAsync(input, step, state.CancellationTokenSource.Token);

        sw.Stop();

        InternalPostEtape(step);
        return (new ProcessStep(step, sw.Elapsed), result, svc);
    }
    
    #region DynamicStep

    private static (IReadOnlyList<IEtape>, IBuilderProcessBlock) GetDynamicEtapes(Etape step, object input,
        RunnerState state, IServiceScope? scope = null)
    {
        var svc = (scope ?? state.Scope).ServiceProvider
            .GetRequiredKeyedService<IBuilderProcessBlock>(step.TypeDuBlock);
        return (svc.GetBuilder(input, step, state.IsExplain), svc);
    }

    #endregion

    #region ExplainAsync

    public async Task<SortieRunner<TOut, TErreurManager>> ExplainAsync<TIn, TOut>(PipeBuilderDetails<TIn, TOut> detail, CancellationToken cancellationToken = default)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var stopwatch = Stopwatch.StartNew();

        using var state = new RunnerState(true, provider, _ErreurManager, cts);

        var explains = await ExplainInMixedAsync(detail.Etapes, state);

        stopwatch.Stop();

        return new SortieRunner<TOut, TErreurManager>(default, explains, _ErreurManager, stopwatch.Elapsed);
    }

    private static async Task<List<ProcessStep>> ExplainInMixedAsync(
        IReadOnlyList<IEtape> steps, RunnerState state)
    {
        var explains = new List<ProcessStep>();

        foreach (var etapeGen in steps)
            switch (etapeGen)
            {
                case Etape step:
                {
                    var result = await ExplainStepAsync(step, state);
                    explains.AddRange(result);
                    break;
                }

                case ParallelEtape agg:
                {
                    foreach (var aggEtape in agg.Etapes)
                    {
                        var result = await ExplainStepAsync(aggEtape, state);
                        explains.AddRange(result);
                    }

                    break;
                }

                case DynamicEtape dynamicStep:
                    var (extraStep, _) = GetDynamicEtapes(dynamicStep.Etape, null!, state);
                    var exp = await ExplainInMixedAsync(extraStep, state);
                    explains.AddRange(exp);
                    break;
            }

        return explains;
    }

    private static async Task<IList<ProcessStep>> ExplainStepAsync(Etape step, RunnerState state)
    {
        var svc = state.Scope.ServiceProvider.GetRequiredKeyedService<IPipeProcessBlock>(step.TypeDuBlock);
        var result = await svc.ExplainAsync(step, state.CancellationTokenSource.Token);

        return result;
    }

    #endregion
    
    
    protected virtual void InternalAnnuler(Etape etape = null)
    {
        _ErreurManager.Cancel();
    }

    protected virtual void InternalPreEtape(Etape etape)
    { }

    protected virtual void InternalPostEtape(Etape etape)
    { }

}

/// <summary>
///     PipeRunnnerBase complet qui prend en charge les erreur, les etats et la progression de chaque etape
/// </summary>
/// <param name="provider"></param>
/// <typeparam name="TErreurManager"></typeparam>
/// <typeparam name="TEtatManager"></typeparam>
/// <typeparam name="TEtat"></typeparam>
/// <typeparam name="TDeclancheur"></typeparam>
/// <typeparam name="TProgressionManager"></typeparam>
public abstract class PipeRunnerBase<TErreurManager, TEtatManager, TEtat, TDeclancheur, TProgressionManager>(
    IServiceProvider provider)
    : PipeRunnerBase<TErreurManager>(provider),
        IPipeRunner<TErreurManager, TEtatManager, TEtat, TDeclancheur, TProgressionManager>
    where TErreurManager : IPipeErreurManager, new()
    where TEtatManager : IPipeEtatManager<TEtat, TDeclancheur>, new()
    where TProgressionManager : IPipeProgressionManager, new()
{
    public TEtatManager EtatManager { get; } = new TEtatManager();
    public TProgressionManager ProgressionManager { get; } = new TProgressionManager();

    public async Task<SortieRunner<TOut, TErreurManager>> RunAsync<TIn, TOut>(
        PipeBuilderDetails<TIn, TOut> detail,
        [DisallowNull] TIn input,
        CancellationToken cancellationToken = default)
    {
        ProgressionManager.NotifierProgression("", 0);
        try
        {
            return await base.RunAsync(detail, input, cancellationToken);
        }
        finally
        {
            ProgressionManager.NotifierProgression("", 100);
        }
    }

    public async Task<SortieRunner<TOut, TErreurManager>> ExplainAsync<TIn, TOut>(
        PipeBuilderDetails<TIn, TOut> detail,
        CancellationToken cancellationToken = default)
    {
        ProgressionManager.NotifierProgression("", 0);
        try
        {
            return await base.ExplainAsync(detail, cancellationToken);
        }
        finally
        {
            ProgressionManager.NotifierProgression("", 100);
        }
    }

    protected override void InternalAnnuler(Etape etape)
    {
        EtatManager.DeclancherEtapeAnnuled(etape.TypeDuBlock.Name);
        base.InternalAnnuler(etape);
    }

    protected override void InternalPreEtape(Etape etape)
    {
        EtatManager.DeclancherEtapeDemarred(etape.TypeDuBlock.Name);
        base.InternalPreEtape(etape);
    }

    protected override void InternalPostEtape(Etape etape)
    {
        EtatManager.DeclancherEtapeTermined(etape.TypeDuBlock.Name);
        base.InternalPostEtape(etape);
    }
}