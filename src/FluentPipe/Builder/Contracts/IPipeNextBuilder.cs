using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Builder.Contracts;

public interface IPipeNextBuilder<TEntree, TSortie>
{
    public IPipeBuilder<TEntree, TOut> Next<TBlock, TOut>(bool? bindingIsEnable = null)
        where TBlock : IPipeBlock<TSortie, TOut>;

    public IPipeBuilder<TEntree, TOut> Next<TBlock, TOut, TOption>(TOption option,
        Func<TOption, bool>? bindingIsEnable = null)
        where TBlock : IPipeBlock<TSortie, TOut, TOption>
        where TOption : IPipeOption;

    public IPipeParallelBuilder<TEntree, IEnumerable<TPrimitif>, TPrimitif> NextEnumerable<TBlock, TPrimitif>(bool? bindingIsEnable = null)
        where TBlock : IPipeEnumerableBlock<TSortie, TPrimitif>;

    public IPipeParallelBuilder<TEntree, IEnumerable<TPrimitif>, TPrimitif> NextEnumerable<TBlock, TPrimitif, TOption>(TOption option, Func<TOption, bool>? bindingIsEnable = null)
        where TBlock : IPipeEnumerableBlock<TSortie, TPrimitif, TOption>
        where TOption : IPipeOption;

    public IPipeBuilder<TEntree, TSortie> ResetScope();
}