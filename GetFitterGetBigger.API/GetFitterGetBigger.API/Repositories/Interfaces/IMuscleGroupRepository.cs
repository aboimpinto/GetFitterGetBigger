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
    /// <returns>The muscle group if found, null otherwise</returns>
    Task<MuscleGroup?> GetByIdAsync(MuscleGroupId id);
    
    /// <summary>
    /// Gets a muscle group by its name
    /// </summary>
    /// <param name="name">The name of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, null otherwise</returns>
    Task<MuscleGroup?> GetByNameAsync(string name);
    
    /// <summary>
    /// Gets all muscle groups for a specific body part
    /// </summary>
    /// <param name="bodyPartId">The ID of the body part</param>
    /// <returns>A collection of muscle groups for the specified body part</returns>
    Task<IEnumerable<MuscleGroup>> GetByBodyPartAsync(BodyPartId bodyPartId);
}
