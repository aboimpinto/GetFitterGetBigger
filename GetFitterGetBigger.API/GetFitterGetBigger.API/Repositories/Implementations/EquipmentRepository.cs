using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
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
            .OrderBy(e => e.Name)
            .ToListAsync();
    
    /// <summary>
    /// Gets equipment by its ID
    /// </summary>
    /// <param name="id">The ID of the equipment to retrieve</param>
    /// <returns>The equipment if found, null otherwise</returns>
    public async Task<Equipment?> GetByIdAsync(EquipmentId id)
    {
        var equipment = await Context.Equipment.FindAsync(id);
        
        if (equipment != null)
        {
            // Detach the entity from the context to achieve the same effect as AsNoTracking
            Context.Entry(equipment).State = EntityState.Detached;
        }
        
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
            .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());
}
