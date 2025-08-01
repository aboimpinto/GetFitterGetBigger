using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Extensions;

/// <summary>
/// Extension methods for ICacheService to provide CacheResult functionality
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Gets a value from cache and returns a CacheResult indicating hit or miss
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="cacheService">The cache service</param>
    /// <param name="key">The cache key</param>
    /// <returns>CacheResult indicating hit/miss and containing the value if hit</returns>
    public static async Task<CacheResult<T>> GetWithResultAsync<T>(this ICacheService cacheService, string key) 
        where T : class
    {
        var value = await cacheService.GetAsync<T>(key);
        return value switch
        {
            not null => CacheResult<T>.Hit(value),
            null => CacheResult<T>.Miss()
        };
    }
}