using System;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for muscle group with full details
/// </summary>
public class MuscleGroupDto
{
    /// <summary>
    /// The ID of the muscle group in the format "musclegroup-{guid}"
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the muscle group
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The ID of the body part this muscle group belongs to
    /// </summary>
    public string BodyPartId { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the body part this muscle group belongs to
    /// </summary>
    public string? BodyPartName { get; set; }
    
    /// <summary>
    /// Indicates whether the muscle group is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// The date and time when the muscle group was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// The date and time when the muscle group was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}