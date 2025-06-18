using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Olimpo;
using Olimpo.NavigationManager;
using GetFitterGetBigger.Model;
using System.Collections.ObjectModel;
using static Olimpo.NavigationManager.NavigationManager;

namespace GetFitterGetBigger.ViewModels;

public partial class WorkoutViewModel(
    INavigationManager navigationManager,
    IAppCaching appCaching) :
    ViewModelBase,
    ILoadableViewModel,
    IHandlesBackButton
{
    private readonly INavigationManager _navigationManager = navigationManager;
    private readonly IAppCaching _appCaching = appCaching;
    private int _workoutId;
    private Workout _selectedWorkout;

    [ObservableProperty]
    private string _workoutTitle = string.Empty;

    [ObservableProperty]
    private string _workoutSubTitle = string.Empty;

    [ObservableProperty]
    private string _workoutPreparationSteps = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _overallCoachNotes = [];

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        this._workoutId = int.Parse(parameters["WorkoutId"].ToString());

        this._selectedWorkout = this._appCaching.Workouts
            .Single(x => x.WorkoutId == this._workoutId);

        this.WorkoutTitle = this._selectedWorkout.Title;
        this.WorkoutSubTitle = this._selectedWorkout.Description;
        this.WorkoutPreparationSteps = this._selectedWorkout.PreparationSteps;
        this.OverallCoachNotes = new ObservableCollection<string>(this._selectedWorkout.CoachNotes);

        return Task.CompletedTask;
    }

    public async Task OnBackButtonPressedAsync()
    {
        await this._navigationManager.GoBackAsync();
    }

    [RelayCommand]
    private async Task BackMenu()
    {
        await this._navigationManager.GoBackAsync();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await this._navigationManager.GoBackAsync();
    }
    
    [RelayCommand]
    private async Task Start()
    {
        var parameters = new Dictionary<string, object>
        {
            ["WorkoutId"] = this._selectedWorkout.WorkoutId
        };

        var navigationOptions = new NavigationOptions("WorkoutWorkflowViewModel", false, false, parameters);
        await this._navigationManager.NavigateAsync(navigationOptions);
    }
}
