using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.ExerciseWeightType.DataServices;

/// <summary>
/// Data service interface for ExerciseWeightType database operations
/// Handles all data access concerns for ExerciseWeightType entities
/// </summary>
public interface IExerciseWeightTypeDataService
{
    /// <summary>
    /// Gets all active exercise weight types from the database
    /// </summary>
    /// <returns>Collection of active exercise weight type DTOs</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets an exercise weight type by its ID
    /// </summary>
    /// <param name="id">The exercise weight type ID</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(ExerciseWeightTypeId id);
    
    /// <summary>
    /// Gets an exercise weight type by its value
    /// </summary>
    /// <param name="value">The exercise weight type value (case-insensitive)</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Gets an exercise weight type by its code
    /// </summary>
    /// <param name="code">The exercise weight type code (e.g., "BODYWEIGHT_ONLY")</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByCodeAsync(string code);
    
    /// <summary>
    /// Checks if an exercise weight type exists by ID
    /// </summary>
    /// <param name="id">The exercise weight type ID</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseWeightTypeId id);
}