using Microsoft.Extensions.DependencyInjection;
using Olimpo.NavigationManager;

namespace Olimpo;

public static class NavigationManagerServiceCollectionExtensions
{
    public static IServiceCollection RegisterNavigationManager(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<INavigationManager, NavigationManager.NavigationManager>();

        return serviceCollection;
    }   
}
