using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for creating a new muscle group
/// </summary>
public class CreateMuscleGroupDto
{
    /// <summary>
    /// The name of the muscle group
    /// </summary>
    [Required(ErrorMessage = "Muscle group name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Muscle group name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The ID of the body part this muscle group belongs to
    /// </summary>
    [Required(ErrorMessage = "Body part ID is required")]
    [RegularExpression(@"^bodypart-[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", 
        ErrorMessage = "Body part ID must be in the format 'bodypart-{guid}'")]
    public string BodyPartId { get; set; } = string.Empty;
}