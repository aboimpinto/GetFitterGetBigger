using System;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for workout objective
/// </summary>
public record WorkoutObjectiveDto
{
    /// <summary>
    /// The ID of the workout objective in the format "workoutobjective-{guid}"
    /// </summary>
    /// <example>workoutobjective-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a</example>
    public string WorkoutObjectiveId { get; init; } = string.Empty;
    
    /// <summary>
    /// The value of the workout objective
    /// </summary>
    /// <example>Muscular Strength</example>
    public string Value { get; init; } = string.Empty;
    
    /// <summary>
    /// The description of the workout objective
    /// </summary>
    /// <example>Focus on building maximum strength and power</example>
    public string? Description { get; init; }
    
    /// <summary>
    /// The display order for sorting
    /// </summary>
    /// <example>1</example>
    public int DisplayOrder { get; init; }
    
    /// <summary>
    /// Indicates whether the workout objective is active
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; init; }
}