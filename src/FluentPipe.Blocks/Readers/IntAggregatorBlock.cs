using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.Readers;

public sealed class IntAggregatorBlock : PipeBlockBase<IEnumerable<IEnumerable<int>>, int[]>
{
    public override Task<ComputeResult<int[]>> ProcessAsync(IEnumerable<IEnumerable<int>> input, CancellationToken cancellationToken = default)
    {
        var r = input.SelectMany(s => s).ToArray();
        return ComputeResult.Success(r).ToTask();
    }
}