using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for managing kinetic chain type reference data
/// </summary>
public interface IKineticChainTypeService
{
    /// <summary>
    /// Gets all active kinetic chain types
    /// </summary>
    /// <returns>A service result containing all active kinetic chain types</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a kinetic chain type by its ID
    /// </summary>
    /// <param name="id">The kinetic chain type ID</param>
    /// <returns>A service result containing the kinetic chain type if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(KineticChainTypeId id);
    
    /// <summary>
    /// Gets a kinetic chain type by its value
    /// </summary>
    /// <param name="value">The kinetic chain type value</param>
    /// <returns>A service result containing the kinetic chain type if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a kinetic chain type exists
    /// </summary>
    /// <param name="id">The kinetic chain type ID to check</param>
    /// <returns>True if the kinetic chain type exists, false otherwise</returns>
    Task<bool> ExistsAsync(KineticChainTypeId id);
    
    /// <summary>
    /// Checks if a kinetic chain type exists with the given string ID
    /// </summary>
    /// <param name="id">The kinetic chain type ID in string format</param>
    /// <returns>True if the kinetic chain type exists and is active, false otherwise</returns>
    Task<bool> ExistsAsync(string id);
}