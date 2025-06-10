using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Builder.Contracts;

public interface IPipeParallelBuilder<TEntree, TListeSortie, TElementSortie> : IPipeBuilder<TEntree, TListeSortie>
    where TListeSortie : IEnumerable<TElementSortie>
{
    public IPipeParallelBuilder<TEntree, IEnumerable<TOut>, TOut> NextMonoParallel<TBlock, TOut>(int maxParallelism)
        where TBlock : IPipeBlock<TElementSortie, TOut>;

    public IPipeParallelBuilder<TEntree, IEnumerable<TOut>, TOut> NextMonoParallel<TBlock, TOut, TOption>(TOption option, int maxParallelism)
        where TBlock : IPipeBlock<TElementSortie, TOut, TOption>
        where TOption : IPipeOption;

    public IPipeParallelBuilder<TEntree, IEnumerable<TOut>, TOut> NextPipeMonoParallel<TOut>(IPipeBuilder<TElementSortie, TOut> pipe, int maxParallelism);

    public IPipeParallelBuilder<TEntree, IEnumerable<TElementSortie>, TElementSortie> NextPipeMonoParallel(IPipeBuilder<TElementSortie, TElementSortie> pipe, int maxParallelism, bool bindingIsEnable);

    public IPipeParallelBuilder<TEntree, IEnumerable<TOut>, TOut> NextDynamicParallel<TBlock, TOut>(int maxParallelism)
        where TBlock : IBuilderProcessBlock<TElementSortie, TOut>;

    public IPipeParallelBuilder<TEntree, IEnumerable<TOut>, TOut> NextDynamicParallel<TBlock, TOut, TOption>(TOption option, int maxParallelism, bool useScope = false)
        where TBlock : IBuilderProcessBlock<TElementSortie, TOut, TOption>
        where TOption : IPipeOption;
}