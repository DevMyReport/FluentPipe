using FluentPipe.Runner;
using Stateless;

namespace FluentPipe.Managers.Etat;

/// <summary>
/// Etapes classique d'un traitement
/// Utilisé pour 
/// </summary>
public enum EnumEtapeEtat
{
    EnAttente,
    EnCours,
    Succes,
    Echec,
    Annuled
}

/// <summary>
/// Ceci est le déclencheur utilisé par le code du traitement qui permettra de mettre en place la logique de changement d'etat.
/// A ne pas confondre avec EnumEtapeEtat qui lui est l'état actuel d'une étape
/// Dans cette version minimaliste, ils se ressemblent beaucoup, mais en pratique les déclencheurs peuvent être tres different et plus precis
/// </summary>
public enum EnumEtapeDeclancheur
{
    Demarrage,
    Succes,
    Erreur,
    Annulation
}

/// <summary>
/// C'est l'implementation metier le plus simple du manager d'etat
/// Il est utilisable en tant que tel
/// Il fonctionne avec EnumEtapeEtat, EnumEtapeDeclancheur
/// Pour aller plus loin et/ou gérer les changements d'etat ou d'état selon le besoin de votre métier
/// Veuillez implementer votre version de PipeEtatManagerBase avec vos états, vos déclancheurs et votre logique d'enchainement des états
/// </summary>
public class PipeEtatManager : PipeEtatManagerBase<EnumEtapeEtat, EnumEtapeDeclancheur>
{
    public override EnumEtapeEtat AgregerEtapesEtats(IEnumerable<string> etapesIds)
    {
        var etatsEnfants = new List<EnumEtapeEtat>();
        foreach (var etapeId in etapesIds)
            etatsEnfants.Add(GetEtapeEtatOrDefault(etapeId, EnumEtapeEtat.EnAttente));

        // Règle d'agrégation
        if (etatsEnfants.Contains(EnumEtapeEtat.Echec))
        {
            return EnumEtapeEtat.Echec;
        }
        else if (etatsEnfants.Contains(EnumEtapeEtat.Annuled))
        {
            return EnumEtapeEtat.Annuled;
        }
        else if (etatsEnfants.Contains(EnumEtapeEtat.EnCours))
        {
            return EnumEtapeEtat.EnCours;
        }
        else if (etatsEnfants.TrueForAll(s => s == EnumEtapeEtat.Succes) && etatsEnfants.Count > 0)
        {
            return EnumEtapeEtat.Succes;
        }
        else
        {
            return EnumEtapeEtat.EnCours;
        }
    }

    protected override EnumEtapeDeclancheur DeclancheurEtapeDemarredParDefaut => EnumEtapeDeclancheur.Demarrage;
    
    protected override EnumEtapeDeclancheur DeclancheurEtapeSuccesParDefaut  => EnumEtapeDeclancheur.Succes;
    
    protected override EnumEtapeDeclancheur DeclancheurEtapeEnEchecParDefaut => EnumEtapeDeclancheur.Erreur;
    
    protected override EnumEtapeDeclancheur DeclancheurEtapeAnnuledParDefaut => EnumEtapeDeclancheur.Annulation;

    protected override StateMachine<EnumEtapeEtat, EnumEtapeDeclancheur> CreerMachineAEtat(string etapeId)
    {
        var machine = new StateMachine<EnumEtapeEtat, EnumEtapeDeclancheur>(EnumEtapeEtat.EnAttente);

        machine.Configure(EnumEtapeEtat.EnAttente)
            .Permit(EnumEtapeDeclancheur.Demarrage, EnumEtapeEtat.EnCours);

        machine.Configure(EnumEtapeEtat.EnCours)
            .Permit(EnumEtapeDeclancheur.Succes, EnumEtapeEtat.Succes)
            .Permit(EnumEtapeDeclancheur.Erreur, EnumEtapeEtat.Echec)
            .Permit(EnumEtapeDeclancheur.Annulation, EnumEtapeEtat.Annuled);

        machine.Configure(EnumEtapeEtat.EnCours)
            .OnEntry(() => Console.WriteLine($"[Step {etapeId}] → {EnumEtapeEtat.EnCours}"))
            .OnExit(() => Console.WriteLine($"[Step {etapeId}] sort de {EnumEtapeEtat.EnCours}"));

        machine.Configure(EnumEtapeEtat.Succes)
            .OnEntry(() => Console.WriteLine($"[Step {etapeId}] → {EnumEtapeEtat.Succes}"));

        machine.Configure(EnumEtapeEtat.Echec)
            .OnEntry(() => Console.WriteLine($"[Step {etapeId}] → {EnumEtapeEtat.Echec}"));

        machine.Configure(EnumEtapeEtat.Annuled)
            .OnEntry(() => Console.WriteLine($"[Step {etapeId}] → {EnumEtapeEtat.Annuled}"));

        return machine;
    }
}