using FluentPipe.Blocks;
using FluentPipe.Builder.Entity;
using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Runner;

namespace FluentPipe.Tests.Managers.Progression;

[TestClass]
public class PipeProgressionManagerTests
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

    [TestMethod]
    public void OnProgressionBlock_Raises_BlockProgressionChanged()
    {
        _manager.NotifierProgressionPipe(2,5);

        ProgressionEvent? blockProgressionEventReceived = null;
        _manager.BlockProgressionChanged += (_, e) => blockProgressionEventReceived = e;
        
        PipeProgressionEvent? pipeProgressionEventReceived = null;
        _manager.PipeProgressionChanged += (_, e) => pipeProgressionEventReceived = e;

        var blockEvent = new ProgressionEvent("step", "Step", 1, 10);

        _manager.OnProgressionBlock(this, blockEvent);

        Assert.IsNotNull(pipeProgressionEventReceived);
        Assert.IsInstanceOfType(pipeProgressionEventReceived, typeof(PipeProgressionEvent));
        Assert.AreEqual("", pipeProgressionEventReceived!.ElementId);
        Assert.AreEqual("", pipeProgressionEventReceived.ElementLibelle);
        Assert.AreEqual(2L, pipeProgressionEventReceived.Fait);
        Assert.AreEqual(5L, pipeProgressionEventReceived.Total);
        Assert.AreEqual(1L, pipeProgressionEventReceived.LastBlockProgressionEvent.Fait);
        Assert.AreEqual(10L, pipeProgressionEventReceived.LastBlockProgressionEvent.Total);
    }

    [TestMethod]
    public void NotifierProgressionPipe_RaisesEvent_And_UpdatesValues()
    {
        ProgressionEvent? received = null;
        _manager.BlockProgressionChanged += (_, e) => received = e;

        _manager.NotifierProgressionPipe(3, 10);

        Assert.AreEqual(3L, _manager.Fait);
        Assert.AreEqual(10L, _manager.Total);
        Assert.IsNotNull(received);
        Assert.IsInstanceOfType(received, typeof(PipeProgressionEvent));
        Assert.AreEqual(3L, received!.Fait);
        Assert.AreEqual(10L, received.Total);
    }

    [TestMethod]
    public void NotifierBlockTraited_Increment_Fait()
    {
        _manager.NotifierProgressionPipe(1,5);
        var info = new BlockInfo(typeof(AddOneBlock));

        _manager.NotifierBlockTraited(info);

        Assert.AreEqual(2L, _manager.Fait);
    }
}
