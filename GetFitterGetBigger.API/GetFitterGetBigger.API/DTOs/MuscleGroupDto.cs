using System;
using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for muscle group with full details
/// </summary>
public class MuscleGroupDto : IEmptyDto<MuscleGroupDto>
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
    
    /// <summary>
    /// Indicates whether this DTO represents an empty instance
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Id) || Id == "musclegroup-00000000-0000-0000-0000-000000000000";
    
    /// <summary>
    /// Returns an empty instance of MuscleGroupDto
    /// </summary>
    public static MuscleGroupDto Empty => new()
    {
        Id = string.Empty,
        Name = string.Empty,
        BodyPartId = string.Empty,
        BodyPartName = null,
        IsActive = false,
        CreatedAt = DateTime.MinValue,
        UpdatedAt = null
    };
}