namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;

/// <summary>
/// Result DTO for updating exercise metadata
/// </summary>
public record UpdateMetadataResultDto(
    WorkoutTemplateExerciseDto UpdatedExercise,
    string Message)
{
    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static UpdateMetadataResultDto Empty => new(WorkoutTemplateExerciseDto.Empty, string.Empty);
}