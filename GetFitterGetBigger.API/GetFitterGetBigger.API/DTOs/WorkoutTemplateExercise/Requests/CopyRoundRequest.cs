using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Requests;

/// <summary>
/// Request model for copying a round with all exercises and generating new GUIDs
/// </summary>
public record CopyRoundRequest
{
    /// <summary>
    /// The source phase containing the round to copy
    /// </summary>
    /// <example>Workout</example>
    [Required]
    public required string SourcePhase { get; init; }

    /// <summary>
    /// The source round number to copy
    /// </summary>
    /// <example>1</example>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Source round number must be positive")]
    public int SourceRoundNumber { get; init; }

    /// <summary>
    /// The target phase where the round will be copied
    /// </summary>
    /// <example>Workout</example>
    [Required]
    public required string TargetPhase { get; init; }

    /// <summary>
    /// The target round number for the copied round
    /// </summary>
    /// <example>2</example>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Target round number must be positive")]
    public int TargetRoundNumber { get; init; }
}