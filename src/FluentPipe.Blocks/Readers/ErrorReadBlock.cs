using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.Readers;

public sealed class ErrorReadBlock : PipeEnumerableBlockBase<int, int>
{
    public static string ErrorMessage = "Code:-1 | ErrorReadBlock";

    public override Task<ComputeResult<IEnumerable<int>>> ProcessAsync(int input, CancellationToken cancellationToken = default)
    {
        return ComputeResult.Failure<IEnumerable<int>>(ErrorMessage).ToTask();
    }
}

public sealed class WarningReadBlock : PipeEnumerableBlockBase<int, int>
{
    public static string WarningMessage = "Code:1 | WarningReadBlock";

    public override Task<ComputeResult<IEnumerable<int>>> ProcessAsync(int input,
        CancellationToken cancellationToken = default)
    {
        return ComputeResult.SuccessWithWarnings<IEnumerable<int>>(new List<int>(){input}, new List<object>() {WarningMessage}).ToTask();
    }
}

public sealed class NullReadBlock : PipeEnumerableBlockBase<int, int>
{
    public override Task<ComputeResult<IEnumerable<int>>> ProcessAsync(int input, CancellationToken cancellationToken = default)
    {
        IEnumerable<int> result = null!;
        return ComputeResult.Success(result).ToTask();
    }
}