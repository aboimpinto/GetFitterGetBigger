using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.ExerciseType;

/// <summary>
/// Service interface for ExerciseType business operations
/// Provides caching and business logic for exercise type reference data
/// </summary>
public interface IExerciseTypeService
{
    /// <summary>
    /// Gets all active exercise types with caching
    /// </summary>
    /// <returns>A service result containing the collection of active exercise types</returns>
    Task<ServiceResult<IEnumerable<ExerciseTypeDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets an exercise type by its ID with caching
    /// </summary>
    /// <param name="id">The exercise type ID</param>
    /// <returns>A service result containing the exercise type if found</returns>
    Task<ServiceResult<ExerciseTypeDto>> GetByIdAsync(ExerciseTypeId id);
    
    /// <summary>
    /// Gets an exercise type by its ID string with caching
    /// </summary>
    /// <param name="id">The exercise type ID as a string</param>
    /// <returns>A service result containing the exercise type if found</returns>
    Task<ServiceResult<ExerciseTypeDto>> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets an exercise type by its value with caching
    /// </summary>
    /// <param name="value">The exercise type value (case-insensitive)</param>
    /// <returns>A service result containing the exercise type if found</returns>
    Task<ServiceResult<ExerciseTypeDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if an exercise type exists by ID with caching
    /// </summary>
    /// <param name="id">The exercise type ID to check</param>
    /// <returns>A service result indicating whether the exercise type exists</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseTypeId id);
    
    /// <summary>
    /// Checks if all exercise types in a collection exist
    /// </summary>
    /// <param name="ids">The exercise type ID strings to check</param>
    /// <returns>True if all exercise types exist, false otherwise</returns>
    Task<bool> AllExistAsync(IEnumerable<string> ids);
    
    /// <summary>
    /// Checks if any of the exercise types is a REST type
    /// </summary>
    /// <param name="ids">The exercise type ID strings to check</param>
    /// <returns>True if any exercise type is REST, false otherwise</returns>
    Task<bool> AnyIsRestTypeAsync(IEnumerable<string> ids);
}