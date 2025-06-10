using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.Readers;

public sealed class LongReadEnumBlock : PipeEnumerableBlockBase<int, int>
{
    public override Task<ComputeResult<IEnumerable<int>>> ProcessAsync(int input, CancellationToken cancellationToken = default)
    {
        // ReSharper disable once MethodSupportsCancellation
        Thread.Sleep(TimeSpan.FromSeconds(1));
        var resu = Enumerable.Range(1, input);
        return ComputeResult.Success(resu).ToTask();
    }
}