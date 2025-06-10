using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.System;

public sealed class PrintBlock : PipeBlockBase<string, string>
{
    public override Task<ComputeResult<string>> ProcessAsync(string input, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(input);
        return ComputeResult.Success(input).ToTask();
    }
}