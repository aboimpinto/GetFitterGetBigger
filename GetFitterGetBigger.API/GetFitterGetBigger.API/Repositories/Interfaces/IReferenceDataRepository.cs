using GetFitterGetBigger.API.Models.Entities;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Base repository interface for reference data entities
/// </summary>
/// <typeparam name="TEntity">The reference data entity type</typeparam>
/// <typeparam name="TId">The ID type of the entity</typeparam>
public interface IReferenceDataRepository<TEntity, TId> : IRepository
    where TEntity : ReferenceDataBase
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
    /// <returns>The reference data item if found, null otherwise</returns>
    Task<TEntity?> GetByIdAsync(TId id);
    
    /// <summary>
    /// Gets a reference data item by its value
    /// </summary>
    /// <param name="value">The value of the item to retrieve</param>
    /// <returns>The reference data item if found, null otherwise</returns>
    Task<TEntity?> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a reference data item exists by its ID
    /// Uses efficient database query with .Any() to avoid loading entire entity
    /// </summary>
    /// <param name="id">The ID of the item to check</param>
    /// <returns>True if the item exists and is active, false otherwise</returns>
    Task<bool> ExistsAsync(TId id);
}
