using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Olimpo.EntityFramework.Persistency;
using System;

namespace GetFitterGetBigger.API.Repositories.Implementations;

/// <summary>
/// Base repository implementation for reference data entities
/// </summary>
/// <typeparam name="TEntity">The reference data entity type</typeparam>
/// <typeparam name="TId">The ID type of the entity</typeparam>
/// <typeparam name="TContext">The database context type</typeparam>
public abstract class ReferenceDataRepository<TEntity, TId, TContext> : 
    RepositoryBase<TContext>, 
    IReferenceDataRepository<TEntity, TId>
    where TEntity : ReferenceDataBase
    where TId : struct
    where TContext : DbContext
{
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
    /// <returns>The reference data item if found, null otherwise</returns>
    public async Task<TEntity?> GetByIdAsync(TId id)
    {
        var entity = await Context.Set<TEntity>().FindAsync(id);
        
        if (entity != null)
        {
            // Detach the entity from the context to achieve the same effect as AsNoTracking
            Context.Entry(entity).State = EntityState.Detached;
        }
        
        return entity;
    }

    /// <summary>
    /// Gets a reference data item by its value (case-insensitive)
    /// </summary>
    /// <param name="value">The value of the item to retrieve</param>
    /// <returns>The reference data item if found, null otherwise</returns>
    public async Task<TEntity?> GetByValueAsync(string value) =>
        await Context.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Value.ToLower() == value.ToLower() && x.IsActive);
}
