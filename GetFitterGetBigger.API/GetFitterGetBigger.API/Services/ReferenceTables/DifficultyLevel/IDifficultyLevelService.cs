using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.DifficultyLevel;

/// <summary>
/// Service interface for DifficultyLevel business operations
/// Provides caching and business logic for difficulty level reference data
/// </summary>
public interface IDifficultyLevelService
{
    /// <summary>
    /// Gets all active difficulty levels with caching
    /// </summary>
    /// <returns>A service result containing the collection of active difficulty levels</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a difficulty level by its ID with caching
    /// </summary>
    /// <param name="id">The difficulty level ID</param>
    /// <returns>A service result containing the difficulty level if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(DifficultyLevelId id);
    
    /// <summary>
    /// Gets a difficulty level by its ID string with caching
    /// </summary>
    /// <param name="id">The difficulty level ID as a string</param>
    /// <returns>A service result containing the difficulty level if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a difficulty level by its value with caching
    /// </summary>
    /// <param name="value">The difficulty level value (case-insensitive)</param>
    /// <returns>A service result containing the difficulty level if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a difficulty level exists by ID with caching
    /// </summary>
    /// <param name="id">The difficulty level ID to check</param>
    /// <returns>A service result indicating whether the difficulty level exists</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(DifficultyLevelId id);
}