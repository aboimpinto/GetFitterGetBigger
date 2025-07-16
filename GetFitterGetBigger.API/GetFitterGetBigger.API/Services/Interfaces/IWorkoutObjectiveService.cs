using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for workout objective operations
/// </summary>
public interface IWorkoutObjectiveService
{
    /// <summary>
    /// Gets all active workout objectives
    /// </summary>
    /// <returns>Service result containing collection of workout objectives</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a workout objective by ID
    /// </summary>
    /// <param name="id">The workout objective ID</param>
    /// <returns>Service result containing the workout objective</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(WorkoutObjectiveId id);
    
    /// <summary>
    /// Gets a workout objective by value
    /// </summary>
    /// <param name="value">The workout objective value</param>
    /// <returns>Service result containing the workout objective</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a workout objective exists
    /// </summary>
    /// <param name="id">The workout objective ID</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(WorkoutObjectiveId id);
    
    /// <summary>
    /// Checks if a workout objective exists with the given string ID
    /// </summary>
    /// <param name="id">The workout objective ID in string format</param>
    /// <returns>True if the workout objective exists and is active, false otherwise</returns>
    Task<bool> ExistsAsync(string id);
}