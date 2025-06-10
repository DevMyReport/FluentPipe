using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Builder.Contracts;

public interface IPipeDynamicBuilder<TEntree, out TSortie>
{
    public IPipeBuilder<TEntree, TOut> OneIn<TIn, TOut>(bool bindingIsEnable = true)
        where TIn : IBuilderProcessBlock<TSortie, TOut>;

    public IPipeBuilder<TEntree, TOut> OneIn<TIn, TOut, TOption>(TOption option, Func<TOption, bool>? bindingIsEnable = null)
        where TIn : IBuilderProcessBlock<TSortie, TOut, TOption>
        where TOption : IPipeOption;
}