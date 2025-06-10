using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.System
{
    public sealed class CancelExceptionBlock : PipeBlockBase<string, string>
    {
        public override Task<ComputeResult<string>> ProcessAsync(string input, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ComputeResult.Success(input).ToTask();
        }
    }
}
