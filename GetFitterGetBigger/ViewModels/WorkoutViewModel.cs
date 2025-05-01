using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Olimpo;
using Olimpo.NavigationManager;

namespace GetFitterGetBigger.ViewModels;

public partial class WorkoutViewModel(INavigationManager navigationManager) :
    ViewModelBase,
    ILoadableViewModel
{
    private readonly INavigationManager _navigationManager = navigationManager;
    private int _workoutId;

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        this._workoutId = int.Parse(parameters["WorkoutId"].ToString());

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
        await this._navigationManager.NavigateAsync("WorkoutWorkflowViewModel");
    }
}
