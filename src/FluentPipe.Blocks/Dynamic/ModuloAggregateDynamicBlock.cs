using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Builder;
using FluentPipe.Builder.Contracts;

namespace FluentPipe.Blocks.Dynamic;

public sealed class ModuloAggregateDynamicBlock : DynamicPipeBlockBase<IEnumerable<int>, string>
{
    public override IPipeBuilder<IEnumerable<int>, string> GetBuilder(IEnumerable<int> input, bool isExplain)
    {
        if (isExplain || input.Sum() % 2 == 0)
            return PipeBuilderFactory.CreerEnumerable<int>()
                .Next<SommeBlock, int>()
                .Next<AddOneBlock, int>()
                .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption());

        return PipeBuilderFactory.CreerEnumerable<int>()
            .Next<SommeBlock, int>()
            .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption());
    }
}