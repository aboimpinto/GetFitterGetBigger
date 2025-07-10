using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for exercise weight type operations
/// </summary>
public interface IExerciseWeightTypeService
{
    /// <summary>
    /// Gets all active exercise weight types
    /// </summary>
    /// <returns>Collection of exercise weight types</returns>
    Task<IEnumerable<ExerciseWeightType>> GetAllAsync();
    
    /// <summary>
    /// Gets all active exercise weight types as DTOs
    /// </summary>
    /// <returns>Collection of reference data DTOs</returns>
    Task<IEnumerable<ReferenceDataDto>> GetAllAsDtosAsync();
    
    /// <summary>
    /// Gets an exercise weight type by ID
    /// </summary>
    /// <param name="id">The exercise weight type ID</param>
    /// <returns>The exercise weight type if found, null otherwise</returns>
    Task<ExerciseWeightType?> GetByIdAsync(ExerciseWeightTypeId id);
    
    /// <summary>
    /// Gets an exercise weight type by ID as DTO
    /// </summary>
    /// <param name="id">The exercise weight type ID string</param>
    /// <returns>The reference data DTO if found, null otherwise</returns>
    Task<ReferenceDataDto?> GetByIdAsDtoAsync(string id);
    
    /// <summary>
    /// Gets an exercise weight type by value
    /// </summary>
    /// <param name="value">The value (name) of the weight type</param>
    /// <returns>The exercise weight type if found, null otherwise</returns>
    Task<ExerciseWeightType?> GetByValueAsync(string value);
    
    /// <summary>
    /// Gets an exercise weight type by value as DTO
    /// </summary>
    /// <param name="value">The value (name) of the weight type</param>
    /// <returns>The reference data DTO if found, null otherwise</returns>
    Task<ReferenceDataDto?> GetByValueAsDtoAsync(string value);
    
    /// <summary>
    /// Gets an exercise weight type by code
    /// </summary>
    /// <param name="code">The code of the weight type (e.g., "BODYWEIGHT_ONLY")</param>
    /// <returns>The exercise weight type if found, null otherwise</returns>
    Task<ExerciseWeightType?> GetByCodeAsync(string code);
    
    /// <summary>
    /// Gets an exercise weight type by code as DTO
    /// </summary>
    /// <param name="code">The code of the weight type (e.g., "BODYWEIGHT_ONLY")</param>
    /// <returns>The reference data DTO if found, null otherwise</returns>
    Task<ReferenceDataDto?> GetByCodeAsDtoAsync(string code);
    
    /// <summary>
    /// Checks if an exercise weight type exists
    /// </summary>
    /// <param name="id">The exercise weight type ID to check</param>
    /// <returns>True if the weight type exists, false otherwise</returns>
    Task<bool> ExistsAsync(ExerciseWeightTypeId id);
    
    /// <summary>
    /// Validates if a weight value is appropriate for the given weight type
    /// </summary>
    /// <param name="weightTypeId">The exercise weight type ID</param>
    /// <param name="weight">The weight value to validate (null for no weight)</param>
    /// <returns>True if the weight is valid for the type, false otherwise</returns>
    Task<bool> IsValidWeightForTypeAsync(ExerciseWeightTypeId weightTypeId, decimal? weight);
}