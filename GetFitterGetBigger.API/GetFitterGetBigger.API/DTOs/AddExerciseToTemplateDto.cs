namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for adding an exercise to a workout template
/// </summary>
public class AddExerciseToTemplateDto
{
    /// <summary>
    /// The exercise ID to add
    /// <example>exercise-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string ExerciseId { get; init; }

    /// <summary>
    /// The zone to add the exercise to (Warmup, Main, Cooldown)
    /// <example>Main</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string Zone { get; init; }

    /// <summary>
    /// Optional notes for the exercise
    /// <example>Focus on form and control</example>
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Optional sequence order. If not provided, will be added at the end
    /// <example>1</example>
    /// </summary>
    public int? SequenceOrder { get; init; }
}