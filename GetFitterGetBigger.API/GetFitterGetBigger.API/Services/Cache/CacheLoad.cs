using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Cache;

/// <summary>
/// Provides a fluent API for loading data from cache with explicit hit/miss handling
/// </summary>
public static class CacheLoad
{
    /// <summary>
    /// Starts a cache load operation for the specified cache key
    /// </summary>
    /// <typeparam name="T">The type of data to load from cache</typeparam>
    /// <param name="cacheService">The cache service to use</param>
    /// <param name="cacheKey">The cache key to load</param>
    /// <returns>A cache load builder for chaining operations</returns>
    public static CacheLoadBuilder<T> For<T>(ICacheService cacheService, string cacheKey) where T : class
    {
        return new CacheLoadBuilder<T>(cacheService, cacheKey);
    }
}

/// <summary>
/// Builder for fluent cache load operations
/// </summary>
/// <typeparam name="T">The type of data being loaded from cache</typeparam>
public class CacheLoadBuilder<T> where T : class
{
    private readonly ICacheService _cacheService;
    private readonly string _cacheKey;
    private readonly Task<CacheResult<T>> _cacheResultTask;

    internal CacheLoadBuilder(ICacheService cacheService, string cacheKey)
    {
        _cacheService = cacheService;
        _cacheKey = cacheKey;
        _cacheResultTask = LoadFromCacheAsync();
    }

    private async Task<CacheResult<T>> LoadFromCacheAsync()
    {
        var value = await _cacheService.GetAsync<T>(_cacheKey);
        return value != null ? CacheResult<T>.Hit(value) : CacheResult<T>.Miss();
    }

    /// <summary>
    /// Executes different logic based on cache hit or miss
    /// </summary>
    /// <typeparam name="TResult">The type of result to return</typeparam>
    /// <param name="onHit">Function to execute when cache hit occurs</param>
    /// <param name="onMiss">Function to execute when cache miss occurs</param>
    /// <returns>The result from either the hit or miss handler</returns>
    public async Task<TResult> MatchAsync<TResult>(
        Func<T, TResult> onHit,
        Func<Task<TResult>> onMiss)
    {
        var cacheResult = await _cacheResultTask;
        return cacheResult.IsHit 
            ? onHit(cacheResult.Value) 
            : await onMiss();
    }

    /// <summary>
    /// Executes different async logic based on cache hit or miss
    /// </summary>
    /// <typeparam name="TResult">The type of result to return</typeparam>
    /// <param name="onHit">Async function to execute when cache hit occurs</param>
    /// <param name="onMiss">Async function to execute when cache miss occurs</param>
    /// <returns>The result from either the hit or miss handler</returns>
    public async Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onHit,
        Func<Task<TResult>> onMiss)
    {
        var cacheResult = await _cacheResultTask;
        return cacheResult.IsHit 
            ? await onHit(cacheResult.Value) 
            : await onMiss();
    }

    /// <summary>
    /// Provides access to the cache key for operations that need it
    /// </summary>
    public string CacheKey => _cacheKey;
    
    /// <summary>
    /// Provides a builder with logging support
    /// </summary>
    /// <param name="logger">The logger to use for cache hit/miss logging</param>
    /// <param name="entityType">The type name for logging purposes</param>
    /// <returns>A cache load builder with logging</returns>
    public CacheLoadBuilderWithLogging<T> WithLogging(Microsoft.Extensions.Logging.ILogger logger, string entityType)
    {
        return new CacheLoadBuilderWithLogging<T>(_cacheResultTask, _cacheKey, logger, entityType);
    }
}

/// <summary>
/// Builder for fluent cache load operations with automatic logging
/// </summary>
/// <typeparam name="T">The type of data being loaded from cache</typeparam>
public class CacheLoadBuilderWithLogging<T> where T : class
{
    private readonly string _cacheKey;
    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly string _entityType;
    private readonly Task<CacheResult<T>> _cacheResultTask;

    internal CacheLoadBuilderWithLogging(
        Task<CacheResult<T>> cacheResultTask,
        string cacheKey, 
        Microsoft.Extensions.Logging.ILogger logger, 
        string entityType)
    {
        _cacheResultTask = cacheResultTask;
        _cacheKey = cacheKey;
        _logger = logger;
        _entityType = entityType;
    }

    /// <summary>
    /// Executes different logic based on cache hit or miss with automatic logging
    /// </summary>
    /// <typeparam name="TResult">The type of result to return</typeparam>
    /// <param name="onHit">Function to execute when cache hit occurs</param>
    /// <param name="onMiss">Function to execute when cache miss occurs</param>
    /// <returns>The result from either the hit or miss handler</returns>
    public async Task<TResult> MatchAsync<TResult>(
        Func<T, TResult> onHit,
        Func<Task<TResult>> onMiss)
    {
        var cacheResult = await _cacheResultTask;
        
        if (cacheResult.IsHit)
        {
            _logger.LogDebug("Cache hit for {EntityType}: {CacheKey}", _entityType, _cacheKey);
            return onHit(cacheResult.Value);
        }
        else
        {
            _logger.LogDebug("Cache miss for {EntityType}: {CacheKey}", _entityType, _cacheKey);
            return await onMiss();
        }
    }

    /// <summary>
    /// Executes different async logic based on cache hit or miss with automatic logging
    /// </summary>
    /// <typeparam name="TResult">The type of result to return</typeparam>
    /// <param name="onHit">Async function to execute when cache hit occurs</param>
    /// <param name="onMiss">Async function to execute when cache miss occurs</param>
    /// <returns>The result from either the hit or miss handler</returns>
    public async Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onHit,
        Func<Task<TResult>> onMiss)
    {
        var cacheResult = await _cacheResultTask;
        
        if (cacheResult.IsHit)
        {
            _logger.LogDebug("Cache hit for {EntityType}: {CacheKey}", _entityType, _cacheKey);
            return await onHit(cacheResult.Value);
        }
        else
        {
            _logger.LogDebug("Cache miss for {EntityType}: {CacheKey}", _entityType, _cacheKey);
            return await onMiss();
        }
    }
}