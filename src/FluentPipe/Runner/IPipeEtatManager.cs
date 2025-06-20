#nullable enable

using System.Collections.Concurrent;
using Stateless;

namespace FluentPipe.Runner;

public interface IPipeEtatManager<TEtat, TDeclancheur>
{
    void AjouterMachineAEtat(string blockId);
    /// <summary>
    /// Obtenir l'état d'une étape
    /// </summary>
    TEtat GetEtapeEtat(string etapeId);
    
    bool Declancher(string etapeId, TDeclancheur declancheur);

    void DeclancherEtapeDemarred(string etapeId);
    
    void DeclancherEtapeTermined(string etapeId);
    
    void DeclancherEtapeAnnuled(string etapeId);
    
    void DeclancherEtapeEnEchec(string etapeId);
}

public abstract class PipeEtatManagerBase<TEtat, TDeclancheur> : IPipeEtatManager<TEtat, TDeclancheur>
{
    protected abstract TDeclancheur? DeclancheurEtapeDemarredParDefaut { get; }
    protected abstract TDeclancheur? DeclancheurEtapeSuccesParDefaut { get; }
    protected abstract TDeclancheur? DeclancheurEtapeEnEchecParDefaut { get; }
    protected abstract TDeclancheur? DeclancheurEtapeAnnuledParDefaut { get; }

    /// <summary>
    /// L'ensemble des Machines à état du Pipe(runner)
    /// </summary>
    private readonly ConcurrentDictionary<string, StateMachine<TEtat, TDeclancheur>> _machines = new();
    
    public void AjouterMachineAEtat(string blockId)
    {
        var machine = CreerMachineAEtat(blockId);
        if (!_machines.TryAdd(blockId, machine))
            throw new InvalidOperationException($"Machine '{blockId}' is existe déjà");
    }
    
    public StateMachine<TEtat, TDeclancheur> GetMachineByEtapeId(string etapeId)
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
    
    public bool Declancher(string etapeId, TDeclancheur declancheur)
    {
        var machine = GetMachineByEtapeId(etapeId);
        
        if (!machine.CanFire(declancheur)) 
            return false;
        
        machine.Fire(declancheur);
        return true;
    }

    public void DeclancherEtapeDemarred(string etapeId)
    {
        Declancher(etapeId, DeclancheurEtapeDemarredParDefaut);
    }

    public void DeclancherEtapeTermined(string etapeId)
    {
        Declancher(etapeId, DeclancheurEtapeSuccesParDefaut);
    }

    public void DeclancherEtapeAnnuled(string etapeId)
    {
        Declancher(etapeId, DeclancheurEtapeAnnuledParDefaut);
    }

    public void DeclancherEtapeEnEchec(string etapeId)
    {
        Declancher(etapeId, DeclancheurEtapeEnEchecParDefaut);
    }

    protected abstract StateMachine<TEtat, TDeclancheur> CreerMachineAEtat(string etapeId);
}