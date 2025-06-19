using FluentPipe.Builder.Contracts;

namespace FluentPipe.Builder.Entity;

public sealed record PipeBuilderDetails<TEntree, TSortie>(IReadOnlyList<IBlockInfo> Blocks);