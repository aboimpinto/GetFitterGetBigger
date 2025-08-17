namespace GetFitterGetBigger.API.Extensions;

/// <summary>
/// Wrapper class for IQueryable that tracks sorting state and provides fluent sorting API
/// </summary>
/// <typeparam name="T">The entity type being queried</typeparam>
public class SortableQuery<T>
{
    private readonly IQueryable<T> _query;
    private readonly bool _hasSorting;
    
    /// <summary>
    /// Gets the underlying query
    /// </summary>
    public IQueryable<T> Query => _query;
    
    /// <summary>
    /// Gets whether sorting has been applied
    /// </summary>
    public bool HasSorting => _hasSorting;
    
    /// <summary>
    /// Initializes a new instance of the SortableQuery class
    /// </summary>
    /// <param name="query">The query to wrap</param>
    /// <param name="hasSorting">Whether sorting has already been applied</param>
    public SortableQuery(IQueryable<T> query, bool hasSorting = false)
    {
        _query = query;
        _hasSorting = hasSorting;
    }
    
    /// <summary>
    /// Applies sorting if the field matches the sortBy parameter
    /// </summary>
    /// <param name="field">The field name to match against sortBy</param>
    /// <param name="sortBy">The field to sort by from the request</param>
    /// <param name="sortOrder">The sort order (asc/desc)</param>
    /// <param name="sortFunc">The function to apply sorting</param>
    /// <returns>A new SortableQuery with sorting applied if field matched</returns>
    public SortableQuery<T> ApplySortIf(
        string field, 
        string? sortBy, 
        string? sortOrder,
        Func<IQueryable<T>, bool, IQueryable<T>> sortFunc)
    {
        if (sortBy?.ToLower() == field.ToLower())
        {
            var isDescending = sortOrder?.ToLower() == "desc";
            var sortedQuery = sortFunc(_query, isDescending);
            return new SortableQuery<T>(sortedQuery, hasSorting: true);
        }
        
        return this;
    }
    
    /// <summary>
    /// Applies a default sort if no sorting has been applied
    /// </summary>
    /// <param name="defaultSortFunc">The default sorting function</param>
    /// <returns>The query with sorting applied</returns>
    public IQueryable<T> WithDefaultSort(Func<IQueryable<T>, IQueryable<T>> defaultSortFunc)
    {
        return _hasSorting ? _query : defaultSortFunc(_query);
    }
}