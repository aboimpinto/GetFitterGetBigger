using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for Equipment data
/// </summary>
public class EquipmentRepository : RepositoryBase<FitnessDbContext>, IEquipmentRepository
{
    /// <summary>
    /// Gets all equipment
    /// </summary>
    /// <returns>A collection of equipment</returns>
    public async Task<IEnumerable<Equipment>> GetAllAsync() =>
        await Context.Equipment
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();
    
    /// <summary>
    /// Gets equipment by its ID
    /// </summary>
    /// <param name="id">The ID of the equipment to retrieve</param>
    /// <returns>The equipment if found, null otherwise</returns>
    public async Task<Equipment?> GetByIdAsync(EquipmentId id)
    {
        // Use AsNoTracking for read operations to avoid tracking conflicts
        var equipment = await Context.Equipment
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
        
        return equipment;
    }
    
    /// <summary>
    /// Gets equipment by its name (case-insensitive)
    /// </summary>
    /// <param name="name">The name of the equipment to retrieve</param>
    /// <returns>The equipment if found, null otherwise</returns>
    public async Task<Equipment?> GetByNameAsync(string name) =>
        await Context.Equipment
            .AsNoTracking()
            .Where(e => e.IsActive)
            .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());
    
    /// <summary>
    /// Creates new equipment
    /// </summary>
    /// <param name="entity">The equipment to create</param>
    /// <returns>The created equipment</returns>
    public async Task<Equipment> CreateAsync(Equipment entity)
    {
        Context.Equipment.Add(entity);
        await Context.SaveChangesAsync();
        
        return entity;
    }
    
    /// <summary>
    /// Updates existing equipment
    /// </summary>
    /// <param name="entity">The equipment to update</param>
    /// <returns>The updated equipment</returns>
    public async Task<Equipment> UpdateAsync(Equipment entity)
    {
        // Log the entity state before any operations
        Console.WriteLine($"[Equipment Repository] UpdateAsync called with entity: ID={entity.Id}, Name='{entity.Name}', IsActive={entity.IsActive}, CreatedAt={entity.CreatedAt}, UpdatedAt={entity.UpdatedAt}");
        Console.WriteLine($"[Equipment Repository] ID as GUID: {(Guid)entity.Id}");
        
        // First, check if there's already a tracked entity with the same ID
        var tracked = Context.ChangeTracker.Entries<Equipment>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);
        
        if (tracked != null)
        {
            Console.WriteLine($"[Equipment Repository] Found tracked entity with same ID, detaching it");
            // Detach the existing tracked entity
            tracked.State = EntityState.Detached;
        }
        
        // Attach the entity and mark it as modified
        Context.Equipment.Attach(entity);
        Context.Entry(entity).State = EntityState.Modified;
        
        // Log the tracked entity values before save
        var entry = Context.Entry(entity);
        Console.WriteLine($"[Equipment Repository] Entity state before SaveChanges: {entry.State}");
        Console.WriteLine($"[Equipment Repository] Current values: ID={entry.CurrentValues["Id"]}, Name='{entry.CurrentValues["Name"]}', IsActive={entry.CurrentValues["IsActive"]}, CreatedAt={entry.CurrentValues["CreatedAt"]}, UpdatedAt={entry.CurrentValues["UpdatedAt"]}");
        Console.WriteLine($"[Equipment Repository] Original values: ID={entry.OriginalValues["Id"]}, Name='{entry.OriginalValues["Name"]}', IsActive={entry.OriginalValues["IsActive"]}, CreatedAt={entry.OriginalValues["CreatedAt"]}, UpdatedAt={entry.OriginalValues["UpdatedAt"]}");
        
        await Context.SaveChangesAsync();
        
        Console.WriteLine($"[Equipment Repository] After SaveChanges - Entity state: {entry.State}");
        
        return entity;
    }
    
    /// <summary>
    /// Deactivates equipment by its ID
    /// </summary>
    /// <param name="id">The ID of the equipment to deactivate</param>
    /// <returns>True if the equipment was deactivated, false if not found</returns>
    public async Task<bool> DeactivateAsync(EquipmentId id)
    {
        var equipment = await Context.Equipment
            .FirstOrDefaultAsync(e => e.Id == id);
        
        if (equipment == null)
            return false;
        
        var deactivated = Equipment.Handler.Deactivate(equipment);
        
        // Update the tracked entity with the new values
        Context.Entry(equipment).CurrentValues.SetValues(deactivated);
        
        await Context.SaveChangesAsync();
        
        return true;
    }
    
    /// <summary>
    /// Checks if equipment with the given name exists
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <returns>True if equipment with the name exists, false otherwise</returns>
    public async Task<bool> ExistsAsync(string name, EquipmentId? excludeId = null)
    {
        var query = Context.Equipment
            .Where(e => e.IsActive && e.Name.ToLower() == name.ToLower());
        
        if (excludeId != null)
        {
            query = query.Where(e => e.Id != excludeId);
        }
        
        return await query.AnyAsync();
    }
    
    /// <summary>
    /// Checks if equipment is in use by any exercises
    /// </summary>
    /// <param name="id">The ID of the equipment to check</param>
    /// <returns>True if the equipment is in use, false otherwise</returns>
    public async Task<bool> IsInUseAsync(EquipmentId id)
    {
        // Check if there are any exercises using this equipment
        var hasExercises = await Context.ExerciseEquipment
            .Where(ee => ee.EquipmentId == id)
            .AnyAsync();
        
        return hasExercises;
    }
}
