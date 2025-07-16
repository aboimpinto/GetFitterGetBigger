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
    private const string CacheKeySeparator = ":";
    private const string AllEntitiesSuffix = "all";
    private const string CacheWildcardPattern = "*";
    
    // Note: This can be either ICacheService or IEternalCacheService depending on the service type
    protected readonly object _cacheService;
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
    /// Initializes a new instance of the EntityServiceBase class with Eternal cache service
    /// This constructor supports Pure Reference services with eternal caching
    /// </summary>
    /// <param name="cacheService">The eternal cache service</param>
    /// <param name="logger">The logger</param>
    protected EntityServiceBase(IEternalCacheService cacheService, ILogger logger)
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
            // Note: Pure references use TimeSpan.MaxValue (eternal), 
            // Enhanced references use TimeSpan.FromHours(1),
            // Domain entities use TimeSpan.FromMinutes(5) or null (no caching)
            
            if (typeof(IPureReference).IsAssignableFrom(typeof(TEntity)))
            {
                return TimeSpan.MaxValue; // Eternal caching for pure references
            }
            else if (typeof(IEnhancedReference).IsAssignableFrom(typeof(TEntity)))
            {
                return TimeSpan.FromHours(1); // Moderate caching for enhanced references
            }
            else if (typeof(IDomainEntity).IsAssignableFrom(typeof(TEntity)))
            {
                return TimeSpan.FromMinutes(5); // Short caching for domain entities
            }
        }
        
        return null; // No caching for non-cacheable entities
    }
    
    /// <summary>
    /// Gets the cache key prefix for this entity type
    /// </summary>
    /// <returns>The cache key prefix</returns>
    protected virtual string GetCacheKeyPrefix()
    {
        return $"{typeof(TEntity).Name}{CacheKeySeparator}";
    }
    
    /// <summary>
    /// Gets the cache key for all entities
    /// </summary>
    /// <returns>The cache key for all entities</returns>
    protected virtual string GetAllCacheKey()
    {
        return $"{GetCacheKeyPrefix()}{AllEntitiesSuffix}";
    }
    
    /// <summary>
    /// Gets the cache key for a specific entity
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>The cache key for the entity</returns>
    protected virtual string GetCacheKey(string id)
    {
        return $"{GetCacheKeyPrefix()}{id}";
    }
    
    /// <summary>
    /// Gets the cache invalidation pattern for this entity type
    /// </summary>
    /// <returns>The cache invalidation pattern</returns>
    protected virtual string GetCacheInvalidationPattern()
    {
        return $"{GetCacheKeyPrefix()}{CacheWildcardPattern}";
    }
    
    /// <summary>
    /// Invalidates all cache entries for this entity type
    /// Only applicable for services using ICacheService (lookup tables)
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    protected virtual async Task InvalidateCacheAsync()
    {
        if (_cacheService is ICacheService cacheService)
        {
            var pattern = GetCacheInvalidationPattern();
            await cacheService.RemoveByPatternAsync(pattern);
            _logger.LogInformation("Cache invalidated for pattern: {Pattern}", pattern);
        }
        else
        {
            _logger.LogDebug("Cache invalidation not supported for eternal cache service");
        }
    }
}