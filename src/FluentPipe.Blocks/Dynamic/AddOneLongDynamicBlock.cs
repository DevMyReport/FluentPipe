using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Builder;
using FluentPipe.Builder.Contracts;

namespace FluentPipe.Blocks.Dynamic;

public sealed class AddOneLongDynamicBlock : DynamicPipeBlockBase<int, int>
{
    public override IPipeBuilder<int, int> GetBuilder(int input, bool isExplain)
    {
        return PipeBuilderFactory.Creer<int>()
            .Next<AddOneLongBlock, int>();
    }
}