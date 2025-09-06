using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for creating a new exercise link (enhanced for four-way linking)
/// </summary>
public class CreateExerciseLinkDto
{
    /// <summary>
    /// The ID of the target exercise to link
    /// </summary>
    [Required(ErrorMessage = "Target exercise ID is required")]
    public string TargetExerciseId { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of link - supports both old string values (Warmup, Cooldown) and new enum values (WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE)
    /// </summary>
    [Required(ErrorMessage = "Link type is required")]
    [RegularExpression("^(Warmup|Cooldown|WARMUP|COOLDOWN|WORKOUT|ALTERNATIVE)$", 
        ErrorMessage = "Link type must be 'Warmup', 'Cooldown', 'WARMUP', 'COOLDOWN', 'WORKOUT', or 'ALTERNATIVE'")]
    public string LinkType { get; set; } = string.Empty;
    
    /// <summary>
    /// The display order for this link (legacy - now calculated server-side for new enum-based API)
    /// Kept for backward compatibility with existing string-based API calls
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Display order must be a non-negative number")]
    public int DisplayOrder { get; set; } = 1;
}