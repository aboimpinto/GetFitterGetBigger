using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.KineticChainType;

/// <summary>
/// Service interface for KineticChainType business operations
/// Provides caching and business logic for kinetic chain type reference data
/// </summary>
public interface IKineticChainTypeService
{
    /// <summary>
    /// Gets all active kinetic chain types with caching
    /// </summary>
    /// <returns>A service result containing the collection of active kinetic chain types</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a kinetic chain type by its ID with caching
    /// </summary>
    /// <param name="id">The kinetic chain type ID</param>
    /// <returns>A service result containing the kinetic chain type if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(KineticChainTypeId id);
    
    /// <summary>
    /// Gets a kinetic chain type by its ID string with caching
    /// </summary>
    /// <param name="id">The kinetic chain type ID as a string</param>
    /// <returns>A service result containing the kinetic chain type if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a kinetic chain type by its value with caching
    /// </summary>
    /// <param name="value">The kinetic chain type value (case-insensitive)</param>
    /// <returns>A service result containing the kinetic chain type if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a kinetic chain type exists by ID with caching
    /// </summary>
    /// <param name="id">The kinetic chain type ID to check</param>
    /// <returns>A service result indicating whether the kinetic chain type exists</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(KineticChainTypeId id);
}