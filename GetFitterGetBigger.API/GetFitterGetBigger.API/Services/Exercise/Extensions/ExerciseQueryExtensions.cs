using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.EntityFrameworkCore;
using ExerciseEntity = GetFitterGetBigger.API.Models.Entities.Exercise;

namespace GetFitterGetBigger.API.Services.Exercise.Extensions;

/// <summary>
/// Fluent extension methods for Exercise query operations.
/// Provides a clean, chainable API for filtering and sorting exercises.
/// </summary>
public static class ExerciseQueryExtensions
{
    /// <summary>
    /// Applies active status filter
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="includeInactive">Whether to include inactive exercises</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<ExerciseEntity> FilterByActiveStatus(
        this IQueryable<ExerciseEntity> query,
        bool includeInactive = false)
    {
        if (includeInactive)
            return query;
        
        return query.Where(e => e.IsActive);
    }
    
    /// <summary>
    /// Applies name pattern filter using case-insensitive search
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="namePattern">The name pattern to search for (null/empty = no filter)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<ExerciseEntity> FilterByNamePattern(
        this IQueryable<ExerciseEntity> query,
        string? namePattern)
    {
        if (string.IsNullOrWhiteSpace(namePattern))
            return query;
        
        return query.Where(e => e.Name.ToLower().Contains(namePattern.ToLower()));
    }
    
    /// <summary>
    /// Applies difficulty filter
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="difficultyId">The difficulty ID to filter by (empty = no filter)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<ExerciseEntity> FilterByDifficulty(
        this IQueryable<ExerciseEntity> query,
        DifficultyLevelId? difficultyId)
    {
        if (difficultyId == null || !difficultyId.HasValue || difficultyId.Value.IsEmpty)
            return query;
        
        return query.Where(e => e.DifficultyId == difficultyId.Value);
    }
    
    /// <summary>
    /// Applies muscle groups filter
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="muscleGroupIds">The muscle group IDs to filter by (empty = no filter)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<ExerciseEntity> FilterByMuscleGroups(
        this IQueryable<ExerciseEntity> query,
        IEnumerable<MuscleGroupId>? muscleGroupIds)
    {
        if (muscleGroupIds == null || !muscleGroupIds.Any())
            return query;
        
        return query.Where(e => 
            e.ExerciseMuscleGroups.Any(emg => muscleGroupIds.Contains(emg.MuscleGroupId)));
    }
    
    /// <summary>
    /// Applies equipment filter
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="equipmentIds">The equipment IDs to filter by (empty = no filter)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<ExerciseEntity> FilterByEquipment(
        this IQueryable<ExerciseEntity> query,
        IEnumerable<EquipmentId>? equipmentIds)
    {
        if (equipmentIds == null || !equipmentIds.Any())
            return query;
        
        return query.Where(e => 
            e.ExerciseEquipment.Any(ee => equipmentIds.Contains(ee.EquipmentId)));
    }
    
    /// <summary>
    /// Applies movement patterns filter
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="movementPatternIds">The movement pattern IDs to filter by (empty = no filter)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<ExerciseEntity> FilterByMovementPatterns(
        this IQueryable<ExerciseEntity> query,
        IEnumerable<MovementPatternId>? movementPatternIds)
    {
        if (movementPatternIds == null || !movementPatternIds.Any())
            return query;
        
        return query.Where(e => 
            e.ExerciseMovementPatterns.Any(emp => movementPatternIds.Contains(emp.MovementPatternId)));
    }
    
    /// <summary>
    /// Applies body parts filter
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="bodyPartIds">The body part IDs to filter by (empty = no filter)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<ExerciseEntity> FilterByBodyParts(
        this IQueryable<ExerciseEntity> query,
        IEnumerable<BodyPartId>? bodyPartIds)
    {
        if (bodyPartIds == null || !bodyPartIds.Any())
            return query;
        
        return query.Where(e => 
            e.ExerciseBodyParts.Any(ebp => bodyPartIds.Contains(ebp.BodyPartId)));
    }
    
    /// <summary>
    /// Applies all filters in a fluent chain
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="namePattern">Name pattern filter</param>
    /// <param name="difficultyId">Difficulty filter</param>
    /// <param name="muscleGroupIds">Muscle groups filter</param>
    /// <param name="equipmentIds">Equipment filter</param>
    /// <param name="movementPatternIds">Movement patterns filter</param>
    /// <param name="bodyPartIds">Body parts filter</param>
    /// <param name="includeInactive">Whether to include inactive exercises</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<ExerciseEntity> ApplyFilters(
        this IQueryable<ExerciseEntity> query,
        string? namePattern = null,
        DifficultyLevelId? difficultyId = null,
        IEnumerable<MuscleGroupId>? muscleGroupIds = null,
        IEnumerable<EquipmentId>? equipmentIds = null,
        IEnumerable<MovementPatternId>? movementPatternIds = null,
        IEnumerable<BodyPartId>? bodyPartIds = null,
        bool includeInactive = false)
    {
        return query
            .FilterByActiveStatus(includeInactive)
            .FilterByNamePattern(namePattern)
            .FilterByDifficulty(difficultyId)
            .FilterByMuscleGroups(muscleGroupIds)
            .FilterByEquipment(equipmentIds)
            .FilterByMovementPatterns(movementPatternIds)
            .FilterByBodyParts(bodyPartIds);
    }
    
    /// <summary>
    /// Applies sorting by name
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="descending">True for descending order, false for ascending</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<ExerciseEntity> SortByName(
        this IQueryable<ExerciseEntity> query,
        bool descending = false)
    {
        return descending 
            ? query.OrderByDescending(e => e.Name)
            : query.OrderBy(e => e.Name);
    }
    
    /// <summary>
    /// Applies sorting by difficulty
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="descending">True for descending order, false for ascending</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<ExerciseEntity> SortByDifficulty(
        this IQueryable<ExerciseEntity> query,
        bool descending = false)
    {
        // Sort by the underlying GUID value since DifficultyLevelId doesn't implement IComparable
        return descending 
            ? query.OrderByDescending(e => e.DifficultyId.ToGuid())
            : query.OrderBy(e => e.DifficultyId.ToGuid());
    }
    
    /// <summary>
    /// Applies sorting based on sort criteria with fluent API
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <param name="sortBy">The field to sort by (name, difficulty)</param>
    /// <param name="sortOrder">The sort order (asc, desc)</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<ExerciseEntity> ApplyFluentSorting(
        this IQueryable<ExerciseEntity> query,
        string? sortBy,
        string? sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";
        
        return (sortBy?.ToLower()) switch
        {
            "name" => query.SortByName(isDescending),
            "difficulty" => query.SortByDifficulty(isDescending),
            _ => query.SortByName() // Default sort by name ascending
        };
    }
    
    /// <summary>
    /// Applies standard includes for Exercise entities
    /// </summary>
    /// <param name="query">The queryable to extend</param>
    /// <returns>Queryable with standard includes applied</returns>
    public static IQueryable<ExerciseEntity> IncludeStandardData(
        this IQueryable<ExerciseEntity> query)
    {
        return query
            .Include(e => e.Difficulty)
            .Include(e => e.KineticChain)
            .Include(e => e.ExerciseWeightType)
            .Include(e => e.CoachNotes)
            .Include(e => e.ExerciseExerciseTypes)
                .ThenInclude(eet => eet.ExerciseType)
            .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleGroup)
            .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleRole)
            .Include(e => e.ExerciseEquipment)
                .ThenInclude(ee => ee.Equipment)
            .Include(e => e.ExerciseMovementPatterns)
                .ThenInclude(emp => emp.MovementPattern)
            .Include(e => e.ExerciseBodyParts)
                .ThenInclude(ebp => ebp.BodyPart)
            .AsSplitQuery()
            .AsNoTracking();
    }
}