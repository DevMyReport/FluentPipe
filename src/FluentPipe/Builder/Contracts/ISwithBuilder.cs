using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Builder.Contracts;

public interface ISwithBuilder<TEntree, TSortie, TInput>
{
    ISwithBuilder<TEntree, TSortie, TInput> Case<TBlock>(
        bool bindingIsEnable)
        where TBlock : IPipeBlock<TEntree, TSortie>;

    ISwithBuilder<TEntree, TSortie, TInput> Case<TBlock, TOption>(
        TOption option,
        Func<TOption, bool> bindingIsEnable)
        where TBlock : IPipeBlock<TEntree, TSortie, TOption>
        where TOption : IPipeOption;

    ISwithBuilder<TEntree, TSortie, TInput> CasePipe(
        IPipeBuilder<TEntree, TSortie> pipe,
        bool bindingIsEnable);

    IPipeBuilder<TInput, TSortie> SwithlEnd();
}