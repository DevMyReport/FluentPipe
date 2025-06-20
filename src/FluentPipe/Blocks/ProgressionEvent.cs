
#nullable enable
using FluentPipe.Runner;

namespace FluentPipe.Blocks;

/// <summary>
/// Événement de progression pour une étape unique.
/// </summary>
public record ProgressionEvent 
{
    /// <summary>
    /// Nom de l'étape qui génère ce progrès (ex : "Upload", "Import", "Persistance", etc.).
    /// Identifient de l'élément qui progresse
    /// </summary>
    public string ElementId { get; set; }

    /// <summary>
    /// Libellé de l'élément qui progresse
    /// </summary>
    public string ElementLibelle { get; set; }

    /// <summary>
    /// Nombre d’unités de travail déjà traitées.
    /// </summary>
    public long Fait { get; set; }

    /// <summary>
    /// Nombre total d’unités de travail à traiter.
    /// </summary>
    public long AFaire { get; set; }

    /// <summary>
    /// Pourcentage calculé automatiquement (0–100).
    /// </summary>
    public int Pourcentage
        => CalculerPourcentage(Fait,AFaire);

    /// <summary>
    /// Horodatage du moment où cet événement a été créé.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public ProgressionEvent(string elementId, string elementLibelle,long fait, long aFaire)
    {
        ElementId = elementId;
        ElementLibelle = elementLibelle;
        Fait = fait;
        AFaire = aFaire;
    }
        
    public static int CalculerPourcentage(long fait, long total)
    {
        return total == 0 ? 0 : (int)((double)fait / total * 100);
    }
}


public record PipeProgressionEvent : ProgressionEvent
{
    /// <summary>
    /// Dernier événement de progression de block
    /// Pour pouvoir avoir un apercu de la progression du pipel et le detail du block en cours (pas compatible blocks en executions parallèles)
    /// </summary>
    public readonly ProgressionEvent? DernierBlockProgressionEvent;

    /// <summary>
    /// Permet d'afficher la progression de l'ensemble du processus, à chaque événement de progression d'un block
    /// </summary>
    /// <param name="dernierBlockProgressionEvent"></param>
    /// <param name="processusEtat"></param>
    /// <param name="faitPipe"></param>
    /// <param name="aFairePipe"></param>
    public PipeProgressionEvent(string pipeId, string PipeLibelle,long faitPipe, long aFairePipe, ProgressionEvent? dernierBlockProgressionEvent = null) :
        base(pipeId, PipeLibelle, faitPipe, aFairePipe)
    {
        DernierBlockProgressionEvent = dernierBlockProgressionEvent;
    }
}

public class ProgressionEventComparerIgnoringTimestamp 
    : IEqualityComparer<ProgressionEvent>
{
    public bool Equals(ProgressionEvent? x, ProgressionEvent? y)
    {
        // mêmes références ou tous les deux null
        if (ReferenceEquals(x, y)) 
            return true;
        // un seul null
        if (x is null || y is null) 
            return false;

        // comparaison des champs de base
        if (x.ElementId     != y.ElementId)     return false;
        if (x.ElementLibelle!= y.ElementLibelle) return false;
        if (x.Fait          != y.Fait)          return false;
        if (x.AFaire         != y.AFaire)         return false;

        // si ce sont des PipeProgressionEvent, comparer aussi LastBlockProgressionEvent
        if (x is PipeProgressionEvent px && y is PipeProgressionEvent py)
        {
            // recursivité
            return Equals(px.DernierBlockProgressionEvent, py.DernierBlockProgressionEvent);
        }

        // sinon, pas de champ supplémentaire => égaux
        return true;
    }

    public int GetHashCode(ProgressionEvent obj)
    {
        if (obj is null) 
            return 0;

        var h = new HashCode();
        // on ajoute uniquement les champs à prendre en compte
        h.Add(obj.ElementId);
        h.Add(obj.ElementLibelle);
        h.Add(obj.Fait);
        h.Add(obj.AFaire);

        // pour PipeProgressionEvent, inclure le hash du LastBlockProgressionEvent
        if (obj is PipeProgressionEvent p && p.DernierBlockProgressionEvent is not null)
        {
            h.Add(GetHashCode(p.DernierBlockProgressionEvent));
        }

        return h.ToHashCode();
    }
}