namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service for managing cache operations
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from the cache
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The cache key</param>
    /// <returns>The cached value if found, otherwise null</returns>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Sets a value in the cache with the specified expiration
    /// </summary>
    /// <typeparam name="T">The type of the value to cache</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expiration">The cache expiration time</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class;

    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes all cache entries that match the specified pattern
    /// </summary>
    /// <param name="pattern">The pattern to match cache keys</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Gets or creates a cached value
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">The factory function to create the value if not cached</param>
    /// <param name="expiration">The cache expiration time</param>
    /// <returns>The cached or newly created value</returns>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration) where T : class;
}