using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Repository implementation for MovementPattern data
/// </summary>
public class MovementPatternRepository : RepositoryBase<FitnessDbContext>, IMovementPatternRepository
{
    /// <summary>
    /// Gets all movement patterns
    /// </summary>
    /// <returns>A collection of movement patterns</returns>
    public async Task<IEnumerable<MovementPattern>> GetAllAsync() =>
        await Context.MovementPatterns
            .AsNoTracking()
            .OrderBy(mp => mp.Name)
            .ToListAsync();
    
    /// <summary>
    /// Gets a movement pattern by its ID
    /// </summary>
    /// <param name="id">The ID of the movement pattern to retrieve</param>
    /// <returns>The movement pattern if found, null otherwise</returns>
    public async Task<MovementPattern?> GetByIdAsync(MovementPatternId id)
    {
        var movementPattern = await Context.MovementPatterns.FindAsync(id);
        
        if (movementPattern != null)
        {
            // Detach the entity from the context to achieve the same effect as AsNoTracking
            Context.Entry(movementPattern).State = EntityState.Detached;
        }
        
        return movementPattern;
    }
    
    /// <summary>
    /// Gets a movement pattern by its name (case-insensitive)
    /// </summary>
    /// <param name="name">The name of the movement pattern to retrieve</param>
    /// <returns>The movement pattern if found, null otherwise</returns>
    public async Task<MovementPattern?> GetByNameAsync(string name) =>
        await Context.MovementPatterns
            .AsNoTracking()
            .FirstOrDefaultAsync(mp => mp.Name.ToLower() == name.ToLower());
}
