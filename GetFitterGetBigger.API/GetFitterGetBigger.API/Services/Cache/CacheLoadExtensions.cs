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
            onMiss: async () => await ProcessCacheMissAsync(cacheService, cacheKey, loader));
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
            onMiss: async () => await ProcessCacheMissAsync(cacheService, cacheKey, loader));
    }
    
    /// <summary>
    /// Automatically caches the result on cache miss if the loader returns a valid result (Eternal Cache)
    /// </summary>
    /// <typeparam name="T">The type of data to cache</typeparam>
    /// <param name="builder">The eternal cache load builder</param>
    /// <param name="cacheService">The eternal cache service to use for storing</param>
    /// <param name="cacheKey">The cache key to use</param>
    /// <param name="loader">Function that loads the data when cache miss occurs</param>
    /// <returns>ServiceResult with the cached or loaded data</returns>
    public static async Task<ServiceResult<T>> WithAutoCache<T>(
        this EternalCacheLoadBuilder<T> builder,
        IEternalCacheService cacheService,
        string cacheKey,
        Func<Task<T?>> loader) where T : class
    {
        return await builder.MatchAsync(
            onHit: cached => ServiceResult<T>.Success(cached),
            onMiss: async () => await ProcessEternalCacheMissAsync(cacheService, cacheKey, loader));
    }
    
    /// <summary>
    /// Automatically caches the result on cache miss with logging support (Eternal Cache)
    /// </summary>
    /// <typeparam name="T">The type of data to cache</typeparam>
    /// <param name="builder">The eternal cache load builder with logging</param>
    /// <param name="cacheService">The eternal cache service to use for storing</param>
    /// <param name="cacheKey">The cache key to use</param>
    /// <param name="loader">Function that loads the data when cache miss occurs</param>
    /// <returns>ServiceResult with the cached or loaded data</returns>
    public static async Task<ServiceResult<T>> WithAutoCache<T>(
        this EternalCacheLoadBuilderWithLogging<T> builder,
        IEternalCacheService cacheService,
        string cacheKey,
        Func<Task<T?>> loader) where T : class
    {
        return await builder.MatchAsync(
            onHit: cached => ServiceResult<T>.Success(cached),
            onMiss: async () => await ProcessEternalCacheMissAsync(cacheService, cacheKey, loader));
    }
    
    /// <summary>
    /// Processes a cache miss by loading data and determining the appropriate result
    /// </summary>
    private static async Task<ServiceResult<T>> ProcessCacheMissAsync<T>(
        ICacheService cacheService,
        string cacheKey,
        Func<Task<T?>> loader) where T : class
    {
        var result = await loader();
        
        await CacheResultIfValidAsync(cacheService, cacheKey, result);
        
        return CreateServiceResult(result);
    }
    
    /// <summary>
    /// Caches the result if it's valid (not null/empty)
    /// </summary>
    private static async Task CacheResultIfValidAsync<T>(
        ICacheService cacheService,
        string cacheKey,
        T? result) where T : class
    {
        if (result != null && !IsEmpty(result))
        {
            await cacheService.SetAsync(cacheKey, result);
        }
    }
    
    /// <summary>
    /// Processes a cache miss by loading data for eternal cache service
    /// </summary>
    private static async Task<ServiceResult<T>> ProcessEternalCacheMissAsync<T>(
        IEternalCacheService cacheService,
        string cacheKey,
        Func<Task<T?>> loader) where T : class
    {
        var result = await loader();
        
        await CacheEternalResultIfValidAsync(cacheService, cacheKey, result);
        
        return CreateServiceResult(result);
    }
    
    /// <summary>
    /// Caches the result if it's valid (not null/empty) using eternal cache service
    /// </summary>
    private static async Task CacheEternalResultIfValidAsync<T>(
        IEternalCacheService cacheService,
        string cacheKey,
        T? result) where T : class
    {
        if (result != null && !IsEmpty(result))
        {
            await cacheService.SetAsync(cacheKey, result);
        }
    }
    
    /// <summary>
    /// Creates the appropriate ServiceResult based on the loaded data
    /// </summary>
    private static ServiceResult<T> CreateServiceResult<T>(T? result) where T : class
    {
        return result switch
        {
            null => HandleNullResult<T>(),
            _ when IsEmpty(result) && !IsCollection(result) => CreateNotFoundForEmptyDto(result),
            _ => ServiceResult<T>.Success(result)
        };
    }
    
    /// <summary>
    /// Handles null results based on whether the type is a collection
    /// </summary>
    private static ServiceResult<T> HandleNullResult<T>() where T : class
    {
        if (IsCollectionType<T>())
        {
            var emptyCollection = CreateEmptyCollection<T>();
            return ServiceResult<T>.Success(emptyCollection);
        }
        
        var emptyInstance = (T)Activator.CreateInstance(typeof(T))!;
        var entityName = GetEntityNameFromType<T>();
        return ServiceResult<T>.Failure(emptyInstance, ServiceError.NotFound(entityName));
    }
    
    /// <summary>
    /// Creates an empty collection instance for various collection types
    /// </summary>
    private static T CreateEmptyCollection<T>() where T : class
    {
        var type = typeof(T);
        
        // Handle arrays
        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            var emptyArray = Array.CreateInstance(elementType, 0);
            return (T)(object)emptyArray;
        }
        
        // Handle generic collections that have parameterless constructors
        try
        {
            return Activator.CreateInstance<T>();
        }
        catch
        {
            // Fallback - this shouldn't happen for well-designed collection types
            throw new InvalidOperationException($"Cannot create empty instance of collection type {type.Name}");
        }
    }
    
    /// <summary>
    /// Creates a NotFound result for empty DTOs
    /// </summary>
    private static ServiceResult<T> CreateNotFoundForEmptyDto<T>(T result) where T : class
    {
        var entityName = GetEntityNameFromType<T>();
        return ServiceResult<T>.Failure(result, ServiceError.NotFound(entityName));
    }
    
    /// <summary>
    /// Extracts entity name from type name (removes "Dto" suffix)
    /// </summary>
    private static string GetEntityNameFromType<T>()
    {
        return typeof(T).Name.Replace("Dto", "");
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