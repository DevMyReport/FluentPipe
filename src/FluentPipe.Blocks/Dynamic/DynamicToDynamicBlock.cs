using FluentPipe.Builder;
using FluentPipe.Builder.Contracts;

namespace FluentPipe.Blocks.Dynamic;

public sealed class DynamicToDynamicBlock : DynamicPipeBlockBase<int, string>
{
    public override IPipeBuilder<int, string> GetBuilder(int input, bool isExplain)
    {
        return PipeBuilderFactory.Creer<int>()
            .OneIn<ModuloDynamicBlock, string>();
    }
}