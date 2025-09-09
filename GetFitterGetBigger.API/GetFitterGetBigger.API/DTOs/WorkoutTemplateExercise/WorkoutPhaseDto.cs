namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;

/// <summary>
/// DTO for a workout phase containing multiple rounds
/// </summary>
public record WorkoutPhaseDto(
    List<RoundDto> Rounds)
{
    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static WorkoutPhaseDto Empty => new(new List<RoundDto>());
}