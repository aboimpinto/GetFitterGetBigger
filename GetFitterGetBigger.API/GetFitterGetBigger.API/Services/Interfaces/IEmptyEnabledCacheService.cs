using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// TEMPORARY: Cache service interface that supports the Empty/Null Object Pattern
/// This interface will be merged with ICacheService once all services are migrated
/// </summary>
public interface IEmptyEnabledCacheService : ICacheService
{
    /// <summary>
    /// Gets a value from the cache with explicit cache hit/miss result
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The cache key</param>
    /// <returns>CacheResult indicating hit/miss and containing the value if hit</returns>
    new Task<CacheResult<T>> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Gets a value from the cache, returning Empty for cache misses
    /// Only works with types that implement IEmptyEntity
    /// </summary>
    /// <typeparam name="T">The type that implements IEmptyEntity</typeparam>
    /// <param name="key">The cache key</param>
    /// <returns>The cached value if hit, T.Empty if miss</returns>
    Task<T> GetOrEmptyAsync<T>(string key) where T : class, IEmptyEntity<T>;

    /// <summary>
    /// Gets a value from the cache using the legacy nullable pattern
    /// This method is inherited from ICacheService for backward compatibility
    /// </summary>
    [Obsolete("This method returns null and will be removed after Empty pattern migration. " +
              "Use GetAsync<T>(key) for CacheResult<T>, or GetOrEmptyAsync<T>(key) for types implementing IEmptyEntity<T>.")]
    Task<T?> ICacheService.GetAsync<T>(string key) where T : class => 
        GetAsync<T>(key).ContinueWith(t => t.Result.IsHit ? t.Result.Value : null);

    /// <summary>
    /// Gets or creates a cached value using the Empty pattern
    /// If the factory returns Empty, the value is NOT cached
    /// </summary>
    /// <typeparam name="T">The type that implements IEmptyEntity</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">The factory function to create the value if not cached</param>
    /// <param name="expiration">The cache expiration time</param>
    /// <returns>The cached value, or the result from factory (which may be Empty)</returns>
    Task<T> GetOrCreateEmptyAwareAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration) 
        where T : class, IEmptyEntity<T>;
}