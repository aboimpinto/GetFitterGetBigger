using System.Threading.Tasks;
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
        await this._navigationManager.NavigateAsync("SplashViewModel");
    }
}
