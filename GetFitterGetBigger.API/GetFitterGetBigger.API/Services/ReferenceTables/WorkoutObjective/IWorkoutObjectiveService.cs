using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.WorkoutObjective;

/// <summary>
/// Service interface for managing workout objective reference data
/// </summary>
public interface IWorkoutObjectiveService
{
    /// <summary>
    /// Gets all active workout objectives
    /// </summary>
    /// <returns>A service result containing all active workout objectives</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a workout objective by its ID
    /// </summary>
    /// <param name="id">The workout objective ID</param>
    /// <returns>A service result containing the workout objective if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(WorkoutObjectiveId id);
    
    /// <summary>
    /// Gets a workout objective by its ID string
    /// </summary>
    /// <param name="id">The workout objective ID as a string</param>
    /// <returns>A service result containing the workout objective if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a workout objective by its value
    /// </summary>
    /// <param name="value">The workout objective value</param>
    /// <returns>A service result containing the workout objective if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a workout objective exists
    /// </summary>
    /// <param name="id">The workout objective ID to check</param>
    /// <returns>A service result containing true if the workout objective exists, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutObjectiveId id);
}