using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for exercise type operations
/// </summary>
public interface IExerciseTypeService
{
    /// <summary>
    /// Gets all active exercise types
    /// </summary>
    /// <returns>A collection of active exercise types</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets an exercise type by ID
    /// </summary>
    /// <param name="id">The exercise type ID</param>
    /// <returns>The exercise type if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(ExerciseTypeId id);
    
    /// <summary>
    /// Gets an exercise type by value
    /// </summary>
    /// <param name="value">The exercise type value</param>
    /// <returns>The exercise type if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if an exercise type exists
    /// </summary>
    /// <param name="id">The exercise type ID to check</param>
    /// <returns>True if the exercise type exists, false otherwise</returns>
    Task<bool> ExistsAsync(ExerciseTypeId id);
    
    /// <summary>
    /// Checks if an exercise type exists by string ID
    /// </summary>
    /// <param name="id">The exercise type ID string to check</param>
    /// <returns>True if the exercise type exists, false otherwise</returns>
    Task<bool> ExistsAsync(string id);

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