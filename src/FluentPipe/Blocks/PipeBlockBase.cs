using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Entity;

namespace FluentPipe.Blocks;

public interface INotifierProgression
{
    public event EventHandler<ProgressionEvent>? ProgressChanged;

    public void NotifierProgression(ProgressionEvent evt);
}

/// <summary>
///     base classe du connecteur
/// </summary>
/// <typeparam name="TIn"></typeparam>
/// <typeparam name="TOut"></typeparam>
public abstract class PipeBlockBase<TIn, TOut> : PipeBlockHelper<TIn, TOut>, IPipeBlock<TIn, TOut>
{
    /// <summary>
    ///     Implémentation par default de l'interface du connecteur
    ///     pour gérer la conversion vers le type connu du connecteur
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<ComputeResult> ComputeAsync(object input, BlockInfo context,
        CancellationToken cancellationToken = default)
    {
        return await ProcessAsync(Converter(input), cancellationToken);
    }

    /// <summary>
    ///     Logique du connecteur
    /// </summary>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract Task<ComputeResult<TOut>> ProcessAsync(TIn input, CancellationToken cancellationToken = default);
    
}

/// <summary>
///     base classe du connecteur gerant une option
/// </summary>
/// <typeparam name="TIn"></typeparam>
/// <typeparam name="TOut"></typeparam>
/// <typeparam name="TOption"></typeparam>
public abstract class PipeBlockBase<TIn, TOut, TOption> : PipeBlockHelper<TIn, TOut>,
    IPipeBlock<TIn, TOut, TOption>
    where TOption : IPipeOption
{
    /// <summary>
    ///     implementation par default de l'interface du connecteur
    ///     pour gérer la convertion vers le type connu du connecteur
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ComputeResult> ComputeAsync(object input, BlockInfo context, CancellationToken cancellationToken)
    {
        return await ProcessAsync(Converter(input), (TOption) context.Option!, cancellationToken);
    }

    public abstract Task<ComputeResult<TOut>> ProcessAsync(TIn input, TOption option,
        CancellationToken cancellationToken = default);
}

public abstract class PipeEnumerableBlockBase<TIn, TOut> : PipeBlockBase<TIn, IEnumerable<TOut>>,
    IPipeEnumerableBlock<TIn, TOut>;

public abstract class PipeEnumerableBlockBase<TIn, TOut, TOption> : PipeBlockBase<TIn, IEnumerable<TOut>, TOption>,
    IPipeEnumerableBlock<TIn, TOut, TOption>
    where TOption : IPipeOption;