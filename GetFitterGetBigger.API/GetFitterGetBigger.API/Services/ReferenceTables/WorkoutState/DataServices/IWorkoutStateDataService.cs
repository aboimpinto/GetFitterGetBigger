using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState.DataServices;

/// <summary>
/// Data service interface for WorkoutState database operations
/// Encapsulates all data access concerns including UnitOfWork and Repository interactions
/// WorkoutStates are pure reference data (read-only) that never changes after deployment
/// </summary>
public interface IWorkoutStateDataService
{
    /// <summary>
    /// Gets all active workout states from the database
    /// </summary>
    /// <returns>A service result containing the collection of workout states</returns>
    Task<ServiceResult<IEnumerable<WorkoutStateDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a workout state by its ID from the database
    /// </summary>
    /// <param name="id">The workout state ID</param>
    /// <returns>A service result containing the workout state if found, Empty otherwise</returns>
    Task<ServiceResult<WorkoutStateDto>> GetByIdAsync(WorkoutStateId id);
    
    /// <summary>
    /// Gets a workout state by its value from the database
    /// </summary>
    /// <param name="value">The workout state value</param>
    /// <returns>A service result containing the workout state if found, Empty otherwise</returns>
    Task<ServiceResult<WorkoutStateDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a workout state exists in the database
    /// </summary>
    /// <param name="id">The workout state ID to check</param>
    /// <returns>A service result containing true if exists, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutStateId id);
}