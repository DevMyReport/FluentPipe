namespace FluentPipe.Blocks.Contracts;

/// <summary>
///     Interface de marquage pour configurer un connecteur
/// </summary>
public interface IPipeOption
{
    IReadOnlyDictionary<string, string> Descrition();
}
