using System.Collections.Concurrent;

namespace FluentPipe.Runner;

public class PipeProgressionManager : IPipeProgressionManager
{
    // 3) Pour la progression, on conserve un pourcentage par étape
    private readonly ConcurrentDictionary<string, double> _ProgressionParEtape
        = new ConcurrentDictionary<string, double>();

    public void NotifierProgression(string etapeId, double percentage)
    {
        // On se force à plafonner entre 0 et 100
        var pct = Math.Max(0, Math.Min(100, percentage));
        _ProgressionParEtape.AddOrUpdate(etapeId, pct, (_, __) => pct);
    }

    public double GetProgressionEtape(string etapeId)
    {
        return _ProgressionParEtape.TryGetValue(etapeId, out var pct)
            ? pct
            : 0.0;
    }

    public IDictionary<string, double> GetAllProgress()
    {
        throw new NotImplementedException();
    }
}