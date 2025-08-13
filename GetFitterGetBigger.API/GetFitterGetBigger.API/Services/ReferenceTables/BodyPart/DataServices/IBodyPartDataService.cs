using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.BodyPart.DataServices;

/// <summary>
/// Data service interface for BodyPart database operations
/// Handles all data access concerns for BodyPart entities
/// </summary>
public interface IBodyPartDataService
{
    /// <summary>
    /// Gets all active body parts from the database
    /// </summary>
    /// <returns>Collection of active body part DTOs</returns>
    Task<ServiceResult<IEnumerable<BodyPartDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a body part by its ID
    /// </summary>
    /// <param name="id">The body part ID</param>
    /// <returns>Body part DTO or Empty if not found</returns>
    Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id);
    
    /// <summary>
    /// Gets a body part by its value
    /// </summary>
    /// <param name="value">The body part value (case-insensitive)</param>
    /// <returns>Body part DTO or Empty if not found</returns>
    Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a body part exists by ID
    /// </summary>
    /// <param name="id">The body part ID</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(BodyPartId id);
}