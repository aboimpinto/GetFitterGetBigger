using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;

/// <summary>
/// DTO for adding an exercise to a workout template
/// </summary>
public record AddExerciseDto(
    ExerciseId ExerciseId,
    string Phase,
    int RoundNumber,
    string Metadata)
{
    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static AddExerciseDto Empty => new(ExerciseId.Empty, string.Empty, 0, string.Empty);
}