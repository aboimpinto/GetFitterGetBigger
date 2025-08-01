using System;
using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for workout objective
/// </summary>
public record WorkoutObjectiveDto : IEmptyDto<WorkoutObjectiveDto>
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
    
    /// <summary>
    /// Indicates whether this DTO represents an empty/default state
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(WorkoutObjectiveId) || WorkoutObjectiveId == "workoutobjective-00000000-0000-0000-0000-000000000000";
    
    /// <summary>
    /// Gets an empty WorkoutObjectiveDto instance for the Empty Object Pattern
    /// </summary>
    public static WorkoutObjectiveDto Empty => new()
    {
        WorkoutObjectiveId = string.Empty,
        Value = string.Empty,
        Description = null,
        DisplayOrder = 0,
        IsActive = false
    };
}