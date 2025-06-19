using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Contracts;
using FluentPipe.Builder.Entity;
using FluentPipe.InjectionHelper;

namespace FluentPipe.Builder;

public sealed class ParallelBuilder<TEntree, TSortie, TInput> : IParallelBuilder<TEntree, TSortie, TInput>
{
    private readonly List<IBlockInfo> _builderEtapes;
    private readonly RunOption _parallelModeOption;
    private readonly List<BlockInfo> _pipes = new();

    internal ParallelBuilder(List<IBlockInfo> builderEtapes)
    {
        _builderEtapes = builderEtapes;
        _parallelModeOption = new RunOption(new RunMultiBlockParallelOption());
    }

    #region With

    public IParallelBuilder<TEntree, TSortie, TInput> With<TBlock>(bool? bindingIsEnable = null)
        where TBlock : IPipeBlock<TEntree, TSortie>
    {
        var isEnabled = bindingIsEnable ?? true;
        if (isEnabled)
            _pipes.Add(new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>(), null, _parallelModeOption));

        return this;
    }

    public IParallelBuilder<TEntree, TSortie, TInput> With<TBlock, TOption>(TOption option, Func<TOption, bool>? bindingIsEnable = null)
        where TBlock : IPipeBlock<TEntree, TSortie, TOption>
        where TOption : IPipeOption
    {
        var isEnabled = bindingIsEnable?.Invoke(option) ?? true;
        if (isEnabled)
            _pipes.Add(new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>(), option, _parallelModeOption));

        return this;
    }

    public IParallelBuilder<TEntree, TSortie, TInput> With<TBlock, TOption>(Func<TOption> bindingOption, Func<TOption, bool>? bindingIsEnable = null)
        where TBlock : IPipeBlock<TEntree, TSortie, TOption>
        where TOption : IPipeOption
    {
        var option = bindingOption.Invoke();
        var isEnabled = bindingIsEnable?.Invoke(option) ?? true;

        if (isEnabled)
            _pipes.Add(new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>(), option, _parallelModeOption));

        return this;
    }

    #endregion With

    public IPipeBuilder<TInput, IEnumerable<TSortie>> ParallelEnd()
    {
        _builderEtapes.Add(new ParallelBlockInfo(_pipes));
        return new PipeBuilder<TInput, IEnumerable<TSortie>>(_builderEtapes);
    }

}