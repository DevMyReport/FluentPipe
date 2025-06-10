using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.Lifetime;

public sealed class LifeTimeMultiInputBlock : PipeBlockBase<int, IEnumerable<LifeTimeResult>>
{
    public override Task<ComputeResult<IEnumerable<LifeTimeResult>>> ProcessAsync(int input, CancellationToken cancellationToken = default)
    {
        var results=new List<LifeTimeResult>();

        for (int i = 0; i < input; i++)
        {
            results.Add(new LifeTimeResult(Guid.NewGuid(), 1));
        }

        return ComputeResult.Success(results.AsEnumerable()).ToTask();
    }
}