using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for managing movement pattern reference data
/// </summary>
public interface IMovementPatternService
{
    /// <summary>
    /// Gets all active movement patterns
    /// </summary>
    /// <returns>A service result containing all active movement patterns</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a movement pattern by its ID
    /// </summary>
    /// <param name="id">The movement pattern ID</param>
    /// <returns>A service result containing the movement pattern if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MovementPatternId id);
    
    /// <summary>
    /// Gets a movement pattern by its value
    /// </summary>
    /// <param name="value">The movement pattern value</param>
    /// <returns>A service result containing the movement pattern if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a movement pattern exists
    /// </summary>
    /// <param name="id">The movement pattern ID to check</param>
    /// <returns>True if the movement pattern exists, false otherwise</returns>
    Task<bool> ExistsAsync(MovementPatternId id);
    
    /// <summary>
    /// Checks if a movement pattern exists with the given string ID
    /// </summary>
    /// <param name="id">The movement pattern ID in string format</param>
    /// <returns>True if the movement pattern exists and is active, false otherwise</returns>
    Task<bool> ExistsAsync(string id);
}