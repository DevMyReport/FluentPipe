using System.Diagnostics;
using FluentPipe.Builder.Entity;

namespace FluentPipe.Blocks.Contracts;

[DebuggerDisplay("Durée : {Duree}, {Key}")]
public sealed record ProcessStep
{
    public ProcessStep(string key, IReadOnlyDictionary<string, string>? description = null, string? parallelGroup = null, TimeSpan duree = new TimeSpan())
    {
        Key = key;
        Description = description;
        ParallelGroup = parallelGroup;
        Duree = duree;
    }

    public ProcessStep(Etape etape, TimeSpan duree = new TimeSpan())
        : this(etape.TypeDuBlock.FullName!, etape.Option?.Descrition(), etape.RunOpt.Description(), duree)
    { }

    public string Key { get; }

    public IReadOnlyDictionary<string, string>? Description { get; }

    public string? ParallelGroup { get; }

    public TimeSpan Duree { get; }
}