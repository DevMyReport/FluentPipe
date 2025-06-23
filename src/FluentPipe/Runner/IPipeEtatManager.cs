#nullable enable

using System.Collections.Concurrent;
using System.ComponentModel.Design;
using Stateless;

namespace FluentPipe.Runner;

public interface IPipeEtatManager<TEtat, TDeclancheur>
{
    void AjouterMachineAEtat(string blockId);
    
    void InitialiserMachineAEtat(string blockId);
    /// <summary>
    /// Obtenir l'état d'une étape
    /// </summary>
    TEtat GetBlockEtat(string blockId);
    
    bool Declancher(string blockId, TDeclancheur declancheur);

    void DeclancherBlockDemarred(string blockId);
    
    void DeclancherBlockTermined(string blockId);
    
    void DeclancherBlockAnnuled(string blockId);
    
    void DeclancherBlockEnEchec(string blockId);
}

public abstract class PipeEtatManagerBase<TEtat, TDeclancheur> : IPipeEtatManager<TEtat, TDeclancheur>
{
    protected abstract TDeclancheur? DeclancheurBlockDemarredParDefaut { get; }
    protected abstract TDeclancheur? DeclancheurBlockSuccesParDefaut { get; }
    protected abstract TDeclancheur? DeclancheurBlockEnEchecParDefaut { get; }
    protected abstract TDeclancheur? DeclancheurBlockAnnuledParDefaut { get; }

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

    /// <summary>
    /// Créer ou recréer la machine a état de l'étape
    /// </summary>
    /// <param name="blockId"></param>
    public void InitialiserMachineAEtat(string blockId)
    {
        if (_machines.ContainsKey(blockId))
            if(!_machines.TryRemove(blockId, out _))
                throw new InvalidOperationException($"Machine '{blockId}' n'a pu être réinitialisée");
        
        AjouterMachineAEtat(blockId);
    }

    public StateMachine<TEtat, TDeclancheur> GetMachineByEtapeId(string etapeId)
    {
        return _machines[etapeId];
    }

    public TEtat GetBlockEtat(string blockId)
    {
        var machine = GetMachineByEtapeId(blockId);
        return machine.State;
    }
    
    public abstract TEtat AgregerEtapesEtats(IEnumerable<string> etapesIds);

    public TEtat GetEtapeEtatOrDefault(string etapeId, TEtat defaultEtape)
    {
        if (_machines.TryGetValue(etapeId, out var m))
            return m.State;
        throw new KeyNotFoundException($"Aucune machine à état trouvée pour l'étape {etapeId}");
    }
    
    public bool Declancher(string blockId, TDeclancheur declancheur)
    {
        var machine = GetMachineByEtapeId(blockId);
        
        if (!machine.CanFire(declancheur)) 
            return false;
        
        machine.Fire(declancheur);
        return true;
    }

    public void DeclancherBlockDemarred(string blockId)
    {
        Declancher(blockId, DeclancheurBlockDemarredParDefaut);
    }

    public void DeclancherBlockTermined(string blockId)
    {
        Declancher(blockId, DeclancheurBlockSuccesParDefaut);
    }

    public void DeclancherBlockAnnuled(string blockId)
    {
        Declancher(blockId, DeclancheurBlockAnnuledParDefaut);
    }

    public void DeclancherBlockEnEchec(string blockId)
    {
        Declancher(blockId, DeclancheurBlockEnEchecParDefaut);
    }

    protected abstract StateMachine<TEtat, TDeclancheur> CreerMachineAEtat(string etapeId);
}