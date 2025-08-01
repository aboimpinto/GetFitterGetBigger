using System;
using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for equipment with full details
/// </summary>
public class EquipmentDto : IEmptyDto<EquipmentDto>
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

    /// <summary>
    /// Gets a value indicating whether this instance represents an empty DTO.
    /// An equipment is considered empty when its ID is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Id);

    /// <summary>
    /// Gets a static empty instance of EquipmentDto.
    /// Used as a default return value for validation failures and not-found scenarios.
    /// </summary>
    public static EquipmentDto Empty => new()
    {
        Id = string.Empty,
        Name = string.Empty,
        IsActive = false,
        CreatedAt = DateTime.MinValue,
        UpdatedAt = null
    };
}