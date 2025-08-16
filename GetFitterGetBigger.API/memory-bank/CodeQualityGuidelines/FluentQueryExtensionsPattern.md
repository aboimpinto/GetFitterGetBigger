# Fluent Query Extensions Pattern

## Purpose
Replace complex chained if-statements in repository query methods with clean, readable, and composable FluentAPI extension methods.

## Problem: Complex Conditional Query Building
When building queries with multiple optional filters, repositories often end up with hard-to-read conditional logic:

```csharp
// ❌ ANTI-PATTERN - Complex chained if-statements
public async Task<(IEnumerable<Exercise>, int)> GetPagedAsync(
    string name, DifficultyLevelId difficultyId, IEnumerable<MuscleGroupId> muscleGroupIds, ...)
{
    var query = Context.Exercises.Include(...);
    
    // Multiple if-statements make the code hard to read
    if (!includeInactive)
    {
        query = query.Where(e => e.IsActive);
    }
    
    if (!string.IsNullOrEmpty(name))
    {
        query = query.Where(e => e.Name.ToLower().Contains(name.ToLower()));
    }
    
    if (!difficultyId.IsEmpty)
    {
        query = query.Where(e => e.DifficultyId == difficultyId);
    }
    
    query = query.Where(e => !muscleGroupIds.Any() || 
        e.ExerciseMuscleGroups.Any(emg => muscleGroupIds.Contains(emg.MuscleGroupId)));
    
    // More complex conditions...
}
```

## Solution: Fluent Query Extensions

Create extension methods that provide a clean, chainable API for query composition:

```csharp
// ✅ CORRECT - Fluent extension methods
public static class ExerciseQueryExtensions
{
    /// <summary>
    /// Applies name pattern filter using case-insensitive search
    /// </summary>
    public static IQueryable<Exercise> FilterByNamePattern(
        this IQueryable<Exercise> query,
        string? namePattern)
    {
        if (string.IsNullOrWhiteSpace(namePattern))
            return query;
            
        return query.Where(e => EF.Functions.ILike(e.Name, $"%{namePattern}%"));
    }
    
    /// <summary>
    /// Applies difficulty filter
    /// </summary>
    public static IQueryable<Exercise> FilterByDifficulty(
        this IQueryable<Exercise> query,
        DifficultyLevelId? difficultyId)
    {
        if (difficultyId == null || !difficultyId.HasValue || difficultyId.Value.IsEmpty)
            return query;
            
        return query.Where(e => e.DifficultyId == difficultyId.Value);
    }
    
    /// <summary>
    /// Applies muscle groups filter
    /// </summary>
    public static IQueryable<Exercise> FilterByMuscleGroups(
        this IQueryable<Exercise> query,
        IEnumerable<MuscleGroupId>? muscleGroupIds)
    {
        if (muscleGroupIds == null || !muscleGroupIds.Any())
            return query;
            
        return query.Where(e => 
            e.ExerciseMuscleGroups.Any(emg => muscleGroupIds.Contains(emg.MuscleGroupId)));
    }
    
    /// <summary>
    /// Applies all filters in a fluent chain
    /// </summary>
    public static IQueryable<Exercise> ApplyFilters(
        this IQueryable<Exercise> query,
        string? namePattern = null,
        DifficultyLevelId? difficultyId = null,
        IEnumerable<MuscleGroupId>? muscleGroupIds = null,
        bool includeInactive = false)
    {
        return query
            .FilterByActiveStatus(includeInactive)
            .FilterByNamePattern(namePattern)
            .FilterByDifficulty(difficultyId)
            .FilterByMuscleGroups(muscleGroupIds);
    }
}
```

## Usage in Repository

```csharp
public async Task<(IEnumerable<Exercise>, int)> GetPagedAsync(...)
{
    var query = Context.Exercises
        .ApplyFilters(
            namePattern: name,
            difficultyId: difficultyId,
            muscleGroupIds: muscleGroupIds,
            includeInactive: includeInactive)
        .ApplyFluentSorting(sortBy, sortOrder);
    
    var totalCount = await query.CountAsync();
    
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .IncludeStandardData()
        .ToListAsync();
    
    return (items, totalCount);
}
```

## Pattern Components

### 1. Individual Filter Methods
Each filter should be a separate extension method that:
- Takes the IQueryable and optional filter parameter
- Returns the IQueryable (allows chaining)
- Handles null/empty cases gracefully
- Has clear, descriptive naming

### 2. Combined ApplyFilters Method
A convenience method that applies all filters in sequence:
- Takes all filter parameters as optional
- Chains individual filter methods
- Provides a single entry point for common scenarios

### 3. Sorting Extensions
Separate methods for different sort criteria:
```csharp
public static IQueryable<Exercise> SortByName(
    this IQueryable<Exercise> query,
    bool descending = false)
{
    return descending 
        ? query.OrderByDescending(e => e.Name)
        : query.OrderBy(e => e.Name);
}

public static IQueryable<Exercise> ApplyFluentSorting(
    this IQueryable<Exercise> query,
    string? sortBy,
    string? sortOrder)
{
    var isDescending = sortOrder?.ToLower() == "desc";
    
    return (sortBy?.ToLower()) switch
    {
        "name" => query.SortByName(isDescending),
        "difficulty" => query.SortByDifficulty(isDescending),
        "createdat" => query.SortByCreatedAt(isDescending),
        _ => query.SortByName() // Default sort
    };
}
```

### 4. Include Extensions
Methods for standard data loading patterns:
```csharp
public static IQueryable<Exercise> IncludeStandardData(
    this IQueryable<Exercise> query)
{
    return query
        .Include(e => e.Difficulty)
        .Include(e => e.ExerciseMuscleGroups)
            .ThenInclude(emg => emg.MuscleGroup)
        .Include(e => e.ExerciseEquipment)
            .ThenInclude(ee => ee.Equipment)
        .AsSplitQuery()
        .AsNoTracking();
}
```

## Benefits

1. **Readability**: Clear, self-documenting method names
2. **Composability**: Mix and match filters as needed
3. **Reusability**: Share filter logic across different queries
4. **Testability**: Each filter can be tested independently
5. **Maintainability**: Changes to filter logic are centralized
6. **Separation of Concerns**: Query building logic separated from repository

## When to Apply This Pattern

Use Fluent Query Extensions when:
- ✅ Building queries with multiple optional filters
- ✅ The same filtering logic is used in multiple places
- ✅ Query building involves more than 2-3 conditional statements
- ✅ You need to compose different combinations of filters
- ✅ Working with complex entity relationships

Don't use when:
- ❌ Simple queries with no conditional logic
- ❌ One-off queries that won't be reused
- ❌ Performance-critical paths where LINQ overhead matters

## Implementation Checklist

When implementing Fluent Query Extensions:
- [ ] Create a static class named `{Entity}QueryExtensions`
- [ ] Place in appropriate namespace (e.g., `Services.{Entity}.Extensions`)
- [ ] Create individual filter methods for each criterion
- [ ] Implement ApplyFilters convenience method
- [ ] Add sorting methods for common sort fields
- [ ] Create IncludeStandardData for common includes
- [ ] Document each method with XML comments
- [ ] Handle null/empty parameters gracefully
- [ ] Use EF.Functions.ILike for case-insensitive search
- [ ] Return IQueryable to maintain composability

## Real-World Example

See `WorkoutTemplateQueryExtensions.cs` for a complete implementation that demonstrates:
- Multiple filter types (name, category, objective, difficulty, state)
- Sorting methods (by name, date, difficulty)
- Standard includes for related data
- Integration with repository and data service layers

## Common Mistakes to Avoid

1. **Breaking the chain**: Always return IQueryable, not IEnumerable
2. **Eager evaluation**: Don't call ToList() inside extension methods
3. **Missing null checks**: Handle null/empty parameters gracefully
4. **Complex logic in extensions**: Keep each method focused on one filter
5. **Not using EF.Functions**: Use EF.Functions.ILike for database-side operations

## Testing Considerations

When testing Fluent Query Extensions:
- Test each filter method independently
- Verify null/empty parameter handling
- Test filter combinations
- Ensure sorting works correctly
- Validate that includes are applied properly
- Use in-memory database for integration tests