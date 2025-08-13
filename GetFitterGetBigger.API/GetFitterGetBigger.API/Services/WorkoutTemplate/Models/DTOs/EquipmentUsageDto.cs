using GetFitterGetBigger.API.DTOs;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Models.DTOs;

/// <summary>
/// Represents equipment usage statistics across workout templates
/// </summary>
public class EquipmentUsageDto
{
    /// <summary>
    /// The equipment information
    /// </summary>
    public EquipmentDto Equipment { get; set; } = new();
    
    /// <summary>
    /// Number of workout templates using this equipment
    /// </summary>
    public int UsageCount { get; set; }
    
    /// <summary>
    /// Percentage of analyzed templates using this equipment
    /// </summary>
    public decimal UsagePercentage { get; set; }
    
    /// <summary>
    /// Indicates if this equipment is essential (used in >50% of templates)
    /// </summary>
    public bool IsEssential => UsagePercentage > 50;
}