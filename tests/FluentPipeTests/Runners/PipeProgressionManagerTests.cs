using FluentPipe.Managers.Etat;
using FluentPipe.Runner;

namespace FluentPipe.Tests.Managers.Etat;

[TestClass]
public class PipeRunnerCompletTests
{
    PipeProgressionManager _manager;
    
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
}
