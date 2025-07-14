using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// TEMPORARY: Repository interface for reference data entities that support the Empty pattern
/// This interface will be merged with IReferenceDataRepository once all entities are migrated
/// </summary>
/// <typeparam name="TEntity">The reference data entity type that implements IEmptyEntity</typeparam>
/// <typeparam name="TId">The ID type of the entity</typeparam>
public interface IEmptyEnabledReferenceDataRepository<TEntity, TId> : IRepository
    where TEntity : ReferenceDataBase, IEmptyEntity<TEntity>
    where TId : struct
{
    /// <summary>
    /// Gets all reference data items (active and inactive) ordered by display order
    /// </summary>
    /// <returns>A collection of all reference data items</returns>
    Task<IEnumerable<TEntity>> GetAllAsync();
    
    /// <summary>
    /// Gets all active reference data items ordered by display order
    /// </summary>
    /// <returns>A collection of active reference data items</returns>
    Task<IEnumerable<TEntity>> GetAllActiveAsync();
    
    /// <summary>
    /// Gets a reference data item by its ID
    /// </summary>
    /// <param name="id">The ID of the item to retrieve</param>
    /// <returns>The reference data item if found, Empty otherwise</returns>
    Task<TEntity> GetByIdAsync(TId id);
    
    /// <summary>
    /// Gets a reference data item by its value
    /// </summary>
    /// <param name="value">The value of the item to retrieve</param>
    /// <returns>The reference data item if found, Empty otherwise</returns>
    Task<TEntity> GetByValueAsync(string value);
}