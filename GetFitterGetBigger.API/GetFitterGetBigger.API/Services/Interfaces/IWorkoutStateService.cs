using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for managing workout state reference data
/// </summary>
public interface IWorkoutStateService
{
    /// <summary>
    /// Gets all active workout states
    /// </summary>
    /// <returns>A service result containing all active workout states</returns>
    Task<ServiceResult<IEnumerable<WorkoutStateDto>>> GetAllAsync();
    
    /// <summary>
    /// Gets a workout state by its ID
    /// </summary>
    /// <param name="id">The workout state ID</param>
    /// <returns>A service result containing the workout state if found</returns>
    Task<ServiceResult<WorkoutStateDto>> GetByIdAsync(WorkoutStateId id);
    
    /// <summary>
    /// Gets a workout state by its value
    /// </summary>
    /// <param name="value">The workout state value</param>
    /// <returns>A service result containing the workout state if found</returns>
    Task<ServiceResult<WorkoutStateDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a workout state exists
    /// </summary>
    /// <param name="id">The workout state ID to check</param>
    /// <returns>True if the workout state exists, false otherwise</returns>
    Task<bool> ExistsAsync(WorkoutStateId id);
    
    /// <summary>
    /// Checks if a workout state exists with the given string ID
    /// </summary>
    /// <param name="id">The workout state ID in string format</param>
    /// <returns>True if the workout state exists and is active, false otherwise</returns>
    Task<bool> ExistsAsync(string id);
}