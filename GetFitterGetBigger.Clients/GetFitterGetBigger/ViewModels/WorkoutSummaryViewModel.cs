using CommunityToolkit.Mvvm.ComponentModel;
using Olimpo.NavigationManager;

namespace GetFitterGetBigger.ViewModels;

public partial class WorkoutSummaryViewModel(
    string workoutName,
    string workoutTime) :
    ViewModelBase
{
    [ObservableProperty]
    private string _workoutName = workoutName;

    [ObservableProperty]
    private string _workoutTime = workoutTime;
}
