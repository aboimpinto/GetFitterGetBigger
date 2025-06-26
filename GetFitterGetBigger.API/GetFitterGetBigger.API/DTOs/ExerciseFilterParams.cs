using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// DTO for exercise filtering parameters
/// </summary>
public class ExerciseFilterParams
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;
    
    /// <summary>
    /// Filter by exercise name (case-insensitive, partial match)
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Filter by difficulty level ID
    /// </summary>
    public string? DifficultyId { get; set; }
    
    /// <summary>
    /// Filter by muscle group IDs (exercises that target any of these muscle groups)
    /// </summary>
    public List<string>? MuscleGroupIds { get; set; }
    
    /// <summary>
    /// Filter by equipment IDs (exercises that use any of this equipment)
    /// </summary>
    public List<string>? EquipmentIds { get; set; }
    
    /// <summary>
    /// Filter by movement pattern IDs (exercises that involve any of these patterns)
    /// </summary>
    public List<string>? MovementPatternIds { get; set; }
    
    /// <summary>
    /// Filter by body part IDs (exercises that involve any of these body parts)
    /// </summary>
    public List<string>? BodyPartIds { get; set; }
    
    /// <summary>
    /// Include inactive exercises in the results
    /// </summary>
    public bool IncludeInactive { get; set; } = false;
}