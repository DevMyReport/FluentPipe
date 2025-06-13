#nullable enable

using System.Collections.Concurrent;
using Stateless;

namespace FluentPipe.Runner;

public interface IPipeEtatManager<TEtat, TTrigger>
{
    /// <summary>
    /// Obtenir l'état d'une étape
    /// </summary>
    TEtat GetEtapeEtat(string etapeId);
    
    bool Declancher(string etapeId, TTrigger trigger);

    void DeclancherEtapeDemarred(string etapeId);
    void DeclancherEtapeTermined(string stepId);
    void DeclancherEtapeAnnuled(string stepId);
    void DeclancherEtapeEnEchec(string stepId);
}

public abstract class PipeEtatManagerBase<TEtat, TTrigger> : IPipeEtatManager<TEtat, TTrigger>
{
    protected abstract TTrigger? DeclancheurEtapeDemarredParDefaut { get; }
    protected abstract TTrigger? DeclancheurEtapeSuccesParDefaut { get; }
    protected abstract TTrigger? DeclancheurEtapeEnEchecParDefaut { get; }
    protected abstract TTrigger? DeclancheurEtapeAnnuledParDefaut { get; }

    /// <summary>
    /// L'ensemble des Machines à état du Pipe(runner)
    /// </summary>
    private readonly ConcurrentDictionary<string, StateMachine<TEtat, TTrigger>> _machines = new();
    
    public void AjouterMachineAEtat(string etapeId)
    {
        var machine = CreerMachineAEtat(etapeId);
        if (!_machines.TryAdd(etapeId, machine))
            throw new InvalidOperationException($"Machine '{etapeId}' is existe déjà");
    }
    
    public StateMachine<TEtat, TTrigger> GetMachineByEtapeId(string etapeId)
    {
        return _machines[etapeId];
    }

    public TEtat GetEtapeEtat(string etapeId)
    {
        var machine = GetMachineByEtapeId(etapeId);
        return machine.State;
    }
    
    public abstract TEtat AgregerEtapesEtats(IEnumerable<string> etapesIds);

    public TEtat GetEtapeEtatOrDefault(string etapeId, TEtat defaultEtape)
    {
        if (_machines.TryGetValue(etapeId, out var m))
            return m.State;
        throw new KeyNotFoundException($"Aucune machine à état trouvée pour l'étape {etapeId}");
    }
    
    public bool Declancher(string etapeId, TTrigger trigger)
    {
        var machine = GetMachineByEtapeId(etapeId);
        
        if (!machine.CanFire(trigger)) 
            return false;
        
        machine.Fire(trigger);
        return true;
    }

    public void DeclancherEtapeDemarred(string etapeId)
    {
        Declancher(etapeId, DeclancheurEtapeDemarredParDefaut);
    }

    public void DeclancherEtapeTermined(string stepId)
    {
        Declancher(stepId, DeclancheurEtapeSuccesParDefaut);
    }

    public void DeclancherEtapeAnnuled(string stepId)
    {
        Declancher(stepId, DeclancheurEtapeAnnuledParDefaut);
    }

    public void DeclancherEtapeEnEchec(string stepId)
    {
        Declancher(stepId, DeclancheurEtapeEnEchecParDefaut);
    }

    protected abstract StateMachine<TEtat, TTrigger> CreerMachineAEtat(string etapeId);
}