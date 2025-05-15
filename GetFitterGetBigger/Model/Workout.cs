namespace GetFitterGetBigger.Model;

public record Workout(
    int WorkoutId,
    string Name,
    string PreparationSteps,
    string CoachNotes,
    WorkoutType WorkoutType,
    WorkoutRounds[] WorkoutRounds);

public record WorkoutRounds(
    int WorkoutSetId,
    int WorkoutId,
    ExerciseWorkoutRound[] ExerciseWorkoutRound,
    int Order
);

public abstract record ExerciseWorkoutRound;

public record RepBaseExerciseWorkoutRound(
    int RepBaseExerciseWorkoutId,
    int WorkoutSetId,
    ExerciseType ExerciseType,
    int NbrReps,
    int Order) : ExerciseWorkoutRound;

public record TimeBaseExerciseWorkoutRound(
    int TimeBaseExerciseWorkoutId,
    int WorkoutSetId,
    ExerciseType ExerciseType,
    int TimeInSeconds,
    int Order) : ExerciseWorkoutRound;

public enum ExerciseType
{
    Rest,
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