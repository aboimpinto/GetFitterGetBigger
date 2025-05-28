using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Olimpo;
using Olimpo.NavigationManager;
using GetFitterGetBigger.Events;
using GetFitterGetBigger.Model;

namespace GetFitterGetBigger.ViewModels;

public partial class WorkoutWorkflowViewModel :
    ViewModelBase,
    ILoadableViewModel,
    IHandlesBackButton,
    IHandleAsync<TimedSetFinishedEvent>,
    IHandleAsync<InitialCountDownFinishedEvent>
{
    // Define a type for the intermediate result
    public record RoundExercise(WorkoutRounds Round, ExerciseWorkoutRound Exercise);

    private readonly IAppCaching _appCaching;
    private readonly INavigationManager _navigationManager;
    private readonly IEventAggregator _eventAggregator;
    private int _seconds = 0;
    private int _minutes = 0;
    private int _hour = 0;
    private IDisposable _workoutTimeLoop;
    private Workout _workflow;
    private List<WorkoutStep> _workoutSequence; // Field to store the ordered steps
    private int _workoutId;
    private int _workoutWorkflowStep;
    private int _totalRounds;
    private Dictionary<int, string> _roundExercises;

    [ObservableProperty]
    private string _workoutTime = "Overall: 00:00";

    [ObservableProperty]
    private ViewModelBase _currentWorkoutSet;
    
    [ObservableProperty]
    private string _nextButtonCaption = "Next Exercise ➔";

    [ObservableProperty]
    private bool _isWorkoutFinished;

    [ObservableProperty]
    private bool _isTimeBasedSet = false;

    [ObservableProperty]
    private int _roundProgress = 0;

    [ObservableProperty]
    private string _roundDescription = string.Empty;

    [ObservableProperty]
    private string _roundExerciseSummary = string.Empty;

    [ObservableProperty]
    private string _workoutTitle = string.Empty;

    [ObservableProperty]
    private bool _isNextExerciseVisible = true;

    public WorkoutWorkflowViewModel(
        IAppCaching appCaching,
        INavigationManager navigationManager,
        IEventAggregator eventAggregator)
    {
        this._appCaching = appCaching;
        this._navigationManager = navigationManager;
        this._eventAggregator = eventAggregator;

        this._eventAggregator.Subscribe(this);
    }

    public async Task LoadAsync(IDictionary<string, object>? parameters = null)
    {
        this._workoutId = int.Parse(parameters["WorkoutId"].ToString());

        this.IsWorkoutFinished = false;
        this.IsNextExerciseVisible = false;
        this.WorkoutTime = string.Empty;
        this.RoundExerciseSummary = string.Empty;
        this.RoundDescription = string.Empty;
        this.WorkoutTime = string.Empty;
        this.WorkoutTitle = string.Empty;
        this._workoutWorkflowStep = 0;
        this._totalRounds = 0;
        this._seconds = 0;
        this._minutes = 0;
        this._hour = 0;

        var countdownViewModel = new CountdownViewModel(this._eventAggregator);
        this.CurrentWorkoutSet = countdownViewModel;
        await countdownViewModel.LoadAsync();
    }
    
    [RelayCommand]
    private async Task CancelWorkout()
    {
        this._hour = 0;
        this._minutes = 0;
        this._seconds = 0;
        this._workoutTimeLoop?.Dispose();
        this._currentWorkoutSet = null;
        GC.SuppressFinalize(this); 

        await this._navigationManager.NavigateAsync("WorkoutsViewModel");
    }

    [RelayCommand]
    private async Task NextWorkoutSet()
    {
        if (this.IsWorkoutFinished)
        {
            return;
        }

        if (this.CurrentWorkoutSet is ISkipableExercise skipableViewModel)
        {
            await skipableViewModel.SkipIt();
        }

        this._workoutWorkflowStep++;

        this.NextButtonCaption = (this._workoutWorkflowStep == this._workoutSequence.Count - 1) ?
            "Finish" :
            "Next Exercise ➔";

        if (this._workoutWorkflowStep >= this._workoutSequence.Count)
        {
            // the end of the Workout is reached. Stop the watch
            this.IsNextExerciseVisible = false;
            this._workoutTimeLoop.Dispose();
            this.IsWorkoutFinished = true;

            this.CurrentWorkoutSet = new WorkoutSummaryViewModel(
                this._workflow.Title,
                this.WorkoutTime);
        }
        else
        {
            this.CurrentWorkoutSet = await this.LoadWorkoutSetExercice(this._workoutWorkflowStep);
        }

        this.UpdateRoundProgress();
    }

    [RelayCommand]
    private async Task CloseWorkoutSummary()
    {
        await this._navigationManager.NavigateAsync("DashboardViewModel");
    }

    private void UpdateRoundProgress()
    {
        if (this._workoutWorkflowStep >= this._workoutSequence.Count)
        {
            return;
        }

        decimal roundProgress = (this._workoutSequence[this._workoutWorkflowStep].RoundIndex * 100) / this._totalRounds;
        this.RoundProgress = (int)roundProgress;

        this.RoundDescription = $"Round {this._workoutSequence[this._workoutWorkflowStep].RoundIndex} of {this._totalRounds}";

        this.RoundExerciseSummary = $"This Round: {this._roundExercises[this._workoutSequence[this._workoutWorkflowStep].RoundIndex]}";

        this.WorkoutTitle = this._workoutSequence[this._workoutWorkflowStep].WorkoutCaption;
    }

    private void StartWorkoutTimer()
    {
        this.WorkoutTime = $"Overall: {this._minutes.ToString("00")}:{this._seconds.ToString("00")}";

        var loop = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .ObserveOn(RxApp.MainThreadScheduler);

        this._workoutTimeLoop = loop.Subscribe(x =>
        {
            this._seconds++;

            if (this._seconds > 59)
            {
                this._seconds = 0;
                this._minutes++;
            }

            if (this._minutes > 59)
            {
                this._minutes = 0;
                this._hour++;
            }

            this.WorkoutTime = this._hour switch
            {
                0 => $"Overall: {this._minutes.ToString("00")}:{this._seconds.ToString("00")}",
                _ => $"Overall: {this._hour.ToString("00")}:{this._minutes.ToString("00")}:{this._seconds.ToString("00")}"
            };
        });
    }

    private async Task StartWorkoutWorkflow()
    {
        this._workflow = this._appCaching.Workouts
            .Single(x => x.WorkoutId == this._workoutId);

        this._workoutSequence = [.. this._workflow.WorkoutRounds // Assign the result to the field
            .OrderBy(round => round.Order) // 1. Order rounds (outer sequence)
            .SelectMany(round => { // 2. Process each round to flatten exercises
                var exercisesInThisSpecificRound = round.ExerciseWorkoutRound.Length; // Get count of exercises in this round

                return round.ExerciseWorkoutRound
                    // Order exercises *within this current round* based on their 'Order' property
                    .OrderBy(ex => ex switch
                    {
                        RepBaseExerciseWorkoutRound repEx => repEx.Order,
                        TimeBaseExerciseWorkoutRound timeEx => timeEx.Order,
                        WeightedRepBaseExerciseWorkoutRound weightRepExercice => weightRepExercice.Order,
                        RestExerciseWorkoutRound restEx => restEx.Order, // Added RestExerciseWorkoutRound
                        _ => int.MaxValue // Fallback for unknown types
                    })
                    // Select each exercise along with its 0-based index *within this ordered round*
                    .Select((ex, indexInRound) => new { // 'indexInRound' is the 0-based index within this round's exercises
                        Round = round,                            // Keep the round object
                        Exercise = ex,                          // Keep the exercise object
                        TotalExercisesInThisRound = exercisesInThisSpecificRound, // Store the count of exercises for this round
                        LocalExerciseIndex = indexInRound + 1     // Calculate 1-based exercise index within this round
                    });
            })
            .OrderBy(item => item.Round.Order) // 3. Ensure global order by Round first
            .ThenBy(item => item.Exercise switch // 4. Then ensure global order by Exercise Order within the round
            {
                RepBaseExerciseWorkoutRound repEx => repEx.Order,
                TimeBaseExerciseWorkoutRound timeEx => timeEx.Order,
                WeightedRepBaseExerciseWorkoutRound weightRepExercice => weightRepExercice.Order,
                RestExerciseWorkoutRound restEx => restEx.Order, // Added RestExerciseWorkoutRound
                _ => int.MaxValue
            })
            .Select((item, globalIndex) => new WorkoutStep( // 5. Project using the WorkoutStep constructor
                WorkoutCaption: this._workflow.Title,
                RoundIndex: item.Round.Order, // Use the round's order from the original WorkoutRounds
                RoundInfo: $"Round {item.Round.Order}",
                ExercisesInRound: item.TotalExercisesInThisRound, // Use the calculated total exercises for this round
                ExerciseIndex: item.LocalExerciseIndex, // Use the calculated 1-based index of the exercise within its round
                SetInfo: item.Exercise switch
                {
                    // globalIndex + 1 gives 1-based set number (overall exercise sequence in the entire workout)
                    RepBaseExerciseWorkoutRound repEx => $"Set {globalIndex + 1}",
                    TimeBaseExerciseWorkoutRound timeEx => $"Set {globalIndex + 1}",
                    WeightedRepBaseExerciseWorkoutRound weightRepExercice => $"Set {globalIndex + 1}",
                    RestExerciseWorkoutRound restEx => $"Set {globalIndex + 1}", // Added RestExerciseWorkoutRound
                    _ => $"Set {globalIndex + 1} | Unknown Exercise"
                },
                ExerciseInfo: item.Exercise switch
                {
                    RepBaseExerciseWorkoutRound repEx => $"{repEx.ExerciseType}",
                    TimeBaseExerciseWorkoutRound timeEx => $"{timeEx.ExerciseType}", // Assuming TimeBaseExerciseWorkoutRound has an ExerciseType for its info
                    WeightedRepBaseExerciseWorkoutRound weightRepExercice => $"{weightRepExercice.ExerciseType}",
                    RestExerciseWorkoutRound _ => "REST", // Added RestExerciseWorkoutRound, specific output "REST"
                    _ => $"Unknown Exercise"
                },
                Round: item.Round, // Pass the original round object
                Exercise: item.Exercise, // Pass the exercise object
                CoachNotes: item.Exercise switch // Populate CoachNotes based on the specific exercise type
                {
                    RepBaseExerciseWorkoutRound repEx => repEx.CoachNotes,
                    WeightedRepBaseExerciseWorkoutRound weightedRepEx => weightedRepEx.CoachNotes,
                    TimeBaseExerciseWorkoutRound timeEx => timeEx.CoacheNotes, // Using "CoacheNotes" as per your model
                    RestExerciseWorkoutRound restEx => restEx.CoacheNotes, // Added RestExerciseWorkoutRound, using "CoacheNotes"
                    _ => [] // Default to an empty array
                }
            ))];

        this._workoutWorkflowStep = 0;

        // 1. Get the number of rounds
        this._totalRounds = this._workflow.WorkoutRounds.Length;

        // 2. Create a dictionary of exercises for each round
        this._roundExercises = this._workflow.WorkoutRounds
            .ToDictionary(
                round => round.Order,
                round => string.Join(", ", round.ExerciseWorkoutRound.Select<ExerciseWorkoutRound, string>(ex => ex switch
                {
                    RepBaseExerciseWorkoutRound repEx => repEx.ExerciseType.ToString(),
                    TimeBaseExerciseWorkoutRound timeEx => timeEx.ExerciseType.ToString(),
                    WeightedRepBaseExerciseWorkoutRound weightRepEx => weightRepEx.ExerciseType.ToString(),
                    _ => "Unknown Exercise"
                }))
            );
    }

    private async Task<ViewModelBase> LoadWorkoutSetExercice(int index)
    {
        var workoutStep = this._workoutSequence[index];

        // Explicitly type the variable before the target-typed switch expression
        ViewModelBase viewModelBase = workoutStep.Exercise switch
        {
            RepBaseExerciseWorkoutRound => new RepBaseExerciseViewModel(workoutStep, this._appCaching),
            RestExerciseWorkoutRound => new RestViewModel(workoutStep, this._eventAggregator),
            TimeBaseExerciseWorkoutRound => new TimeBaseExerciseViewModel(workoutStep, this._eventAggregator),
            WeightedRepBaseExerciseWorkoutRound => new WeightedRepBaseExerciseViewModel(workoutStep, this._appCaching),
            _ => throw new InvalidOperationException()
        };

        this.IsTimeBasedSet =
            viewModelBase is TimeBaseExerciseViewModel ||
             viewModelBase is RestViewModel;

        if (viewModelBase is ILoadableViewModel model)
        {
            await model.LoadAsync();
        }

        return viewModelBase;
    }

    public async Task HandleAsync(InitialCountDownFinishedEvent message)
    {
        this.IsWorkoutFinished = false;
        this.StartWorkoutTimer();

        await this.StartWorkoutWorkflow();
        this.UpdateRoundProgress();

        if (this._workoutSequence.Count > 0)
        {
            await this.LoadWorkoutSetExercice(0);
        }

        this.IsNextExerciseVisible = true;
        this.CurrentWorkoutSet = await this.LoadWorkoutSetExercice(this._workoutWorkflowStep);
    }

    public async Task HandleAsync(TimedSetFinishedEvent message)
    {
        if (this.IsWorkoutFinished)
        {
            return;
        }

        this._workoutWorkflowStep++;

        this.NextButtonCaption = (this._workoutWorkflowStep == this._workoutSequence.Count - 1) ?
            "Finish" :
            "Next Exercise ➔";

        if (this._workoutWorkflowStep >= this._workoutSequence.Count)
        {
            // the end of the Workout is reached. Stop the watch
            this.IsNextExerciseVisible = false;
            this._workoutTimeLoop.Dispose();
            this.IsWorkoutFinished = true;

            this.CurrentWorkoutSet = new WorkoutSummaryViewModel(
                this._workflow.Title,
                this.WorkoutTime);
        }
        else
        {
            this.LoadNextWorkoutSet();
        }

        this.IsTimeBasedSet = false;
        this.UpdateRoundProgress();
    }

    private async void LoadNextWorkoutSet()
    {
        this.CurrentWorkoutSet = await this.LoadWorkoutSetExercice(this._workoutWorkflowStep);
    }

    public async Task OnBackButtonPressedAsync()
    {
        await this._navigationManager.GoBackAsync();
    }
}

// Define a record to hold the structured workout step information
public record WorkoutStep(
    string WorkoutCaption,
    int RoundIndex,
    string RoundInfo,
    string SetInfo,
    string ExerciseInfo,
    int ExercisesInRound,
    int ExerciseIndex,
    WorkoutRounds Round,
    ExerciseWorkoutRound Exercise,
    string[] CoachNotes);
