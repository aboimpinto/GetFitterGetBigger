# Fluent Sorting Pattern

**🎯 PURPOSE**: Provide an elegant, readable, and maintainable approach to conditional sorting in queries using fluent API design patterns.

## Problem Statement

Traditional sorting implementations suffer from verbosity and poor readability:

```csharp
// ❌ ANTI-PATTERN - Verbose and hard to maintain
var isDescending = sortOrder?.ToLower() == "desc";
query = (sortBy?.ToLower()) switch
{
    "name" => query.SortByName(isDescending),
    "createdat" => query.SortByCreatedAt(isDescending),
    "updatedat" => query.SortByUpdatedAt(isDescending),
    "difficulty" => query.SortByDifficulty(isDescending),
    "category" => query.SortByCategory(isDescending),
    _ => query.SortByUpdatedAt(descending: true)
};
```

### Issues with Traditional Approach:
1. **Verbosity**: Requires temporary variables and switch statements
2. **Separation of Concerns**: Sorting logic separated from query building
3. **No State Tracking**: Cannot tell if sorting was already applied
4. **Limited Extensibility**: Hard to add multiple sort fields
5. **Default Handling**: Default case mixed with specific cases

## Solution: SortableQuery Pattern

The SortableQuery pattern wraps IQueryable with sorting state tracking and provides a fluent API for conditional sorting:

```csharp
// ✅ CORRECT - Elegant and maintainable
var query = repository.GetQueryable()
    .FilterByNamePattern(namePattern)
    .FilterByCategory(categoryId)
    .ToSortable()
    .ApplySortByName(sortBy, sortOrder)
    .ApplySortByCreatedAt(sortBy, sortOrder)
    .ApplySortByUpdatedAt(sortBy, sortOrder)
    .WithDefaultSort(q => q.OrderByDescending(x => x.UpdatedAt));
```

## Implementation

### 1. SortableQuery Wrapper Class

```csharp
public class SortableQuery<T>
{
    private readonly IQueryable<T> _query;
    private bool _hasSorting;
    
    public IQueryable<T> Query => _query;
    public bool HasSorting => _hasSorting;
    
    public SortableQuery(IQueryable<T> query, bool hasSorting = false)
    {
        _query = query;
        _hasSorting = hasSorting;
    }
    
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
    
    public IQueryable<T> WithDefaultSort(Func<IQueryable<T>, IQueryable<T>> defaultSortFunc)
    {
        return _hasSorting ? _query : defaultSortFunc(_query);
    }
}
```

### 2. Extension Methods

```csharp
public static class SortableQueryExtensions
{
    public static SortableQuery<T> ToSortable<T>(this IQueryable<T> query)
    {
        return new SortableQuery<T>(query);
    }
    
    // Entity-specific sorting methods
    public static SortableQuery<WorkoutTemplate> ApplySortByName(
        this SortableQuery<WorkoutTemplate> sortable,
        string? sortBy,
        string? sortOrder)
    {
        return sortable.ApplySortIf("name", sortBy, sortOrder, 
            (query, desc) => desc 
                ? query.OrderByDescending(wt => wt.Name)
                : query.OrderBy(wt => wt.Name));
    }
}
```

## Key Benefits

### 1. **Readability**
The fluent chain clearly shows all possible sorts and the default fallback. Each line represents a potential sort field.

### 2. **State Tracking**
The wrapper tracks whether any sorting was applied, enabling intelligent default handling.

### 3. **Conditional Application**
Each sort method only applies if its field matches the sortBy parameter - no need for if statements or switch expressions.

### 4. **Default Fallback**
The `WithDefaultSort` method provides a clean way to apply default sorting only when no other sorting was specified.

### 5. **Extensibility**
Easy to add new sort fields or support multiple sort columns in the future.

## Usage Patterns

### Basic Usage
```csharp
var query = repository.GetQueryable()
    .ToSortable()
    .ApplySortByName(sortBy, sortOrder)
    .ApplySortByDate(sortBy, sortOrder)
    .WithDefaultSort(q => q.OrderBy(x => x.Id));
```

### With Filtering
```csharp
var query = repository.GetQueryable()
    .FilterByStatus(status)          // Apply filters first
    .FilterByCategory(category)
    .ToSortable()                     // Then convert to sortable
    .ApplySortByName(sortBy, sortOrder)
    .WithDefaultSort(q => q.OrderBy(x => x.CreatedAt));
```

### Multiple Sort Fields (Future Enhancement)
```csharp
var query = repository.GetQueryable()
    .ToSortable()
    .ApplySortByName(primarySort, primaryOrder)
    .ThenApplySortByDate(secondarySort, secondaryOrder)
    .WithDefaultSort(q => q.OrderBy(x => x.Id));
```

## Guidelines

### DO ✅
- Apply filters BEFORE converting to sortable
- Use descriptive method names (ApplySortByName, not just ApplySort)
- Always provide a default sort for predictable results
- Keep sort methods simple and focused on one field

### DON'T ❌
- Mix filtering and sorting in the same method
- Use generic ApplySort methods that hide the field being sorted
- Forget to handle the default case
- Apply multiple sorts to the same field

## Real-World Example

```csharp
public async Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> SearchAsync(
    int page,
    int pageSize,
    string namePattern,
    WorkoutCategoryId categoryId,
    WorkoutObjectiveId objectiveId,
    DifficultyLevelId difficultyId,
    WorkoutStateId stateId,
    string sortBy,
    string sortOrder)
{
    using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
    var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
    
    // Clean, readable query building
    var query = repository.GetWorkoutTemplatesQueryable()
        .FilterByNamePattern(namePattern)
        .FilterByCategory(categoryId)
        .FilterByObjective(objectiveId)
        .FilterByDifficulty(difficultyId)
        .FilterByState(stateId)
        .ToSortable()
        .ApplySortByName(sortBy, sortOrder)
        .ApplySortByCreatedAt(sortBy, sortOrder)
        .ApplySortByUpdatedAt(sortBy, sortOrder)
        .ApplySortByDifficulty(sortBy, sortOrder)
        .ApplySortByCategory(sortBy, sortOrder)
        .WithDefaultWorkoutTemplateSort();
    
    var totalCount = await query.CountAsync();
    
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .IncludeStandardData()
        .ToListAsync();
    
    // ... rest of implementation
}
```

## Testing Considerations

When testing sortable queries:

1. **Test each sort field individually**
```csharp
[Fact]
public async Task SearchAsync_SortByName_AppliesNameSorting()
{
    // Arrange
    var sortBy = "name";
    var sortOrder = "asc";
    
    // Act
    var result = await service.SearchAsync(1, 10, "", empty, empty, empty, empty, sortBy, sortOrder);
    
    // Assert
    result.Data.Items.Should().BeInAscendingOrder(x => x.Name);
}
```

2. **Test default sorting**
```csharp
[Fact]
public async Task SearchAsync_NoSortSpecified_AppliesDefaultSort()
{
    // Act
    var result = await service.SearchAsync(1, 10, "", empty, empty, empty, empty, null, null);
    
    // Assert
    result.Data.Items.Should().BeInDescendingOrder(x => x.UpdatedAt);
}
```

3. **Test invalid sort fields fall back to default**
```csharp
[Fact]
public async Task SearchAsync_InvalidSortField_AppliesDefaultSort()
{
    // Act
    var result = await service.SearchAsync(1, 10, "", empty, empty, empty, empty, "invalid", "asc");
    
    // Assert
    result.Data.Items.Should().BeInDescendingOrder(x => x.UpdatedAt);
}
```

## Migration Guide

To migrate existing sorting code to the SortableQuery pattern:

1. **Create extension methods** for each sortable field
2. **Replace switch/if statements** with fluent chain
3. **Add ToSortable()** after filters
4. **Chain all ApplySort methods** for each field
5. **End with WithDefaultSort()** for fallback

### Before
```csharp
var isDescending = sortOrder?.ToLower() == "desc";
if (sortBy?.ToLower() == "name")
    query = isDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);
else if (sortBy?.ToLower() == "date")
    query = isDescending ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date);
else
    query = query.OrderBy(x => x.Id);
```

### After
```csharp
var query = baseQuery
    .ToSortable()
    .ApplySortByName(sortBy, sortOrder)
    .ApplySortByDate(sortBy, sortOrder)
    .WithDefaultSort(q => q.OrderBy(x => x.Id));
```

## Summary

The Fluent Sorting Pattern provides:
- **Elegant syntax** that reads like natural language
- **State tracking** to handle default cases intelligently
- **Conditional application** without verbose if/switch statements
- **Clear separation** between filtering and sorting
- **Easy extensibility** for new sort fields

This pattern exemplifies our commitment to readable, maintainable code that follows the principle: "Code should read like well-written prose."