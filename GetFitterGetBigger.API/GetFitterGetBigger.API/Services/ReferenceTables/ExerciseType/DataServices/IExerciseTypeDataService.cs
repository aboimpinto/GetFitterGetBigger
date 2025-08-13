using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.ExerciseType.DataServices;

/// <summary>
/// Data service interface for ExerciseType database operations
/// Handles all data access concerns for ExerciseType entities
/// </summary>
public interface IExerciseTypeDataService
{
    /// <summary>
    /// Gets all active exercise types from the database
    /// </summary>
    /// <returns>Collection of active exercise type DTOs</returns>
    Task<ServiceResult<IEnumerable<ExerciseTypeDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets an exercise type by its ID
    /// </summary>
    /// <param name="id">The exercise type ID</param>
    /// <returns>Exercise type DTO or Empty if not found</returns>
    Task<ServiceResult<ExerciseTypeDto>> GetByIdAsync(ExerciseTypeId id);
    
    /// <summary>
    /// Gets an exercise type by its value
    /// </summary>
    /// <param name="value">The exercise type value (case-insensitive)</param>
    /// <returns>Exercise type DTO or Empty if not found</returns>
    Task<ServiceResult<ExerciseTypeDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if an exercise type exists by ID
    /// </summary>
    /// <param name="id">The exercise type ID</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseTypeId id);
    
    /// <summary>
    /// Checks if all exercise types in a collection exist
    /// </summary>
    /// <param name="ids">The exercise type IDs to check</param>
    /// <returns>True if all exercise types exist, false otherwise</returns>
    Task<bool> AllExistAsync(IEnumerable<ExerciseTypeId> ids);
    
    /// <summary>
    /// Checks if any of the exercise types is a REST type
    /// </summary>
    /// <param name="ids">The exercise type IDs to check</param>
    /// <returns>True if any exercise type is REST, false otherwise</returns>
    Task<bool> AnyIsRestTypeAsync(IEnumerable<ExerciseTypeId> ids);
}