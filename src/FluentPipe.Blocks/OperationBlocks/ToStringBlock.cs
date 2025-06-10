using System.Collections.ObjectModel;
using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.OperationBlocks;

public sealed record ToStringBlockOption : IPipeOption
{
    public string Format { get; init; } = "";

    public IReadOnlyDictionary<string, string> Descrition() => ReadOnlyDictionary<string, string>.Empty;
}

public sealed class ToStringBlock : PipeBlockBase<int, string, ToStringBlockOption>
{
    public override Task<ComputeResult<string>> ProcessAsync(int input, ToStringBlockOption option, CancellationToken cancellationToken)
    {
        return ComputeResult.Success(option.Format + input).ToTask();
    }
}