using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service for managing workout categories reference data
/// </summary>
public interface IWorkoutCategoryService
{
    /// <summary>
    /// Get all active workout categories
    /// </summary>
    /// <returns>Collection of active workout categories</returns>
    Task<IEnumerable<WorkoutCategory>> GetAllAsync();
    
    /// <summary>
    /// Get all active workout categories as DTOs
    /// </summary>
    /// <returns>Collection of workout category DTOs</returns>
    Task<IEnumerable<WorkoutCategoryDto>> GetAllAsDtosAsync();
    
    /// <summary>
    /// Get all workout categories as WorkoutCategoryDto (with option to include inactive)
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive categories</param>
    /// <returns>Collection of workout category DTOs</returns>
    Task<IEnumerable<WorkoutCategoryDto>> GetAllAsWorkoutCategoryDtosAsync(bool includeInactive = false);
    
    /// <summary>
    /// Get workout category by ID
    /// </summary>
    /// <param name="id">The workout category ID</param>
    /// <returns>The workout category or null if not found</returns>
    Task<WorkoutCategory?> GetByIdAsync(WorkoutCategoryId id);
    
    /// <summary>
    /// Get workout category by ID as DTO
    /// </summary>
    /// <param name="id">The workout category ID as string</param>
    /// <returns>The workout category DTO or null if not found</returns>
    Task<WorkoutCategoryDto?> GetByIdAsDtoAsync(string id);
    
    /// <summary>
    /// Get workout category by ID as WorkoutCategoryDto (with option to include inactive)
    /// </summary>
    /// <param name="id">The workout category ID as string</param>
    /// <param name="includeInactive">Whether to include inactive categories</param>
    /// <returns>The workout category DTO or null if not found</returns>
    Task<WorkoutCategoryDto?> GetByIdAsWorkoutCategoryDtoAsync(string id, bool includeInactive = false);
    
    /// <summary>
    /// Get workout category by value
    /// </summary>
    /// <param name="value">The workout category value (case insensitive)</param>
    /// <returns>The workout category or null if not found</returns>
    Task<WorkoutCategory?> GetByValueAsync(string value);
    
    /// <summary>
    /// Check if workout category exists
    /// </summary>
    /// <param name="id">The workout category ID</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(WorkoutCategoryId id);
}