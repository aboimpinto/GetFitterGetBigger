using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.KineticChainType.DataServices;

/// <summary>
/// Data service interface for KineticChainType database operations
/// Handles all data access concerns for KineticChainType entities
/// </summary>
public interface IKineticChainTypeDataService
{
    /// <summary>
    /// Gets all active kinetic chain types from the database
    /// </summary>
    /// <returns>Collection of active kinetic chain type DTOs</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a kinetic chain type by its ID
    /// </summary>
    /// <param name="id">The kinetic chain type ID</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(KineticChainTypeId id);
    
    /// <summary>
    /// Gets a kinetic chain type by its value
    /// </summary>
    /// <param name="value">The kinetic chain type value (case-insensitive)</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a kinetic chain type exists by ID
    /// </summary>
    /// <param name="id">The kinetic chain type ID</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(KineticChainTypeId id);
}