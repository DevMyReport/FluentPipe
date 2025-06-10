namespace FluentPipe.Blocks.Contracts;

public interface IPipeEnumerableBlock<in TIn, TOut> : IPipeBlock<TIn, IEnumerable<TOut>>;

public interface IPipeEnumerableBlock<in TIn, TOut, in TOption> : IPipeBlock<TIn, IEnumerable<TOut>, TOption>
    where TOption : IPipeOption;