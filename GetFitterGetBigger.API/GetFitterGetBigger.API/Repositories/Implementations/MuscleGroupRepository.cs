using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for MuscleGroup data
/// </summary>
public class MuscleGroupRepository : RepositoryBase<FitnessDbContext>, IMuscleGroupRepository
{
    /// <summary>
    /// Gets all muscle groups
    /// </summary>
    /// <returns>A collection of muscle groups</returns>
    public async Task<IEnumerable<MuscleGroup>> GetAllAsync() =>
        await Context.MuscleGroups
            .AsNoTracking()
            .Include(mg => mg.BodyPart)
            .OrderBy(mg => mg.Name)
            .ToListAsync();
    
    /// <summary>
    /// Gets a muscle group by its ID
    /// </summary>
    /// <param name="id">The ID of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, null otherwise</returns>
    public async Task<MuscleGroup?> GetByIdAsync(MuscleGroupId id)
    {
        var muscleGroup = await Context.MuscleGroups
            .Include(mg => mg.BodyPart)
            .FirstOrDefaultAsync(mg => mg.Id == id);
        
        if (muscleGroup != null)
        {
            // Detach the entity from the context to achieve the same effect as AsNoTracking
            Context.Entry(muscleGroup).State = EntityState.Detached;
        }
        
        return muscleGroup;
    }
    
    /// <summary>
    /// Gets a muscle group by its name (case-insensitive)
    /// </summary>
    /// <param name="name">The name of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, null otherwise</returns>
    public async Task<MuscleGroup?> GetByNameAsync(string name) =>
        await Context.MuscleGroups
            .AsNoTracking()
            .Include(mg => mg.BodyPart)
            .FirstOrDefaultAsync(mg => mg.Name.ToLower() == name.ToLower());
    
    /// <summary>
    /// Gets all muscle groups for a specific body part
    /// </summary>
    /// <param name="bodyPartId">The ID of the body part</param>
    /// <returns>A collection of muscle groups for the specified body part</returns>
    public async Task<IEnumerable<MuscleGroup>> GetByBodyPartAsync(BodyPartId bodyPartId) =>
        await Context.MuscleGroups
            .AsNoTracking()
            .Include(mg => mg.BodyPart)
            .Where(mg => mg.BodyPartId == bodyPartId)
            .OrderBy(mg => mg.Name)
            .ToListAsync();
}
