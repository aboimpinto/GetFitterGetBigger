using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Implementation of cache service for lookup/enhanced reference tables
/// This cache supports standard nullable patterns and cache invalidation
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

    /// <summary>
    /// Gets a value from the cache
    /// </summary>
    public Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = _memoryCache.Get<T>(key);
        return Task.FromResult(value);
    }

    /// <summary>
    /// Gets or creates a cached value with automatic 24-hour expiration
    /// </summary>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory) where T : class
    {
        var value = await GetAsync<T>(key);
        if (value != null)
        {
            return value;
        }

        value = await factory();
        if (value != null)
        {
            await SetAsync(key, value);
        }

        return value!;
    }

    /// <summary>
    /// Sets a value in the cache with automatic 24-hour expiration
    /// </summary>
    public Task SetAsync<T>(string key, T value) where T : class
    {
        // Fixed 24-hour expiration for volatile data
        const int VOLATILE_HOURS = 24;
        var expiration = TimeSpan.FromHours(VOLATILE_HOURS);
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration)
            .RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
            {
                _cacheKeys.TryRemove(evictedKey.ToString()!, out _);
            });

        _memoryCache.Set(key, value, cacheEntryOptions);
        _cacheKeys.TryAdd(key, true);
        
        _logger.LogDebug("Cache set for key: {Key} with {Hours}-hour expiration", key, VOLATILE_HOURS);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        _cacheKeys.TryRemove(key, out _);
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes all values from the cache that match a pattern
    /// </summary>
    public Task RemoveByPatternAsync(string pattern)
    {
        if (pattern.EndsWith("*"))
        {
            var prefix = pattern[..^1];
            var keysToRemove = _cacheKeys.Keys.Where(k => k.StartsWith(prefix)).ToList();
            
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
            }
            
            _logger.LogInformation("Removed {Count} cache entries matching pattern: {Pattern}", 
                keysToRemove.Count, pattern);
        }
        else
        {
            _logger.LogWarning("Unsupported cache pattern: {Pattern}. Only prefix patterns (ending with *) are supported.", 
                pattern);
        }
        
        return Task.CompletedTask;
    }
}