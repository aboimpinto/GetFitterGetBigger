namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for updating an exercise in a workout template
/// </summary>
public class UpdateTemplateExerciseDto
{
    /// <summary>
    /// Updated notes for the exercise
    /// <example>Updated: Focus on proper breathing technique</example>
    /// </summary>
    public string? Notes { get; init; }
}