using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using Olimpo;
using Olimpo.NavigationManager;
using GetFitterGetBigger.Events;
using GetFitterGetBigger.Model;

namespace GetFitterGetBigger.Workflows;

public class InitializationWorkflow:
    IInitializationWorkflow,
    IHandleAsync<StartBootstrappingEvent>
{
    private readonly IAppCaching _appCaching;
    private readonly INavigationManager _navigationManager;

    public InitializationWorkflow(
        IAppCaching appCaching,
        INavigationManager navigationManager,
        IEventAggregator eventAggregator)
    {
        this._appCaching = appCaching;
        this._navigationManager = navigationManager;

        eventAggregator.Subscribe(this);
    }

    public async Task HandleAsync(StartBootstrappingEvent message)
    {
        await Dispatcher.UIThread.InvokeAsync(async () => 
        {
            await this._navigationManager.NavigateAsync("SplashViewModel");
        });

        var pushupsCrunchesAndSquatsWorkout = BuildPushupsCrunchesAndSquatsWorkout();
        this._appCaching.Workouts.Add(pushupsCrunchesAndSquatsWorkout);
        this._appCaching.WorkoutOfTheDay = pushupsCrunchesAndSquatsWorkout;

        await Task.Delay(TimeSpan.FromSeconds(1));

        await Dispatcher.UIThread.InvokeAsync(async () => 
        {
            await this._navigationManager.NavigateAsync("DashboardViewModel");
        });
    }

    public static Workout BuildPushupsCrunchesAndSquatsWorkout()
    {
        var roundOneExerciseScheme = new List<ExerciseWorkoutRound>
        {
            new RepBaseExerciseWorkoutRound(1, 1, ExerciseType.Pushups, 10, 1),
            new RepBaseExerciseWorkoutRound(2, 1, ExerciseType.Crunches, 10, 2),
            new RepBaseExerciseWorkoutRound(3, 1, ExerciseType.Squats, 10, 3),
            new TimeBaseExerciseWorkoutRound(4, 1, ExerciseType.Rest, 5, 4)
        };

        var roundTwoExerciseScheme = new List<ExerciseWorkoutRound>
        {
            new RepBaseExerciseWorkoutRound(4, 2, ExerciseType.Pushups, 10, 1),
            new RepBaseExerciseWorkoutRound(5, 2, ExerciseType.Crunches, 10, 2),
            new RepBaseExerciseWorkoutRound(6, 2, ExerciseType.Squats, 10, 3),
            new TimeBaseExerciseWorkoutRound(7, 2, ExerciseType.Rest, 5, 4)
        };

        var roundThreeExerciseScheme = new List<ExerciseWorkoutRound>
        {
            new RepBaseExerciseWorkoutRound(8, 3, ExerciseType.Pushups, 10, 1),
            new RepBaseExerciseWorkoutRound(9, 3, ExerciseType.Crunches, 10, 2),
            new RepBaseExerciseWorkoutRound(10, 3, ExerciseType.Squats, 10, 3),
            new TimeBaseExerciseWorkoutRound(11, 3, ExerciseType.Rest, 5, 4)
        };

        var roundScheme = new List<WorkoutRounds>
        {
            new(1, 1, [.. roundOneExerciseScheme], 1),
            new(2, 1, [.. roundTwoExerciseScheme], 2),
            new(3, 1, [.. roundThreeExerciseScheme], 3),

        };

        return new Workout(
            1, 
            "Pushups / Crunches / Squats",
            "No equipment need",
            "* Rest 2 minutes between sets\n* Keep good form and full range of motion",
            WorkoutType.HIIT, 
            [.. roundScheme]);
    }
}
