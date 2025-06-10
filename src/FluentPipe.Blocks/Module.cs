using FluentPipe.Blocks.Dynamic;
using FluentPipe.Blocks.Lifetime;
using FluentPipe.Blocks.System;
using FluentPipe.InjectionHelper;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPipe.Blocks;

public static class Module
{
    public static IServiceCollection RegisterBlocks(this IServiceCollection services)
    {
        services.RegisterBlockScoped<ScopedBlock>();
        services.RegisterDynamicBlockScoped<AddMultiScopedOptionBlock>();
        services.RegisterBlockSingleton<SingletonBlock>();
        services.RegisterBlockTransient<TransientBlock>();

        services.RegisterAllBlocksFromAssembly<CancelExceptionBlock>();
        return services;
    }
}