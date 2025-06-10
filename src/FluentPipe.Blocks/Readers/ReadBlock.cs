using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Contracts;

namespace FluentPipe.Blocks.Readers;

public sealed class ReadBlock : PipeEnumerableBlockBase<int, int>
{
    public override Task<ComputeResult<IEnumerable<int>>> ProcessAsync(int input, CancellationToken cancellationToken = default)
    {
        var result = Enumerable.Range(1, input);
        return ComputeResult.Success(result).ToTask();
    }
}

public static class ReadBlockExtension
{
    public static IPipeParallelBuilder<int, IEnumerable<int>, int> Next_ReadBlock(this IPipeBuilder<int, int> builder)
    {
        return builder.NextEnumerable<ReadBlock, int>();
    }
}
