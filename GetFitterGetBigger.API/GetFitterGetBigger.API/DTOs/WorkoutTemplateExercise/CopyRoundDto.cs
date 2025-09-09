namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;

/// <summary>
/// DTO for copying a round of exercises
/// </summary>
public record CopyRoundDto(
    string SourcePhase,
    int SourceRoundNumber,
    string TargetPhase,
    int TargetRoundNumber)
{
    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static CopyRoundDto Empty => new(string.Empty, 0, string.Empty, 0);
}