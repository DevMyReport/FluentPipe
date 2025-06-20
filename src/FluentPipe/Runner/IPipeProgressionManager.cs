#nullable enable
using FluentPipe.Blocks;
using FluentPipe.Builder.Entity;

namespace FluentPipe.Runner;

public interface IPipeProgressionManager
{
    public event EventHandler<ProgressionEvent>? BlockProgressionChanged;

    void OnProgressionBlock(object? sender, ProgressionEvent e);
    
    public event EventHandler<PipeProgressionEvent>? PipeProgressionChanged;

    void NotifierProgressionPipe(long fait, long total, ProgressionEvent? lastBlockProgressionEvent = null);
    
    void NotifierBlockTraited(BlockInfo blockInfo);
    
}