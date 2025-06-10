using System.Collections.ObjectModel;
using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.Readers;
public sealed class LongReadBlock : PipeBlockBase<int, IEnumerable<int>>
{
    public override async Task<ComputeResult<IEnumerable<int>>> ProcessAsync(int input, CancellationToken cancellationToken = default)
    {
        // ReSharper disable once MethodSupportsCancellation
        await Task.Delay(TimeSpan.FromSeconds(1));
        var result = Enumerable.Range(1, input);
        return ComputeResult.Success(result);
    }
}

public sealed class LongReadOptBlock : PipeEnumerableBlockBase<int, int, LongReadOption>
{
    public override async Task<ComputeResult<IEnumerable<int>>> ProcessAsync(int input, LongReadOption option, CancellationToken cancellationToken = default)
    {
        // ReSharper disable once MethodSupportsCancellation
        await Task.Delay(TimeSpan.FromSeconds(1));
        var result = Enumerable.Range(1, input);
        return ComputeResult.Success(result);
    }
}

public sealed class LongReadOption : IPipeOption
{
    public IReadOnlyDictionary<string, string> Descrition() => ReadOnlyDictionary<string, string>.Empty;
}