namespace GetFitterGetBigger.Model;

public record Workout(
    int WorkoutId,
    string Name,
    string PreparationSteps,
    string CoachNotes,
    WorkoutType WorkoutType,
    WorkoutSet[] WorkoutSets);

public record WorkoutSet(
    int WorkoutSetId,
    int WorkoutId,
    ExerciseWorkoutSet[] ExerciseWorkoutSets,
    int Order
);

public record ExerciseWorkoutSet;

public record RepBaseExerciseWorkoutSet(
    int WorkoutSetId,
    ExerciseType ExerciseType,
    int NbrReps,
    int Order) : ExerciseWorkoutSet;

public record TimeBaseExerciseWorkoutSet(
    int WorkoutSetId,
    ExerciseType ExerciseType,
    int TimeInSeconds,
    int Order) : ExerciseWorkoutSet;

public enum ExerciseType
{
    Pushups,
    Crunches,
    Squats
}

public enum WorkoutType
{
    HIIT,
    UpperBody,
    LowerBody,
    Arms,
    Abs,
    Tabata,
    Calistenics
}