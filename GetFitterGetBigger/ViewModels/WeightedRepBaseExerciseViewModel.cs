using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using GetFitterGetBigger.Model;
using Olimpo.NavigationManager;

namespace GetFitterGetBigger.ViewModels;

public partial class WeightedRepBaseExerciseViewModel : ViewModelBase
{
    private readonly WorkoutStep _workoutStep;

    [ObservableProperty]
    private string _workoutName = string.Empty;

    [ObservableProperty]
    private string _roundInfo = string.Empty;

    [ObservableProperty]
    private string _setInfo = string.Empty;

    [ObservableProperty]
    private string _exerciseInfo = string.Empty;

    [ObservableProperty]
    private string _reps = string.Empty;

    [ObservableProperty]
    private Bitmap _imageSource;

    [ObservableProperty]
    private ObservableCollection<string> _exerciseCoachNotes = [];

    public WeightedRepBaseExerciseViewModel(WorkoutStep workoutStep, IAppCaching appCaching)
    {
        this._workoutStep = workoutStep;

        this.WorkoutName = this._workoutStep.WorkoutCaption;
        this.RoundInfo = this._workoutStep.RoundInfo;
        this.SetInfo = this._workoutStep.SetInfo;
        this.ExerciseInfo = this._workoutStep.ExerciseInfo;

        this.Reps = string.Format("{0} Reps - {1}Kg",
            ((WeightedRepBaseExerciseWorkoutRound)this._workoutStep.Exercise).NbrReps.ToString(),
            ((WeightedRepBaseExerciseWorkoutRound)this._workoutStep.Exercise).Weight.ToString());

        this.ExerciseCoachNotes = new ObservableCollection<string>(this._workoutStep.CoachNotes);

        var imageForExercise = appCaching.WeightExerciseImages[((WeightedRepBaseExerciseWorkoutRound)workoutStep.Exercise).ExerciseType];
        this.ImageSource = ImageManipulation
            .LoadImageFromAssets(imageForExercise);
    }
}
