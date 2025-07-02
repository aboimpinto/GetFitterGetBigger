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
                    _logger.LogDebug("[Cache EVICTED] Key: {Key} was evicted. Reason: {Reason}", evictedKey, reason);
                });

            _memoryCache.Set(key, value, cacheEntryOptions);
            _cacheKeys.TryAdd(key, true);
            _logger.LogDebug("[Cache SET] Successfully stored value for key: {Key} with sliding expiration: {Expiration}", key, expiration);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Cache ERROR] Failed to set value in cache for key: {Key}", key);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
            _logger.LogDebug("[Cache REMOVE] Successfully removed key: {Key}", key);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Cache ERROR] Failed to remove value from cache for key: {Key}", key);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            // Handle wildcard pattern (e.g., "MuscleGroups:*" should match all keys starting with "MuscleGroups:")
            var actualPrefix = pattern.EndsWith("*") ? pattern.Substring(0, pattern.Length - 1) : pattern;
            var keysToRemove = _cacheKeys.Keys.Where(k => k.StartsWith(actualPrefix)).ToList();
            
            _logger.LogInformation("[Cache PATTERN REMOVE] Removing {Count} keys matching pattern: {Pattern}", keysToRemove.Count, pattern);
            
            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key);
            }
            
            _logger.LogDebug("[Cache PATTERN REMOVE] Completed removal of all keys matching pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Cache ERROR] Failed to remove values from cache by pattern: {Pattern}", pattern);
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

            return value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrCreateAsync for key: {Key}", key);
            // If caching fails, still try to get the value from the factory
            return await factory();
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetOrCreateNullableAsync<T>(string key, Func<Task<T?>> factory, TimeSpan expiration) where T : class
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
            
            // Only cache non-null values
            if (value != null)
            {
                await SetAsync(key, value, expiration);
            }

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrCreateNullableAsync for key: {Key}", key);
            // If caching fails, still try to get the value from the factory
            return await factory();
        }
    }
}