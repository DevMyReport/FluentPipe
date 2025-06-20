
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
    public long Total { get; set; }

    /// <summary>
    /// Pourcentage calculé automatiquement (0–100).
    /// </summary>
    public int Pourcentage
        => CalculerPourcentage(Fait,Total);

    /// <summary>
    /// Horodatage du moment où cet événement a été créé.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public ProgressionEvent(string elementId, string elementLibelle,long fait, long total)
    {
        ElementId = elementId;
        ElementLibelle = elementLibelle;
        Fait = fait;
        Total = total;
    }
        
    public static int CalculerPourcentage(long fait, long total)
    {
        return total == 0 ? 0 : (int)((double)fait / total * 100);
    }
}


public record PipeProgressionEvent : ProgressionEvent
{
    public readonly ProgressionEvent? LastBlockProgressionEvent;

    /// <summary>
    /// Permet d'afficher la progression de l'ensemble du processus, a chaque evenement de progression d'un block
    /// </summary>
    /// <param name="lastBlockProgressionEvent"></param>
    /// <param name="processusEtat"></param>
    /// <param name="faitPipe"></param>
    /// <param name="aFairePipe"></param>
    public PipeProgressionEvent(string pipeId, string PipeLibelle,long faitPipe, long aFairePipe, ProgressionEvent? lastBlockProgressionEvent = null) :
        base(pipeId, PipeLibelle, faitPipe, aFairePipe)
    {
        LastBlockProgressionEvent = lastBlockProgressionEvent;
    }
}