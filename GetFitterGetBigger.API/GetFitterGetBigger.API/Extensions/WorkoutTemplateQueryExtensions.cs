using System.Linq;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.EntityFrameworkCore;

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
    /// Applies sorting to the workout template query
    /// </summary>
    /// <param name="query">The workout template query</param>
    /// <param name="sortBy">The field to sort by</param>
    /// <param name="sortOrder">The sort order (asc/desc)</param>
    /// <returns>The sorted query</returns>
    public static IQueryable<WorkoutTemplate> ApplySorting(
        this IQueryable<WorkoutTemplate> query, string sortBy, string sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";
        
        return sortBy?.ToLower() switch
        {
            "name" => isDescending 
                ? query.OrderByDescending(w => w.Name)
                : query.OrderBy(w => w.Name),
            "createdat" or "created" => isDescending
                ? query.OrderByDescending(w => w.CreatedAt)
                : query.OrderBy(w => w.CreatedAt),
            "updatedat" or "updated" or "lastmodified" => isDescending
                ? query.OrderByDescending(w => w.UpdatedAt)
                : query.OrderBy(w => w.UpdatedAt),
            "duration" => isDescending
                ? query.OrderByDescending(w => w.EstimatedDurationMinutes)
                : query.OrderBy(w => w.EstimatedDurationMinutes),
            "category" => isDescending
                ? query.OrderByDescending(w => w.Category != null ? w.Category.Value : "")
                : query.OrderBy(w => w.Category != null ? w.Category.Value : ""),
            "difficulty" => isDescending
                ? query.OrderByDescending(w => w.Difficulty != null ? w.Difficulty.DisplayOrder : 0)
                : query.OrderBy(w => w.Difficulty != null ? w.Difficulty.DisplayOrder : 0),
            _ => query.OrderBy(w => w.Name) // default sort by name ascending
        };
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