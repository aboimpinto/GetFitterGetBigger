using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for MuscleGroup data
/// </summary>
public interface IMuscleGroupRepository : IRepository
{
    /// <summary>
    /// Gets all muscle groups
    /// </summary>
    /// <returns>A collection of muscle groups</returns>
    Task<IEnumerable<MuscleGroup>> GetAllAsync();
    
    /// <summary>
    /// Gets a muscle group by its ID
    /// </summary>
    /// <param name="id">The ID of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, MuscleGroup.Empty otherwise</returns>
    Task<MuscleGroup> GetByIdAsync(MuscleGroupId id);
    
    /// <summary>
    /// Gets a muscle group by its name
    /// </summary>
    /// <param name="name">The name of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, MuscleGroup.Empty otherwise</returns>
    Task<MuscleGroup> GetByNameAsync(string name);
    
    /// <summary>
    /// Gets all muscle groups for a specific body part
    /// </summary>
    /// <param name="bodyPartId">The ID of the body part</param>
    /// <returns>A collection of muscle groups for the specified body part</returns>
    Task<IEnumerable<MuscleGroup>> GetByBodyPartAsync(BodyPartId bodyPartId);
    
    /// <summary>
    /// Creates a new muscle group
    /// </summary>
    /// <param name="entity">The muscle group to create</param>
    /// <returns>The created muscle group</returns>
    Task<MuscleGroup> CreateAsync(MuscleGroup entity);
    
    /// <summary>
    /// Updates an existing muscle group
    /// </summary>
    /// <param name="entity">The muscle group to update</param>
    /// <returns>The updated muscle group</returns>
    Task<MuscleGroup> UpdateAsync(MuscleGroup entity);
    
    /// <summary>
    /// Deactivates a muscle group by its ID
    /// </summary>
    /// <param name="id">The ID of the muscle group to deactivate</param>
    /// <returns>True if the muscle group was deactivated, false if not found</returns>
    Task<bool> DeactivateAsync(MuscleGroupId id);
    
    /// <summary>
    /// Checks if a muscle group exists by its ID
    /// Uses efficient database query with .Any() to avoid loading entire entity
    /// </summary>
    /// <param name="id">The ID of the muscle group to check</param>
    /// <returns>True if the muscle group exists and is active, false otherwise</returns>
    Task<bool> ExistsAsync(MuscleGroupId id);
    
    /// <summary>
    /// Checks if a muscle group with the given name exists
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <returns>True if a muscle group with the name exists, false otherwise</returns>
    Task<bool> ExistsByNameAsync(string name, MuscleGroupId? excludeId = null);
    
    /// <summary>
    /// Checks if a muscle group can be deactivated (no active exercise dependencies)
    /// </summary>
    /// <param name="id">The ID of the muscle group to check</param>
    /// <returns>True if the muscle group can be deactivated, false otherwise</returns>
    Task<bool> CanDeactivateAsync(MuscleGroupId id);
}
