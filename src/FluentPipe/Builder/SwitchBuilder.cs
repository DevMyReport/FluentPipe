using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Contracts;
using FluentPipe.Builder.Entity;
using FluentPipe.InjectionHelper;

namespace FluentPipe.Builder;

public sealed class SwitchBuilder<TEntree, TSortie, TInput>(List<IBlockInfo> builderEtapes) : ISwithBuilder<TEntree, TSortie, TInput>
{
    private List<IBlockInfo> _pipes = new();

    public ISwithBuilder<TEntree, TSortie, TInput> Case<TBlock>(bool bindingIsEnable)
        where TBlock : IPipeBlock<TEntree, TSortie>
    {
        if (bindingIsEnable && _pipes.Count == 0)
            _pipes.Add(new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>()));

        return this;
    }

    public ISwithBuilder<TEntree, TSortie, TInput> Case<TBlock, TOption>(TOption option, Func<TOption, bool> bindingIsEnable)
        where TBlock : IPipeBlock<TEntree, TSortie, TOption>
        where TOption : IPipeOption
    {
        var isEnabled = bindingIsEnable(option);
        if (isEnabled && _pipes.Count == 0)
            _pipes.Add(new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>(), option));

        return this;
    }

    public ISwithBuilder<TEntree, TSortie, TInput> CasePipe(IPipeBuilder<TEntree, TSortie> pipe, bool bindingIsEnable)
    {
        if (bindingIsEnable && _pipes.Count == 0)
        {
            var etapes = pipe.GetDetails();
            _pipes.AddRange(etapes.Blocks);
        }

        return this;
    }

    public IPipeBuilder<TInput, TSortie> SwithlEnd()
    {
        if (!_pipes.Any())
            throw new InvalidOperationException("Aucune étape valide dans le switch");

        builderEtapes.AddRange(_pipes);
        return new PipeBuilder<TInput, TSortie>(builderEtapes);
    }
}