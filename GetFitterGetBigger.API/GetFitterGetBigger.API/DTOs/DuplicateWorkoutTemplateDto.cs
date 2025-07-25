namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for duplicating a workout template
/// </summary>
public class DuplicateWorkoutTemplateDto
{
    /// <summary>
    /// The name for the duplicated template
    /// <example>Copy of Upper Body Strength</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 3)]
    public required string NewName { get; init; }
}