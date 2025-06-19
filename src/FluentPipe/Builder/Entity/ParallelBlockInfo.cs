using FluentPipe.Builder.Contracts;

namespace FluentPipe.Builder.Entity;

public sealed record ParallelBlockInfo(IEnumerable<BlockInfo> Etapes) : IBlockInfo;