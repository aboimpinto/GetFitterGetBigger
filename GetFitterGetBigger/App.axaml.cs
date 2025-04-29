using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Olimpo;
using Olimpo.NavigationManager;
using GetFitterGetBigger.ViewModels;
using GetFitterGetBigger.Views;
using GetFitterGetBigger.Workflows;

namespace GetFitterGetBigger;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var viewModel = ServiceCollectionManager.ServiceProvider.GetService<ViewModelBase>("MainViewModel");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = viewModel
            };
        }

        if (viewModel is ILoadableViewModel loadableViewModel)
        {
            loadableViewModel.LoadAsync().Wait();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    public override void RegisterServices()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddLogging()
            .RegisterEventAggregator()
            .RegisterNavigationManager();

        serviceCollection.AddScoped<ViewModelBase, MainViewModel>("MainViewModel");
        serviceCollection.AddScoped<ViewModelBase, SplashViewModel>("SplashViewModel");

        serviceCollection.AddSingleton<IInitializationWorkflow, InitializationWorkflow>();

        ServiceCollectionManager.SetServiceProvider(serviceCollection);

        base.RegisterServices();
    }
}