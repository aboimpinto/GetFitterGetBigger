using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.MuscleGroup;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup.DataServices;

/// <summary>
/// Data service interface for MuscleGroup database operations
/// Handles all data access concerns for MuscleGroup entities including CRUD operations
/// </summary>
public interface IMuscleGroupDataService
{
    /// <summary>
    /// Gets all active muscle groups from the database
    /// </summary>
    /// <returns>Collection of active muscle group DTOs</returns>
    Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a muscle group by its ID
    /// </summary>
    /// <param name="id">The muscle group ID</param>
    /// <returns>Muscle group DTO or Empty if not found</returns>
    Task<ServiceResult<MuscleGroupDto>> GetByIdAsync(MuscleGroupId id);
    
    /// <summary>
    /// Gets a muscle group by its name
    /// </summary>
    /// <param name="name">The muscle group name (case-insensitive)</param>
    /// <returns>Muscle group DTO or Empty if not found</returns>
    Task<ServiceResult<MuscleGroupDto>> GetByNameAsync(string name);
    
    /// <summary>
    /// Gets muscle groups by body part
    /// </summary>
    /// <param name="bodyPartId">The body part ID</param>
    /// <returns>Collection of muscle group DTOs for the specified body part</returns>
    Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetByBodyPartAsync(BodyPartId bodyPartId);
    
    /// <summary>
    /// Creates a new muscle group
    /// </summary>
    /// <param name="command">The muscle group data to create</param>
    /// <returns>Created muscle group DTO</returns>
    Task<ServiceResult<MuscleGroupDto>> CreateAsync(CreateMuscleGroupCommand command);
    
    /// <summary>
    /// Updates an existing muscle group
    /// </summary>
    /// <param name="id">The muscle group ID to update</param>
    /// <param name="command">The updated muscle group data</param>
    /// <returns>Updated muscle group DTO</returns>
    Task<ServiceResult<MuscleGroupDto>> UpdateAsync(MuscleGroupId id, UpdateMuscleGroupCommand command);
    
    /// <summary>
    /// Soft deletes a muscle group (sets IsActive to false)
    /// </summary>
    /// <param name="id">The muscle group ID to delete</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult<BooleanResultDto>> DeleteAsync(MuscleGroupId id);
    
    /// <summary>
    /// Checks if a muscle group exists by ID
    /// </summary>
    /// <param name="id">The muscle group ID</param>
    /// <returns>Boolean result indicating existence</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(MuscleGroupId id);
    
    /// <summary>
    /// Checks if a muscle group name is unique (excluding a specific ID if provided)
    /// </summary>
    /// <param name="name">The muscle group name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <returns>True if unique, false otherwise</returns>
    Task<bool> IsNameUniqueAsync(string name, MuscleGroupId? excludeId = null);
    
    /// <summary>
    /// Checks if a muscle group can be deleted (not referenced by other entities)
    /// </summary>
    /// <param name="id">The muscle group ID</param>
    /// <returns>True if can be deleted, false otherwise</returns>
    Task<bool> CanDeleteAsync(MuscleGroupId id);
}