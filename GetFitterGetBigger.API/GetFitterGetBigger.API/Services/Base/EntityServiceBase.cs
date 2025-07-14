using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;

namespace GetFitterGetBigger.API.Services.Base;

/// <summary>
/// Base service class providing common functionality for all entity services
/// </summary>
/// <typeparam name="TEntity">The entity type this service manages</typeparam>
public abstract class EntityServiceBase<TEntity> where TEntity : class, IEntity
{
    protected readonly ICacheService _cacheService;
    protected readonly ILogger _logger;
    
    /// <summary>
    /// Initializes a new instance of the EntityServiceBase class
    /// </summary>
    /// <param name="cacheService">The cache service</param>
    /// <param name="logger">The logger</param>
    protected EntityServiceBase(ICacheService cacheService, ILogger logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes a new instance of the EntityServiceBase class with Empty-enabled cache service
    /// This constructor supports services migrated to the Empty pattern
    /// </summary>
    /// <param name="cacheService">The Empty-enabled cache service</param>
    /// <param name="logger">The logger</param>
    protected EntityServiceBase(IEmptyEnabledCacheService cacheService, ILogger logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Gets the cache duration for this entity type based on its tier
    /// </summary>
    /// <returns>The cache duration, or null if caching is disabled</returns>
    protected virtual TimeSpan? GetCacheDuration()
    {
        // Check if the entity implements ICacheableEntity
        if (typeof(ICacheableEntity).IsAssignableFrom(typeof(TEntity)))
        {
            // Create a default instance to get cache strategy
            // This is a compile-time check, so we use the interface definitions
            return typeof(TEntity) switch
            {
                Type t when typeof(IPureReference).IsAssignableFrom(t) => TimeSpan.FromDays(365),
                Type t when typeof(IEnhancedReference).IsAssignableFrom(t) => TimeSpan.FromHours(1),
                _ => TimeSpan.FromMinutes(5) // Short-lived for domain entities
            };
        }
        
        return null; // No caching for non-cacheable entities
    }
    
    /// <summary>
    /// Gets the cache strategy for this entity type
    /// </summary>
    /// <returns>The appropriate cache strategy</returns>
    protected virtual CacheStrategy GetCacheStrategy()
    {
        return typeof(TEntity) switch
        {
            Type t when typeof(IPureReference).IsAssignableFrom(t) => CacheStrategy.Eternal,
            Type t when typeof(IEnhancedReference).IsAssignableFrom(t) => CacheStrategy.Invalidatable,
            Type t when typeof(IDomainEntity).IsAssignableFrom(t) => CacheStrategy.ShortLived,
            _ => CacheStrategy.None
        };
    }
    
    /// <summary>
    /// Gets the cache key prefix for this entity type
    /// </summary>
    /// <returns>The cache key prefix</returns>
    protected virtual string GetCacheKeyPrefix()
    {
        return $"{typeof(TEntity).Name}:";
    }
    
    /// <summary>
    /// Invalidates all caches for this entity type
    /// </summary>
    protected async Task InvalidateAllCachesAsync()
    {
        var prefix = GetCacheKeyPrefix();
        _logger.LogInformation("Invalidating all caches with prefix: {Prefix}", prefix);
        await _cacheService.RemoveByPatternAsync($"{prefix}*");
    }
    
    /// <summary>
    /// Gets a specific cache key for an entity ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>The cache key</returns>
    protected string GetCacheKey(string id)
    {
        return $"{GetCacheKeyPrefix()}{id}";
    }
    
    /// <summary>
    /// Gets the cache key for all entities
    /// </summary>
    /// <returns>The cache key for all entities</returns>
    protected string GetAllCacheKey()
    {
        return $"{GetCacheKeyPrefix()}all";
    }
}