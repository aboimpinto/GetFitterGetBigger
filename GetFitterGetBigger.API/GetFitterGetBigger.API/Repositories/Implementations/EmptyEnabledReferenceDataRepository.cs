using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;
using System;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Base repository implementation for reference data entities that support the Empty pattern
/// </summary>
/// <typeparam name="TEntity">The reference data entity type that implements IEmptyEntity</typeparam>
/// <typeparam name="TId">The ID type of the entity</typeparam>
/// <typeparam name="TContext">The database context type</typeparam>
public abstract class EmptyEnabledReferenceDataRepository<TEntity, TId, TContext> : 
    RepositoryBase<TContext>, 
    IEmptyEnabledReferenceDataRepository<TEntity, TId>
    where TEntity : ReferenceDataBase, IEmptyEntity<TEntity>
    where TId : struct
    where TContext : DbContext
{
    /// <summary>
    /// Gets all reference data items (active and inactive) ordered by display order
    /// </summary>
    /// <returns>A collection of all reference data items</returns>
    public async Task<IEnumerable<TEntity>> GetAllAsync() =>
        await Context.Set<TEntity>()
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

    /// <summary>
    /// Gets all active reference data items ordered by display order
    /// </summary>
    /// <returns>A collection of active reference data items</returns>
    public async Task<IEnumerable<TEntity>> GetAllActiveAsync() =>
        await Context.Set<TEntity>()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();

    /// <summary>
    /// Gets a reference data item by its ID
    /// </summary>
    /// <param name="id">The ID of the item to retrieve</param>
    /// <returns>The reference data item if found, Empty otherwise</returns>
    public async Task<TEntity> GetByIdAsync(TId id) =>
        await Context.Set<TEntity>().FindAsync(id) switch
        {
            null => TEntity.Empty,
            var entity => DetachAndReturn(entity)
        };
    
    private TEntity DetachAndReturn(TEntity entity)
    {
        // Detach the entity from the context to achieve the same effect as AsNoTracking
        Context.Entry(entity).State = EntityState.Detached;
        return entity;
    }

    /// <summary>
    /// Gets a reference data item by its value (case-insensitive)
    /// </summary>
    /// <param name="value">The value of the item to retrieve</param>
    /// <returns>The reference data item if found, Empty otherwise</returns>
    public async Task<TEntity> GetByValueAsync(string value) =>
        await Context.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Value.ToLower() == value.ToLower() && x.IsActive) ?? TEntity.Empty;
    
    /// <summary>
    /// Checks if a reference data item exists by its ID
    /// Uses efficient database query with .Any() to avoid loading entire entity
    /// </summary>
    /// <param name="id">The ID of the item to check</param>
    /// <returns>True if the item exists and is active, false otherwise</returns>
    public async Task<bool> ExistsAsync(TId id)
    {
        // Use FindAsync to check existence as it properly handles specialized ID types
        var entity = await Context.Set<TEntity>().FindAsync(id);
        return entity != null && entity.IsActive;
    }
}