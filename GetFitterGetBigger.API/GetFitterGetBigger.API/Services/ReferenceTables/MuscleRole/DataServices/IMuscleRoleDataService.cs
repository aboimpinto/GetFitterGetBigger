using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MuscleRole.DataServices;

/// <summary>
/// Data service interface for MuscleRole database operations
/// Handles all data access concerns for MuscleRole entities
/// </summary>
public interface IMuscleRoleDataService
{
    /// <summary>
    /// Gets all active muscle roles from the database
    /// </summary>
    /// <returns>Collection of active muscle role DTOs</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a muscle role by its ID
    /// </summary>
    /// <param name="id">The muscle role ID</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MuscleRoleId id);
    
    /// <summary>
    /// Gets a muscle role by its value
    /// </summary>
    /// <param name="value">The muscle role value (case-insensitive)</param>
    /// <returns>Reference data DTO or Empty if not found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a muscle role exists by ID
    /// </summary>
    /// <param name="id">The muscle role ID</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(MuscleRoleId id);
}