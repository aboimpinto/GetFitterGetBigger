using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.Admin.Models.Dtos;

/// <summary>
/// DTO for updating an existing exercise link
/// </summary>
public class UpdateExerciseLinkDto
{
    /// <summary>
    /// New display order (min: 1) (required)
    /// </summary>
    [Required(ErrorMessage = "Display order is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Display order must be at least 1")]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether the link is active (required)
    /// </summary>
    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }
}