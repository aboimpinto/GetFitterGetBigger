using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GetFitterGetBigger.Model;
using Olimpo;
using Olimpo.NavigationManager;
using static Olimpo.NavigationManager.NavigationManager;

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
    private Workout _workoutOfTheDaySummary;

    [ObservableProperty]
    private Workout _activePlanWorkout;

    [ObservableProperty]
    private string _workoutOfTheDayCaption = string.Empty;

    [ObservableProperty]
    private string _workoutActivePlan = string.Empty;

    [ObservableProperty]
    private string _activePlanWorkoutName = string.Empty;

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        this.WorkoutOfTheDaySummary = this._appCaching.WorkoutOfTheDay;
        this.ActivePlanWorkout = this._appCaching.ActivePlanWorkout;
        this.WorkoutActivePlan = this._appCaching.ActivePlan;

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task BackMenu()
    {
        await this._navigationManager.GoBackAsync();
    }

    [RelayCommand]
    private async Task WorkoutOfTheDay()
    {
        var parameters = new Dictionary<string, object>
        {
            ["WorkoutId"] = this._appCaching.WorkoutOfTheDay.WorkoutId
        };

        var navigationOptions = new NavigationOptions("WorkoutViewModel", false, false, parameters);
        await this._navigationManager.NavigateAsync(navigationOptions);
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

    public async Task OnBackButtonPressedAsync()
    {
        await this.BackMenu();
    }
}
