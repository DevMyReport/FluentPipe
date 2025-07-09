using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Entity;

namespace FluentPipe.Blocks;

public abstract class PipeBlockHelper<TIn, TOut>
{
    public virtual string Nom { get; } = nameof(PipeBlockBase<TIn, TOut>);

    public Guid Identifiant { get; } = Guid.NewGuid();

    public virtual object ToTypedEnumerableConverter(IEnumerable<object?> value)
    {
        return value.Cast<TOut>();
    }

    /// <summary>
    ///     Convertit l'object generic du runner en objet typé pour le connecteur
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public virtual TIn Converter(object input)
    {
        if (input is TIn val)   
            return val;

        throw new InvalidCastException($"{GetType().FullName} : can't convert {input.GetType()} to {typeof(TIn)}");
    }

    #region ExplainAsync

    public virtual Task<IList<ProcessBlock>> ExplainAsync(BlockInfo context, CancellationToken ct)
    {
        var block = FormatExplainEtape(context);
        return Task.FromResult<IList<ProcessBlock>>(new List<ProcessBlock> {block});
    }

    protected virtual ProcessBlock FormatExplainEtape(BlockInfo context)
    {
        return new ProcessBlock(context);
    }

    #endregion

    public event EventHandler<ProgressionEvent>? ProgressChanged;

    public void NotifierProgression(ProgressionEvent evt)
    {
        ProgressChanged?.Invoke(this, evt);
    }

    protected void NotifierProgression(long fait, long aFaire)
    {
        NotifierProgression(new ProgressionEvent(Identifiant.ToString(), Nom, fait, aFaire));
    }
}