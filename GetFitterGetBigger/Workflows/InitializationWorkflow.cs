using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using Olimpo;
using Olimpo.NavigationManager;
using GetFitterGetBigger.Events;

namespace GetFitterGetBigger.Workflows;

public class InitializationWorkflow:
    IInitializationWorkflow,
    IHandleAsync<StartBootstrappingEvent>
{
    private readonly INavigationManager _navigationManager;

    public InitializationWorkflow(
        INavigationManager navigationManager,
        IEventAggregator eventAggregator)
    {
        this._navigationManager = navigationManager;

        eventAggregator.Subscribe(this);
    }

    public async Task HandleAsync(StartBootstrappingEvent message)
    {
        await Dispatcher.UIThread.InvokeAsync(async () => 
        {
            await this._navigationManager.NavigateAsync("SplashViewModel");
        });

        await Task.Delay(TimeSpan.FromSeconds(1));

        await Dispatcher.UIThread.InvokeAsync(async () => 
        {
            await this._navigationManager.NavigateAsync("DashboardViewModel");
        });
    }
}
