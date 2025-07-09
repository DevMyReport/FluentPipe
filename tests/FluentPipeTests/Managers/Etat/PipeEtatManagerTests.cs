using FluentPipe.Managers.Etat;

namespace FluentPipe.Tests.Managers.Etat;

[TestClass]
public class PipeEtatManagerTests
{
    PipeEtatManager _manager;
    
    [ClassInitialize]
    public static void MyClassInitialize(TestContext testContext)
    {
    }

    [TestInitialize]
    public void MyTestInitialize()
    {
        _manager = new ();
    }

    [TestCleanup]
    public void MyTestCleanup()
    {
        _manager = null;
    }
    
    [TestMethod]
    public void Declancher_Demarrage_To_Succes()
    {
        const string etape = "etape";
        _manager.AjouterMachineAEtat(etape);
        _manager.DeclancherBlock(etape, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(etape, EnumMachineDeclancheur.Succes);

        var etat = _manager.GetBlockEtat(etape);
        Assert.AreEqual(EnumMachineEtat.Succes, etat);
    }
    
    [TestMethod]
    public void Declancher_Succes_SansDemarrage()
    {
        const string etape = "etape";
        _manager.AjouterMachineAEtat(etape);
        var isDeclenched =_manager.DeclancherBlock(etape, EnumMachineDeclancheur.Succes);
        Assert.IsFalse(isDeclenched);
    }
    
    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void Declancher_Demarrage_SansMachineAEtat()
    {
        const string etape = "etape";
        _manager.DeclancherBlock(etape, EnumMachineDeclancheur.Demarrage);
    }

    [TestMethod]
    public void Declancher_Demarrage_To_Erreur()
    {
        const string etape = "etape";
        _manager.AjouterMachineAEtat(etape);
        
        _manager.DeclancherBlock(etape, EnumMachineDeclancheur.Demarrage);
        var etat = _manager.GetBlockEtat(etape);
        Assert.AreEqual(EnumMachineEtat.EnCours, etat);
        
        _manager.DeclancherBlock(etape, EnumMachineDeclancheur.Erreur);
        etat = _manager.GetBlockEtat(etape);
        Assert.AreEqual(EnumMachineEtat.Echec, etat);
    }

    [TestMethod]
    public void Declancher_Demarrage_To_Annulation()
    {
        const string etape = "etape";
        _manager.AjouterMachineAEtat(etape);
        _manager.DeclancherBlock(etape, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(etape, EnumMachineDeclancheur.Annulation);

        var etat = _manager.GetBlockEtat(etape);
        Assert.AreEqual(EnumMachineEtat.Annuled, etat);
    }

    [TestMethod]
    public void AgregerEtapesEtats_Erreur_Succes()
    {
        const string enfant1 = "enfant1";
        _manager.AjouterMachineAEtat(enfant1);
        const string enfant2 = "enfant2";
        _manager.AjouterMachineAEtat(enfant2);
        const string parent = "parent";
        _manager.AjouterMachineAEtat(parent);
        
        _manager.DeclancherBlock(enfant1, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant1, EnumMachineDeclancheur.Erreur);
        var etat = _manager.GetBlockEtat(enfant1);
        Assert.AreEqual(EnumMachineEtat.Echec, etat);
        _manager.DeclancherBlock(enfant2, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant2, EnumMachineDeclancheur.Succes);
        etat = _manager.GetBlockEtat(enfant2);
        Assert.AreEqual(EnumMachineEtat.Succes, etat);

        var etatAgreged = _manager.AgregerEtapesEtats(new[] { enfant1, enfant2 });
        Assert.AreEqual(EnumMachineEtat.Echec, etatAgreged);
    }
    
    [TestMethod]
    public void AgregerEtapesEtats_Erreurs()
    {
        const string enfant1 = "enfant1";
        _manager.AjouterMachineAEtat(enfant1);
        const string enfant2 = "enfant2";
        _manager.AjouterMachineAEtat(enfant2);
        const string enfant3 = "enfant3";
        _manager.AjouterMachineAEtat(enfant3);
        const string parent = "parent";
        _manager.AjouterMachineAEtat(parent);
        
        _manager.DeclancherBlock(enfant1, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant1, EnumMachineDeclancheur.Erreur);
        _manager.DeclancherBlock(enfant2, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant2, EnumMachineDeclancheur.Erreur);
        _manager.DeclancherBlock(enfant3, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant3, EnumMachineDeclancheur.Erreur);

        var etatAgreged = _manager.AgregerEtapesEtats(new[] { enfant1, enfant2, enfant3 });
        Assert.AreEqual(EnumMachineEtat.Echec, etatAgreged);
    }
    
    [TestMethod]
    public void AgregerEtapesEtats_Succes_EnAttente_Demarrage()
    {
        const string enfant1 = "enfant1";
        _manager.AjouterMachineAEtat(enfant1);
        const string enfant2 = "enfant2";
        _manager.AjouterMachineAEtat(enfant2);
        const string enfant3 = "enfant3";
        _manager.AjouterMachineAEtat(enfant3);
        const string parent = "parent";
        _manager.AjouterMachineAEtat(parent);
        
        _manager.DeclancherBlock(enfant1, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant1, EnumMachineDeclancheur.Succes);
        _manager.DeclancherBlock(enfant2, EnumMachineDeclancheur.Demarrage);

        var etatAgreged = _manager.AgregerEtapesEtats(new[] { enfant1, enfant2, enfant3 });
        Assert.AreEqual(EnumMachineEtat.EnCours, etatAgreged);
    }
    
    [TestMethod]
    public void AgregerEtapesEtats_Succes()
    {
        const string enfant1 = "enfant1";
        _manager.AjouterMachineAEtat(enfant1);
        const string enfant2 = "enfant2";
        _manager.AjouterMachineAEtat(enfant2);
        const string enfant3 = "enfant3";
        _manager.AjouterMachineAEtat(enfant3);
        const string parent = "parent";
        _manager.AjouterMachineAEtat(parent);
        
        _manager.DeclancherBlock(enfant1, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant1, EnumMachineDeclancheur.Succes);
        _manager.DeclancherBlock(enfant2, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant2, EnumMachineDeclancheur.Succes);
        _manager.DeclancherBlock(enfant3, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant3, EnumMachineDeclancheur.Succes);

        var etatAgreged = _manager.AgregerEtapesEtats(new[] { enfant1, enfant2, enfant3 });
        Assert.AreEqual(EnumMachineEtat.Succes, etatAgreged);
    }
    
    [TestMethod]
    public void AgregerEtapesEtats_Annulation_Erreur()
    {
        const string enfant1 = "enfant1";
        _manager.AjouterMachineAEtat(enfant1);
        const string enfant2 = "enfant2";
        _manager.AjouterMachineAEtat(enfant2);
        const string enfant3 = "enfant3";
        _manager.AjouterMachineAEtat(enfant3);
        const string parent = "parent";
        _manager.AjouterMachineAEtat(parent);
        
        _manager.DeclancherBlock(enfant1, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant1, EnumMachineDeclancheur.Annulation);
        _manager.DeclancherBlock(enfant2, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant2, EnumMachineDeclancheur.Erreur);
        _manager.DeclancherBlock(enfant3, EnumMachineDeclancheur.Demarrage);

        var etatAgreged = _manager.AgregerEtapesEtats(new[] { enfant1, enfant2, enfant3 });
        Assert.AreEqual(EnumMachineEtat.Echec, etatAgreged);
    }
    
    [TestMethod]
    public void AgregerEtapesEtats_Annulation_Succes()
    {
        const string enfant1 = "enfant1";
        _manager.AjouterMachineAEtat(enfant1);
        const string enfant2 = "enfant2";
        _manager.AjouterMachineAEtat(enfant2);
        const string enfant3 = "enfant3";
        _manager.AjouterMachineAEtat(enfant3);
        const string parent = "parent";
        _manager.AjouterMachineAEtat(parent);
        
        _manager.DeclancherBlock(enfant1, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant1, EnumMachineDeclancheur.Annulation);
        _manager.DeclancherBlock(enfant2, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant2, EnumMachineDeclancheur.Succes);
        _manager.DeclancherBlock(enfant3, EnumMachineDeclancheur.Demarrage);
        _manager.DeclancherBlock(enfant3, EnumMachineDeclancheur.Succes);

        var etatAgreged = _manager.AgregerEtapesEtats(new[] { enfant1, enfant2, enfant3 });
        Assert.AreEqual(EnumMachineEtat.Annuled, etatAgreged);
    }
}
