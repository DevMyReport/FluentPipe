using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.Lifetime;

public sealed class LifeTimeInputBlock : PipeBlockBase<int, LifeTimeResult>
{
    public override Task<ComputeResult<LifeTimeResult>> ProcessAsync(int input, CancellationToken cancellationToken = default)
    {
        var result = new LifeTimeResult(Guid.NewGuid(), input);
        return ComputeResult.Success(result).ToTask();
    }
}