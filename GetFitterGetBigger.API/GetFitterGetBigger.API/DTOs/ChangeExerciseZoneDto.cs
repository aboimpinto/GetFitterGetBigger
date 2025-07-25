namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for changing an exercise zone
/// </summary>
public class ChangeExerciseZoneDto
{
    /// <summary>
    /// The new zone for the exercise (Warmup, Main, Cooldown)
    /// <example>Cooldown</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string Zone { get; init; }

    /// <summary>
    /// The sequence order within the new zone
    /// <example>1</example>
    /// </summary>
    public int? SequenceOrder { get; init; }
}