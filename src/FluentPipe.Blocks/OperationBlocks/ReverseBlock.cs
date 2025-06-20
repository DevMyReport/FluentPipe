using FluentPipe.Blocks.Contracts;

namespace FluentPipe.Blocks.OperationBlocks;

public sealed class ReverseBlock : PipeBlockBase<string, string>
{
    public override Task<ComputeResult<string>> ProcessAsync(string input, CancellationToken cancellationToken = default)
    {
        string retour = "";
        for (int i = input.Length -1 ; i >= 0; i--)
        {
            retour += input[i];
            NotifierProgression(retour.Length,input.Length);
        }
        
        return ComputeResult.Success(retour).ToTask();
    }
}