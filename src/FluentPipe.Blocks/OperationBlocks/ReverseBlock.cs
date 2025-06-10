using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.OperationBlocks;

public sealed class ReverseBlock : PipeBlockBase<string, string>
{
    public override Task<ComputeResult<string>> ProcessAsync(string input, CancellationToken cancellationToken = default)
    {
        var result = new string(input.Reverse().ToArray());
        return ComputeResult.Success(result).ToTask();
    }
}