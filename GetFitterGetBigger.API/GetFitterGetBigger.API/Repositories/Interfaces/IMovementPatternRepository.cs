using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for MovementPattern data
/// </summary>
public interface IMovementPatternRepository : IRepository
{
    /// <summary>
    /// Gets all movement patterns
    /// </summary>
    /// <returns>A collection of movement patterns</returns>
    Task<IEnumerable<MovementPattern>> GetAllAsync();
    
    /// <summary>
    /// Gets a movement pattern by its ID
    /// </summary>
    /// <param name="id">The ID of the movement pattern to retrieve</param>
    /// <returns>The movement pattern if found, null otherwise</returns>
    Task<MovementPattern?> GetByIdAsync(MovementPatternId id);
    
    /// <summary>
    /// Gets a movement pattern by its name
    /// </summary>
    /// <param name="name">The name of the movement pattern to retrieve</param>
    /// <returns>The movement pattern if found, null otherwise</returns>
    Task<MovementPattern?> GetByNameAsync(string name);
}
