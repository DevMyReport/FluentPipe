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
        _manager.Declancher(etape, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(etape, EnumEtapeDeclancheur.Succes);

        var etat = _manager.GetBlockEtat(etape);
        Assert.AreEqual(EnumEtapeEtat.Succes, etat);
    }
    
    [TestMethod]
    public void Declancher_Succes_SansDemarrage()
    {
        const string etape = "etape";
        _manager.AjouterMachineAEtat(etape);
        var isDeclenched =_manager.Declancher(etape, EnumEtapeDeclancheur.Succes);
        Assert.IsFalse(isDeclenched);
    }
    
    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void Declancher_Demarrage_SansMachineAEtat()
    {
        const string etape = "etape";
        _manager.Declancher(etape, EnumEtapeDeclancheur.Demarrage);
    }

    [TestMethod]
    public void Declancher_Demarrage_To_Erreur()
    {
        const string etape = "etape";
        _manager.AjouterMachineAEtat(etape);
        
        _manager.Declancher(etape, EnumEtapeDeclancheur.Demarrage);
        var etat = _manager.GetBlockEtat(etape);
        Assert.AreEqual(EnumEtapeEtat.EnCours, etat);
        
        _manager.Declancher(etape, EnumEtapeDeclancheur.Erreur);
        etat = _manager.GetBlockEtat(etape);
        Assert.AreEqual(EnumEtapeEtat.Echec, etat);
    }

    [TestMethod]
    public void Declancher_Demarrage_To_Annulation()
    {
        const string etape = "etape";
        _manager.AjouterMachineAEtat(etape);
        _manager.Declancher(etape, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(etape, EnumEtapeDeclancheur.Annulation);

        var etat = _manager.GetBlockEtat(etape);
        Assert.AreEqual(EnumEtapeEtat.Annuled, etat);
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
        
        _manager.Declancher(enfant1, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant1, EnumEtapeDeclancheur.Erreur);
        var etat = _manager.GetBlockEtat(enfant1);
        Assert.AreEqual(EnumEtapeEtat.Echec, etat);
        _manager.Declancher(enfant2, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant2, EnumEtapeDeclancheur.Succes);
        etat = _manager.GetBlockEtat(enfant2);
        Assert.AreEqual(EnumEtapeEtat.Succes, etat);

        var etatAgreged = _manager.AgregerEtapesEtats(new[] { enfant1, enfant2 });
        Assert.AreEqual(EnumEtapeEtat.Echec, etatAgreged);
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
        
        _manager.Declancher(enfant1, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant1, EnumEtapeDeclancheur.Erreur);
        _manager.Declancher(enfant2, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant2, EnumEtapeDeclancheur.Erreur);
        _manager.Declancher(enfant3, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant3, EnumEtapeDeclancheur.Erreur);

        var etatAgreged = _manager.AgregerEtapesEtats(new[] { enfant1, enfant2, enfant3 });
        Assert.AreEqual(EnumEtapeEtat.Echec, etatAgreged);
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
        
        _manager.Declancher(enfant1, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant1, EnumEtapeDeclancheur.Succes);
        _manager.Declancher(enfant2, EnumEtapeDeclancheur.Demarrage);

        var etatAgreged = _manager.AgregerEtapesEtats(new[] { enfant1, enfant2, enfant3 });
        Assert.AreEqual(EnumEtapeEtat.EnCours, etatAgreged);
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
        
        _manager.Declancher(enfant1, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant1, EnumEtapeDeclancheur.Succes);
        _manager.Declancher(enfant2, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant2, EnumEtapeDeclancheur.Succes);
        _manager.Declancher(enfant3, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant3, EnumEtapeDeclancheur.Succes);

        var etatAgreged = _manager.AgregerEtapesEtats(new[] { enfant1, enfant2, enfant3 });
        Assert.AreEqual(EnumEtapeEtat.Succes, etatAgreged);
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
        
        _manager.Declancher(enfant1, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant1, EnumEtapeDeclancheur.Annulation);
        _manager.Declancher(enfant2, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant2, EnumEtapeDeclancheur.Erreur);
        _manager.Declancher(enfant3, EnumEtapeDeclancheur.Demarrage);

        var etatAgreged = _manager.AgregerEtapesEtats(new[] { enfant1, enfant2, enfant3 });
        Assert.AreEqual(EnumEtapeEtat.Echec, etatAgreged);
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
        
        _manager.Declancher(enfant1, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant1, EnumEtapeDeclancheur.Annulation);
        _manager.Declancher(enfant2, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant2, EnumEtapeDeclancheur.Succes);
        _manager.Declancher(enfant3, EnumEtapeDeclancheur.Demarrage);
        _manager.Declancher(enfant3, EnumEtapeDeclancheur.Succes);

        var etatAgreged = _manager.AgregerEtapesEtats(new[] { enfant1, enfant2, enfant3 });
        Assert.AreEqual(EnumEtapeEtat.Annuled, etatAgreged);
    }
}
