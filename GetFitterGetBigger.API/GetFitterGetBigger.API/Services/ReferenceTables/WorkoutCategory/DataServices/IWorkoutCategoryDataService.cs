using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.WorkoutCategory.DataServices;

/// <summary>
/// Data service interface for WorkoutCategory database operations
/// Encapsulates all data access concerns including UnitOfWork and Repository interactions
/// WorkoutCategories are pure reference data (read-only) that never changes after deployment
/// </summary>
public interface IWorkoutCategoryDataService
{
    /// <summary>
    /// Gets all active workout categories from the database
    /// </summary>
    /// <returns>A service result containing the collection of workout categories</returns>
    Task<ServiceResult<IEnumerable<WorkoutCategoryDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a workout category by its ID from the database
    /// </summary>
    /// <param name="id">The workout category ID</param>
    /// <returns>A service result containing the workout category if found, Empty otherwise</returns>
    Task<ServiceResult<WorkoutCategoryDto>> GetByIdAsync(WorkoutCategoryId id);
    
    /// <summary>
    /// Gets a workout category by its value from the database
    /// </summary>
    /// <param name="value">The workout category value</param>
    /// <returns>A service result containing the workout category if found, Empty otherwise</returns>
    Task<ServiceResult<WorkoutCategoryDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a workout category exists in the database
    /// </summary>
    /// <param name="id">The workout category ID to check</param>
    /// <returns>A service result containing true if exists, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutCategoryId id);
}