using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.BodyPart;

/// <summary>
/// Service interface for BodyPart business operations
/// Provides caching and business logic for body part reference data
/// </summary>
public interface IBodyPartService
{
    /// <summary>
    /// Gets all active body parts with caching
    /// </summary>
    /// <returns>A service result containing the collection of active body parts</returns>
    Task<ServiceResult<IEnumerable<BodyPartDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a body part by its ID with caching
    /// </summary>
    /// <param name="id">The body part ID</param>
    /// <returns>A service result containing the body part if found</returns>
    Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id);
    
    /// <summary>
    /// Gets a body part by its ID string with caching
    /// </summary>
    /// <param name="id">The body part ID as a string</param>
    /// <returns>A service result containing the body part if found</returns>
    Task<ServiceResult<BodyPartDto>> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a body part by its value with caching
    /// </summary>
    /// <param name="value">The body part value (case-insensitive)</param>
    /// <returns>A service result containing the body part if found</returns>
    Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a body part exists by ID with caching
    /// </summary>
    /// <param name="id">The body part ID to check</param>
    /// <returns>A service result indicating whether the body part exists</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(BodyPartId id);
}