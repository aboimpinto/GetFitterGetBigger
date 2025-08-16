using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for MuscleGroup data
/// </summary>
public class MuscleGroupRepository : DomainRepository<MuscleGroup, MuscleGroupId, FitnessDbContext>, IMuscleGroupRepository
{
    /// <summary>
    /// Gets all active muscle groups with BodyPart navigation property, ordered by name
    /// </summary>
    /// <returns>A collection of active muscle groups</returns>
    public override async Task<IEnumerable<MuscleGroup>> GetAllAsync() =>
        await Context.MuscleGroups
            .AsNoTracking()
            .Include(mg => mg.BodyPart)
            .Where(mg => mg.IsActive)
            .OrderBy(mg => mg.Name)
            .ToListAsync();
    
    /// <summary>
    /// Gets a muscle group by its ID with BodyPart navigation property (only active)
    /// </summary>
    /// <param name="id">The ID of the muscle group to retrieve</param>
    /// <returns>The muscle group if found and active, MuscleGroup.Empty otherwise</returns>
    public override async Task<MuscleGroup> GetByIdAsync(MuscleGroupId id)
    {
        var muscleGroup = await Context.MuscleGroups
            .AsNoTracking()
            .Include(mg => mg.BodyPart)
            .FirstOrDefaultAsync(mg => mg.Id == id && mg.IsActive);
            
        return muscleGroup ?? MuscleGroup.Empty;
    }

    /// <summary>
    /// Checks if a muscle group exists by its ID (only active muscle groups)
    /// </summary>
    /// <param name="id">The ID of the muscle group to check</param>
    /// <returns>True if the muscle group exists and is active, false otherwise</returns>
    public override async Task<bool> ExistsAsync(MuscleGroupId id) =>
        await Context.MuscleGroups
            .AsNoTracking()
            .AnyAsync(mg => mg.Id == id && mg.IsActive);
    
    /// <summary>
    /// Gets a muscle group by its name (case-insensitive)
    /// </summary>
    /// <param name="name">The name of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, MuscleGroup.Empty otherwise</returns>
    public async Task<MuscleGroup> GetByNameAsync(string name)
    {
        var muscleGroup = await Context.MuscleGroups
            .AsNoTracking()
            .Include(mg => mg.BodyPart)
            .Where(mg => mg.IsActive)
            .FirstOrDefaultAsync(mg => mg.Name.ToLower() == name.ToLower());
            
        return muscleGroup ?? MuscleGroup.Empty;
    }
    
    /// <summary>
    /// Gets all muscle groups for a specific body part
    /// </summary>
    /// <param name="bodyPartId">The ID of the body part</param>
    /// <returns>A collection of muscle groups for the specified body part</returns>
    public async Task<IEnumerable<MuscleGroup>> GetByBodyPartAsync(BodyPartId bodyPartId) =>
        await Context.MuscleGroups
            .AsNoTracking()
            .Include(mg => mg.BodyPart)
            .Where(mg => mg.BodyPartId == bodyPartId && mg.IsActive)
            .OrderBy(mg => mg.Name)
            .ToListAsync();
    
    /// <summary>
    /// Creates a new muscle group
    /// </summary>
    /// <param name="entity">The muscle group to create</param>
    /// <returns>The created muscle group</returns>
    public async Task<MuscleGroup> CreateAsync(MuscleGroup entity)
    {
        Context.MuscleGroups.Add(entity);
        await Context.SaveChangesAsync();
        
        // Load the BodyPart navigation property
        await Context.Entry(entity)
            .Reference(mg => mg.BodyPart)
            .LoadAsync();
        
        return entity;
    }
    
    /// <summary>
    /// Updates an existing muscle group
    /// </summary>
    /// <param name="entity">The muscle group to update</param>
    /// <returns>The updated muscle group</returns>
    public async Task<MuscleGroup> UpdateAsync(MuscleGroup entity)
    {
        // Check if entity is already tracked
        var tracked = Context.ChangeTracker.Entries<MuscleGroup>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);
        
        if (tracked != null)
        {
            // Detach the existing tracked entity
            tracked.State = EntityState.Detached;
        }
        
        // Ensure we're not tracking any BodyPart entities
        var trackedBodyParts = Context.ChangeTracker.Entries<BodyPart>()
            .Where(e => e.State != EntityState.Detached)
            .ToList();
        
        foreach (var bodyPart in trackedBodyParts)
        {
            bodyPart.State = EntityState.Detached;
        }
        
        // Create a clean entity without navigation properties to avoid unwanted updates
        var cleanEntity = entity with { BodyPart = null };
        
        // Attach and update only the MuscleGroup entity
        Context.MuscleGroups.Attach(cleanEntity);
        Context.Entry(cleanEntity).State = EntityState.Modified;
        
        await Context.SaveChangesAsync();
        
        // Now reload the entity with its navigation property
        Context.Entry(cleanEntity).State = EntityState.Detached;
        
        var updated = await GetByIdAsync(entity.Id);
        return updated.IsEmpty ? entity : updated;
    }
    
    /// <summary>
    /// Deactivates a muscle group by its ID
    /// </summary>
    /// <param name="id">The ID of the muscle group to deactivate</param>
    /// <returns>True if the muscle group was deactivated, false if not found</returns>
    public async Task<bool> DeactivateAsync(MuscleGroupId id)
    {
        var muscleGroup = await Context.MuscleGroups
            .FirstOrDefaultAsync(mg => mg.Id == id);
        
        if (muscleGroup == null || muscleGroup.IsEmpty)
            return false;
        
        var deactivated = MuscleGroup.Handler.Deactivate(muscleGroup);
        
        // Update the tracked entity with the new values
        Context.Entry(muscleGroup).CurrentValues.SetValues(deactivated);
        
        await Context.SaveChangesAsync();
        
        return true;
    }
    
    /// <summary>
    /// Checks if a muscle group with the given name exists
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <returns>True if a muscle group with the name exists, false otherwise</returns>
    public async Task<bool> ExistsByNameAsync(string name, MuscleGroupId? excludeId = null)
    {
        var query = Context.MuscleGroups
            .Where(mg => mg.IsActive && mg.Name.ToLower() == name.ToLower());
        
        if (excludeId != null)
        {
            query = query.Where(mg => mg.Id != excludeId);
        }
        
        return await query.AnyAsync();
    }
    
    /// <summary>
    /// Checks if a muscle group can be deactivated (no active exercise dependencies)
    /// </summary>
    /// <param name="id">The ID of the muscle group to check</param>
    /// <returns>True if the muscle group can be deactivated, false otherwise</returns>
    public async Task<bool> CanDeactivateAsync(MuscleGroupId id)
    {
        // Check if there are any active exercises using this muscle group
        var hasActiveExercises = await Context.ExerciseMuscleGroups
            .Include(emg => emg.Exercise)
            .Where(emg => emg.MuscleGroupId == id && emg.Exercise.IsActive)
            .AnyAsync();
        
        return !hasActiveExercises;
    }
}
