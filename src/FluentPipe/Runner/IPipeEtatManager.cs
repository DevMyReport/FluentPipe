#nullable enable

using System.Collections.Concurrent;
using Stateless;

namespace FluentPipe.Runner;

public enum EnumEtapeEtat
{
    EnAttente,
    Demarred,
    Termined,
    TerminedErreur,
    TerminedAnnuled
}

/// <summary>
/// Ceci est le déclencheur utilisé par le code du traitement qui permettra de mettre en place la logique de changement d'etat.
/// A ne pas confondre avec EnumEtapeEtat qui lui est l'état actuel d'une étape
/// Dans cette version minimaliste, ils se ressemblent beaucoup, mais en pratique les déclencheurs peuvent être tres different et plus precis
/// </summary>
public enum EnumEtapeTrigger
{
    Demarrage,
    Succes,
    Erreur,
    Annulation
}

public interface IPipeEtatManager<TEtat, TTrigger>
{
    /// <summary>
    /// Obtenir la machine d'une étape
    /// </summary>
    StateMachine<TEtat, TTrigger> GetMachineByEtapeId(string etapeId);

    /// <summary>
    /// Obtenir l'état d'une étape
    /// </summary>
    TEtat GetEtapeEtat(string etapeId, TEtat defaultEtapeEtat);
    
    void OnEtapeTrigger(string etapeId, TTrigger trigger);

    TEtat CalculerEtatParent(string parentStepId, IEnumerable<string> childStepIds);


    IDictionary<string, TEtat> GetEtapesEtats();
}

public abstract class PipeEtatManagerBase<TEtat, TTrigger> : IPipeEtatManager<TEtat, TTrigger>
{
    public TTrigger? OnPipeEtapeDemarredTrigger { get; }
    public TTrigger? OnPipeEtapeTerminedTrigger { get; }
    public TTrigger? OnPipeEtapeTerminedAvecErreurTrigger { get; }
    public TTrigger? OnPipeEtapeTerminedAvecWarningTrigger { get; }
    public TTrigger? OnPipeEtapeAnnuledTrigger { get; }

    /// <summary>
    /// L'ensemble des Machines à état du Pipe(runner)
    /// </summary>
    private readonly ConcurrentDictionary<string, StateMachine<TEtat, TTrigger>> _machines = new();

    public StateMachine<TEtat, TTrigger> GetMachineByEtapeId(string etapeId)
    {
        return _machines.GetOrAdd(etapeId, CreerMachineAEtat);
    }

    public TEtat GetEtapeEtat(string etapeId, TEtat defaultEtapeEtat)
    {
        var machine = GetMachineByEtapeId(etapeId);
        
        if (machine == null)
            return defaultEtapeEtat;
        
        return machine.State;
    }

    public TEtat GetEtapeEtatOrDefault(string etapeId, TEtat defaultEtape)
    {
        if (_machines.TryGetValue(etapeId, out var m))
            return m.State;
        throw new KeyNotFoundException($"Aucune machine à état trouvée pour l'étape {etapeId}");
    }
 
    public IDictionary<string, TEtat> GetEtapesEtats()
    {
        var dict = new Dictionary<string, TEtat>();
        foreach (var kv in _machines)
        {
            dict[kv.Key] = kv.Value.State;
        }

        return dict;
    }

    public void OnEtapeTrigger(string etapeId, TTrigger trigger)
    {
        var machine = GetMachineByEtapeId(etapeId);
        if (machine.CanFire(trigger))
            machine.Fire(trigger);
    }

    internal void OnPipeEtapeDemarred(string etapeId)
    {
        OnEtapeTrigger(etapeId, OnPipeEtapeDemarredTrigger);
    }

    internal void OnPipeEtapeTermined(string stepId)
    {
        OnEtapeTrigger(stepId, OnPipeEtapeTerminedTrigger);
    }

    internal void OnPipeEtapeAnnuled(string stepId)
    {
        OnEtapeTrigger(stepId, OnPipeEtapeAnnuledTrigger);
    }

    internal void OnPipeEtapeTerminedAvecErreur(string stepId)
    {
        OnEtapeTrigger(stepId, OnPipeEtapeTerminedAvecErreurTrigger);
    }

    internal void OnPipeEtapeTerminedAvecWarning(string stepId)
    {
        OnEtapeTrigger(stepId, OnPipeEtapeTerminedAvecWarningTrigger);
    }


    public virtual TEtat CalculerEtatParent(string parentStepId, IEnumerable<string> childStepIds)
    {
        throw new NotImplementedException("Vous devez override CalculerEtatParent");
    }

    protected virtual StateMachine<TEtat, TTrigger> CreerMachineAEtat(string stepId)
    {
        throw new NotImplementedException("Vous devez override CreerMachineAEtat");
    }
}

public class PipeEtatManagerStateless : PipeEtatManagerBase<EnumEtapeEtat, EnumEtapeTrigger>
{
    public void OnStepCompleted(string stepId, EnumEtapeEtat status)
    {
        var machine = this.GetMachineByEtapeId(stepId);

        // Si la machine est encore en Pending, on force d'abord un Start
        if (machine.State == EnumEtapeEtat.EnAttente && machine.CanFire(EnumEtapeTrigger.Demarrage))
        {
            machine.Fire(EnumEtapeTrigger.Demarrage);
        }

        switch (status)
        {
            case EnumEtapeEtat.Termined:
                if (machine.CanFire(EnumEtapeTrigger.Succes))
                    machine.Fire(EnumEtapeTrigger.Succes);
                break;
            case EnumEtapeEtat.TerminedErreur:
                if (machine.CanFire(EnumEtapeTrigger.Erreur))
                    machine.Fire(EnumEtapeTrigger.Erreur);
                break;
            case EnumEtapeEtat.TerminedAnnuled:
                if (machine.CanFire(EnumEtapeTrigger.Annulation))
                    machine.Fire(EnumEtapeTrigger.Annulation);
                break;
            default:
                // Running ou Pending, on ne fait rien
                break;
        }
    }

    public void AggregateParentState(string parentEtapeId, IEnumerable<string> enfantEtapeId)
    {
        var parentMachine = GetMachineByEtapeId(parentEtapeId);

        var childStates = new List<EnumEtapeEtat>();
        foreach (var etapeId in enfantEtapeId)
            childStates.Add(GetEtapeEtatOrDefault(etapeId, EnumEtapeEtat.EnAttente));

        // Règle d'agrégation
        if (childStates.Contains(EnumEtapeEtat.TerminedErreur))
        {
            if (parentMachine.CanFire(EnumEtapeTrigger.Erreur))
                parentMachine.Fire(EnumEtapeTrigger.Erreur);
        }
        else if (childStates.Contains(EnumEtapeEtat.Demarred))
        {
            // On veut que le parent soit au moins en Running si l'un des enfants tourne
            if (parentMachine.CanFire(EnumEtapeTrigger.Demarrage))
                parentMachine.Fire(EnumEtapeTrigger.Demarrage);
        }
        else if (childStates.TrueForAll(s => s == EnumEtapeEtat.Termined) && childStates.Count > 0)
        {
            if (parentMachine.CanFire(EnumEtapeTrigger.Succes))
                parentMachine.Fire(EnumEtapeTrigger.Succes);
        }
        else if (childStates.TrueForAll(s => s == EnumEtapeEtat.TerminedAnnuled) && childStates.Count > 0)
        {
            if (parentMachine.CanFire(EnumEtapeTrigger.Annulation))
                parentMachine.Fire(EnumEtapeTrigger.Annulation);
        }
        else
        {
            // S’il reste des enfants en Pending, on laisse le parent en Pending
            // ou en Running s’il a déjà commencé (on ne force rien).
            if (parentMachine.State == EnumEtapeEtat.EnAttente && parentMachine.CanFire(EnumEtapeTrigger.Demarrage))
                parentMachine.Fire(EnumEtapeTrigger.Demarrage);
        }
    }

    protected override StateMachine<EnumEtapeEtat, EnumEtapeTrigger> CreerMachineAEtat(string stepId)
    {
        var machine = new StateMachine<EnumEtapeEtat, EnumEtapeTrigger>(EnumEtapeEtat.EnAttente);

        machine.Configure(EnumEtapeEtat.EnAttente)
            .Permit(EnumEtapeTrigger.Demarrage, EnumEtapeEtat.Demarred);

        machine.Configure(EnumEtapeEtat.Demarred)
            .Permit(EnumEtapeTrigger.Succes, EnumEtapeEtat.Termined)
            .Permit(EnumEtapeTrigger.Erreur, EnumEtapeEtat.TerminedErreur)
            .Permit(EnumEtapeTrigger.Annulation, EnumEtapeEtat.TerminedAnnuled);

        machine.Configure(EnumEtapeEtat.Demarred)
            .OnEntry(() => Console.WriteLine($"[Step {stepId}] → {EnumEtapeEtat.Demarred}"))
            .OnExit(() => Console.WriteLine($"[Step {stepId}] sort de {EnumEtapeEtat.Demarred}"));

        machine.Configure(EnumEtapeEtat.Termined)
            .OnEntry(() => Console.WriteLine($"[Step {stepId}] → {EnumEtapeEtat.Termined}"));

        machine.Configure(EnumEtapeEtat.TerminedErreur)
            .OnEntry(() => Console.WriteLine($"[Step {stepId}] → {EnumEtapeEtat.TerminedErreur}"));

        machine.Configure(EnumEtapeEtat.TerminedAnnuled)
            .OnEntry(() => Console.WriteLine($"[Step {stepId}] → {EnumEtapeEtat.TerminedAnnuled}"));

        return machine;
    }
}