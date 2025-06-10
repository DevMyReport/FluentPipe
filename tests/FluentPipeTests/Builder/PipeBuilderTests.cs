using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Blocks.Readers;
using FluentPipe.Blocks.System;
using FluentPipe.Builder;
using FluentPipe.Builder.Entity;

namespace FluentPipe.Tests.Builder;

[TestClass]
public class PipeBuilderTests
{
    [TestMethod]
    public void PipeBuilder_Simple_chain()
    {
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>()
            .Next<ToIntBlock, int>();

        var expected = new List<Etape>
        {
            new Etape(typeof(ReverseBlock)),
            new Etape(typeof(ToIntBlock))
        }.ToArray();

        var values = builder.GetDetails();

        Assert.AreEqual(expected.Length, values.Etapes.Count);

        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i].TypeDuBlock, ((Etape)values.Etapes[i]).TypeDuBlock);
    }

    [TestMethod]
    public void PipeBuilder_condition_chain()
    {
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>(false)
            .Next<ReverseBlock, string>(false)
            .Next<ToIntBlock, int>();

        var expected = new List<Etape>
        {
            new Etape(typeof(ToIntBlock))
        }.ToArray();

        var values = builder.GetDetails();

        Assert.AreEqual(expected.Length, values.Etapes.Count);

        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i].TypeDuBlock, ((Etape)values.Etapes[i]).TypeDuBlock);
    }

    [TestMethod]
    public void PipeBuilder_condition_et_options()
    {
        var builder = PipeBuilderFactory.Creer<string>()
            .Next<ReverseBlock, string>(false)
            .Next<ToIntBlock, int>()
            .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption())
            .Next<ReverseBlock, string>(false);

        var expected = new List<Etape>
        {
            new Etape(typeof(ToIntBlock)),
            new Etape(typeof(ToStringBlock), new ToStringBlockOption())
        }.ToArray();

        var values = builder.GetDetails();

        Assert.AreEqual(expected.Length, values.Etapes.Count);

        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i].TypeDuBlock, ((Etape)values.Etapes[i]).TypeDuBlock);
    }

    [TestMethod]
    public void PipeBuilder_Can_Inject_Builder()
    {
        var builderOperation = PipeBuilderFactory.CreerEnumerable<int>()
                .SwitchStart<int>()
                .Case<SommeBlock>(false)
                .Case<MinBlock>(true)
                .Case<MoyBlock>(false)
                .SwithlEnd();

        var builder = PipeBuilderFactory.Creer<int>()
            .Next_ReadBlock()
            .NextPipe(builderOperation)
            .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption { Format = "TOTO" })
            .Next<PrintBlock, string>()
            ;

        var expected = new List<Etape>
        {
            new Etape(typeof(ReadBlock)),
            new Etape(typeof(MinBlock)),
            new Etape(typeof(ToStringBlock), new ToStringBlockOption { Format = "TOTO" }),
            new Etape(typeof(PrintBlock))
        }.ToArray();

        var values = builder.GetDetails();

        Assert.AreEqual(expected.Length, values.Etapes.Count);

        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i].TypeDuBlock, ((Etape)values.Etapes[i]).TypeDuBlock);
    }

    [TestMethod]
    public void PipeBuilder_Can_Not_Inject_Builder()
    {
        var builderPlusUn = PipeBuilderFactory.Creer<int>()
            .Next<AddOneBlock, int>();

        var builder = PipeBuilderFactory.Creer<int>()
                .Next<AddOneBlock, int>()
                .NextPipe(builderPlusUn, false)
                .Next<ToStringBlock, string, ToStringBlockOption>(new ToStringBlockOption { Format = "TOTO" })
                .Next<PrintBlock, string>()
            ;

        var expected = new List<Etape>
        {
            new Etape(typeof(AddOneBlock)),
            new Etape(typeof(ToStringBlock), new ToStringBlockOption { Format = "TOTO" }),
            new Etape(typeof(PrintBlock))
        }.ToArray();

        var values = builder.GetDetails();

        Assert.AreEqual(expected.Length, values.Etapes.Count);

        for (var i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i].TypeDuBlock, ((Etape)values.Etapes[i]).TypeDuBlock);
    }
}