using System.Collections.Concurrent;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Caching.Memory;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Cache service implementation for eternal/immutable Pure Reference data
/// This cache is optimized for data that never changes after application startup
/// </summary>
public class EternalCacheService : IEternalCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<EternalCacheService> _logger;
    
    // Track all keys for potential diagnostics/monitoring
    private readonly ConcurrentDictionary<string, DateTime> _cacheKeys = new();

    public EternalCacheService(IMemoryCache memoryCache, ILogger<EternalCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <summary>
    /// Gets a value from the eternal cache with explicit cache hit/miss result
    /// </summary>
    public Task<CacheResult<T>> GetAsync<T>(string key) where T : class
    {
        var value = _memoryCache.Get<T>(key);
        var result = value != null ? CacheResult<T>.Hit(value) : CacheResult<T>.Miss();
        
        if (result.IsHit)
        {
            _logger.LogTrace("Eternal cache hit for key: {Key}", key);
        }
        else
        {
            _logger.LogDebug("Eternal cache miss for key: {Key}", key);
        }
        
        return Task.FromResult(result);
    }

    /// <summary>
    /// Gets a value from the cache, returning Empty for cache misses
    /// </summary>
    public async Task<T> GetOrEmptyAsync<T>(string key) where T : class, IEmptyEntity<T>
    {
        var result = await GetAsync<T>(key);
        return result.IsHit ? result.Value : T.Empty;
    }

    /// <summary>
    /// Gets or creates a cached value using the Empty pattern
    /// If the factory returns Empty, the value is NOT cached
    /// </summary>
    public async Task<T> GetOrCreateEmptyAwareAsync<T>(string key, Func<Task<T>> factory) 
        where T : class, IEmptyEntity<T>
    {
        var result = await GetAsync<T>(key);
        if (result.IsHit)
        {
            return result.Value;
        }

        var value = await factory();
        
        // Don't cache empty values
        if (!value.IsEmpty)
        {
            await SetAsync(key, value);
        }
        
        return value;
    }

    /// <summary>
    /// Sets a value in the eternal cache with automatic 365-day expiration
    /// Values are cached with a very long expiration (effectively forever)
    /// </summary>
    public Task SetAsync<T>(string key, T value) where T : class
    {
        // For eternal cache, we use a fixed 365-day expiration
        // This is effectively "forever" for most applications
        const int ETERNAL_DAYS = 365;
        var eternalExpiration = TimeSpan.FromDays(ETERNAL_DAYS);
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(eternalExpiration)
            .SetPriority(CacheItemPriority.High); // High priority since this data never changes
        
        _memoryCache.Set(key, value, cacheEntryOptions);
        _cacheKeys[key] = DateTime.UtcNow;
        
        _logger.LogInformation("Eternal cache set for key: {Key} (will expire in {Days} days)", 
            key, eternalExpiration.Days);
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Gets cache statistics for monitoring
    /// </summary>
    public Task<CacheStatistics> GetStatisticsAsync()
    {
        var stats = new CacheStatistics
        {
            TotalKeys = _cacheKeys.Count,
            OldestEntry = _cacheKeys.Values.Any() ? _cacheKeys.Values.Min() : null,
            NewestEntry = _cacheKeys.Values.Any() ? _cacheKeys.Values.Max() : null
        };
        
        return Task.FromResult(stats);
    }
}

/// <summary>
/// Cache statistics for monitoring
/// </summary>
public class CacheStatistics
{
    public int TotalKeys { get; init; }
    public DateTime? OldestEntry { get; init; }
    public DateTime? NewestEntry { get; init; }
}