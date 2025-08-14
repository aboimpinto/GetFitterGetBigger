using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.WorkoutObjective.DataServices;

/// <summary>
/// Data service interface for WorkoutObjective database operations
/// Encapsulates all data access concerns including UnitOfWork and Repository interactions
/// WorkoutObjectives are pure reference data (read-only) that never changes after deployment
/// </summary>
public interface IWorkoutObjectiveDataService
{
    /// <summary>
    /// Gets all active workout objectives from the database
    /// </summary>
    /// <returns>A service result containing the collection of workout objectives</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a workout objective by its ID from the database
    /// </summary>
    /// <param name="id">The workout objective ID</param>
    /// <returns>A service result containing the workout objective if found, Empty otherwise</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(WorkoutObjectiveId id);
    
    /// <summary>
    /// Gets a workout objective by its value from the database
    /// </summary>
    /// <param name="value">The workout objective value</param>
    /// <returns>A service result containing the workout objective if found, Empty otherwise</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a workout objective exists in the database
    /// </summary>
    /// <param name="id">The workout objective ID to check</param>
    /// <returns>A service result containing true if exists, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutObjectiveId id);
}