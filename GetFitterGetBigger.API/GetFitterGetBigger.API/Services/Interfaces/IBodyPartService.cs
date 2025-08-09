using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for managing body part reference data
/// </summary>
public interface IBodyPartService
{
    /// <summary>
    /// Gets all active body parts
    /// </summary>
    /// <returns>A service result containing all active body parts</returns>
    Task<ServiceResult<IEnumerable<BodyPartDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a body part by its ID
    /// </summary>
    /// <param name="id">The body part ID</param>
    /// <returns>A service result containing the body part if found</returns>
    Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id);
    
    /// <summary>
    /// Gets a body part by its value
    /// </summary>
    /// <param name="value">The body part value</param>
    /// <returns>A service result containing the body part if found</returns>
    Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a body part exists
    /// </summary>
    /// <param name="id">The body part ID to check</param>
    /// <returns>A service result containing a BooleanResultDto with true if the body part exists, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(BodyPartId id);
}