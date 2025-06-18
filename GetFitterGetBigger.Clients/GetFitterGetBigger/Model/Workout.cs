namespace GetFitterGetBigger.Model;


public record Workout(
    int WorkoutId,
    string Title,
    string SubTitle,
    string Description,
    string PreparationSteps,
    string[] CoachNotes,
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
    int Order,
    string[] CoachNotes) : ExerciseWorkoutRound;

public record WeightedRepBaseExerciseWorkoutRound(
    int RepBaseExerciseWorkoutId,
    int WorkoutSetId,
    WeightExerciseType ExerciseType,
    int NbrReps,
    WeightedEquipment Equipment,
    string Weight,
    int Order,
    string[] CoachNotes) : ExerciseWorkoutRound;

public record TimeBaseExerciseWorkoutRound(
    int TimeBaseExerciseWorkoutId,
    int WorkoutSetId,
    ExerciseType ExerciseType,
    int TimeInSeconds,
    int Order,
    string[] CoacheNotes) : ExerciseWorkoutRound;

public record RestExerciseWorkoutRound(
    int TimeBaseExerciseWorkoutId,
    int WorkoutSetId,
    int TimeInSeconds,
    int Order,
    string[] CoacheNotes) : ExerciseWorkoutRound;

public enum ExerciseType
{
    Rest,
    Pushups,
    Crunches,
    AirSquats,
    WalkingLudges
}

public enum WeightExerciseType
{
    Squats,
    BulgarianSplitSquat,
    DeadLift,
    SumoDeadLift,
    Ludges
}

public enum WeightedEquipment
{
    Barbell,
    Dumbbell,
}

public enum NonWeightedEquipment
{
    JumpRope
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