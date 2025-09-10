using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Requests;

/// <summary>
/// Request model for reordering an exercise within its round
/// </summary>
public record ReorderExerciseRequest
{
    /// <summary>
    /// The new position within the round (1-based)
    /// </summary>
    /// <example>1</example>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Order must be positive")]
    public int NewOrderInRound { get; init; }
}