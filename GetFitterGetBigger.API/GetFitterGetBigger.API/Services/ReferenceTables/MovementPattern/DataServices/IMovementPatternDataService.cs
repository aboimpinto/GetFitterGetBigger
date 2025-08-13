using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MovementPattern.DataServices;

/// <summary>
/// Data service interface for MovementPattern database operations
/// Handles all data access concerns for MovementPattern entities
/// </summary>
public interface IMovementPatternDataService
{
    /// <summary>
    /// Gets all active movement patterns from the database
    /// </summary>
    /// <returns>Collection of active movement pattern DTOs</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a movement pattern by its ID
    /// </summary>
    /// <param name="id">The movement pattern ID</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MovementPatternId id);
    
    /// <summary>
    /// Gets a movement pattern by its value
    /// </summary>
    /// <param name="value">The movement pattern value (case-insensitive)</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a movement pattern exists by ID
    /// </summary>
    /// <param name="id">The movement pattern ID</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(MovementPatternId id);
}