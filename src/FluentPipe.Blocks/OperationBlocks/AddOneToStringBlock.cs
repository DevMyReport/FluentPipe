using System.Collections.ObjectModel;
using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.OperationBlocks;

public sealed class AddOneToStringBlock : PipeBlockBase<int, string>
{
    public override Task<ComputeResult<string>> ProcessAsync(int input, CancellationToken cancellationToken = default)
    {
        return ComputeResult.Success((input + 1).ToString()).ToTask();
    }
}

public record AddOneToStringOption(int Increment) : IPipeOption
{
    public IReadOnlyDictionary<string, string> Descrition() => ReadOnlyDictionary<string, string>.Empty;
}

public sealed class AddOneToStringOptionBlock : PipeBlockBase<int, string, AddOneToStringOption>
{
    public override Task<ComputeResult<string>> ProcessAsync(int input, AddOneToStringOption option, CancellationToken cancellationToken = default)
    {
        return ComputeResult.Success((input + option.Increment).ToString()).ToTask();
    }
}