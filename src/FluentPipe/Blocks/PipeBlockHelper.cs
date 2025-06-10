using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Entity;

namespace FluentPipe.Blocks;

public abstract class PipeBlockHelper<TIn, TOut>
{
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

    public virtual Task<IList<ProcessStep>> ExplainAsync(Etape context, CancellationToken ct)
    {
        var step = FormatExplainEtape(context);
        return Task.FromResult<IList<ProcessStep>>(new List<ProcessStep> { step });
    }

    protected virtual ProcessStep FormatExplainEtape(Etape context)
    {
        return new ProcessStep(context);
    }

    #endregion
}