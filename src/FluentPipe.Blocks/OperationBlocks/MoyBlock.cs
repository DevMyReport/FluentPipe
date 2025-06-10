using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.OperationBlocks;

public sealed class MoyBlock : PipeBlockBase<IEnumerable<int>, int>
{
    public override Task<ComputeResult<int>> ProcessAsync(IEnumerable<int> input, CancellationToken cancellationToken = default)
    {
        return ComputeResult.Success((int)input.Average()).ToTask();
    }
}