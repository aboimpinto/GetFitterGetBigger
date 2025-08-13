using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.ExerciseWeightType;

/// <summary>
/// Service interface for ExerciseWeightType operations with caching
/// ExerciseWeightTypes are pure reference data that never changes after deployment
/// </summary>
public interface IExerciseWeightTypeService
{
    /// <summary>
    /// Gets all active exercise weight types with eternal caching
    /// </summary>
    /// <returns>Collection of active exercise weight type DTOs</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets an exercise weight type by its ID with eternal caching
    /// </summary>
    /// <param name="id">The exercise weight type ID</param>
    /// <returns>Reference data DTO or failure with NotFound error</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(ExerciseWeightTypeId id);
    
    /// <summary>
    /// Gets an exercise weight type by its ID string with eternal caching
    /// </summary>
    /// <param name="id">The exercise weight type ID as a string</param>
    /// <returns>Reference data DTO or failure with NotFound error</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets an exercise weight type by its value with eternal caching
    /// </summary>
    /// <param name="value">The exercise weight type value (case-insensitive)</param>
    /// <returns>Reference data DTO or failure with NotFound error</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Gets an exercise weight type by its code with eternal caching
    /// </summary>
    /// <param name="code">The exercise weight type code (e.g., "BODYWEIGHT_ONLY")</param>
    /// <returns>Reference data DTO or failure with NotFound error</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByCodeAsync(string code);
    
    /// <summary>
    /// Checks if an exercise weight type exists by ID
    /// </summary>
    /// <param name="id">The exercise weight type ID</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseWeightTypeId id);
}