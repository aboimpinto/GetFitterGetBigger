using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Implementation of cache service using IMemoryCache
/// Implements both ICacheService (legacy nullable) and IEmptyEnabledCacheService (CacheResult)
/// </summary>
public class CacheService : IEmptyEnabledCacheService
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
    /// Gets a value from the cache with explicit cache hit/miss result
    /// </summary>
    public Task<CacheResult<T>> GetAsync<T>(string key) where T : class
    {
        var value = _memoryCache.Get<T>(key);
        var result = value != null ? CacheResult<T>.Hit(value) : CacheResult<T>.Miss();
        return Task.FromResult(result);
    }

    /// <summary>
    /// Gets a value from the cache, returning Empty for cache misses
    /// </summary>
    public async Task<T> GetOrEmptyAsync<T>(string key) where T : class, IEmptyEntity<T>
    {
        var result = await GetAsync<T>(key);
        return result switch
        {
            { IsHit: true } => result.Value,
            _ => T.Empty
        };
    }

    /// <summary>
    /// Legacy method for backward compatibility - returns null for cache miss
    /// </summary>
    [Obsolete("This method returns null and will be removed after Empty pattern migration. " +
              "Use GetAsync<T>(key) for CacheResult<T>, or GetOrEmptyAsync<T>(key) for types implementing IEmptyEntity<T>.")]
    async Task<T?> ICacheService.GetAsync<T>(string key) where T : class
    {
        var result = await GetAsync<T>(key);
        return result switch
        {
            { IsHit: true } => result.Value,
            _ => null
        };
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This method returns a Task for interface compatibility, but the operation is synchronous
    /// since we're using in-memory cache (IMemoryCache). If we migrate to a distributed cache
    /// (Redis, SQL, etc.) in the future, the async signature will be necessary.
    /// </remarks>
    public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(expiration)
            .RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
            {
                _cacheKeys.TryRemove(evictedKey.ToString()!, out _);
            });

        _memoryCache.Set(key, value, cacheEntryOptions);
        _cacheKeys.TryAdd(key, true);
        
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This method returns a Task for interface compatibility, but the operation is synchronous
    /// since we're using in-memory cache (IMemoryCache). If we migrate to a distributed cache
    /// (Redis, SQL, etc.) in the future, the async signature will be necessary.
    /// </remarks>
    public Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        _cacheKeys.TryRemove(key, out _);
        
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This method returns a Task for interface compatibility, but the operation is synchronous
    /// since we're using in-memory cache (IMemoryCache). If we migrate to a distributed cache
    /// (Redis, SQL, etc.) in the future, the async signature will be necessary.
    /// </remarks>
    public Task RemoveByPatternAsync(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return Task.CompletedTask;

        // Handle wildcard pattern (e.g., "MuscleGroups:*" should match all keys starting with "MuscleGroups:")
        var actualPrefix = pattern.EndsWith("*") ? pattern[..^1] : pattern;
        var keysToRemove = _cacheKeys.Keys.Where(k => k.StartsWith(actualPrefix)).ToList();
        
        if (keysToRemove.Count > 0)
        {
            _logger.LogDebug("[Cache] Removing {Count} keys matching pattern: {Pattern}", keysToRemove.Count, pattern);
        }
        
        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
        }
        
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration) where T : class
    {
        // Try to get from cache using the new CacheResult pattern
        var cacheResult = await GetAsync<T>(key);
        
        // Use pattern matching on CacheResult
        return cacheResult switch
        {
            { IsHit: true } => cacheResult.Value,
            _ => await CreateAndCacheAsync(key, factory, expiration)
        };
    }
    
    private async Task<T> CreateAndCacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration) where T : class
    {
        var value = await factory();
        
        if (value != null)
        {
            await SetAsync(key, value, expiration);
        }
        
        return value!;
    }

    /// <summary>
    /// Gets or creates a cached value using the Empty pattern
    /// </summary>
    public async Task<T> GetOrCreateEmptyAwareAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration) 
        where T : class, IEmptyEntity<T>
    {
        // Try to get from cache using CacheResult pattern
        var cacheResult = await GetAsync<T>(key);
        
        return cacheResult switch
        {
            { IsHit: true } => cacheResult.Value,
            _ => await CreateAndCacheEmptyAwareAsync(key, factory, expiration)
        };
    }
    
    private async Task<T> CreateAndCacheEmptyAwareAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration) 
        where T : class, IEmptyEntity<T>
    {
        var value = await factory();
        
        // Only cache non-empty values
        if (!value.IsEmpty)
        {
            await SetAsync(key, value, expiration);
        }
        
        return value;
    }
}