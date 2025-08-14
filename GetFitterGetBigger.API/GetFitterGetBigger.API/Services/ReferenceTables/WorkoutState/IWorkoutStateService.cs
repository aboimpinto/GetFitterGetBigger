using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState;

/// <summary>
/// Service interface for managing workout state reference data
/// </summary>
public interface IWorkoutStateService
{
    /// <summary>
    /// Gets all active workout states
    /// </summary>
    /// <returns>A service result containing all active workout states</returns>
    Task<ServiceResult<IEnumerable<WorkoutStateDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a workout state by its ID
    /// </summary>
    /// <param name="id">The workout state ID</param>
    /// <returns>A service result containing the workout state if found</returns>
    Task<ServiceResult<WorkoutStateDto>> GetByIdAsync(WorkoutStateId id);
    
    /// <summary>
    /// Gets a workout state by its ID string
    /// </summary>
    /// <param name="id">The workout state ID as a string</param>
    /// <returns>A service result containing the workout state if found</returns>
    Task<ServiceResult<WorkoutStateDto>> GetByIdAsync(string id);
    
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
    /// <returns>A service result containing true if the workout state exists, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutStateId id);
}