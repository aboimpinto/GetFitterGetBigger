namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for reordering exercises within a zone
/// </summary>
public class ReorderTemplateExercisesDto
{
    /// <summary>
    /// The zone to reorder exercises in
    /// <example>Main</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string Zone { get; init; }

    /// <summary>
    /// List of exercise orders
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required List<ExerciseOrderDto> ExerciseOrders { get; init; }
}