using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.Lifetime;

public sealed class TransientBlock : PipeBlockBase<LifeTimeResult, LifeTimeResult>
{
    private readonly Guid _guid;
    private int _value;

    public TransientBlock()
    {
        _guid = Guid.NewGuid();
        _value = 0;
    }

    public override Task<ComputeResult<LifeTimeResult>> ProcessAsync(LifeTimeResult input, CancellationToken cancellationToken = default)
    {
        _value += input.Value;
        var result = new LifeTimeResult(_guid, _value);
        return ComputeResult.Success(result).ToTask();
    }
}