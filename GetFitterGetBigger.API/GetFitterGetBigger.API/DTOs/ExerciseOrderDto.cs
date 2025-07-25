namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for exercise order information
/// </summary>
public class ExerciseOrderDto
{
    /// <summary>
    /// The workout template exercise ID
    /// <example>workouttemplateexercise-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string ExerciseId { get; init; }

    /// <summary>
    /// The new sequence order
    /// <example>1</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required int SequenceOrder { get; init; }
}