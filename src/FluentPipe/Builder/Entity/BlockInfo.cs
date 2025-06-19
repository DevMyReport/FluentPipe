using System.Diagnostics;
using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Contracts;

namespace FluentPipe.Builder.Entity;

[DebuggerDisplay("{TypeDuBlock.Name} - {Option}")]
public sealed record BlockInfo : IBlockInfo
{
    public BlockInfo(Type TypeDuBlock, IPipeOption? Option = null, RunOption? RunOpt = null)
    {
        this.TypeDuBlock = TypeDuBlock;
        this.Option = Option;
        this.RunOpt = RunOpt ?? new RunOption();
    }

    public Type TypeDuBlock { get; }

    public IPipeOption? Option { get; }

    public RunOption RunOpt { get; }
}