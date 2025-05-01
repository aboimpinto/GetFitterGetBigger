using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using Olimpo;
using Olimpo.NavigationManager;
using GetFitterGetBigger.Events;
using GetFitterGetBigger.Model;

namespace GetFitterGetBigger.Workflows;

public class InitializationWorkflow:
    IInitializationWorkflow,
    IHandleAsync<StartBootstrappingEvent>
{
    private readonly IAppCaching _appCaching;
    private readonly INavigationManager _navigationManager;

    public InitializationWorkflow(
        IAppCaching appCaching,
        INavigationManager navigationManager,
        IEventAggregator eventAggregator)
    {
        this._appCaching = appCaching;
        this._navigationManager = navigationManager;

        eventAggregator.Subscribe(this);
    }

    public async Task HandleAsync(StartBootstrappingEvent message)
    {
        await Dispatcher.UIThread.InvokeAsync(async () => 
        {
            await this._navigationManager.NavigateAsync("SplashViewModel");
        });

        var pushupsCrunchesAndSquatsWorkout = this.BuildPushupsCrunchesAndSquatsWorkout();
        this._appCaching.Workouts.Add(pushupsCrunchesAndSquatsWorkout);
        this._appCaching.WorkoutOfTheDay = pushupsCrunchesAndSquatsWorkout;

        await Task.Delay(TimeSpan.FromSeconds(1));

        await Dispatcher.UIThread.InvokeAsync(async () => 
        {
            await this._navigationManager.NavigateAsync("DashboardViewModel");
        });
    }

    public Workout BuildPushupsCrunchesAndSquatsWorkout()
    {
        return new Workout(
            1, 
            "Pushups / Crunches / Squats",
            "No equipment need",
            "* Rest 2 minutes between sets\n* Keep good form and full range of motion",
            WorkoutType.HIIT, 
            []);
    }
}
