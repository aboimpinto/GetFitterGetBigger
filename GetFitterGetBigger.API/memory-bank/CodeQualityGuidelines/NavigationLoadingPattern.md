# Navigation Loading Pattern

## Overview
A reusable pattern for efficiently loading navigation properties after `SaveChangesAsync()` in repository methods. This pattern eliminates repetitive code and provides a consistent approach across all repositories.

## Why This Is a Good Extension Method
Following our [Extension Method Philosophy](./ExtensionMethodPhilosophy.md), this pattern is ideal for an extension because:
- **It's pure technical boilerplate** - no business logic involved
- **The operation never varies** - navigation properties are always loaded the same way
- **It doesn't hide what's being loaded** - the properties are defined in the entity
- **It's a performance optimization** - uses single query instead of multiple LoadAsync calls

## Problem Statement
Repository methods like `AddAsync` and `UpdateAsync` often require 40-60+ lines of manual `LoadAsync()` calls to populate navigation properties after saving an entity. This leads to:
- Repetitive, hard-to-maintain code
- Risk of forgetting navigation properties
- Inconsistent loading strategies
- Poor readability

## Solution
The Navigation Loading Pattern provides:
1. **One-line replacement** for all navigation loading
2. **Entity-specific optimized loaders** using existing query extensions
3. **Fallback generic loader** for entities without specific implementations
4. **Fluent API** for selective loading scenarios

## Implementation

### Extension Method Location
`/GetFitterGetBigger.API/Extensions/EntityEntryNavigationExtensions.cs`

### Basic Usage
```csharp
// Replace 60+ lines of manual loading with one line
public async Task<Exercise> AddAsync(Exercise exercise)
{
    Context.Exercises.Add(exercise);
    await Context.SaveChangesAsync();
    return await Context.Entry(exercise).LoadAllNavigationPropertiesAsync();
}
```

### Selective Loading with Fluent API
```csharp
// Load only specific navigation properties
return await Context.Entry(exercise)
    .LoadNavigation()
    .Reference(e => e.Difficulty)
    .Collection(e => e.ExerciseMuscleGroups)
    .ExecuteAsync();
```

## Pattern Components

### 1. Main Extension Method
- `LoadAllNavigationPropertiesAsync<TEntity>()` - Loads all navigation properties
- Uses pattern matching to select entity-specific loader
- Falls back to generic reflection-based loading

### 2. Entity-Specific Loaders
- Optimized single-query approach using includes
- Reuses existing query extensions (e.g., `IncludeStandardData()`)
- Maintains consistency with query patterns

### 3. Fluent API Builder
- `LoadNavigation()` - Starts fluent chain
- `Reference()` - Loads one-to-one relationships
- `Collection()` - Loads one-to-many relationships
- `CollectionWithInclude()` - Loads collections with nested includes
- `ExecuteAsync()` - Executes all pending loads

## Benefits

### Code Reduction
- **95% less code** in repository methods
- ExerciseRepository.AddAsync: 64 lines → 3 lines
- WorkoutTemplateRepository.AddAsync: 40 lines → 3 lines

### Consistency
- Single pattern across all repositories
- Reuses existing include patterns
- Centralized navigation loading logic

### Performance
- Optimized single-query loading when possible
- Leverages EF Core's `AsSplitQuery()`
- Eliminates N+1 query problems

### Maintainability
- Add navigation properties in one place
- No risk of forgetting properties
- Clear, declarative intent

## Where to Use

### Required Usage
- All `AddAsync` methods in repositories
- All `UpdateAsync` methods that return entities
- Any method that modifies and returns entities

### Optional Usage
- Methods that need selective navigation loading
- Test setup requiring specific navigation properties
- Bulk operations on multiple entities

## Adding Support for New Entities

1. Add case to switch expression in `LoadAllNavigationPropertiesAsync`:
```csharp
return entry.Entity switch
{
    Exercise exercise => ...,
    WorkoutTemplate template => ...,
    YourNewEntity entity => (TEntity)(object)await LoadYourNewEntityNavigationPropertiesAsync(entry.Context, entity),
    _ => await LoadNavigationPropertiesGenericAsync(entry)
};
```

2. Implement entity-specific loader:
```csharp
private static async Task<YourNewEntity> LoadYourNewEntityNavigationPropertiesAsync(
    DbContext context, 
    YourNewEntity entity)
{
    var dbContext = context as FitnessDbContext;
    var loaded = await dbContext.YourEntities
        .Where(e => e.Id == entity.Id)
        .Include(e => e.NavigationProperty1)
        .Include(e => e.NavigationProperty2)
        .AsNoTracking()
        .FirstOrDefaultAsync();
        
    return loaded ?? entity;
}
```

## Integration with Other Patterns

### Works With
- **ServiceValidate Pattern** - Load before validation
- **Empty Pattern** - Returns entity or Empty
- **Unit of Work Pattern** - Transaction boundaries
- **Repository Base Classes** - Can extend DomainRepository

### Complements
- **Query Extensions** - Reuses existing include logic
- **Specification Pattern** - Can use specifications for loading
- **Builder Pattern** - Fluent API follows builder pattern

## Examples

### Simple Add with Full Loading
```csharp
public async Task<Equipment> CreateAsync(Equipment entity)
{
    Context.Equipment.Add(entity);
    await Context.SaveChangesAsync();
    return await Context.Entry(entity).LoadAllNavigationPropertiesAsync();
}
```

### Update with Navigation Refresh
```csharp
public async Task<Exercise> UpdateAsync(Exercise exercise)
{
    // Update logic...
    await Context.SaveChangesAsync();
    
    // Refresh all navigation properties
    return await Context.Entry(existingExercise).LoadAllNavigationPropertiesAsync();
}
```

### Selective Loading for Performance
```csharp
public async Task<WorkoutTemplate> AddQuickAsync(WorkoutTemplate template)
{
    Context.WorkoutTemplates.Add(template);
    await Context.SaveChangesAsync();
    
    // Only load essential properties for quick response
    return await Context.Entry(template)
        .LoadNavigation()
        .Reference(t => t.WorkoutState)
        .Reference(t => t.Difficulty)
        .ExecuteAsync();
}
```

## Anti-Patterns to Avoid

### ❌ Don't Do This
```csharp
// Manual loading - repetitive and error-prone
await Context.Entry(exercise).Reference(e => e.Difficulty).LoadAsync();
await Context.Entry(exercise).Reference(e => e.KineticChain).LoadAsync();
await Context.Entry(exercise).Collection(e => e.CoachNotes).LoadAsync();
// ... 60 more lines
```

### ✅ Do This Instead
```csharp
// One line, all properties loaded
return await Context.Entry(exercise).LoadAllNavigationPropertiesAsync();
```

## Testing Considerations

The pattern simplifies testing:
- Navigation properties are guaranteed to be loaded
- No need to manually setup navigation in tests
- Can use selective loading for test-specific scenarios
- Mockable for unit tests

## Performance Notes

- **Single Query**: Entity-specific loaders use one optimized query
- **Split Queries**: Complex includes use `AsSplitQuery()` to avoid cartesian products
- **Lazy Loading**: Pattern eliminates need for lazy loading proxies
- **Caching**: Leverages EF Core's context caching automatically

## Migration Guide

To migrate existing repositories:

1. Add reference to `EntityEntryNavigationExtensions`
2. Replace manual `LoadAsync()` calls with `LoadAllNavigationPropertiesAsync()`
3. Test to ensure all navigation properties are loaded correctly
4. Remove any redundant `GetByIdAsync()` calls after updates

## See Also
- [Repository Base Class Pattern](./RepositoryBaseClassPattern.md)
- [Service Validate Pattern](./ServiceValidatePattern.md)
- [Query Extensions Pattern](../Services/Exercise/Extensions/)