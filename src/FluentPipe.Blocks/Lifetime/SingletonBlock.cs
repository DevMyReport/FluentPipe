using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.Lifetime;

public sealed class SingletonBlock : PipeBlockBase<LifeTimeResult, LifeTimeResult>
{
    private int _value;
    private readonly object _valueLock = new();

    public SingletonBlock()
    {
        _value = 0;
    }

    public override Task<ComputeResult<LifeTimeResult>> ProcessAsync(LifeTimeResult input, CancellationToken cancellationToken = default)
    {
        lock (_valueLock)
        {
            _value += input.Value;
            var result = new LifeTimeResult(Identifiant, _value);
            return ComputeResult.Success(result).ToTask();
        }
    }
}

public sealed record LifeTimeResult(Guid Guid, int Value);