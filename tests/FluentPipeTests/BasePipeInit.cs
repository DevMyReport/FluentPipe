using FluentPipe.Blocks;
using FluentPipe.Managers.Erreur;
using FluentPipe.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluentPipe.Tests;

public abstract class BasePipeInit
{
    protected IServiceProvider? ServiceProvider { get; set; }

    protected virtual IServiceProvider GetContainer()
    {
        if (ServiceProvider != null) return ServiceProvider;

        var host = Host.CreateDefaultBuilder();
        host.ConfigureServices(services => SetServices(services));

        ServiceProvider = host.Build().Services;
        return ServiceProvider;
    }

    protected virtual IServiceCollection SetServices(IServiceCollection services)
    {
        services.RegisterBlocks();
        services.RegisterRunner();
        return services;
    }

    protected static void AssertTime<T>(SortieRunner<T, PipeErreurManager> sortie)
    {
        var temps = sortie.Blocks.Select(s => s.Duree.TotalMilliseconds).Sum();
        Console.WriteLine($"Temps total:{sortie.DureeComplete.Milliseconds} / {temps} ms");
        Assert.IsTrue(sortie.DureeComplete.Milliseconds < temps);
    }

    protected static void AssertTime<T>(SortieRunner<T, PipeErreurManager> sortie, int maxTime)
    {
        var temps = sortie.Blocks.Select(s => s.Duree.TotalMilliseconds).Sum();
        Console.WriteLine($"Temps total:{temps} / {maxTime} ms");
        Assert.IsTrue(temps < maxTime);
    }
}