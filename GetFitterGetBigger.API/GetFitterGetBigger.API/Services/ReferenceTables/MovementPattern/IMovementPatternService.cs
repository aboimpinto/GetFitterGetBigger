using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MovementPattern;

/// <summary>
/// Service interface for MovementPattern business operations
/// Provides caching and business logic for movement pattern reference data
/// </summary>
public interface IMovementPatternService
{
    /// <summary>
    /// Gets all active movement patterns with caching
    /// </summary>
    /// <returns>A service result containing the collection of active movement patterns</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a movement pattern by its ID with caching
    /// </summary>
    /// <param name="id">The movement pattern ID</param>
    /// <returns>A service result containing the movement pattern if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MovementPatternId id);
    
    /// <summary>
    /// Gets a movement pattern by its ID string with caching
    /// </summary>
    /// <param name="id">The movement pattern ID as a string</param>
    /// <returns>A service result containing the movement pattern if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a movement pattern by its value with caching
    /// </summary>
    /// <param name="value">The movement pattern value (case-insensitive)</param>
    /// <returns>A service result containing the movement pattern if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a movement pattern exists by ID with caching
    /// </summary>
    /// <param name="id">The movement pattern ID to check</param>
    /// <returns>A service result indicating whether the movement pattern exists</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(MovementPatternId id);
}