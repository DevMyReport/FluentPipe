namespace FluentPipe.Blocks.Contracts;

public interface IBuilderProcessBlock<in TIn, TOut> : IBuilderProcessBlock;

public interface IBuilderProcessBlock<in TIn, TOut, in TOption> : IBuilderProcessBlock
    where TOption : IPipeOption;