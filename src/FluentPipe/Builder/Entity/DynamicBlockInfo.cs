using FluentPipe.Builder.Contracts;

namespace FluentPipe.Builder.Entity;

public sealed record DynamicBlockInfo(BlockInfo BlockInfo) : IBlockInfo;