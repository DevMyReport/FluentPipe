using FluentPipe.Blocks.Contracts;
using FluentPipe.Builder;
using FluentPipe.Builder.Contracts;
using FluentPipe.Builder.Entity;
using FluentPipe.Runner;

#nullable enable

namespace FluentPipe.Blocks.OperationBlocks;

public record AddThreeOption(int Increment) : IPipeOption
{
    public IReadOnlyDictionary<string, string> Descrition()
    {
        return new Dictionary<string, string>
        {
            {nameof(Increment), Increment.ToString()}
        };
    }
}

public sealed class AddThreeBlock(IPipeRunner pipeRunner) : PipeBlockBase<int, int, AddThreeOption>
{
    private IPipeBuilder<int, int> CreerPipeline(int increment)
    {
        IPipeBuilder<int, int> pipe = PipeBuilderFactory.Creer<int>();

        for (int i = 0; i < increment; i++)
            pipe = pipe.Next<AddOneBlock, int>();

        return pipe;
    }

    public override async Task<ComputeResult<int>> ProcessAsync(int input, AddThreeOption option, CancellationToken cancellationToken = default)
    {
        var pipe = CreerPipeline(option.Increment);
        var sortie = await pipeRunner.RunAsync(pipe.GetDetails(), input, cancellationToken);

        return ComputeResult.FromResult(sortie);
    }

    public override async Task<IList<ProcessStep>> ExplainAsync(BlockInfo context, CancellationToken ct)
    {
        var pipe = CreerPipeline(((AddThreeOption)context.Option!).Increment);
        var sortie = await pipeRunner.ExplainAsync(pipe.GetDetails(), ct);

        var etapes = (await base.ExplainAsync(context, ct)).ToList();
        etapes.AddRange(sortie.Etapes);
        return etapes;
    }
}