namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;

/// <summary>
/// Result DTO for copying a round of exercises
/// </summary>
public record CopyRoundResultDto(
    List<WorkoutTemplateExerciseDto> CopiedExercises,
    string Message)
{
    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static CopyRoundResultDto Empty => new(new List<WorkoutTemplateExerciseDto>(), string.Empty);
}