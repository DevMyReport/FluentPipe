#nullable enable
namespace FluentPipe.Runner;

public interface IPipeProgressionManager
{
    /// <summary>
    ///   Notifie la progression (en pourcentage 0–100) d’une étape.
    /// </summary>
    void NotifierProgression(string etapeId, double percentage);

    /// <summary>
    ///   Récupère la progression courante (0–100) pour l’étape donnée.
    ///   Si l’étape n’a jamais été rapportée, retourne 0.
    /// </summary>
    double GetProgressionEtape(string etapeId);
}