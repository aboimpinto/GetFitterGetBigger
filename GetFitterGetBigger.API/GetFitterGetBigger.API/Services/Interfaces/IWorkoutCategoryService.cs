using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service for managing workout categories reference data with Empty pattern support
/// </summary>
public interface IWorkoutCategoryService
{
    /// <summary>
    /// Get all active workout categories
    /// </summary>
    /// <returns>Service result containing collection of active workout categories</returns>
    Task<ServiceResult<IEnumerable<WorkoutCategoryDto>>> GetAllAsync();
    
    /// <summary>
    /// Get workout category by ID
    /// </summary>
    /// <param name="id">The workout category ID</param>
    /// <returns>Service result containing the workout category or error</returns>
    Task<ServiceResult<WorkoutCategoryDto>> GetByIdAsync(WorkoutCategoryId id);
    
    /// <summary>
    /// Get workout category by value
    /// </summary>
    /// <param name="value">The workout category value (case insensitive)</param>
    /// <returns>Service result containing the workout category or error</returns>
    Task<ServiceResult<WorkoutCategoryDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Check if workout category exists
    /// </summary>
    /// <param name="id">The workout category ID</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(WorkoutCategoryId id);
    
    /// <summary>
    /// Check if workout category exists by string ID
    /// </summary>
    /// <param name="id">The workout category ID as string</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(string id);
}