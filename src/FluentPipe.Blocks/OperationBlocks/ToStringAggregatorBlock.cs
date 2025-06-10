using System.Text;
using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.OperationBlocks;

public sealed class ToStringAggregatorBlock : PipeBlockBase<IEnumerable<int>, string>
{
    public override Task<ComputeResult<string>> ProcessAsync(IEnumerable<int> input, CancellationToken cancellationToken = default)
    {
        var res = input.Select(s => (s + 1));
        var result = new StringBuilder();
        foreach (var i in res)
        {
            result.Append(i);
        }
        return ComputeResult.Success(result.ToString()).ToTask();
    }
}