using System.Collections.Concurrent;
using FluentPipe.Blocks;
using FluentPipe.Builder.Entity;

namespace FluentPipe.Runner;

public class PipeProgressionManager : IPipeProgressionManager
{
    // 3) Pour la progression, on conserve un pourcentage par étape
    private readonly ConcurrentDictionary<string, ProgressionEvent> _ProgressionParBlock = new();

    public long Fait { get; private set; }
    public long Total { get; private set; }

    public event EventHandler<ProgressionEvent>? BlockProgressionChanged;
    
    public PipeProgressionEvent DernierProgressionEvent { get; private set; }

    public void OnProgressionBlock(object? sender, ProgressionEvent e)
    {
        _ProgressionParBlock[e.ElementId] = e;
        BlockProgressionChanged?.Invoke(this, e);
        
        var progressionEvent = new PipeProgressionEvent("", "", Fait, Total, e);
        PipeProgressionChanged?.Invoke(this, progressionEvent);
    }

    public event EventHandler<PipeProgressionEvent>? PipeProgressionChanged;

    private void NotifierProgressionPipe(ProgressionEvent? lastBlockProgressionEvent = null)
    {
        var progressionEvent = new PipeProgressionEvent("", "", Fait, Total, lastBlockProgressionEvent);
        // Si rien n'a changé inutile d'envoyer des doublon d'event
        var comparer = new ProgressionEventComparerIgnoringTimestamp();
        if (comparer.Equals(DernierProgressionEvent, progressionEvent))
            return; 
        PipeProgressionChanged?.Invoke(this, progressionEvent);
        DernierProgressionEvent = progressionEvent;
    }
 
    
    public void NotifierProgressionPipe(long fait, long total, ProgressionEvent? lastBlockProgressionEvent = null)
    {
        Fait = fait;
        Total = total;
        NotifierProgressionPipe(lastBlockProgressionEvent);
    }

    public void NotifierBlockTraited(BlockInfo blockInfo)
    {
        Fait++;
        NotifierProgressionPipe();
    }
}