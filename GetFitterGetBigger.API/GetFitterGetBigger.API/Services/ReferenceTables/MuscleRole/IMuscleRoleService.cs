using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MuscleRole;

/// <summary>
/// Service interface for MuscleRole operations with caching
/// MuscleRoles are pure reference data that never changes after deployment
/// </summary>
public interface IMuscleRoleService
{
    /// <summary>
    /// Gets all active muscle roles with eternal caching
    /// </summary>
    /// <returns>Collection of active muscle role DTOs</returns>
    Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a muscle role by its ID with eternal caching
    /// </summary>
    /// <param name="id">The muscle role ID</param>
    /// <returns>Reference data DTO or failure with NotFound error</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MuscleRoleId id);
    
    /// <summary>
    /// Gets a muscle role by its ID string with eternal caching
    /// </summary>
    /// <param name="id">The muscle role ID as a string</param>
    /// <returns>Reference data DTO or failure with NotFound error</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a muscle role by its value with eternal caching
    /// </summary>
    /// <param name="value">The muscle role value (case-insensitive)</param>
    /// <returns>Reference data DTO or failure with NotFound error</returns>
    Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a muscle role exists by ID
    /// </summary>
    /// <param name="id">The muscle role ID</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(MuscleRoleId id);
}