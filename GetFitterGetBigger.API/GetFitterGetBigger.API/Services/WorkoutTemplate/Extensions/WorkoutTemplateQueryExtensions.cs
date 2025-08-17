using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.EntityFrameworkCore;
using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;

/// <summary>
/// Fluent extension methods for WorkoutTemplate query operations.
/// Provides a clean, chainable API for filtering and sorting workout templates.
/// </summary>
public static class WorkoutTemplateQueryExtensions
{
    /// <summary>
    /// Applies name pattern filter using case-insensitive LIKE search
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="namePattern">The name pattern to search for (null/empty = no filter)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<WorkoutTemplateEntity> FilterByNamePattern(
        this IQueryable<WorkoutTemplateEntity> query,
        string? namePattern)
    {
        if (string.IsNullOrWhiteSpace(namePattern))
            return query;

        return query.Where(wt => wt.Name.ToLower().Contains(namePattern.ToLower()));
    }

    /// <summary>
    /// Applies category filter
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="categoryId">The category ID to filter by (empty = no filter)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<WorkoutTemplateEntity> FilterByCategory(
        this IQueryable<WorkoutTemplateEntity> query,
        WorkoutCategoryId? categoryId)
    {
        if (categoryId == null || !categoryId.HasValue || categoryId.Value.IsEmpty)
            return query;

        return query.Where(wt => wt.CategoryId == categoryId.Value);
    }

    /// <summary>
    /// Applies objective filter
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="objectiveId">The objective ID to filter by (empty = no filter)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<WorkoutTemplateEntity> FilterByObjective(
        this IQueryable<WorkoutTemplateEntity> query,
        WorkoutObjectiveId? objectiveId)
    {
        if (objectiveId == null || !objectiveId.HasValue || objectiveId.Value.IsEmpty)
            return query;

        return query.Where(wt => wt.Objectives.Any(o => o.WorkoutObjectiveId == objectiveId.Value));
    }

    /// <summary>
    /// Applies difficulty filter
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="difficultyId">The difficulty ID to filter by (empty = no filter)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<WorkoutTemplateEntity> FilterByDifficulty(
        this IQueryable<WorkoutTemplateEntity> query,
        DifficultyLevelId? difficultyId)
    {
        if (difficultyId == null || !difficultyId.HasValue || difficultyId.Value.IsEmpty)
            return query;

        return query.Where(wt => wt.DifficultyId == difficultyId.Value);
    }

    /// <summary>
    /// Applies workout state filter
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="stateId">The state ID to filter by (empty = no filter)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<WorkoutTemplateEntity> FilterByState(
        this IQueryable<WorkoutTemplateEntity> query,
        WorkoutStateId? stateId)
    {
        if (stateId == null || !stateId.HasValue || stateId.Value.IsEmpty)
            return query;

        return query.Where(wt => wt.WorkoutStateId == stateId.Value);
    }

    /// <summary>
    /// Applies sorting by name
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="descending">True for descending order, false for ascending</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<WorkoutTemplateEntity> SortByName(
        this IQueryable<WorkoutTemplateEntity> query,
        bool descending = false)
    {
        return descending 
            ? query.OrderByDescending(wt => wt.Name)
            : query.OrderBy(wt => wt.Name);
    }

    /// <summary>
    /// Applies sorting by creation date
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="descending">True for descending order, false for ascending</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<WorkoutTemplateEntity> SortByCreatedAt(
        this IQueryable<WorkoutTemplateEntity> query,
        bool descending = false)
    {
        return descending 
            ? query.OrderByDescending(wt => wt.CreatedAt)
            : query.OrderBy(wt => wt.CreatedAt);
    }

    /// <summary>
    /// Applies sorting by update date
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="descending">True for descending order, false for ascending</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<WorkoutTemplateEntity> SortByUpdatedAt(
        this IQueryable<WorkoutTemplateEntity> query,
        bool descending = false)
    {
        return descending 
            ? query.OrderByDescending(wt => wt.UpdatedAt)
            : query.OrderBy(wt => wt.UpdatedAt);
    }

    /// <summary>
    /// Applies sorting by difficulty level
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="descending">True for descending order, false for ascending</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<WorkoutTemplateEntity> SortByDifficulty(
        this IQueryable<WorkoutTemplateEntity> query,
        bool descending = false)
    {
        return descending 
            ? query.OrderByDescending(wt => wt.Difficulty.Value)
            : query.OrderBy(wt => wt.Difficulty.Value);
    }

    /// <summary>
    /// Applies sorting by category
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="descending">True for descending order, false for ascending</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<WorkoutTemplateEntity> SortByCategory(
        this IQueryable<WorkoutTemplateEntity> query,
        bool descending = false)
    {
        return descending 
            ? query.OrderByDescending(wt => wt.Category.Value)
            : query.OrderBy(wt => wt.Category.Value);
    }

    /// <summary>
    /// Applies standard includes for WorkoutTemplate entities
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <returns>Queryable with standard includes applied</returns>
    public static IQueryable<WorkoutTemplateEntity> IncludeStandardData(
        this IQueryable<WorkoutTemplateEntity> query)
    {
        return query
            .Include(wt => wt.Exercises)
                .ThenInclude(e => e.Exercise)
            .Include(wt => wt.Objectives)
            .Include(wt => wt.Category)
            .Include(wt => wt.Difficulty)
            .Include(wt => wt.WorkoutState);
    }
}