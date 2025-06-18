using System;
using Microsoft.Extensions.DependencyInjection;

namespace Olimpo;

public static class ServiceCollectionManager
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public static void SetServiceProvider(IServiceCollection serviceCollection)
    {
        ServiceProvider = serviceCollection.BuildServiceProvider();
    }
}