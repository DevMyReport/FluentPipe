using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.OperationBlocks;

public sealed class ToIntBlock : PipeBlockBase<string, int>
{
    public override Task<ComputeResult<int>> ProcessAsync(string input, CancellationToken cancellationToken = default)
    {
        var result = int.Parse(input);
        return ComputeResult.Success(result).ToTask();
    }
}