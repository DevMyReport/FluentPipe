using FluentPipe.Blocks.Lifetime;
using FluentPipe.Builder;
using FluentPipe.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Tests.LifeTime;

[TestClass]
public class LifeTimeTests : BasePipeInit
{
    private IPipeRunner CurrentService => GetContainer().GetRequiredService<IPipeRunner>();

    [TestCleanup]
    public void OnCleanUp()
    {
        //Clear the cache for the singleton to be reset
        ServiceProvider = null;
    }

    [TestMethod]
    public async Task Seq_Singleton_Is_Created_Only_One_Time()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next<LifeTimeInputBlock, LifeTimeResult>()
                .Next<SingletonBlock, LifeTimeResult>()
                .Next<SingletonBlock, LifeTimeResult>()
            ;

        var plan = builder.GetDetails();

        var result1 = await CurrentService.RunAsync(plan, 1);

        var result2 = await CurrentService.RunAsync(plan, 1);

        Assert.AreEqual(2, result1.Sortie.Value);
        Assert.AreEqual(6, result2.Sortie.Value);

        Assert.AreEqual(result1.Sortie.Guid, result2.Sortie.Guid);
    }

    [TestMethod]
    public async Task Seq_Transient_Is_Created_Every_Time()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next<LifeTimeInputBlock, LifeTimeResult>()
                .Next<TransientBlock, LifeTimeResult>()
                .Next<TransientBlock, LifeTimeResult>()
                .Next<TransientBlock, LifeTimeResult>()
            ;

        var plan = builder.GetDetails();

        var result1 = await CurrentService.RunAsync(plan, 1);

        var result2 = await CurrentService.RunAsync(plan, 1);

        Assert.AreEqual(1, result1.Sortie.Value);
        Assert.AreEqual(1, result2.Sortie.Value);

        Assert.AreNotEqual(result1.Sortie.Guid, result2.Sortie.Guid);
    }

    [TestMethod]
    public async Task Seq_Scoped_Is_Created_For_each_Runner()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next<LifeTimeInputBlock, LifeTimeResult>()
                .Next<ScopedBlock, LifeTimeResult>() //0->1
                .Next<ScopedBlock, LifeTimeResult>() //1->2
                .Next<ScopedBlock, LifeTimeResult>() //2->4
            ;

        var plan = builder.GetDetails();

        var result1 = await CurrentService.RunAsync(plan, 1);
        var result2 = await CurrentService.RunAsync(plan, 1);

        Assert.AreEqual(4, result1.Sortie.Value);
        Assert.AreEqual(4, result2.Sortie.Value);

        Assert.AreNotEqual(result1.Sortie.Guid, result2.Sortie.Guid);
    }


    /// <summary>
    /// Lancer des block qui sont en singleton en parrallele, cela n'as pas trop de sens.
    /// Car du coup il ne peut pas y avoir de gestion d'etat, d'erreur ou de progression (on a qu'une instance des managers par block)
    /// Si on veut faire des "block Singleton en parallele" il faut faire des block transient qui utilisent par composition un singleton
    /// Idem pour les scoped
    /// Les block Singleton ou scoped n'ont pas je pense vocation à fonctionner en parallele car on ne pourra suivre leur avancement de manière fiable
    /// Du coup pour ce test je remplace l'usage de 2 SingletonBlock par un SingletonBlock et une ScopedBlock
    /// </summary>
    [TestMethod]
    public async Task Parallel_Singleton_Is_Created_Only_One_Time()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next<LifeTimeInputBlock, LifeTimeResult>()
                .ParallelStart<LifeTimeResult>()
                .With<SingletonBlock>()
                .ParallelEnd()
            ;

        var plan = builder.GetDetails();

        var result1 = await CurrentService.RunAsync(plan, 1);
        var result2 = await CurrentService.RunAsync(plan, 1);

        var s1 = result1.Sortie.Concat(result2.Sortie).ToList();

        var distinctGuid = s1.DistinctBy(d => d.Guid);
        var distinctValues = s1.DistinctBy(d => d.Value);

        Assert.AreEqual(1, distinctGuid.Count());
        Assert.AreEqual(s1.Count, distinctValues.Count());
    }

    [TestMethod]
    public async Task Parallel_Singleton_Is_Created_Only_One_Time_2()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next<LifeTimeInputBlock, LifeTimeResult>()
                .ParallelStart<LifeTimeResult>()
                .With<SingletonBlock>()
                .ParallelEnd()
            ;

        var plan = builder.GetDetails();

        var result1 = await CurrentService.RunAsync(plan, 1);
        var result2 = await CurrentService.RunAsync(plan, 1);

        var s1 = result1.Sortie.Concat(result2.Sortie).ToList();

        var distinctGuid = s1.DistinctBy(d => d.Guid);
        var distinctValues = s1.DistinctBy(d => d.Value);
        Assert.AreEqual(1, distinctGuid.Count());
        Assert.AreEqual(s1.Count, distinctValues.Count());
    }

    [TestMethod]
    public async Task Parallel_Transient_Is_Created_Every_Time()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next<LifeTimeInputBlock, LifeTimeResult>()
                .ParallelStart<LifeTimeResult>()
                .With<TransientBlock>()
                .With<TransientBlock>()
                .ParallelEnd()
            ;

        var plan = builder.GetDetails();

        var result1 = await CurrentService.RunAsync(plan, 1);

        var result2 = await CurrentService.RunAsync(plan, 1);

        var s1 = result1.Sortie.Concat(result2.Sortie).ToList();

        var distinctGuid = s1.DistinctBy(d => d.Guid);
        var distinctValues = s1.DistinctBy(d => d.Value);
        Assert.AreEqual(4, distinctGuid.Count());
        Assert.AreEqual(1, distinctValues.Count());
    }


    [TestMethod]
    public async Task Parallel_Scoped_Is_Created_For_each_Runner()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next<LifeTimeInputBlock, LifeTimeResult>()
                .ParallelStart<LifeTimeResult>()
                .With<ScopedBlock>()
                .ParallelEnd()
            ;

        var plan = builder.GetDetails();

        var result1 = await CurrentService.RunAsync(plan, 1);
        var result2 = await CurrentService.RunAsync(plan, 1);

        var s1 = result1.Sortie.Concat(result2.Sortie).ToList();

        var distinctGuid = s1.DistinctBy(d => d.Guid); // une instance par scope
        var distinctValues = s1.DistinctBy(d => d.Value); // un seul résultat identique
        Assert.AreEqual(2, distinctGuid.Count());
        Assert.AreEqual(1, distinctValues.Count());
    }

    [TestMethod]
    public async Task Seq_Scope_Can_Be_Reset()
    {
        var builder = PipeBuilderFactory.Creer<int>()
                .Next<LifeTimeInputBlock, LifeTimeResult>()
                .Next<ScopedBlock, LifeTimeResult>()
                .Next<ScopedBlock, LifeTimeResult>()
                .ResetScope()
                .Next<ScopedBlock, LifeTimeResult>()
                .Next<ScopedBlock, LifeTimeResult>()
            ;

        var plan = builder.GetDetails();

        var result1 = await CurrentService.RunAsync(plan, 1);
        var result2 = await CurrentService.RunAsync(plan, 1);

        Assert.AreEqual(4, result1.Sortie.Value);
        Assert.AreEqual(4, result2.Sortie.Value);

        Assert.AreNotEqual(result1.Sortie.Guid, result2.Sortie.Guid);
    }

}