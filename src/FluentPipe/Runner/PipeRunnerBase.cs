using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FluentPipe.Blocks;
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
    private TErreurManager _ErreurManager { get; } = new();
    
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
    public virtual async Task<SortieRunner<TOut, TErreurManager>> RunAsync<TIn, TOut>(
        PipeBuilderDetails<TIn, TOut> detail,
        [DisallowNull] TIn input,
        CancellationToken cancellationToken = default)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var stopwatch = Stopwatch.StartNew();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var state = new RunnerState(false, provider, _ErreurManager, cts);
        var (listEtapes, data) = await RunInMixed(detail.Blocks, input, state);
        
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
    /// <param name="blocks"></param>
    /// <param name="input"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private async Task<(List<ProcessBlock>, object?)> RunInMixed(IReadOnlyList<IBlockInfo> blocks, object input,
        RunnerState state)
    {
        var data = input;
        var listeBlocks = new List<ProcessBlock>();

        foreach (var blockInfo in blocks)
        {
            if (state.CancellationTokenSource.IsCancellationRequested)
                break;

            if (data == null)
                break;

            switch (blockInfo)
            {
                case ScopeBlockInfo:
                    state.ResetScope();
                    break;

                case DynamicBlockInfo dynamicStep when dynamicStep.BlockInfo.RunOpt.MonoBlockParallelOption is
                    { } optionMono:

                    //il faut lancer le meme connecteur autant de fois qu'il y'a de valeur dans la list
                    var dynamicValues = (data as IEnumerable)?.Cast<object>();
                    if (dynamicValues == null)
                        throw new InvalidCastException(
                            $"Failed to cast Etape {dynamicStep.BlockInfo.TypeDuBlock.Name}");

                    (var listSteps, data) = await RunInDynamicParallel(dynamicStep, dynamicValues,
                        optionMono.MaxDegreeOfParallelism, state);

                    listeBlocks.AddRange(listSteps);
                    break;

                case DynamicBlockInfo dynamicBlock:
                    //un block qui rajoute des étapes en fonction de la valeur
                    var (extraSteps, _) = GetDynamicEtapes(dynamicBlock.BlockInfo, data, state);
                    var (s, o) = await RunInMixed(extraSteps, data, state);

                    listeBlocks.AddRange(s);
                    data = o;
                    break;

                case ParallelBlockInfo blockMulti:
                {
                    //plusieurs connecteur a lancer en parallele
                    var subList = blockMulti.Etapes;
                    (var listStepsMulti, data) = await RunInParallel(subList, data, state);
                    listeBlocks.AddRange(listStepsMulti);
                    break;
                }

                case BlockInfo blockMono when blockMono.RunOpt.MonoBlockParallelOption is { } optionMono:
                {
                    //il faut lancer le meme connecteur autant de fois qu'il y a de valeur dans la list
                    var values = (data as IEnumerable)?.Cast<object>();
                    if (values == null)
                        throw new InvalidCastException($"Failed to cast Etape {blockMono.TypeDuBlock.Name}");

                    (var listStepsMono, data) =
                        await RunInParallel(blockMono, values, optionMono.MaxDegreeOfParallelism, state);

                    listeBlocks.AddRange(listStepsMono);
                    break;
                }

                case BlockInfo blockMono:
                    var (fini, blockResult, _) = await ComputeBlockAsync(blockMono, data, state);
                    data = blockResult.Value;
                    listeBlocks.AddRange(GetAllProcessSteps(fini, blockResult));
                    state.SetStateOnError(blockResult);
                    state.SetStateWarning(blockResult);
                    break;
            }
        }

        return (listeBlocks, data);
    }

    /// <summary>
    ///     Lance toutes les etapes en parallele
    /// </summary>
    /// <param name="steps"></param>
    /// <param name="input"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private async Task<(IEnumerable<ProcessBlock>, object?)> RunInParallel(IEnumerable<BlockInfo> steps,
        object input, RunnerState state)
    {
        var values = new ConcurrentBag<(ProcessBlock, ComputeResult)>();
        var option = new ParallelOptions();
        IPipeProcessBlock? converter = null;

        //TakeWhile permet d'arreter le parallel sans généré de cancellationException
        await Parallel.ForEachAsync(steps.TakeWhile(_ => !state.CancellationTokenSource.IsCancellationRequested),
            option,
            async (blockInfo, _) =>
            {
                var (e, c, f) = await ComputeBlockAsync(blockInfo, input, state);
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
    /// <param name="blockInfo"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private async Task<(IEnumerable<ProcessBlock>, object?)> RunInParallel(BlockInfo blockInfo,
        IEnumerable<object> inputs, int maxDegreeOfParallelism, RunnerState state)
    {
        var values = new ConcurrentBag<(ProcessBlock, ComputeResult)>();
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
                var (e, c, f) = await ComputeBlockAsync(blockInfo, input, state);
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
    /// <param name="blockInfo"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private async Task<(List<ProcessBlock>, object?)> RunInDynamicParallel(DynamicBlockInfo blockInfo,
        IEnumerable<object> inputs, int maxDegreeOfParallelism, RunnerState state)
    {
        var values = new ConcurrentBag<(List<ProcessBlock> etapes, object? data)>();
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
                if (blockInfo.BlockInfo.RunOpt.UseSubscope)
                    subScop = state.Provider.CreateScope();
                var (listEtapes, f) = GetDynamicEtapes(blockInfo.BlockInfo, input, state, subScop);
                var (etapesTermine, o) = await RunInMixed(listEtapes, input, state);
                svc ??= f;
                values.Add((etapesTermine, o));
                subScop?.Dispose();
            });

        var allEtapes = values.SelectMany(e => e.etapes).ToList();
        var allData = values.Select(e => e.Item2);

        return (allEtapes, svc!.ToTypedEnumerableConverter(allData));
    }

    private IEnumerable<ProcessBlock> GetAllProcessSteps(ProcessBlock process, ComputeResult compute)
    {
        yield return process;
        foreach (var etape in compute.SousBlocs)
            yield return etape;
    }

    private IPipeProcessBlock InstancierBlock(BlockInfo blockInfo, RunnerState state)
    {
        var block = state.Scope.ServiceProvider.GetRequiredKeyedService<IPipeProcessBlock>(blockInfo.TypeDuBlock);
        OnInstancierBlock(blockInfo, block);
        return block;
    }

    /// <summary>
    ///     Execute le connecteur avec options
    /// </summary>
    /// <param name="blockInfo"></param>
    /// <param name="input"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    private async Task<(ProcessBlock, ComputeResult, IPipeProcessBlock)> ComputeBlockAsync(BlockInfo blockInfo,
        object input, RunnerState state)
    {
        var sw = Stopwatch.StartNew();

        var svc = InstancierBlock(blockInfo, state);
        var result = await svc.ComputeAsync(input, blockInfo, state.CancellationTokenSource.Token);

        sw.Stop();

        OnTerminerBlock(blockInfo, svc);
        return (new ProcessBlock(blockInfo, sw.Elapsed), result, svc);
    }

    #region DynamicStep

    private static (IReadOnlyList<IBlockInfo>, IBuilderProcessBlock) GetDynamicEtapes(BlockInfo blockInfo, object input,
        RunnerState state, IServiceScope? scope = null)
    {
        var svc = (scope ?? state.Scope).ServiceProvider
            .GetRequiredKeyedService<IBuilderProcessBlock>(blockInfo.TypeDuBlock);
        return (svc.GetBuilder(input, blockInfo, state.IsExplain), svc);
    }

    #endregion


    protected virtual void InternalAnnuler(BlockInfo blockInfo = null)
    {
        _ErreurManager.Cancel();
    }

    /// <summary>
    /// À surcharger dans la classe metier qui gere le Pipe pour s'abonner aux événements des blocks
    /// Progression vie ProgressionManager
    /// </summary>
    /// <param name="blockInfo"></param>
    /// <param name="block"></param>
    protected virtual void OnInstancierBlock(BlockInfo blockInfo, IPipeProcessBlock block)
    {
    }
    
    /// <summary>
    /// À surcharger dans la classe metier qui gere le Pipe pour se désabonner aux événements des blocks
    /// Progression vie ProgressionManager
    /// </summary>
    /// <param name="blockInfo"></param>
    /// <param name="block"></param>
    protected virtual void OnTerminerBlock(BlockInfo blockInfo, IPipeProcessBlock block)
    {
    }

    #region ExplainAsync

    public virtual async Task<SortieRunner<TOut, TErreurManager>> ExplainAsync<TIn, TOut>(PipeBuilderDetails<TIn, TOut> detail,
        CancellationToken cancellationToken = default)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var stopwatch = Stopwatch.StartNew();

        using var state = new RunnerState(true, provider, _ErreurManager, cts);

        var explains = await ExplainInMixedAsync(detail.Blocks, state);

        stopwatch.Stop();

        return new SortieRunner<TOut, TErreurManager>(default, explains, _ErreurManager, stopwatch.Elapsed);
    }

    private async Task<List<ProcessBlock>> ExplainInMixedAsync(
        IReadOnlyList<IBlockInfo> blocks, RunnerState state)
    {
        var explains = new List<ProcessBlock>();

        foreach (var etapeGen in blocks)
            switch (etapeGen)
            {
                case BlockInfo blockInfo:
                {
                    var result = await ExplainStepAsync(blockInfo, state);
                    explains.AddRange(result);
                    break;
                }

                case ParallelBlockInfo agg:
                {
                    foreach (var aggEtape in agg.Etapes)
                    {
                        var result = await ExplainStepAsync(aggEtape, state);
                        explains.AddRange(result);
                    }

                    break;
                }

                case DynamicBlockInfo dynamicBlock:
                    var (extraStep, _) = GetDynamicEtapes(dynamicBlock.BlockInfo, null!, state);
                    var exp = await ExplainInMixedAsync(extraStep, state);
                    explains.AddRange(exp);
                    break;
            }

        return explains;
    }

    private async Task<IList<ProcessBlock>> ExplainStepAsync(BlockInfo blockInfo, RunnerState state)
    {
        var block = InstancierBlock(blockInfo, state);
        var result = await block.ExplainAsync(blockInfo, state.CancellationTokenSource.Token);
        OnTerminerBlock(blockInfo, block);
        return result;
    }

    #endregion
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
    public TEtatManager EtatManager { get; } = new();

    public TProgressionManager ProgressionManager { get; } = new();

    public override async Task<SortieRunner<TOut, TErreurManager>> RunAsync<TIn, TOut>(
        PipeBuilderDetails<TIn, TOut> detail,
        [DisallowNull] TIn input,
        CancellationToken cancellationToken = default)
    {
        ProgressionManager.NotifierProgressionPipe(0, detail.Blocks.Count());
        try
        {
            return await base.RunAsync(detail, input, cancellationToken);
        }
        finally
        {
            ProgressionManager.NotifierProgressionPipe(detail.Blocks.Count(), detail.Blocks.Count());
        }
    }

    public override async Task<SortieRunner<TOut, TErreurManager>> ExplainAsync<TIn, TOut>(
        PipeBuilderDetails<TIn, TOut> detail,
        CancellationToken cancellationToken = default)
    {
        ProgressionManager.NotifierProgressionPipe(0, detail.Blocks.Count());
        try
        {
            return await base.ExplainAsync(detail, cancellationToken);
        }
        finally
        {
            ProgressionManager.NotifierProgressionPipe(detail.Blocks.Count(),
                detail.Blocks.Count());
        }
    }

    protected override void InternalAnnuler(BlockInfo blockInfo)
    {
        base.InternalAnnuler(blockInfo);
    }

    protected override void OnInstancierBlock(BlockInfo blockInfo, IPipeProcessBlock block)
    {
        base.OnInstancierBlock(blockInfo, block);
        block.ProgressChanged += ProgressionManager.OnProgressionBlock;
        EtatManager.InitialiserBlockMachineAEtat(block.Identifiant.ToString());
        EtatManager.DeclancherBlockDemarred(block.Identifiant.ToString());
    }

    protected override void OnTerminerBlock(BlockInfo blockInfo, IPipeProcessBlock block)
    {
        base.OnTerminerBlock(blockInfo, block);
        EtatManager.DeclancherBlockTermined(block.Identifiant.ToString());
        ProgressionManager.NotifierBlockTraited(blockInfo);
        block.ProgressChanged -= ProgressionManager.OnProgressionBlock;
    }
}