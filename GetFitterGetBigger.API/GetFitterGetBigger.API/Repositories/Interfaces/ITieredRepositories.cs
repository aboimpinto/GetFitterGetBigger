using GetFitterGetBigger.API.Models;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Base interface for read-only repository operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IReadOnlyRepository<TEntity> where TEntity : class, IEntity
{
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<TEntity?> GetByIdAsync(string id);
    
    /// <summary>
    /// Gets all active entities
    /// </summary>
    /// <returns>A collection of all active entities</returns>
    Task<IEnumerable<TEntity>> GetAllActiveAsync();
    
    /// <summary>
    /// Checks if an entity exists with the given ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>True if the entity exists and is active, false otherwise</returns>
    Task<bool> ExistsAsync(string id);
}

/// <summary>
/// Base interface for repositories with write operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IWritableRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : class, IEntity
{
    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity</returns>
    Task<TEntity> AddAsync(TEntity entity);
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    Task<TEntity> UpdateAsync(TEntity entity);
    
    /// <summary>
    /// Deletes an entity (soft delete - sets IsActive to false)
    /// </summary>
    /// <param name="id">The entity ID to delete</param>
    /// <returns>True if the entity was deleted, false otherwise</returns>
    Task<bool> DeleteAsync(string id);
}

/// <summary>
/// Repository interface for pure reference data
/// Only provides read operations as pure references never change
/// </summary>
/// <typeparam name="TEntity">The pure reference entity type</typeparam>
public interface IPureReferenceRepository<TEntity> : IReadOnlyRepository<TEntity> 
    where TEntity : class, IPureReference
{
    /// <summary>
    /// Gets an entity by its value
    /// </summary>
    /// <param name="value">The reference value</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<TEntity?> GetByValueAsync(string value);
}

/// <summary>
/// Repository interface for enhanced reference data
/// Provides full CRUD operations for admin-managed reference data
/// </summary>
/// <typeparam name="TEntity">The enhanced reference entity type</typeparam>
public interface IEnhancedReferenceRepository<TEntity> : IWritableRepository<TEntity> 
    where TEntity : class, IEnhancedReference
{
    /// <summary>
    /// Gets an entity by its value
    /// </summary>
    /// <param name="value">The reference value</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<TEntity?> GetByValueAsync(string value);
    
    /// <summary>
    /// Checks if a value already exists (for uniqueness validation)
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check (for updates)</param>
    /// <returns>True if the value exists, false otherwise</returns>
    Task<bool> ValueExistsAsync(string value, string? excludeId = null);
}

/// <summary>
/// Repository interface for domain entities
/// Provides full CRUD operations with complex query support
/// </summary>
/// <typeparam name="TEntity">The domain entity type</typeparam>
public interface IDomainRepository<TEntity> : IWritableRepository<TEntity> 
    where TEntity : class, IDomainEntity
{
    /// <summary>
    /// Gets entities created within a date range
    /// </summary>
    /// <param name="startDate">The start date (inclusive)</param>
    /// <param name="endDate">The end date (inclusive)</param>
    /// <returns>A collection of entities within the date range</returns>
    Task<IEnumerable<TEntity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets paginated results
    /// </summary>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The page size</param>
    /// <returns>A collection of entities for the requested page</returns>
    Task<IEnumerable<TEntity>> GetPaginatedAsync(int pageNumber, int pageSize);
    
    /// <summary>
    /// Gets the total count of active entities
    /// </summary>
    /// <returns>The count of active entities</returns>
    Task<int> GetCountAsync();
}