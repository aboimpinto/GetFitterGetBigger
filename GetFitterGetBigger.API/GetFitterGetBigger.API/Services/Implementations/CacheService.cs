using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Implementation of cache service using IMemoryCache
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private static readonly ConcurrentDictionary<string, bool> _cacheKeys = new();

    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            return await Task.FromResult(_memoryCache.Get<T>(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving value from cache for key: {Key}", key);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        try
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(expiration)
                .RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
                {
                    _cacheKeys.TryRemove(evictedKey.ToString() ?? string.Empty, out _);
                });

            _memoryCache.Set(key, value, cacheEntryOptions);
            _cacheKeys.TryAdd(key, true);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var keysToRemove = _cacheKeys.Keys.Where(k => k.StartsWith(pattern)).ToList();
            
            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing values from cache by pattern: {Pattern}", pattern);
        }
    }

    /// <inheritdoc/>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration) where T : class
    {
        try
        {
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            var value = await factory();
            
            if (value != null)
            {
                await SetAsync(key, value, expiration);
            }

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrCreateAsync for key: {Key}", key);
            // If caching fails, still try to get the value from the factory
            return await factory();
        }
    }
}