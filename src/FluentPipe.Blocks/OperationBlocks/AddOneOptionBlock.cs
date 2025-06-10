using System.Collections.ObjectModel;
using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.OperationBlocks;

public sealed record AddOneOption(int Increment) : IPipeOption
{
    public IReadOnlyDictionary<string, string> Descrition() => ReadOnlyDictionary<string, string>.Empty;
}

public sealed class AddOneOptionBlock : PipeBlockBase<IEnumerable<int>, int, AddOneOption>
{
    public override Task<ComputeResult<int>> ProcessAsync(IEnumerable<int> input, AddOneOption option, CancellationToken cancellationToken = default)
    {
        return ComputeResult.Success(input.Sum() + option.Increment).ToTask();
    }
}