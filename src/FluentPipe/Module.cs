using FluentPipe.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentPipe;

public static class Module
{
    public static IServiceCollection RegisterRunner(this IServiceCollection services)
    {
        services.TryAddTransient<IRunner,PipeRunnerComplet>();
       
        return services;
    }
}