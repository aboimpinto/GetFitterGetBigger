using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Base repository implementation for domain entities that support the Empty pattern
/// </summary>
/// <typeparam name="TEntity">The domain entity type that implements IEmptyEntity</typeparam>
/// <typeparam name="TId">The ID type of the entity</typeparam>
/// <typeparam name="TContext">The database context type</typeparam>
public abstract class DomainRepository<TEntity, TId, TContext> : 
    RepositoryBase<TContext>, 
    IDomainRepository<TEntity, TId>
    where TEntity : class, IEmptyEntity<TEntity>
    where TId : struct
    where TContext : DbContext
{
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve</param>
    /// <returns>The entity if found, Empty otherwise</returns>
    public virtual async Task<TEntity> GetByIdAsync(TId id)
    {
        var entity = await Context.Set<TEntity>().FindAsync(id);
        
        if (entity != null)
        {
            // Detach the entity from the context to achieve the same effect as AsNoTracking
            Context.Entry(entity).State = EntityState.Detached;
        }
        
        return entity ?? TEntity.Empty;
    }
    
    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <returns>A collection of all entities (never null)</returns>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        var entities = await Context.Set<TEntity>()
            .AsNoTracking()
            .ToListAsync();
            
        return entities ?? new List<TEntity>();
    }
    
    /// <summary>
    /// Checks if an entity exists by its ID
    /// </summary>
    /// <param name="id">The ID of the entity to check</param>
    /// <returns>True if the entity exists, false otherwise</returns>
    public virtual async Task<bool> ExistsAsync(TId id)
    {
        // Use FindAsync for efficient existence check
        var entity = await Context.Set<TEntity>().FindAsync(id);
        
        if (entity != null)
        {
            // Detach to avoid tracking
            Context.Entry(entity).State = EntityState.Detached;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Helper method to detach any tracked entity with the same ID
    /// Useful before update operations to prevent tracking conflicts
    /// </summary>
    /// <param name="id">The ID of the entity to detach</param>
    protected void DetachTrackedEntity(TId id)
    {
        var tracked = Context.ChangeTracker.Entries<TEntity>()
            .FirstOrDefault(e => e.Property("Id").CurrentValue?.Equals(id) == true);
        
        if (tracked != null)
        {
            tracked.State = EntityState.Detached;
        }
    }
}