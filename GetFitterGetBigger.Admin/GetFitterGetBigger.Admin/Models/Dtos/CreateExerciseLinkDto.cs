using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.Admin.Models.Dtos;

/// <summary>
/// DTO for creating a new exercise link
/// </summary>
public class CreateExerciseLinkDto
{
    /// <summary>
    /// The ID of the exercise to link to (required)
    /// </summary>
    [Required(ErrorMessage = "Target exercise ID is required")]
    public string TargetExerciseId { get; set; } = string.Empty;

    /// <summary>
    /// Type of link - must be either "Warmup" or "Cooldown" (required)
    /// </summary>
    [Required(ErrorMessage = "Link type is required")]
    [RegularExpression("^(Warmup|Cooldown)$", ErrorMessage = "Link type must be either 'Warmup' or 'Cooldown'")]
    public string LinkType { get; set; } = string.Empty;

    /// <summary>
    /// Order in which to display this link (1-based, min: 1) (required)
    /// </summary>
    [Required(ErrorMessage = "Display order is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Display order must be at least 1")]
    public int DisplayOrder { get; set; }
}