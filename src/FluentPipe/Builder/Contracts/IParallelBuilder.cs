using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Builder.Contracts;

public interface IParallelBuilder<out TEntree, TSortie, TInput>
{
    IParallelBuilder<TEntree, TSortie, TInput> With<TBlock>(bool? bindingIsEnable = null)
        where TBlock : IPipeBlock<TEntree, TSortie>;

    IParallelBuilder<TEntree, TSortie, TInput> With<TBlock, TOption>(TOption option,
        Func<TOption, bool>? bindingIsEnable = null)
        where TBlock : IPipeBlock<TEntree, TSortie, TOption>
        where TOption : IPipeOption;

    IParallelBuilder<TEntree, TSortie, TInput> With<TBlock, TOption>(Func<TOption> bindingOption,
        Func<TOption, bool>? bindingIsEnable = null)
        where TBlock : IPipeBlock<TEntree, TSortie, TOption>
        where TOption : IPipeOption;

    IPipeBuilder<TInput, IEnumerable<TSortie>> ParallelEnd();
}