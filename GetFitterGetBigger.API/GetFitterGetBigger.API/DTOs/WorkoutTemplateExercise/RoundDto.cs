namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;

/// <summary>
/// DTO for a round of exercises within a phase
/// </summary>
public record RoundDto(
    int RoundNumber,
    List<WorkoutTemplateExerciseDto> Exercises)
{
    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static RoundDto Empty => new(0, new List<WorkoutTemplateExerciseDto>());
}