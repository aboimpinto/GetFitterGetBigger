using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for managing difficulty level reference data
/// </summary>
public interface IDifficultyLevelService
{
    /// <summary>
    /// Gets all active difficulty levels
    /// </summary>
    /// <returns>A service result containing all active difficulty levels</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a difficulty level by its ID
    /// </summary>
    /// <param name="id">The difficulty level ID</param>
    /// <returns>A service result containing the difficulty level if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(DifficultyLevelId id);
    
    /// <summary>
    /// Gets a difficulty level by its value
    /// </summary>
    /// <param name="value">The difficulty level value</param>
    /// <returns>A service result containing the difficulty level if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a difficulty level exists
    /// </summary>
    /// <param name="id">The difficulty level ID to check</param>
    /// <returns>True if the difficulty level exists, false otherwise</returns>
    Task<bool> ExistsAsync(DifficultyLevelId id);
    
    /// <summary>
    /// Checks if a difficulty level exists with the given string ID
    /// </summary>
    /// <param name="id">The difficulty level ID in string format</param>
    /// <returns>True if the difficulty level exists and is active, false otherwise</returns>
    Task<bool> ExistsAsync(string id);
}