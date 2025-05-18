using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GetFitterGetBigger.Model;
using Olimpo;
using Olimpo.NavigationManager;

namespace GetFitterGetBigger.ViewModels;

public partial class WorkoutViewModel(
    INavigationManager navigationManager,
    IAppCaching appCaching) :
    ViewModelBase,
    ILoadableViewModel
{
    private readonly INavigationManager _navigationManager = navigationManager;
    private readonly IAppCaching _appCaching = appCaching;
    private int _workoutId;
    private Workout _selectedWorkout;

    [ObservableProperty]
    private string _workoutCaption = string.Empty;

    [ObservableProperty]
    private string _workoutPreparationSteps = string.Empty;

    [ObservableProperty]
    private string _overallCoachNotes = string.Empty;

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        this._workoutId = int.Parse(parameters["WorkoutId"].ToString());

        this._selectedWorkout = this._appCaching.Workouts
            .Single(x => x.WorkoutId == this._workoutId);

        this.WorkoutCaption = this._selectedWorkout.Title;
        this.WorkoutPreparationSteps = this._selectedWorkout.PreparationSteps;
        this.OverallCoachNotes = this._selectedWorkout.CoachNotes;

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task BackMenu()
    {
        await this._navigationManager.NavigateAsync("WorkoutsViewModel");
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await this._navigationManager.NavigateAsync("WorkoutsViewModel");
    }
    
    [RelayCommand]
    private async Task Start()
    {
        var parameters = new Dictionary<string, object>
        {
            ["WorkoutId"] = this._selectedWorkout.WorkoutId
        };

        await this._navigationManager.NavigateAsync("WorkoutWorkflowViewModel", parameters);
    }
}
