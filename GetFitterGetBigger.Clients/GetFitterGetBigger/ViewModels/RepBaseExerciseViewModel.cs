using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using GetFitterGetBigger.Model;
using Olimpo.NavigationManager;

namespace GetFitterGetBigger.ViewModels;

public partial class RepBaseExerciseViewModel :
    ViewModelBase
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
    private string _exercisesProgress = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _exerciseCoachNotes = [];

    public RepBaseExerciseViewModel(WorkoutStep workoutStep, IAppCaching appCaching)
    {
        this._workoutStep = workoutStep;

        this.WorkoutName = this._workoutStep.WorkoutCaption;
        this.RoundInfo = this._workoutStep.RoundInfo;
        this.SetInfo = this._workoutStep.SetInfo;
        this.ExerciseInfo = this._workoutStep.ExerciseInfo;

        this.ExerciseCoachNotes = new ObservableCollection<string>(this._workoutStep.CoachNotes);

        this.Reps = string.Format("{0} Reps", ((RepBaseExerciseWorkoutRound)this._workoutStep.Exercise).NbrReps.ToString());

        var imageForExercise = appCaching.ExerciseImages[((RepBaseExerciseWorkoutRound)workoutStep.Exercise).ExerciseType];
        this.ImageSource = ImageManipulation
            .LoadImageFromAssets(imageForExercise);

        this.ExercisesProgress = $"Exercise {workoutStep.ExerciseIndex} of {workoutStep.ExercisesInRound} this round";
    }
}
