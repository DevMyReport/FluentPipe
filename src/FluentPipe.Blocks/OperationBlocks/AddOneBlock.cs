using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.OperationBlocks;

public sealed class AddOneBlock : PipeBlockBase<int, int>
{
    public override Task<ComputeResult<int>> ProcessAsync(int input, CancellationToken cancellationToken = default)
    {
        return ComputeResult.Success(input + 1).ToTask();
    }
}