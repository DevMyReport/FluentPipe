using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.OperationBlocks;

public sealed class ConcatBlock : PipeBlockBase<IEnumerable<string>, string>
{
    public override Task<ComputeResult<string>> ProcessAsync(IEnumerable<string> input, CancellationToken cancellationToken = default)
    {
        var result = string.Empty;
        foreach (var se in input)
        {
            result += se;
        }

        return ComputeResult.Success(result).ToTask();
    }
}