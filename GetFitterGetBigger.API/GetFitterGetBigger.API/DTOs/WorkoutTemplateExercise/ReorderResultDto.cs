namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;

/// <summary>
/// Result DTO for reordering exercises within a round
/// </summary>
public record ReorderResultDto(
    List<WorkoutTemplateExerciseDto> ReorderedExercises,
    string Message)
{
    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static ReorderResultDto Empty => new(new List<WorkoutTemplateExerciseDto>(), string.Empty);
}