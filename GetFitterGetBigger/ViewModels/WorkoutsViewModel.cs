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
    ILoadableViewModel
{
    private readonly INavigationManager _navigationManager = navigationManager;
    private readonly IAppCaching _appCaching = appCaching;

    [ObservableProperty]
    public string _workoutOfTheDayCaption = string.Empty;

    public Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        this.WorkoutOfTheDayCaption = this._appCaching.WorkoutOfTheDay.Name;

        return Task.CompletedTask;
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
}
