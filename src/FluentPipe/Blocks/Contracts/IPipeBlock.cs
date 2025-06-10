namespace FluentPipe.Blocks.Contracts;

public interface IPipeBlock<in TIn, TOut> : IPipeProcessBlock;

public interface IPipeBlock<in TIn, TOut, in TOption> : IPipeProcessBlock
    where TOption : IPipeOption;
