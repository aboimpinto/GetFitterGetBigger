using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Extensions;

/// <summary>
/// Extension methods for building WorkoutTemplate queries in a fluent manner
/// </summary>
public static class WorkoutTemplateQueryExtensions
{
    /// <summary>
    /// Applies a name pattern filter to the workout template query (case-insensitive)
    /// </summary>
    /// <param name="query">The workout template query</param>
    /// <param name="namePattern">The name pattern to search for</param>
    /// <returns>The filtered query</returns>
    public static IQueryable<WorkoutTemplate> ApplyNamePatternFilter(
        this IQueryable<WorkoutTemplate> query, string namePattern)
    {
        if (string.IsNullOrEmpty(namePattern))
            return query;
            
        // Use standard LINQ for case-insensitive search that works everywhere
        // EF Core will translate this to appropriate SQL (ILIKE for PostgreSQL, LIKE LOWER for others)
        var lowerPattern = namePattern.ToLower();
        return query.Where(w => w.Name != null && w.Name.ToLower().Contains(lowerPattern));
    }
    
    /// <summary>
    /// Applies a category filter to the workout template query
    /// </summary>
    /// <param name="query">The workout template query</param>
    /// <param name="categoryId">The category ID to filter by</param>
    /// <returns>The filtered query</returns>
    public static IQueryable<WorkoutTemplate> ApplyCategoryFilter(
        this IQueryable<WorkoutTemplate> query, WorkoutCategoryId categoryId)
    {
        return categoryId.IsEmpty 
            ? query 
            : query.Where(w => w.CategoryId == categoryId);
    }
    
    /// <summary>
    /// Applies a difficulty filter to the workout template query
    /// </summary>
    /// <param name="query">The workout template query</param>
    /// <param name="difficultyId">The difficulty level ID to filter by</param>
    /// <returns>The filtered query</returns>
    public static IQueryable<WorkoutTemplate> ApplyDifficultyFilter(
        this IQueryable<WorkoutTemplate> query, DifficultyLevelId difficultyId)
    {
        return difficultyId.IsEmpty 
            ? query 
            : query.Where(w => w.DifficultyId == difficultyId);
    }
    
    /// <summary>
    /// Applies an objective filter to the workout template query
    /// </summary>
    /// <param name="query">The workout template query</param>
    /// <param name="objectiveId">The workout objective ID to filter by</param>
    /// <returns>The filtered query</returns>
    public static IQueryable<WorkoutTemplate> ApplyObjectiveFilter(
        this IQueryable<WorkoutTemplate> query, WorkoutObjectiveId objectiveId)
    {
        return objectiveId.IsEmpty 
            ? query 
            : query.Where(w => w.Objectives != null && w.Objectives.Any(o => o.WorkoutObjectiveId == objectiveId));
    }
    
    /// <summary>
    /// Applies a state filter to the workout template query
    /// </summary>
    /// <param name="query">The workout template query</param>
    /// <param name="stateId">The workout state ID to filter by</param>
    /// <returns>The filtered query</returns>
    public static IQueryable<WorkoutTemplate> ApplyStateFilter(
        this IQueryable<WorkoutTemplate> query, WorkoutStateId stateId)
    {
        return stateId.IsEmpty 
            ? query 
            : query.Where(w => w.WorkoutStateId == stateId);
    }
    
    /// <summary>
    /// Excludes archived templates from the query
    /// </summary>
    /// <param name="query">The workout template query</param>
    /// <returns>The filtered query excluding archived templates</returns>
    public static IQueryable<WorkoutTemplate> ExcludeArchived(
        this IQueryable<WorkoutTemplate> query)
    {
        // This assumes there's a way to identify archived state
        // We'll need to filter by state value since we don't have the ID constant
        return query.Where(w => w.WorkoutState != null && w.WorkoutState.Value != "ARCHIVED");
    }
    
    /// <summary>
    /// Applies state filter if provided, otherwise excludes archived templates
    /// </summary>
    /// <param name="query">The workout template query</param>
    /// <param name="stateId">The workout state ID to filter by (optional)</param>
    /// <returns>The filtered query</returns>
    public static IQueryable<WorkoutTemplate> ApplyStateFilterOrExcludeArchived(
        this IQueryable<WorkoutTemplate> query, WorkoutStateId stateId)
    {
        return stateId.IsEmpty 
            ? query.ExcludeArchived()
            : query.ApplyStateFilter(stateId);
    }
    
    /// <summary>
    /// Converts a query to a sortable query for fluent sorting
    /// </summary>
    /// <param name="query">The query to make sortable</param>
    /// <returns>A sortable query wrapper</returns>
    public static SortableQuery<WorkoutTemplate> ToSortable(
        this IQueryable<WorkoutTemplate> query)
    {
        return new SortableQuery<WorkoutTemplate>(query);
    }
    
    /// <summary>
    /// Applies sorting by name if the sortBy field matches
    /// </summary>
    public static SortableQuery<WorkoutTemplate> ApplySortByName(
        this SortableQuery<WorkoutTemplate> sortable,
        string? sortBy,
        string? sortOrder)
    {
        return sortable.ApplySortIf("name", sortBy, sortOrder,
            (query, desc) => desc
                ? query.OrderByDescending(w => w.Name)
                : query.OrderBy(w => w.Name));
    }
    
    /// <summary>
    /// Applies sorting by created date if the sortBy field matches
    /// </summary>
    public static SortableQuery<WorkoutTemplate> ApplySortByCreatedAt(
        this SortableQuery<WorkoutTemplate> sortable,
        string? sortBy,
        string? sortOrder)
    {
        return sortable
            .ApplySortIf("createdat", sortBy, sortOrder,
                (query, desc) => desc
                    ? query.OrderByDescending(w => w.CreatedAt)
                    : query.OrderBy(w => w.CreatedAt))
            .ApplySortIf("created", sortBy, sortOrder,
                (query, desc) => desc
                    ? query.OrderByDescending(w => w.CreatedAt)
                    : query.OrderBy(w => w.CreatedAt));
    }
    
    /// <summary>
    /// Applies sorting by updated date if the sortBy field matches
    /// </summary>
    public static SortableQuery<WorkoutTemplate> ApplySortByUpdatedAt(
        this SortableQuery<WorkoutTemplate> sortable,
        string? sortBy,
        string? sortOrder)
    {
        return sortable
            .ApplySortIf("updatedat", sortBy, sortOrder,
                (query, desc) => desc
                    ? query.OrderByDescending(w => w.UpdatedAt)
                    : query.OrderBy(w => w.UpdatedAt))
            .ApplySortIf("updated", sortBy, sortOrder,
                (query, desc) => desc
                    ? query.OrderByDescending(w => w.UpdatedAt)
                    : query.OrderBy(w => w.UpdatedAt))
            .ApplySortIf("lastmodified", sortBy, sortOrder,
                (query, desc) => desc
                    ? query.OrderByDescending(w => w.UpdatedAt)
                    : query.OrderBy(w => w.UpdatedAt));
    }
    
    /// <summary>
    /// Applies sorting by duration if the sortBy field matches
    /// </summary>
    public static SortableQuery<WorkoutTemplate> ApplySortByDuration(
        this SortableQuery<WorkoutTemplate> sortable,
        string? sortBy,
        string? sortOrder)
    {
        return sortable.ApplySortIf("duration", sortBy, sortOrder,
            (query, desc) => desc
                ? query.OrderByDescending(w => w.EstimatedDurationMinutes)
                : query.OrderBy(w => w.EstimatedDurationMinutes));
    }
    
    /// <summary>
    /// Applies sorting by category if the sortBy field matches
    /// </summary>
    public static SortableQuery<WorkoutTemplate> ApplySortByCategory(
        this SortableQuery<WorkoutTemplate> sortable,
        string? sortBy,
        string? sortOrder)
    {
        return sortable.ApplySortIf("category", sortBy, sortOrder,
            (query, desc) => desc
                ? query.OrderByDescending(w => w.Category != null ? w.Category.Value : "")
                : query.OrderBy(w => w.Category != null ? w.Category.Value : ""));
    }
    
    /// <summary>
    /// Applies sorting by difficulty if the sortBy field matches
    /// </summary>
    public static SortableQuery<WorkoutTemplate> ApplySortByDifficulty(
        this SortableQuery<WorkoutTemplate> sortable,
        string? sortBy,
        string? sortOrder)
    {
        return sortable.ApplySortIf("difficulty", sortBy, sortOrder,
            (query, desc) => desc
                ? query.OrderByDescending(w => w.Difficulty != null ? w.Difficulty.DisplayOrder : 0)
                : query.OrderBy(w => w.Difficulty != null ? w.Difficulty.DisplayOrder : 0));
    }
    
    /// <summary>
    /// Applies the default sort for workout templates (by name ascending)
    /// </summary>
    public static IQueryable<WorkoutTemplate> WithDefaultWorkoutTemplateSort(
        this SortableQuery<WorkoutTemplate> sortable)
    {
        return sortable.WithDefaultSort(q => q.OrderBy(w => w.Name));
    }
    
    /// <summary>
    /// Applies sorting to the workout template query using the fluent sorting pattern
    /// </summary>
    /// <param name="query">The workout template query</param>
    /// <param name="sortBy">The field to sort by</param>
    /// <param name="sortOrder">The sort order (asc/desc)</param>
    /// <returns>The sorted query</returns>
    public static IQueryable<WorkoutTemplate> ApplySorting(
        this IQueryable<WorkoutTemplate> query, string sortBy, string sortOrder)
    {
        return query
            .ToSortable()
            .ApplySortByName(sortBy, sortOrder)
            .ApplySortByCreatedAt(sortBy, sortOrder)
            .ApplySortByUpdatedAt(sortBy, sortOrder)
            .ApplySortByDuration(sortBy, sortOrder)
            .ApplySortByCategory(sortBy, sortOrder)
            .ApplySortByDifficulty(sortBy, sortOrder)
            .WithDefaultWorkoutTemplateSort();
    }
    
    /// <summary>
    /// Applies pagination to the workout template query
    /// </summary>
    /// <param name="query">The workout template query</param>
    /// <param name="page">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <returns>The paginated query</returns>
    public static IQueryable<WorkoutTemplate> ApplyPaging(
        this IQueryable<WorkoutTemplate> query, int page, int pageSize)
    {
        // Ensure valid page and pageSize values
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize; // Cap at 100 items per page
        
        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }
}