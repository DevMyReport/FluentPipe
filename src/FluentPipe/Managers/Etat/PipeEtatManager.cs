using FluentPipe.Runner;
using Stateless;

namespace FluentPipe.Managers.Etat;

/// <summary>
/// Etapes classique d'un traitement
/// Utilisé pour 
/// </summary>
public enum EnumMachineEtat
{
    EnAttente,
    EnCours,
    Succes,
    Echec,
    Annuled
}

/// <summary>
/// Ceci est le déclencheur utilisé par le code du traitement qui permettra de mettre en place la logique de changement d'etat.
/// A ne pas confondre avec EnumMachineEtat qui lui est l'état actuel d'une étape
/// Dans cette version minimaliste, ils se ressemblent beaucoup, mais en pratique les déclencheurs peuvent être tres different et plus precis
/// </summary>
public enum EnumMachineDeclancheur
{
    Demarrage,
    Succes,
    Erreur,
    Annulation
}

/// <summary>
/// C'est l'implementation metier le plus simple du manager d'etat
/// Il est utilisable en tant que tel
/// Il fonctionne avec EnumMachineEtat, EnumMachineDeclancheur
/// Pour aller plus loin et/ou gérer les changements d'etat ou d'état selon le besoin de votre métier
/// Veuillez implementer votre version de PipeEtatManagerBase avec vos états, vos déclancheurs et votre logique d'enchainement des états
/// </summary>
public class PipeEtatManager : PipeEtatManagerBase<EnumMachineEtat, EnumMachineDeclancheur>
{
    public override EnumMachineEtat AgregerEtapesEtats(IEnumerable<string> etapesIds)
    {
        var etatsEnfants = new List<EnumMachineEtat>();
        foreach (var etapeId in etapesIds)
            etatsEnfants.Add(GetEtapeEtatOrDefault(etapeId, EnumMachineEtat.EnAttente));

        // Règle d'agrégation
        if (etatsEnfants.Contains(EnumMachineEtat.Echec))
        {
            return EnumMachineEtat.Echec;
        }
        else if (etatsEnfants.Contains(EnumMachineEtat.Annuled))
        {
            return EnumMachineEtat.Annuled;
        }
        else if (etatsEnfants.Contains(EnumMachineEtat.EnCours))
        {
            return EnumMachineEtat.EnCours;
        }
        else if (etatsEnfants.TrueForAll(s => s == EnumMachineEtat.Succes) && etatsEnfants.Count > 0)
        {
            return EnumMachineEtat.Succes;
        }
        else
        {
            return EnumMachineEtat.EnCours;
        }
    }

    protected override EnumMachineDeclancheur DeclancheurBlockDemarredParDefaut => EnumMachineDeclancheur.Demarrage;
    
    protected override EnumMachineDeclancheur DeclancheurBlockSuccesParDefaut  => EnumMachineDeclancheur.Succes;
    
    protected override EnumMachineDeclancheur DeclancheurBlockEnEchecParDefaut => EnumMachineDeclancheur.Erreur;
    
    protected override EnumMachineDeclancheur DeclancheurBlockAnnuledParDefaut => EnumMachineDeclancheur.Annulation;

    protected override StateMachine<EnumMachineEtat, EnumMachineDeclancheur> CreerBlockMachineAEtat(string blockId)
    {
        var machine = new StateMachine<EnumMachineEtat, EnumMachineDeclancheur>(EnumMachineEtat.EnAttente);

        machine.Configure(EnumMachineEtat.EnAttente)
            .Permit(EnumMachineDeclancheur.Demarrage, EnumMachineEtat.EnCours);

        machine.Configure(EnumMachineEtat.EnCours)
            .Permit(EnumMachineDeclancheur.Succes, EnumMachineEtat.Succes)
            .Permit(EnumMachineDeclancheur.Erreur, EnumMachineEtat.Echec)
            .Permit(EnumMachineDeclancheur.Annulation, EnumMachineEtat.Annuled);

        machine.Configure(EnumMachineEtat.EnCours)
            .OnEntry(() => Console.WriteLine($"[Block {blockId}] → {EnumMachineEtat.EnCours}"))
            .OnExit(() => Console.WriteLine($"[Block {blockId}] sort de {EnumMachineEtat.EnCours}"));

        machine.Configure(EnumMachineEtat.Succes)
            .OnEntry(() => Console.WriteLine($"[Block {blockId}] → {EnumMachineEtat.Succes}"));

        machine.Configure(EnumMachineEtat.Echec)
            .OnEntry(() => Console.WriteLine($"[Block {blockId}] → {EnumMachineEtat.Echec}"));

        machine.Configure(EnumMachineEtat.Annuled)
            .OnEntry(() => Console.WriteLine($"[Block {blockId}] → {EnumMachineEtat.Annuled}"));

        return machine;
    }

    protected override StateMachine<EnumMachineEtat, EnumMachineDeclancheur> CreerPipeMachineAEtat()
    {
        var machine = new StateMachine<EnumMachineEtat, EnumMachineDeclancheur>(EnumMachineEtat.EnAttente);

        machine.Configure(EnumMachineEtat.EnAttente)
            .Permit(EnumMachineDeclancheur.Demarrage, EnumMachineEtat.EnCours);

        machine.Configure(EnumMachineEtat.EnCours)
            .Permit(EnumMachineDeclancheur.Succes, EnumMachineEtat.Succes)
            .Permit(EnumMachineDeclancheur.Erreur, EnumMachineEtat.Echec)
            .Permit(EnumMachineDeclancheur.Annulation, EnumMachineEtat.Annuled);

        machine.Configure(EnumMachineEtat.EnCours)
            .OnEntry(() => Console.WriteLine($"[Pipe] → {EnumMachineEtat.EnCours}"))
            .OnExit(() => Console.WriteLine($"[Pipe] sort de {EnumMachineEtat.EnCours}"));

        machine.Configure(EnumMachineEtat.Succes)
            .OnEntry(() => Console.WriteLine($"[Pipe] → {EnumMachineEtat.Succes}"));

        machine.Configure(EnumMachineEtat.Echec)
            .OnEntry(() => Console.WriteLine($"[Pipe] → {EnumMachineEtat.Echec}"));

        machine.Configure(EnumMachineEtat.Annuled)
            .OnEntry(() => Console.WriteLine($"[Pipe] → {EnumMachineEtat.Annuled}"));

        return machine;
    }
}