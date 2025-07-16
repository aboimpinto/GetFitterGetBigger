using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Cache service interface for eternal/immutable Pure Reference data
/// This cache stores data that never changes after deployment (e.g., BodyParts, DifficultyLevels)
/// Features:
/// - Supports Empty/Null Object Pattern with CacheResult
/// - No remove operations (data is eternal)
/// - Optimized for read-heavy, write-once scenarios
/// </summary>
public interface IEternalCacheService
{
    /// <summary>
    /// Gets a value from the cache with explicit cache hit/miss result
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The cache key</param>
    /// <returns>CacheResult indicating hit/miss and containing the value if hit</returns>
    Task<CacheResult<T>> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Gets a value from the cache, returning Empty for cache misses
    /// Only works with types that implement IEmptyEntity
    /// </summary>
    /// <typeparam name="T">The type that implements IEmptyEntity</typeparam>
    /// <param name="key">The cache key</param>
    /// <returns>The cached value if hit, T.Empty if miss</returns>
    Task<T> GetOrEmptyAsync<T>(string key) where T : class, IEmptyEntity<T>;

    /// <summary>
    /// Sets a value in the eternal cache with automatic 365-day expiration
    /// Once set, values remain cached for effectively forever (365 days)
    /// </summary>
    /// <typeparam name="T">The type of the value to cache</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SetAsync<T>(string key, T value) where T : class;

    /// <summary>
    /// Gets or creates a cached value using the Empty pattern with automatic 365-day expiration
    /// If the factory returns Empty, the value is NOT cached
    /// </summary>
    /// <typeparam name="T">The type that implements IEmptyEntity</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">The factory function to create the value if not cached</param>
    /// <returns>The cached value, or the result from factory (which may be Empty)</returns>
    Task<T> GetOrCreateEmptyAwareAsync<T>(string key, Func<Task<T>> factory) 
        where T : class, IEmptyEntity<T>;
}