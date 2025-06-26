namespace GetFitterGetBigger.API.Utilities;

/// <summary>
/// Utility class for generating consistent cache keys
/// </summary>
public static class CacheKeyGenerator
{
    private const string Prefix = "ReferenceTable";

    /// <summary>
    /// Generates a cache key for getting all items from a reference table
    /// </summary>
    /// <param name="tableName">The name of the reference table</param>
    /// <returns>The generated cache key</returns>
    public static string GetAllKey(string tableName)
    {
        return $"{Prefix}:{tableName}:GetAll";
    }

    /// <summary>
    /// Generates a cache key for getting an item by ID
    /// </summary>
    /// <param name="tableName">The name of the reference table</param>
    /// <param name="id">The ID of the item</param>
    /// <returns>The generated cache key</returns>
    public static string GetByIdKey(string tableName, string id)
    {
        return $"{Prefix}:{tableName}:GetById:{id}";
    }

    /// <summary>
    /// Generates a cache key for getting an item by value
    /// </summary>
    /// <param name="tableName">The name of the reference table</param>
    /// <param name="value">The value to search for</param>
    /// <returns>The generated cache key</returns>
    public static string GetByValueKey(string tableName, string value)
    {
        // Normalize the value to handle case-insensitive searches
        var normalizedValue = value?.ToLowerInvariant() ?? string.Empty;
        return $"{Prefix}:{tableName}:GetByValue:{normalizedValue}";
    }

    /// <summary>
    /// Generates a pattern for removing all cache entries for a specific table
    /// </summary>
    /// <param name="tableName">The name of the reference table</param>
    /// <returns>The pattern to match cache keys</returns>
    public static string GetTablePattern(string tableName)
    {
        return $"{Prefix}:{tableName}:";
    }
}