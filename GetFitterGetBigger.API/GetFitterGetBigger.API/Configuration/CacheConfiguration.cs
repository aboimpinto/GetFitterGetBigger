namespace GetFitterGetBigger.API.Configuration;

/// <summary>
/// Configuration settings for caching
/// </summary>
public class CacheConfiguration
{
    /// <summary>
    /// Configuration for static reference tables
    /// </summary>
    public TableCacheConfiguration StaticTables { get; set; } = new();

    /// <summary>
    /// Configuration for dynamic reference tables
    /// </summary>
    public TableCacheConfiguration DynamicTables { get; set; } = new();

    /// <summary>
    /// Gets the cache duration for a specific table
    /// </summary>
    /// <param name="tableName">The name of the table</param>
    /// <returns>The cache duration for the table</returns>
    public TimeSpan GetCacheDuration(string tableName)
    {
        if (StaticTables.Tables.Contains(tableName))
        {
            return TimeSpan.FromHours(StaticTables.DurationInHours);
        }
        
        if (DynamicTables.Tables.Contains(tableName))
        {
            return TimeSpan.FromHours(DynamicTables.DurationInHours);
        }

        // Default to dynamic table duration if not configured
        return TimeSpan.FromHours(DynamicTables.DurationInHours);
    }
}

/// <summary>
/// Configuration for a group of tables with the same cache duration
/// </summary>
public class TableCacheConfiguration
{
    /// <summary>
    /// Cache duration in hours
    /// </summary>
    public int DurationInHours { get; set; }

    /// <summary>
    /// List of table names in this group
    /// </summary>
    public List<string> Tables { get; set; } = new();
}