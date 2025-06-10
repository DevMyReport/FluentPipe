using FluentPipe.Blocks.OperationBlocks;
using FluentPipe.Blocks.Readers;
using FluentPipe.Builder;
using FluentPipe.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Tests.Switch;

[TestClass]
public class SwitchTests : BasePipeInit
{
    private IRunner _CurrentService => GetContainer().GetRequiredService<IRunner>();

    [TestMethod]
    public async Task Switch_Condition_Multiple_First_Position()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next_ReadBlock()
                .SwitchStart<int>()
                .Case<SommeBlock>(true)
                .Case<MinBlock>(false)
                .Case<MoyBlock>(false)
                .SwithlEnd()
            ;

        var plan = builder.GetDetails();

        var result = await _CurrentService.RunAsync(plan, 1234);

        Assert.AreEqual(761995, result.Sortie);
        Assert.AreEqual(2, result.Etapes.Count);
    }

    [TestMethod]
    public async Task Switch_Condition_Multiple_Middle_Position()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next_ReadBlock()
                .SwitchStart<int>()
                .Case<SommeBlock>(false)
                .Case<MinBlock>(true)
                .Case<MoyBlock>(false)
                .SwithlEnd()
            ;

        var plan = builder.GetDetails();

        var result = await _CurrentService.RunAsync(plan, 1234);

        Assert.AreEqual(1, result.Sortie);
        Assert.AreEqual(2, result.Etapes.Count);
    }

    [TestMethod]
    public async Task Switch_Condition_Multiple_Last_Position()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next_ReadBlock()
                .SwitchStart<int>()
                .Case<SommeBlock>(false)
                .Case<MinBlock>(false)
                .Case<MoyBlock>(true)
                .SwithlEnd()
            ;

        var plan = builder.GetDetails();

        var result = await _CurrentService.RunAsync(plan, 1234);

        Assert.AreEqual(617, result.Sortie);
        Assert.AreEqual(2, result.Etapes.Count);
    }

    [TestMethod]
    public async Task Switch_Condition_First_True_Is_Taken()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next_ReadBlock()
                .SwitchStart<int>()
                .Case<SommeBlock>(true)
                .Case<MinBlock>(false)
                .Case<MoyBlock>(true)
                .SwithlEnd()
            ;

        var plan = builder.GetDetails();

        var result = await _CurrentService.RunAsync(plan, 1234);

        Assert.AreEqual(761995, result.Sortie);
        Assert.AreEqual(2, result.Etapes.Count);
    }

    [TestMethod]
    public async Task Switch_Condition_Multiple_Correct_Output_Type()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next_ReadBlock()
                .SwitchStart<int>()
                .Case<SommeBlock>(true)
                .Case<MinBlock>(false)
                .Case<MoyBlock>(false)
                .SwithlEnd()
                .Next<ToStringBlock, string, ToStringBlockOption>(new())
            ;

        var plan = builder.GetDetails();

        var result = await _CurrentService.RunAsync(plan, 1234);

        Assert.AreEqual("761995", result.Sortie);
        Assert.AreEqual(3, result.Etapes.Count);
    }

    [TestMethod]
    public void Switch_No_Condition_Throw_InvalidOperationException()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            PipeBuilderFactory.Creer<int>()
                .Next_ReadBlock()
                .SwitchStart<int>()
                .Case<SommeBlock>(false)
                .Case<MinBlock>(false)
                .Case<MoyBlock>(false)
                .SwithlEnd()
                ;
        });
    }

    [TestMethod]
    public async Task Switch_Condition_Multiple_First_Position_With_Option()
    {
        var builder = PipeBuilderFactory.Creer<int>()
            .Next_ReadBlock()
            .SwitchStart<int>()
            .Case<AddOneOptionBlock, AddOneOption>(new(1), _ => true)
            .Case<MinBlock>(false)
            .Case<MoyBlock>(false)
            .SwithlEnd()
            ;

        var plan = builder.GetDetails();

        var result = await _CurrentService.RunAsync(plan, 1234);

        Assert.AreEqual(761996, result.Sortie);
        Assert.AreEqual(2, result.Etapes.Count);
    }


    [TestMethod]
    public async Task Switch_Condition_Case_Pipe()
    {
        var underbuilder = PipeBuilderFactory.CreerEnumerable<int>()
            .Next<SommeBlock, int>();

        var builder = PipeBuilderFactory.Creer<int>()
                .Next_ReadBlock()
                .SwitchStart<int>()
                .Case<MinBlock>(false)
                .CasePipe(underbuilder, true)
                .SwithlEnd()
            ;

        var plan = builder.GetDetails();

        var result = await _CurrentService.RunAsync(plan, 1234);

        Assert.AreEqual(761995, result.Sortie);
        Assert.AreEqual(2, result.Etapes.Count);
    }
}