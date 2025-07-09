using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for creating a new exercise link
/// </summary>
public class CreateExerciseLinkDto
{
    /// <summary>
    /// The ID of the target exercise to link
    /// </summary>
    [Required(ErrorMessage = "Target exercise ID is required")]
    public string TargetExerciseId { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of link (Warmup or Cooldown)
    /// </summary>
    [Required(ErrorMessage = "Link type is required")]
    [RegularExpression("^(Warmup|Cooldown)$", ErrorMessage = "Link type must be either 'Warmup' or 'Cooldown'")]
    public string LinkType { get; set; } = string.Empty;
    
    /// <summary>
    /// The display order for this link (must be non-negative)
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Display order must be a non-negative number")]
    public int DisplayOrder { get; set; }
}