using System.Collections.Generic;
using GetFitterGetBigger.Model;

namespace GetFitterGetBigger;

public interface IAppCaching
{
    IDictionary<ExerciseType, string> ExerciseImages { get; }

    IDictionary<WeightExerciseType, string> WeightExerciseImages { get; }

    IList<Workout> Workouts { get; }

    Workout WorkoutOfTheDay { get; set; }

    string ActivePlan { get; set; }

    Workout ActivePlanWorkout { get; set; }
}
