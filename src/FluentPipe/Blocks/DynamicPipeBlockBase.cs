using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Contracts;
using FluentPipe.Builder.Entity;

namespace FluentPipe.Blocks;

public abstract class DynamicPipeBlockBase<TIn, TOut> : PipeBlockHelper<TIn, TOut>, IBuilderProcessBlock<TIn, TOut>
{
    public abstract IPipeBuilder<TIn, TOut> GetBuilder(TIn? input, bool isExplain);

    public IReadOnlyList<IBlockInfo> GetBuilder(object input, BlockInfo context, bool isExplain)
    {
        return GetBuilder(isExplain ? default : Converter(input), isExplain).GetDetails().Blocks;
    }
}

public abstract class DynamicPipeBlockBase<TIn, TOut, TOption> : PipeBlockHelper<TIn, TOut>,
    IBuilderProcessBlock<TIn, TOut, TOption>
    where TOption : IPipeOption
{
    public abstract IPipeBuilder<TIn, TOut> GetBuilder(TIn? input, TOption option, bool isExplain);

    public IReadOnlyList<IBlockInfo> GetBuilder(object input, BlockInfo context, bool isExplain)
    {
        return GetBuilder(isExplain ? default : Converter(input), (TOption)context.Option!,isExplain).GetDetails().Blocks;
    }
}