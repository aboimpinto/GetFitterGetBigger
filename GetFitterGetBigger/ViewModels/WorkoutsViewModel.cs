using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Olimpo.NavigationManager;

namespace GetFitterGetBigger.ViewModels;

public partial class WorkoutsViewModel(
    INavigationManager navigationManager) 
    : ViewModelBase
{
    private readonly INavigationManager _navigationManager = navigationManager;

    [RelayCommand]
    private async Task WorkoutOfTheDay()
    {
        var parameters = new Dictionary<string, object>
        {
            ["WorkoutId"] = 1
        };

        await this._navigationManager.NavigateAsync("WorkoutViewModel", parameters);
    }
}
