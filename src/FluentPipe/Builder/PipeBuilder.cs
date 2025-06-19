using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Contracts;
using FluentPipe.Builder.Entity;
using FluentPipe.InjectionHelper;

namespace FluentPipe.Builder;

public class PipeBuilder<TEntree, TSortie> : IPipeBuilder<TEntree, TSortie>
{
    protected readonly List<IBlockInfo> _pipes = new();

    private RunOption? _parallelModeOption;

    internal PipeBuilder()
    {
    }

    internal PipeBuilder(IReadOnlyList<IBlockInfo> pipes)
    {
        _pipes = [.. pipes];
    }

    public PipeBuilderDetails<TEntree, TSortie> GetDetails()
    {
        return new PipeBuilderDetails<TEntree, TSortie>(_pipes);
    }

    /// <summary>
    ///     Traitements suivant utilisant un autre builder
    /// </summary>
    public IPipeBuilder<TEntree, TOut> NextPipe<TOut>(IPipeBuilder<TSortie, TOut> pipe)
    {
        var details = pipe.GetDetails();

        _pipes.AddRange(details.Blocks);
        return Convert<TOut>();
    }

    /// <summary>
    ///     Traitements suivant utilisant un autre builder conditionnel
    /// </summary>
    public IPipeBuilder<TEntree, TSortie> NextPipe(IPipeBuilder<TSortie, TSortie> pipe, bool bindingIsEnable)
    {
        if (bindingIsEnable)
        {
            var details = pipe.GetDetails();
            _pipes.AddRange(details.Blocks);
        }

        return Convert<TSortie>();
    }

    private IPipeBuilder<TEntree, TOut> Convert<TOut>()
    {
        return new PipeBuilder<TEntree, TOut>(_pipes);
    }

    #region Dynamic Etape

    public IPipeBuilder<TEntree, TOut> OneIn<TIn, TOut>(bool bindingIsEnable = true)
        where TIn : IBuilderProcessBlock<TSortie, TOut>
    {
        if (bindingIsEnable)
        {
            var etape = new BlockInfo(ServiceRegistryHelper.GetServiceType<TIn>(), null, _parallelModeOption);
            _pipes.Add(new DynamicBlockInfo(etape));
        }

        return Convert<TOut>();
    }

    public IPipeBuilder<TEntree, TOut> OneIn<TIn, TOut, TOption>(TOption option, Func<TOption, bool>? bindingIsEnable = null)
        where TIn : IBuilderProcessBlock<TSortie, TOut, TOption>
        where TOption : IPipeOption
    {
        var isEnabled = bindingIsEnable?.Invoke(option) ?? true;
        if (isEnabled)
        {
            var etape = new BlockInfo(ServiceRegistryHelper.GetServiceType<TIn>(), option, _parallelModeOption);
            _pipes.Add(new DynamicBlockInfo(etape));
        }

        return Convert<TOut>();
    }

    #endregion

    #region Next

    /// <summary>
    ///     Traitement suivant avec une condition de prise en compte depuis la configuration
    /// </summary>
    /// <param name="bindingIsEnable">Condition de prise en compte</param>
    public IPipeBuilder<TEntree, TOut> Next<TBlock, TOut>(bool? bindingIsEnable = null)
        where TBlock : IPipeBlock<TSortie, TOut>
    {
        var isEnabled = bindingIsEnable ?? true;
        if (isEnabled)
            _pipes.Add(new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>(), null, _parallelModeOption));
        return Convert<TOut>();
    }

    /// <summary>
    ///     Traitement suivant avec une option de traitement
    ///     et une condition de prise en compte
    /// </summary>
    /// <param name="option">Option du traitement</param>
    /// <param name="bindingIsEnable">Condition de prise en compte</param>
    public IPipeBuilder<TEntree, TOut> Next<TBlock, TOut, TOption>(
        TOption option,
        Func<TOption, bool>? bindingIsEnable = null)
        where TBlock : IPipeBlock<TSortie, TOut, TOption>
        where TOption : IPipeOption
    {
        var isEnabled = bindingIsEnable?.Invoke(option) ?? true;
        if (isEnabled)
            _pipes.Add(new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>(), option, _parallelModeOption));
        return Convert<TOut>();
    }

    public IPipeParallelBuilder<TEntree, IEnumerable<TPrimitif>, TPrimitif> NextEnumerable<TBlock, TPrimitif>(bool? bindingIsEnable = null)
        where TBlock : IPipeEnumerableBlock<TSortie, TPrimitif>
    {
        var isEnabled = bindingIsEnable ?? true;
        if (isEnabled)
            _pipes.Add(new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>(), null, _parallelModeOption));
        return new PipeParallelBuilder<TEntree, IEnumerable<TPrimitif>, TPrimitif>(_pipes);
    }

    public IPipeParallelBuilder<TEntree, IEnumerable<TPrimitif>, TPrimitif> NextEnumerable<TBlock, TPrimitif, TOption>(TOption option, Func<TOption, bool>? bindingIsEnable = null)
        where TBlock : IPipeEnumerableBlock<TSortie, TPrimitif, TOption>
        where TOption : IPipeOption
    {
        var isEnabled = bindingIsEnable?.Invoke(option) ?? true;
        if (isEnabled)
            _pipes.Add(new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>(), option, _parallelModeOption));
        return new PipeParallelBuilder<TEntree, IEnumerable<TPrimitif>, TPrimitif>(_pipes);
    }

    public IPipeBuilder<TEntree, TSortie> ResetScope()
    {
        _pipes.Add(new ScopeBlockInfo());
        return this;
    }

    #endregion

    public ISwithBuilder<TSortie, TOut, TEntree> SwitchStart<TOut>()
    {
        return new SwitchBuilder<TSortie, TOut, TEntree>(_pipes);
    }

    public IParallelBuilder<TSortie, TOut, TEntree> ParallelStart<TOut>()
    {
        _parallelModeOption = new RunOption(new RunMultiBlockParallelOption());
        return new ParallelBuilder<TSortie, TOut, TEntree>(_pipes);
    }
}