using System.Collections.Generic;
using GetFitterGetBigger.Model;

namespace GetFitterGetBigger;

public class AppCaching : IAppCaching
{
    public IDictionary<ExerciseType, string> ExerciseImages { get; } = new Dictionary<ExerciseType, string>();

    public IDictionary<WeightExerciseType, string> WeightExerciseImages { get; } = new Dictionary<WeightExerciseType, string>();

    public IList<Workout> Workouts { get; } = [];

    public Workout WorkoutOfTheDay { get; set; }

    public string ActivePlan { get; set; } = string.Empty;

    public Workout ActivePlanWorkout { get; set; }
}