using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder.Contracts;
using FluentPipe.Builder.Entity;
using FluentPipe.InjectionHelper;

namespace FluentPipe.Builder;

public sealed class PipeParallelBuilder<TEntree, TListeSortie, TElementSortie> : PipeBuilder<TEntree, TListeSortie>, IPipeParallelBuilder<TEntree, TListeSortie, TElementSortie>
    where TListeSortie : IEnumerable<TElementSortie>
{
    public PipeParallelBuilder()
    { }

    public PipeParallelBuilder(IReadOnlyList<IBlockInfo> pipes)
        : base(pipes)
    { }

    private IPipeParallelBuilder<TEntree, IEnumerable<TOut>, TOut> Convert<TOut>()
    {
        return new PipeParallelBuilder<TEntree, IEnumerable<TOut>, TOut>(_pipes);
    }

    /// <summary>
    ///     Traitement suivant en parallele, chaque valeur de la liste d'entree est passé au connecteur en tant que type base TIn
    /// </summary>
    /// <typeparam name="TBlock">Type de base du connecteur</typeparam>
    /// <typeparam name="TOut">Sortie du runner en tant IEnumerable</typeparam>
    /// <returns></returns>
    public IPipeParallelBuilder<TEntree, IEnumerable<TOut>, TOut> NextMonoParallel<TBlock, TOut>(int maxParallelism)
        where TBlock : IPipeBlock<TElementSortie, TOut>
    {
        var option = new RunOption(new RunMonoBlockParallelOption(maxParallelism));
        _pipes.Add(new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>(), RunOpt: option));

        return Convert<TOut>();
    }

    public IPipeParallelBuilder<TEntree, IEnumerable<TOut>, TOut> NextMonoParallel<TBlock, TOut, TOption>(TOption option, int maxParallelism)
        where TBlock : IPipeBlock<TElementSortie, TOut, TOption>
        where TOption : IPipeOption
    {
        var runOption = new RunOption(new RunMonoBlockParallelOption(maxParallelism));
        _pipes.Add(new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>(), option, RunOpt: runOption));

        return Convert<TOut>();
    }

    public IPipeParallelBuilder<TEntree, IEnumerable<TOut>, TOut> NextPipeMonoParallel<TOut>(IPipeBuilder<TElementSortie, TOut> pipe, int maxParallelism)
    {
        var runOption = new RunOption(new RunMonoBlockParallelOption(maxParallelism));
        var details = pipe.GetDetails();

        _pipes.AddRange(details.Blocks.Select(ie =>
            ie switch
            {
                BlockInfo blockInfo => new BlockInfo(blockInfo.TypeDuBlock, blockInfo.Option, runOption), // Permet d'intégrer le RunOption
                _ => ie
            }
        ));

        return Convert<TOut>();
    }

    public IPipeParallelBuilder<TEntree, IEnumerable<TElementSortie>, TElementSortie> NextPipeMonoParallel(IPipeBuilder<TElementSortie, TElementSortie> pipe, int maxParallelism, bool bindingIsEnable)
    {
        if (bindingIsEnable)
        {
            var runOption = new RunOption(new RunMonoBlockParallelOption(maxParallelism));
            var details = pipe.GetDetails();
            _pipes.AddRange(details.Blocks.Select(ie =>
                ie switch
                {
                    BlockInfo blockInfo => new BlockInfo(blockInfo.TypeDuBlock, blockInfo.Option, runOption), // Permet d'intégrer le RunOption
                    _ => ie
                }
            ));
        }

        return Convert<TElementSortie>();
    }

    /// <summary>
    ///     Traitement suivant en parallele, chaque valeur de la liste d'entree est passé au connecteur en tant que type base TIn
    /// </summary>
    /// <typeparam name="TBlock">Type de base du connecteur</typeparam>
    /// <typeparam name="TIn">Type d'entrée du bloc</typeparam>
    /// <typeparam name="TOut">Sortie du runner en tant IEnumerable</typeparam>
    /// <returns></returns>
    public IPipeParallelBuilder<TEntree, IEnumerable<TOut>, TOut> NextDynamicParallel<TBlock, TOut>(int maxParallelism)
        where TBlock : IBuilderProcessBlock<TElementSortie, TOut>
    {
        var option = new RunOption(new RunMonoBlockParallelOption(maxParallelism));
        var blockInfo = new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>(), RunOpt: option);
        _pipes.Add(new DynamicBlockInfo(blockInfo));
        return Convert<TOut>();
    }

    public IPipeParallelBuilder<TEntree, IEnumerable<TOut>, TOut> NextDynamicParallel<TBlock, TOut, TOption>(TOption option, int maxParallelism, bool useScope = false)
        where TBlock : IBuilderProcessBlock<TElementSortie, TOut, TOption>
        where TOption : IPipeOption
    {
        var runOption = new RunOption(new RunMonoBlockParallelOption(maxParallelism))
        {
            UseSubscope = useScope
        };
        var blockInfo = new BlockInfo(ServiceRegistryHelper.GetServiceType<TBlock>(), option, RunOpt: runOption);
        _pipes.Add(new DynamicBlockInfo(blockInfo));
        return Convert<TOut>();
    }
}