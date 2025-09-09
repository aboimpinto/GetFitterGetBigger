namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;

/// <summary>
/// Result DTO for adding exercises to a workout template
/// </summary>
public record AddExerciseResultDto(
    List<WorkoutTemplateExerciseDto> AddedExercises,
    string Message)
{
    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static AddExerciseResultDto Empty => new(new List<WorkoutTemplateExerciseDto>(), string.Empty);
}