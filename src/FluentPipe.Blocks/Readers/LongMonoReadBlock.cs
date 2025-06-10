using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.Readers;

public sealed class LongMonoReadBlock : PipeBlockBase<int, int>
{
    public override async Task<ComputeResult<int>> ProcessAsync(int input, CancellationToken cancellationToken = default)
    {
        // ReSharper disable once MethodSupportsCancellation
        await Task.Delay(TimeSpan.FromSeconds(1));
        return ComputeResult.Success(500);
    }
}