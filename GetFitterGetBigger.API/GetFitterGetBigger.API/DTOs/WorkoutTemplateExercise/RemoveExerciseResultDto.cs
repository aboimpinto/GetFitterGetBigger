namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;

/// <summary>
/// Result DTO for removing exercises from a workout template
/// </summary>
public record RemoveExerciseResultDto(
    List<WorkoutTemplateExerciseDto> RemovedExercises,
    string Message)
{
    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static RemoveExerciseResultDto Empty => new(new List<WorkoutTemplateExerciseDto>(), string.Empty);
}