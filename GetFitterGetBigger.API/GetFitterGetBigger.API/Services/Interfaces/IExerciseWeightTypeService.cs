using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for exercise weight type operations
/// </summary>
public interface IExerciseWeightTypeService
{
    /// <summary>
    /// Gets all active exercise weight types
    /// </summary>
    /// <returns>Service result containing collection of reference data DTOs</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets an exercise weight type by ID
    /// </summary>
    /// <param name="id">The exercise weight type ID</param>
    /// <returns>Service result containing the reference data DTO</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(ExerciseWeightTypeId id);
    
    /// <summary>
    /// Gets an exercise weight type by value
    /// </summary>
    /// <param name="value">The value (name) of the weight type</param>
    /// <returns>Service result containing the reference data DTO</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Gets an exercise weight type by code
    /// </summary>
    /// <param name="code">The code of the weight type (e.g., "BODYWEIGHT_ONLY")</param>
    /// <returns>Service result containing the reference data DTO</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByCodeAsync(string code);
    
    /// <summary>
    /// Checks if an exercise weight type exists
    /// </summary>
    /// <param name="id">The exercise weight type ID to check</param>
    /// <returns>A service result containing true if the weight type exists and is active, false otherwise</returns>
    Task<ServiceResult<bool>> ExistsAsync(ExerciseWeightTypeId id);
    
    
    /// <summary>
    /// Validates if a weight value is appropriate for the given weight type
    /// </summary>
    /// <param name="weightTypeId">The exercise weight type ID</param>
    /// <param name="weight">The weight value to validate (null for no weight)</param>
    /// <returns>True if the weight is valid for the type, false otherwise</returns>
    Task<bool> IsValidWeightForTypeAsync(ExerciseWeightTypeId weightTypeId, decimal? weight);
}