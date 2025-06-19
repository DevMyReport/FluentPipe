using System.Collections.Concurrent;
using FluentPipe.Blocks;
using FluentPipe.Builder.Entity;

namespace FluentPipe.Runner;

public class PipeProgressionManager : IPipeProgressionManager
{
    
    // 3) Pour la progression, on conserve un pourcentage par étape
    private readonly ConcurrentDictionary<string, ProgressionEvent> _ProgressionParBlock
        = new ConcurrentDictionary<string, ProgressionEvent>();

    public event EventHandler<ProgressionEvent>? BlockProgressionChanged;

    public void OnProgressionBlock(object? sender, ProgressionEvent e)
    {
        _ProgressionParBlock[e.ElementId] = e;
        //...
        var progressionEvent = new PipeProgressionEvent(ElementId,ElementLibelle,Fait, Total, e);
        BlockProgressionChanged?.Invoke(this, progressionEvent);
    }

    public event EventHandler<ProgressionEvent>? PipeProgressionChanged;

    public void NotifierProgressionPipe(long fait, long total, ProgressionEvent? lastBlockProgressionEvent = null)
    {
        Fait = fait;
        Total = total;
        var progressionEvent = new PipeProgressionEvent(ElementId,ElementLibelle,Fait, Total, lastBlockProgressionEvent);
        BlockProgressionChanged?.Invoke(this, progressionEvent);
    }

    public void NotifierProgressionPipe(ProgressionEvent e)
    {
        throw new NotImplementedException();
    }

    public void NotifierBlockTraited(BlockInfo blockInfo)
    {
        Fait++;
    }

    public string ElementId { get; set; }
    public string ElementLibelle { get;  set; }
    public long Fait { get; set; }
    public long Total { get; set; }
}
