using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for muscle role operations
/// </summary>
public interface IMuscleRoleService
{
    /// <summary>
    /// Gets all active muscle roles
    /// </summary>
    /// <returns>A collection of muscle role DTOs</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a muscle role by its ID
    /// </summary>
    /// <param name="id">The muscle role ID</param>
    /// <returns>The muscle role DTO if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MuscleRoleId id);
    
    /// <summary>
    /// Gets a muscle role by its value
    /// </summary>
    /// <param name="value">The muscle role value</param>
    /// <returns>The muscle role DTO if found</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a muscle role exists
    /// </summary>
    /// <param name="id">The muscle role ID to check</param>
    /// <returns>A service result containing true if the muscle role exists, false otherwise</returns>
    Task<ServiceResult<bool>> ExistsAsync(MuscleRoleId id);
    
}