using FluentPipe.Builder.Entity;

namespace FluentPipe.Builder.Contracts;

public interface IPipeBuilder<TEntree, TSortie> :
    IPipeNextBuilder<TEntree, TSortie>,
    IPipeDynamicBuilder<TEntree, TSortie>
{
    PipeBuilderDetails<TEntree, TSortie> GetDetails();

    IPipeBuilder<TEntree, TOut> NextPipe<TOut>(IPipeBuilder<TSortie, TOut> pipe);

    IPipeBuilder<TEntree, TSortie> NextPipe(IPipeBuilder<TSortie, TSortie> pipe, bool bindingIsEnable);

    IParallelBuilder<TSortie, TOut, TEntree> ParallelStart<TOut>();

    ISwithBuilder<TSortie, TOut, TEntree> SwitchStart<TOut>();
}