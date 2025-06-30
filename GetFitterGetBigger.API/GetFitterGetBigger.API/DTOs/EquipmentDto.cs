using System;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for equipment with full details
/// </summary>
public class EquipmentDto
{
    /// <summary>
    /// The ID of the equipment in the format "equipment-{guid}"
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the equipment
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates whether the equipment is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// The date and time when the equipment was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// The date and time when the equipment was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}