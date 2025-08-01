using System;
using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for workout category
/// </summary>
public record WorkoutCategoryDto : IEmptyDto<WorkoutCategoryDto>
{
    /// <summary>
    /// The ID of the workout category in the format "workoutcategory-{guid}"
    /// </summary>
    /// <example>workoutcategory-a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a</example>
    public string WorkoutCategoryId { get; init; } = string.Empty;
    
    /// <summary>
    /// The value of the workout category
    /// </summary>
    /// <example>Strength Training</example>
    public string Value { get; init; } = string.Empty;
    
    /// <summary>
    /// The description of the workout category
    /// </summary>
    /// <example>Workouts focused on building muscle strength and power</example>
    public string? Description { get; init; }
    
    /// <summary>
    /// The icon identifier for the workout category
    /// </summary>
    /// <example>💪</example>
    public string Icon { get; init; } = string.Empty;
    
    /// <summary>
    /// The color code for the workout category in hex format
    /// </summary>
    /// <example>#FF5722</example>
    public string Color { get; init; } = string.Empty;
    
    /// <summary>
    /// Comma-separated list of primary muscle groups for this category
    /// </summary>
    /// <example>Chest,Shoulders,Triceps</example>
    public string? PrimaryMuscleGroups { get; init; }
    
    /// <summary>
    /// The display order for sorting
    /// </summary>
    /// <example>1</example>
    public int DisplayOrder { get; init; }
    
    /// <summary>
    /// Indicates whether the workout category is active
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; init; }
    
    /// <summary>
    /// Indicates whether this DTO represents an empty/default state
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(WorkoutCategoryId) || WorkoutCategoryId == "workoutcategory-00000000-0000-0000-0000-000000000000";
    
    /// <summary>
    /// Gets an empty WorkoutCategoryDto instance for the Empty Object Pattern
    /// </summary>
    public static WorkoutCategoryDto Empty => new()
    {
        WorkoutCategoryId = string.Empty,
        Value = string.Empty,
        Description = null,
        Icon = string.Empty,
        Color = string.Empty,
        PrimaryMuscleGroups = null,
        DisplayOrder = 0,
        IsActive = false
    };
}