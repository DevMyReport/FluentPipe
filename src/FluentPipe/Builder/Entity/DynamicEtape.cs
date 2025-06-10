using FluentPipe.Builder.Contracts;

namespace FluentPipe.Builder.Entity;

public sealed record DynamicEtape(Etape Etape) : IEtape;