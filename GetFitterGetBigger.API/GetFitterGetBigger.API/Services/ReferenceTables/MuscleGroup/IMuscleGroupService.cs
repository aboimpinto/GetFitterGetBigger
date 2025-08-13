using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.MuscleGroup;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup;

/// <summary>
/// Service interface for MuscleGroup business operations
/// Provides caching and business logic for muscle group reference data
/// </summary>
public interface IMuscleGroupService
{
    /// <summary>
    /// Gets all active muscle groups with caching
    /// </summary>
    /// <returns>A service result containing the collection of active muscle groups</returns>
    Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a muscle group by its ID with caching
    /// </summary>
    /// <param name="id">The muscle group ID</param>
    /// <returns>A service result containing the muscle group if found</returns>
    Task<ServiceResult<MuscleGroupDto>> GetByIdAsync(MuscleGroupId id);
    
    /// <summary>
    /// Gets a muscle group by its ID string with caching
    /// </summary>
    /// <param name="id">The muscle group ID as a string</param>
    /// <returns>A service result containing the muscle group if found</returns>
    Task<ServiceResult<MuscleGroupDto>> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets a muscle group by its value with caching
    /// </summary>
    /// <param name="value">The muscle group value (case-insensitive)</param>
    /// <returns>A service result containing the muscle group if found</returns>
    Task<ServiceResult<MuscleGroupDto>> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a muscle group exists by ID with caching
    /// </summary>
    /// <param name="id">The muscle group ID to check</param>
    /// <returns>A service result indicating whether the muscle group exists</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(MuscleGroupId id);
    
    /// <summary>
    /// Gets all muscle groups (same as GetAllActiveAsync for backward compatibility)
    /// </summary>
    /// <returns>A service result containing the collection of muscle groups</returns>
    Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetAllAsync();
    
    /// <summary>
    /// Gets a muscle group by its name (same as GetByValueAsync for backward compatibility)
    /// </summary>
    /// <param name="name">The muscle group name</param>
    /// <returns>A service result containing the muscle group if found</returns>
    Task<ServiceResult<MuscleGroupDto>> GetByNameAsync(string name);
    
    /// <summary>
    /// Gets muscle groups by body part ID
    /// </summary>
    /// <param name="bodyPartId">The body part ID</param>
    /// <returns>A service result containing the collection of muscle groups for the body part</returns>
    Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetByBodyPartAsync(BodyPartId bodyPartId);
    
    /// <summary>
    /// Creates a new muscle group
    /// </summary>
    /// <param name="command">The muscle group data to create</param>
    /// <returns>A service result containing the created muscle group</returns>
    Task<ServiceResult<MuscleGroupDto>> CreateAsync(CreateMuscleGroupCommand command);
    
    /// <summary>
    /// Updates an existing muscle group
    /// </summary>
    /// <param name="id">The muscle group ID to update</param>
    /// <param name="command">The updated muscle group data</param>
    /// <returns>A service result containing the updated muscle group</returns>
    Task<ServiceResult<MuscleGroupDto>> UpdateAsync(MuscleGroupId id, UpdateMuscleGroupCommand command);
    
    /// <summary>
    /// Soft deletes a muscle group (sets IsActive to false)
    /// </summary>
    /// <param name="id">The muscle group ID to delete</param>
    /// <returns>A service result indicating success or failure</returns>
    Task<ServiceResult<BooleanResultDto>> DeleteAsync(MuscleGroupId id);
}