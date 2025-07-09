using System.Collections.Concurrent;
using FluentPipe.Managers.Etat;
using Stateless;

namespace FluentPipe.Runner;

public abstract class PipeEtatManagerBase<TEtat, TDeclancheur> : IPipeEtatManager<TEtat, TDeclancheur>
{
    public PipeEtatManagerBase()
    {
        PipeMachine = CreerPipeMachineAEtat();
    }
    
    protected abstract TDeclancheur? DeclancheurBlockDemarredParDefaut { get; }
    protected abstract TDeclancheur? DeclancheurBlockSuccesParDefaut { get; }
    protected abstract TDeclancheur? DeclancheurBlockEnEchecParDefaut { get; }
    protected abstract TDeclancheur? DeclancheurBlockAnnuledParDefaut { get; }

    /// <summary>
    /// L'ensemble des Machines à état du Pipe(runner)
    /// </summary>
    private readonly ConcurrentDictionary<string, StateMachine<TEtat, TDeclancheur>> _blocksMachines = new();

    /// <summary>
    /// 
    /// </summary>
    public readonly StateMachine<EnumMachineEtat, EnumMachineDeclancheur> PipeMachine;

    public void AjouterMachineAEtat(string blockId)
    {
        var machine = CreerBlockMachineAEtat(blockId);
        if (!_blocksMachines.TryAdd(blockId, machine))
            throw new InvalidOperationException($"Machine '{blockId}' is existe déjà");
    }

    public List<string> GetIdBlockSelonEtat(TEtat etat)
    {
        return _blocksMachines
            .Where(kvp => EqualityComparer<TEtat>.Default.Equals(kvp.Value.State, etat))
            .Select(kvp => kvp.Key)
            .ToList();
    }

    /// <summary>
    /// Créer ou recréer la machine a état de l'étape
    /// </summary>
    /// <param name="blockId"></param>
    public void InitialiserBlockMachineAEtat(string blockId)
    {
        if (_blocksMachines.ContainsKey(blockId))
            if (!_blocksMachines.TryRemove(blockId, out _))
                throw new InvalidOperationException($"Machine '{blockId}' n'a pu être réinitialisée");

        AjouterMachineAEtat(blockId);
    }

    public StateMachine<TEtat, TDeclancheur> GetMachineByBlockId(string blockId)
    {
        return _blocksMachines[blockId];
    }

    public TEtat GetBlockEtat(string blockId)
    {
        var machine = GetMachineByBlockId(blockId);
        return machine.State;
    }

    public abstract TEtat AgregerEtapesEtats(IEnumerable<string> etapesIds);

    public TEtat GetEtapeEtatOrDefault(string etapeId, TEtat defaultEtape)
    {
        if (_blocksMachines.TryGetValue(etapeId, out var m))
            return m.State;
        throw new KeyNotFoundException($"Aucune machine à état trouvée pour l'étape {etapeId}");
    }

    public bool DeclancherBlock(string blockId, TDeclancheur declancheur)
    {
        var machine = GetMachineByBlockId(blockId);

        if (!machine.CanFire(declancheur))
            return false;

        machine.Fire(declancheur);
        return true;
    }

    public void DeclancherBlockDemarred(string blockId)
    {
        if (DeclancheurBlockDemarredParDefaut != null)
            DeclancherBlock(blockId, DeclancheurBlockDemarredParDefaut);
    }

    public void DeclancherBlockTermined(string blockId)
    {
        if (DeclancheurBlockSuccesParDefaut != null)
            DeclancherBlock(blockId, DeclancheurBlockSuccesParDefaut);
    }

    public void DeclancherBlockAnnuled(string blockId)
    {
        if (DeclancheurBlockAnnuledParDefaut != null)
            DeclancherBlock(blockId, DeclancheurBlockAnnuledParDefaut);
    }

    public void DeclancherBlockEnEchec(string blockId)
    {
        if (DeclancheurBlockEnEchecParDefaut != null)
            DeclancherBlock(blockId, DeclancheurBlockEnEchecParDefaut);
    }

    protected abstract StateMachine<TEtat, TDeclancheur> CreerBlockMachineAEtat(string blockId);
    protected abstract StateMachine<EnumMachineEtat, EnumMachineDeclancheur> CreerPipeMachineAEtat();
}