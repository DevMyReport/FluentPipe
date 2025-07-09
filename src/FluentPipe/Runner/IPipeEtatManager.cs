#nullable enable

using System.ComponentModel.Design;

namespace FluentPipe.Runner;

public interface IPipeEtatManager<TEtat, TDeclancheur>
{
    void AjouterMachineAEtat(string blockId);

    void InitialiserBlockMachineAEtat(string blockId);

    /// <summary>
    /// Obtenir l'état d'une étape
    /// </summary>
    TEtat GetBlockEtat(string blockId);

    bool DeclancherBlock(string blockId, TDeclancheur declancheur);

    void DeclancherBlockDemarred(string blockId);

    void DeclancherBlockTermined(string blockId);

    void DeclancherBlockAnnuled(string blockId);

    void DeclancherBlockEnEchec(string blockId);
}