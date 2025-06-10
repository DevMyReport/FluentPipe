using FluentPipe.Builder.Contracts;

namespace FluentPipe.Builder.Entity;

public sealed record ParallelEtape(IEnumerable<Etape> Etapes) : IEtape;