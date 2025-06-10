using FluentPipe.Blocks.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentPipe.InjectionHelper;

public static class ServiceRegistryHelper
{
    #region lifetimeBlock
    public static IServiceCollection RegisterBlockTransient<T>(this IServiceCollection services)
        where T : class, IPipeProcessBlock
    {
        services.AddKeyedTransient<IPipeProcessBlock, T>(GetServiceType<T>());
        return services;
    }

    public static IServiceCollection RegisterBlockSingleton<T>(this IServiceCollection services)
        where T : class, IPipeProcessBlock
    {
        services.AddKeyedSingleton<IPipeProcessBlock, T>(GetServiceType<T>());
        return services;
    }

    public static IServiceCollection RegisterBlockScoped<T>(this IServiceCollection services)
        where T : class, IPipeProcessBlock
    {
        services.AddKeyedScoped<IPipeProcessBlock, T>(GetServiceType<T>());
        return services;
    }

    #endregion

    #region Lifeteime Dynamic Pipe
    public static IServiceCollection RegisterDynamicBlockTransient<T>(this IServiceCollection services)
        where T : class, IBuilderProcessBlock
    {
        services.AddKeyedTransient<IBuilderProcessBlock, T>(GetServiceType<T>());
        return services;
    }

    public static IServiceCollection RegisterDynamicBlockSingleton<T>(this IServiceCollection services)
        where T : class, IBuilderProcessBlock
    {
        services.AddKeyedSingleton<IBuilderProcessBlock, T>(GetServiceType<T>());
        return services;
    }

    public static IServiceCollection RegisterDynamicBlockScoped<T>(this IServiceCollection services)
        where T : class, IBuilderProcessBlock
    {
        services.AddKeyedScoped<IBuilderProcessBlock, T>(GetServiceType<T>());
        return services;
    }
    #endregion
    internal static Type GetServiceType<T>()
    {
        return typeof(T);
    }

    public static IServiceCollection RegisterAllBlocksFromAssembly<TAssembly>(this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.RegisterAllBlocksFromAssembly<TAssembly, IPipeProcessBlock>(lifetime);
        services.RegisterAllBlocksFromAssembly<TAssembly, IBuilderProcessBlock>(lifetime);

        return services;
    }

    private static IServiceCollection RegisterAllBlocksFromAssembly<TAssembly,TType>(this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        var interfaceType = typeof(TType);

        var blockTypes = typeof(TAssembly).Assembly
            .GetTypes()
            .Where(type => interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        foreach (var type in blockTypes)
            services.TryAdd(new ServiceDescriptor(interfaceType, type, type, lifetime));

        return services;
    }
}