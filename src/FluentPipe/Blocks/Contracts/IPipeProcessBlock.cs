using FluentPipe.Builder.Contracts;
using FluentPipe.Builder.Entity;

namespace FluentPipe.Blocks.Contracts;

/// <summary>
///     Interface de contract pour lancer le connecteur via le runner
/// </summary>
public interface IPipeProcessBlock : IProcessConverter
{
    /// <summary>
    ///     Generic interface lancé par le runner
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ComputeResult> ComputeAsync(object input, Etape context, CancellationToken cancellationToken);

    /// <summary>
    ///     Retourne le detail sur la chaine d'execution
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IList<ProcessStep>> ExplainAsync(Etape context, CancellationToken cancellationToken);
}

public interface IBuilderProcessBlock : IProcessConverter
{
    IReadOnlyList<IEtape> GetBuilder(object input, Etape context, bool isExplain);
}

public interface IProcessConverter
{
    /// <summary>
    /// Permet de gérer les conversions pour les operation parallele
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    object ToTypedEnumerableConverter(IEnumerable<object?> value);
}