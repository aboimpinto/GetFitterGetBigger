using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.DifficultyLevel.DataServices;

/// <summary>
/// Data service interface for DifficultyLevel database operations
/// Handles all data access concerns for DifficultyLevel entities
/// </summary>
public interface IDifficultyLevelDataService
{
    /// <summary>
    /// Gets all active difficulty levels from the database
    /// </summary>
    /// <returns>Collection of active difficulty level DTOs</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a difficulty level by its ID
    /// </summary>
    /// <param name="id">The difficulty level ID</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(DifficultyLevelId id);
    
    /// <summary>
    /// Gets a difficulty level by its value
    /// </summary>
    /// <param name="value">The difficulty level value (case-insensitive)</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a difficulty level exists by ID
    /// </summary>
    /// <param name="id">The difficulty level ID</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(DifficultyLevelId id);
}