using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;

namespace GetFitterGetBigger.API.Services.Cache;

/// <summary>
/// Extensions for CacheLoad to provide auto-caching functionality
/// </summary>
public static class CacheLoadExtensions
{
    /// <summary>
    /// Automatically caches the result on cache miss if the loader returns a valid result
    /// </summary>
    /// <typeparam name="T">The type of data to cache</typeparam>
    /// <param name="builder">The cache load builder</param>
    /// <param name="cacheService">The cache service to use for storing</param>
    /// <param name="cacheKey">The cache key to use</param>
    /// <param name="loader">Function that loads the data when cache miss occurs</param>
    /// <returns>ServiceResult with the cached or loaded data</returns>
    public static async Task<ServiceResult<T>> WithAutoCache<T>(
        this CacheLoadBuilder<T> builder,
        ICacheService cacheService,
        string cacheKey,
        Func<Task<T?>> loader) where T : class
    {
        return await builder.MatchAsync(
            onHit: cached => ServiceResult<T>.Success(cached),
            onMiss: async () =>
            {
                var result = await loader();
                
                // Only cache if we got a valid result (not null/empty)
                if (result != null && !IsEmpty(result))
                {
                    await cacheService.SetAsync(cacheKey, result);
                }
                
                // For DTOs with IsEmpty property (but NOT collections), return NotFound if empty
                if (result != null && IsEmpty(result) && !IsCollection(result))
                {
                    var entityName = typeof(T).Name.Replace("Dto", "");
                    return ServiceResult<T>.Failure(result, ServiceError.NotFound(entityName));
                }
                
                // Return success with the result, or NotFound if null (unless it's a collection)
                if (result == null)
                {
                    if (IsCollectionType<T>())
                    {
                        // For collections, return an empty collection as success
                        var emptyCollection = Activator.CreateInstance<T>();
                        return ServiceResult<T>.Success(emptyCollection);
                    }
                    return ServiceResult<T>.Failure((T)Activator.CreateInstance(typeof(T))!, ServiceError.NotFound(typeof(T).Name.Replace("Dto", "")));
                }
                
                return ServiceResult<T>.Success(result);
            });
    }
    
    /// <summary>
    /// Automatically caches the result on cache miss with logging support
    /// </summary>
    /// <typeparam name="T">The type of data to cache</typeparam>
    /// <param name="builder">The cache load builder with logging</param>
    /// <param name="cacheService">The cache service to use for storing</param>
    /// <param name="cacheKey">The cache key to use</param>
    /// <param name="loader">Function that loads the data when cache miss occurs</param>
    /// <returns>ServiceResult with the cached or loaded data</returns>
    public static async Task<ServiceResult<T>> WithAutoCache<T>(
        this CacheLoadBuilderWithLogging<T> builder,
        ICacheService cacheService,
        string cacheKey,
        Func<Task<T?>> loader) where T : class
    {
        return await builder.MatchAsync(
            onHit: cached => ServiceResult<T>.Success(cached),
            onMiss: async () =>
            {
                var result = await loader();
                
                // Only cache if we got a valid result (not null/empty)
                if (result != null && !IsEmpty(result))
                {
                    await cacheService.SetAsync(cacheKey, result);
                }
                
                // For DTOs with IsEmpty property (but NOT collections), return NotFound if empty
                if (result != null && IsEmpty(result) && !IsCollection(result))
                {
                    var entityName = typeof(T).Name.Replace("Dto", "");
                    return ServiceResult<T>.Failure(result, ServiceError.NotFound(entityName));
                }
                
                // Return success with the result, or NotFound if null (unless it's a collection)
                if (result == null)
                {
                    if (IsCollectionType<T>())
                    {
                        // For collections, return an empty collection as success
                        var emptyCollection = Activator.CreateInstance<T>();
                        return ServiceResult<T>.Success(emptyCollection);
                    }
                    return ServiceResult<T>.Failure((T)Activator.CreateInstance(typeof(T))!, ServiceError.NotFound(typeof(T).Name.Replace("Dto", "")));
                }
                
                return ServiceResult<T>.Success(result);
            });
    }
    
    /// <summary>
    /// Checks if the result is considered empty and should not be cached
    /// </summary>
    private static bool IsEmpty<T>(T result) where T : class
    {
        // Check for IEnumerable (collections)
        if (result is System.Collections.IEnumerable enumerable && !(result is string))
        {
            return !enumerable.Cast<object>().Any();
        }
        
        // Check for IsEmpty property (DTOs implementing empty pattern)
        var isEmptyProperty = result.GetType().GetProperty("IsEmpty");
        if (isEmptyProperty?.GetValue(result) is bool isEmpty)
        {
            return isEmpty;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if the given instance is a collection type
    /// </summary>
    private static bool IsCollection<T>(T result) where T : class
    {
        return result is System.Collections.IEnumerable && !(result is string);
    }
    
    /// <summary>
    /// Checks if the type T is a collection type
    /// </summary>
    private static bool IsCollectionType<T>()
    {
        var type = typeof(T);
        return type != typeof(string) && 
               (type.IsArray || 
                type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
    }
}

/// <summary>
/// Provides generic cache key generation for reference tables
/// </summary>
public static class CacheKeyGenerator
{
    /// <summary>
    /// Generates a standardized cache key based on DTO type and operation parameters
    /// </summary>
    /// <typeparam name="TDto">The DTO type to extract entity name from</typeparam>
    /// <param name="operation">The operation identifier (e.g., "byName", "byBodyPart")</param>
    /// <param name="parameters">The parameters to include in the cache key</param>
    /// <returns>A standardized cache key</returns>
    public static string Generate<TDto>(string operation, params object[] parameters)
    {
        var entityName = typeof(TDto).Name.Replace("Dto", "");
        var paramString = parameters.Length > 0 
            ? string.Join(":", parameters.Select(p => p?.ToString()?.ToLowerInvariant() ?? ""))
            : "";
        
        return string.IsNullOrEmpty(paramString) 
            ? $"{entityName}:{operation}"
            : $"{entityName}:{operation}:{paramString}";
    }
    
    /// <summary>
    /// Generates a cache key for "all" operations
    /// </summary>
    /// <typeparam name="TDto">The DTO type to extract entity name from</typeparam>
    /// <returns>A cache key for retrieving all entities</returns>
    public static string GenerateForAll<TDto>()
    {
        return Generate<TDto>("all");
    }
    
    /// <summary>
    /// Generates a cache key for "byId" operations
    /// </summary>
    /// <typeparam name="TDto">The DTO type to extract entity name from</typeparam>
    /// <param name="id">The entity ID</param>
    /// <returns>A cache key for retrieving entity by ID</returns>
    public static string GenerateForId<TDto>(object id)
    {
        return Generate<TDto>("byId", id);
    }
}