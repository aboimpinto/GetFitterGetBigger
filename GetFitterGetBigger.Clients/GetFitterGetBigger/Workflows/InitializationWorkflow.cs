using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using Olimpo;
using Olimpo.NavigationManager;
using GetFitterGetBigger.Events;
using GetFitterGetBigger.Model;
using static Olimpo.NavigationManager.NavigationManager;

namespace GetFitterGetBigger.Workflows;

public class InitializationWorkflow :
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
            await this._navigationManager.NavigateAsync(new NavigationOptions("SplashViewModel", false));
        });

        this.BuildExercisesImagesList();

        var pushupsCrunchesAndSquatsWorkout = BuildPushupsCrunchesAndSquatsWorkout();
        this._appCaching.Workouts.Add(pushupsCrunchesAndSquatsWorkout);

        var legBurningOne = BuildLegsOneWorkout();
        this._appCaching.Workouts.Add(legBurningOne);

        this._appCaching.WorkoutOfTheDay = pushupsCrunchesAndSquatsWorkout;
        this._appCaching.ActivePlan = "Get Stronger Plan";
        this._appCaching.ActivePlanWorkout = legBurningOne;

        await Task.Delay(TimeSpan.FromSeconds(1));

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await this._navigationManager.NavigateAsync("DashboardViewModel");
        });
    }

    private void BuildExercisesImagesList()
    {
        this._appCaching.ExerciseImages.Add(ExerciseType.AirSquats, "/Assets/AirSquat.jpeg");
        this._appCaching.ExerciseImages.Add(ExerciseType.Pushups, "/Assets/Pushup.jpeg");
        this._appCaching.ExerciseImages.Add(ExerciseType.Crunches, "/Assets/Crunch.jpeg");
        this._appCaching.ExerciseImages.Add(ExerciseType.WalkingLudges, "/Assets/WalkingLudges.jpeg");
        this._appCaching.WeightExerciseImages.Add(WeightExerciseType.Squats, "/Assets/BarbellSquat.jpeg");
        this._appCaching.WeightExerciseImages.Add(WeightExerciseType.BulgarianSplitSquat, "/Assets/BulgarianSplitSquats.jpg");
    }

    public static Workout BuildPushupsCrunchesAndSquatsWorkout()
    {
        var roundOneExerciseScheme = new List<ExerciseWorkoutRound>
        {
            new RepBaseExerciseWorkoutRound(1, 1, ExerciseType.Pushups, 10, 1, ["Body Straight Line", "Elbows Track Back", "Chest Towards Floor"]),
            new RepBaseExerciseWorkoutRound(2, 1, ExerciseType.Crunches, 10, 2, ["Lift With Abs, Not Neck", "Shoulder Blades Off Floor", "Control The Movement"]),
            new RepBaseExerciseWorkoutRound(3, 1, ExerciseType.AirSquats, 10, 3, ["Keep chest up", "Weight in heels", "Go below parallel"]),
            new RestExerciseWorkoutRound(4, 1, 5, 4, [])
        };

        var roundTwoExerciseScheme = new List<ExerciseWorkoutRound>
        {
            new RepBaseExerciseWorkoutRound(4, 2, ExerciseType.Pushups, 10, 1, ["Body Straight Line", "Elbows Track Back", "Chest Towards Floor"]),
            new RepBaseExerciseWorkoutRound(5, 2, ExerciseType.Crunches, 10, 2, ["Lift With Abs, Not Neck", "Shoulder Blades Off Floor", "Control The Movement"]),
            new RepBaseExerciseWorkoutRound(6, 2, ExerciseType.AirSquats, 10, 3, ["Keep chest up", "Weight in heels", "Go below parallel"]),
            new RestExerciseWorkoutRound(7, 2, 5, 4, [])
        };

        var roundThreeExerciseScheme = new List<ExerciseWorkoutRound>
        {
            new RepBaseExerciseWorkoutRound(8, 3, ExerciseType.Pushups, 10, 1, ["Body Straight Line", "Elbows Track Back", "Chest Towards Floor"]),
            new RepBaseExerciseWorkoutRound(9, 3, ExerciseType.Crunches, 10, 2, ["Lift With Abs, Not Neck", "Shoulder Blades Off Floor", "Control The Movement"]),
            new RepBaseExerciseWorkoutRound(10, 3, ExerciseType.AirSquats, 10, 3, ["Keep chest up", "Weight in heels", "Go below parallel"]),
            new RestExerciseWorkoutRound(11, 3, 5, 4, [])
        };

        var roundScheme = new List<WorkoutRounds>
        {
            new(1, 1, [.. roundOneExerciseScheme], 1),
            new(2, 1, [.. roundTwoExerciseScheme], 2),
            new(3, 1, [.. roundThreeExerciseScheme], 3),

        };

        return new Workout(
            1,
            "Full Body Blitz",
            "Pushups / Crunches / Squats",
            "A quick and effective routine to energize your day. Ready to go!",
            "No equipment need",
            ["Rest 2 minutes between rounds.", "Control the negative movement.", "Stay hydrated! Drink water before, during, and after."],
            WorkoutType.HIIT,
            [.. roundScheme]);
    }

    private static Workout BuildLegsOneWorkout()
    {
        var roundOneExerciseScheme = new List<ExerciseWorkoutRound>
        {
            new WeightedRepBaseExerciseWorkoutRound(1, 1, WeightExerciseType.Squats, 10, WeightedEquipment.Barbell, "40", 1, ["Brace Your Core", "Spread the Floor With Your Feet", "Hips and Chest Rise Together"]),
            new RestExerciseWorkoutRound(2, 1, 5, 2, ["Add 10kg place on each side."]),
            new WeightedRepBaseExerciseWorkoutRound(3, 1, WeightExerciseType.Squats, 10, WeightedEquipment.Barbell, "60", 3, ["Brace Your Core", "Spread the Floor With Your Feet", "Hips and Chest Rise Together"]),
            new RestExerciseWorkoutRound(4, 1, 5, 4, ["Add 10kg place on each side."]),
            new WeightedRepBaseExerciseWorkoutRound(5, 1, WeightExerciseType.Squats, 10, WeightedEquipment.Barbell, "80", 5, ["Brace Your Core", "Spread the Floor With Your Feet", "Hips and Chest Rise Together"]),
            new RestExerciseWorkoutRound(6, 1, 5, 6, ["Add 10kg place on each side."])
        };

        var roundTwoExerciseScheme = new List<ExerciseWorkoutRound>
        {
            new WeightedRepBaseExerciseWorkoutRound(7, 2, WeightExerciseType.BulgarianSplitSquat, 20, WeightedEquipment.Dumbbell, "10", 1, ["10 reps each leg"]),
            new RestExerciseWorkoutRound(8, 2, 5, 2, []),
            new WeightedRepBaseExerciseWorkoutRound(9, 2, WeightExerciseType.BulgarianSplitSquat, 20, WeightedEquipment.Dumbbell, "10", 3, ["10 reps each leg"]),
            new RestExerciseWorkoutRound(10, 2, 5, 4, []),
            new WeightedRepBaseExerciseWorkoutRound(11, 2, WeightExerciseType.BulgarianSplitSquat, 20, WeightedEquipment.Dumbbell, "10", 5, ["10 reps each leg"]),
            new RestExerciseWorkoutRound(12, 2, 5, 6, [])
        };

        var roundThreeExerciseScheme = new List<ExerciseWorkoutRound>
        {
            new RepBaseExerciseWorkoutRound(8, 3, ExerciseType.WalkingLudges, 50, 1, []),
        };

        var roundScheme = new List<WorkoutRounds>
        {
            new(1, 1, [.. roundOneExerciseScheme], 1),
            new(2, 1, [.. roundTwoExerciseScheme], 2),
            new(3, 1, [.. roundThreeExerciseScheme], 3),
        };

        return new Workout(
            2,
            "Leg burner I",
            "Focus: Quads, Hamstrings, Glutes",
            "Part of your 8-week strength building program. Keep up the great work!",
            "Barbell, Weights and Dumbbells",
            ["Rest 2 minutes between rounds.", "Control the negative movement.", "Stay hydrated! Drink water before, during, and after."],
            WorkoutType.LowerBody,
            [.. roundScheme]);
    }
}
