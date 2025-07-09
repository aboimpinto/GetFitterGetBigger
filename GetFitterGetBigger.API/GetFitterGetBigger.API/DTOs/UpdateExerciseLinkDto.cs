using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for updating an existing exercise link
/// </summary>
public class UpdateExerciseLinkDto
{
    /// <summary>
    /// The display order for this link (must be non-negative)
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Display order must be a non-negative number")]
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Whether the link is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}