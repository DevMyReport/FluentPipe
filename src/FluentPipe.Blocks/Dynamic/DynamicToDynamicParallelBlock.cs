using FluentPipe.Blocks.Readers;
using FluentPipe.Builder;
using FluentPipe.Builder.Contracts;

namespace FluentPipe.Blocks.Dynamic;

public sealed class DynamicToDynamicParallelBlock : DynamicPipeBlockBase<IEnumerable<int>, IEnumerable<string>>
{
    public override IPipeBuilder<IEnumerable<int>, IEnumerable<string>> GetBuilder(IEnumerable<int> input, bool isExplain)
    {
        return PipeBuilderFactory.CreerEnumerable<int>()
            .NextMonoParallel<ReadBlock, IEnumerable<int>>(3)
            .NextDynamicParallel<ModuloAggregateDynamicBlock, string>(3);
    }
}