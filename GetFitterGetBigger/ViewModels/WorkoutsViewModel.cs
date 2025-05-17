using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Olimpo;
using Olimpo.NavigationManager;

namespace GetFitterGetBigger.ViewModels;

public partial class WorkoutsViewModel(
    INavigationManager navigationManager,
    IAppCaching appCaching)
    : ViewModelBase,
    ILoadableViewModel,
    IHandlesBackButton
{
    private readonly INavigationManager _navigationManager = navigationManager;
    private readonly IAppCaching _appCaching = appCaching;

    [ObservableProperty]
    private string _workoutOfTheDayCaption = string.Empty;

    [ObservableProperty]
    private string _workoutActivePlan = string.Empty;

    [ObservableProperty]
    private string _activePlanWorkoutName = string.Empty;

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        this.WorkoutOfTheDayCaption = this._appCaching.WorkoutOfTheDay.Name;
        this.WorkoutActivePlan = this._appCaching.ActivePlan;
        this.ActivePlanWorkoutName = this._appCaching.ActivePlanWorkout.Name;

        return Task.CompletedTask;
    }

    public async Task OnBackButtonPressedAsync()
    {
        await this._navigationManager.NavigateAsync("DashboardViewModel");
    }

    [RelayCommand]
    private async Task WorkoutOfTheDay()
    {
        var parameters = new Dictionary<string, object>
        {
            ["WorkoutId"] = this._appCaching.WorkoutOfTheDay.WorkoutId
        };

        await this._navigationManager.NavigateAsync("WorkoutViewModel", parameters);
    }

    [RelayCommand]
    private async Task PlanedWorkout()
    {
        var parameters = new Dictionary<string, object>
        {
            ["WorkoutId"] = this._appCaching.ActivePlanWorkout.WorkoutId
        };

        await this._navigationManager.NavigateAsync("WorkoutViewModel", parameters);
    }
}
