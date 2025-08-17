using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;

/// <summary>
/// Wrapper class that tracks sorting state for IQueryable chains.
/// Enables conditional sorting and default fallback patterns.
/// </summary>
public class SortableQuery<T>
{
    private readonly IQueryable<T> _query;
    private bool _hasSorting;
    
    /// <summary>
    /// Gets the underlying query
    /// </summary>
    public IQueryable<T> Query => _query;
    
    /// <summary>
    /// Indicates whether any sorting has been applied
    /// </summary>
    public bool HasSorting => _hasSorting;
    
    public SortableQuery(IQueryable<T> query, bool hasSorting = false)
    {
        _query = query;
        _hasSorting = hasSorting;
    }
    
    /// <summary>
    /// Applies sorting if the specified field matches the sortBy parameter
    /// </summary>
    /// <param name="field">The field name to match against sortBy</param>
    /// <param name="sortBy">The current sort field from the request</param>
    /// <param name="sortOrder">The sort order (asc/desc)</param>
    /// <param name="sortFunc">Function to apply the sorting</param>
    /// <returns>Updated sortable query</returns>
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
    /// Applies a default sort if no sorting has been applied yet
    /// </summary>
    /// <param name="defaultSortFunc">The default sorting function</param>
    /// <returns>The final query with sorting applied</returns>
    public IQueryable<T> WithDefaultSort(Func<IQueryable<T>, IQueryable<T>> defaultSortFunc)
    {
        return _hasSorting ? _query : defaultSortFunc(_query);
    }
}

/// <summary>
/// Extension methods for creating and working with SortableQuery
/// </summary>
public static class SortableQueryExtensions
{
    /// <summary>
    /// Converts an IQueryable to a SortableQuery for fluent sorting operations
    /// </summary>
    public static SortableQuery<T> ToSortable<T>(this IQueryable<T> query)
    {
        return new SortableQuery<T>(query);
    }
    
    /// <summary>
    /// Specialized extension for WorkoutTemplate sorting by name
    /// </summary>
    public static SortableQuery<WorkoutTemplateEntity> ApplySortByName(
        this SortableQuery<WorkoutTemplateEntity> sortable,
        string? sortBy,
        string? sortOrder)
    {
        return sortable.ApplySortIf("name", sortBy, sortOrder, 
            (query, desc) => desc 
                ? query.OrderByDescending(wt => wt.Name)
                : query.OrderBy(wt => wt.Name));
    }
    
    /// <summary>
    /// Specialized extension for WorkoutTemplate sorting by created date
    /// </summary>
    public static SortableQuery<WorkoutTemplateEntity> ApplySortByCreatedAt(
        this SortableQuery<WorkoutTemplateEntity> sortable,
        string? sortBy,
        string? sortOrder)
    {
        return sortable.ApplySortIf("createdat", sortBy, sortOrder,
            (query, desc) => desc
                ? query.OrderByDescending(wt => wt.CreatedAt)
                : query.OrderBy(wt => wt.CreatedAt));
    }
    
    /// <summary>
    /// Specialized extension for WorkoutTemplate sorting by updated date
    /// </summary>
    public static SortableQuery<WorkoutTemplateEntity> ApplySortByUpdatedAt(
        this SortableQuery<WorkoutTemplateEntity> sortable,
        string? sortBy,
        string? sortOrder)
    {
        return sortable.ApplySortIf("updatedat", sortBy, sortOrder,
            (query, desc) => desc
                ? query.OrderByDescending(wt => wt.UpdatedAt)
                : query.OrderBy(wt => wt.UpdatedAt));
    }
    
    /// <summary>
    /// Specialized extension for WorkoutTemplate sorting by difficulty
    /// </summary>
    public static SortableQuery<WorkoutTemplateEntity> ApplySortByDifficulty(
        this SortableQuery<WorkoutTemplateEntity> sortable,
        string? sortBy,
        string? sortOrder)
    {
        return sortable.ApplySortIf("difficulty", sortBy, sortOrder,
            (query, desc) => desc
                ? query.OrderByDescending(wt => wt.Difficulty.Value)
                : query.OrderBy(wt => wt.Difficulty.Value));
    }
    
    /// <summary>
    /// Specialized extension for WorkoutTemplate sorting by category
    /// </summary>
    public static SortableQuery<WorkoutTemplateEntity> ApplySortByCategory(
        this SortableQuery<WorkoutTemplateEntity> sortable,
        string? sortBy,
        string? sortOrder)
    {
        return sortable.ApplySortIf("category", sortBy, sortOrder,
            (query, desc) => desc
                ? query.OrderByDescending(wt => wt.Category.Value)
                : query.OrderBy(wt => wt.Category.Value));
    }
    
    /// <summary>
    /// Applies default sorting for WorkoutTemplate (by UpdatedAt descending)
    /// </summary>
    public static IQueryable<WorkoutTemplateEntity> WithDefaultWorkoutTemplateSort(
        this SortableQuery<WorkoutTemplateEntity> sortable)
    {
        return sortable.WithDefaultSort(query => query.OrderByDescending(wt => wt.UpdatedAt));
    }
}