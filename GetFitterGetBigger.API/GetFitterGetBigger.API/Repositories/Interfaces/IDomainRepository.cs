using GetFitterGetBigger.API.Models;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Base repository interface for domain entities that support the Empty pattern
/// </summary>
/// <typeparam name="TEntity">The domain entity type that implements IEmptyEntity</typeparam>
/// <typeparam name="TId">The ID type of the entity</typeparam>
public interface IDomainRepository<TEntity, TId> : IRepository
    where TEntity : IEmptyEntity<TEntity>
    where TId : struct
{
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve</param>
    /// <returns>The entity if found, Empty otherwise</returns>
    Task<TEntity> GetByIdAsync(TId id);
    
    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <returns>A collection of all entities (never null)</returns>
    Task<IEnumerable<TEntity>> GetAllAsync();
    
    /// <summary>
    /// Checks if an entity exists by its ID
    /// </summary>
    /// <param name="id">The ID of the entity to check</param>
    /// <returns>True if the entity exists, false otherwise</returns>
    Task<bool> ExistsAsync(TId id);
}