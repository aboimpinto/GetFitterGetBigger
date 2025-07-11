using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Extensions;

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
    /// Search term for filtering exercises (case-insensitive, partial match)
    /// </summary>
    public string? SearchTerm { get; set; }
    
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
    
    /// <summary>
    /// Filter by active status (null = all, true = active only, false = inactive only)
    /// </summary>
    public bool? IsActive { get; set; }
}

public static class ExerciseFilterParamsExtensions
{
    public static GetExercisesCommand ToCommand(this ExerciseFilterParams filterParams)
    {
        return new GetExercisesCommand(
            Page: filterParams.Page,
            PageSize: filterParams.PageSize,
            Name: filterParams.Name ?? string.Empty,
            SearchTerm: filterParams.SearchTerm ?? string.Empty,
            DifficultyLevelId: DifficultyLevelId.ParseOrEmpty(filterParams.DifficultyId),
            MuscleGroupIds: filterParams.MuscleGroupIds.ParseMuscleGroupIds(),
            EquipmentIds: filterParams.EquipmentIds.ParseEquipmentIds(),
            MovementPatternIds: filterParams.MovementPatternIds.ParseMovementPatternIds(),
            BodyPartIds: filterParams.BodyPartIds.ParseBodyPartIds(),
            IncludeInactive: filterParams.IncludeInactive,
            IsActive: filterParams.IsActive ?? false
        );
    }
}