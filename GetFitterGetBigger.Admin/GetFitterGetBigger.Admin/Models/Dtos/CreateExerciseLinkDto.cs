using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.Admin.Models.Dtos;

/// <summary>
/// DTO for creating a new exercise link
/// </summary>
public class CreateExerciseLinkDto
{
    /// <summary>
    /// The ID of the source exercise (optional - can be inferred from context)
    /// </summary>
    public string? SourceExerciseId { get; set; }

    /// <summary>
    /// The ID of the exercise to link to (required)
    /// </summary>
    [Required(ErrorMessage = "Target exercise ID is required")]
    public string TargetExerciseId { get; set; } = string.Empty;

    /// <summary>
    /// Type of link - must be "Warmup", "Cooldown", or "Alternative" (required)
    /// </summary>
    [Required(ErrorMessage = "Link type is required")]
    [RegularExpression("^(Warmup|Cooldown|Alternative)$", ErrorMessage = "Link type must be 'Warmup', 'Cooldown', or 'Alternative'")]
    public string LinkType { get; set; } = string.Empty;

    /// <summary>
    /// Order in which to display this link (1-based, min: 1)
    /// Required for Warmup and Cooldown links. Ignored for Alternative links (calculated server-side).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Display order must be at least 1")]
    public int? DisplayOrder { get; set; }
}