namespace GetFitterGetBigger.API.Models;

/// <summary>
/// Defines caching strategies for different entity types
/// </summary>
public enum CacheStrategy
{
    /// <summary>
    /// Cache forever until application restart (Pure References)
    /// </summary>
    Eternal,
    
    /// <summary>
    /// Cache with automatic invalidation on modifications (Enhanced References)
    /// </summary>
    Invalidatable,
    
    /// <summary>
    /// Brief caching for frequently accessed data (Domain Entities)
    /// </summary>
    ShortLived,
    
    /// <summary>
    /// No caching at all
    /// </summary>
    None
}

/// <summary>
/// Interface for entities that support caching
/// </summary>
public interface ICacheableEntity
{
    /// <summary>
    /// Gets the caching strategy for this entity type
    /// </summary>
    /// <returns>The appropriate cache strategy</returns>
    CacheStrategy GetCacheStrategy();
    
    /// <summary>
    /// Gets the cache duration for this entity type
    /// </summary>
    /// <returns>The cache duration, or null if using eternal caching</returns>
    TimeSpan? GetCacheDuration();
}