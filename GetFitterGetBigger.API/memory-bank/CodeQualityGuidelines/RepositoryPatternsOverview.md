# Repository Patterns Overview

## Our Approach: The Right Balance

We've implemented two complementary patterns that demonstrate the proper use of extension methods - eliminating technical boilerplate while keeping business logic explicit and visible.

## The Patterns

### 1. Navigation Loading Pattern
**Purpose**: Load all navigation properties after SaveChangesAsync  
**Extension**: `LoadAllNavigationPropertiesAsync()`  
**Location**: `/Extensions/EntityEntryNavigationExtensions.cs`

**Why it's a good extension:**
- Pure technical operation (no business logic)
- Always done the same way
- Replaces 60+ lines of repetitive LoadAsync calls
- Performance optimization (single query)

### 2. Collection Replace Pattern  
**Purpose**: Efficiently replace collection contents  
**Extension**: `ReplaceWith()`  
**Location**: `/Extensions/CollectionReplaceExtensions.cs`

**Why it's a good extension:**
- Technical optimization (AddRange vs foreach)
- The WHAT is still visible (which collections)
- Only the HOW is hidden (optimal add method)
- Performance benefit would be missed otherwise

## The Philosophy

### ✅ Hide Technical Complexity
```csharp
// GOOD: Technical boilerplate hidden
return await Context.Entry(exercise).LoadAllNavigationPropertiesAsync();

// Instead of 60+ lines of LoadAsync calls
```

### ✅ Show Business Logic
```csharp
// GOOD: Can see exactly which collections are updated
existingExercise.CoachNotes.ReplaceWith(exercise.CoachNotes);
existingExercise.ExerciseMuscleGroups.ReplaceWith(exercise.ExerciseMuscleGroups);
existingExercise.ExerciseEquipment.ReplaceWith(exercise.ExerciseEquipment);
// Can easily comment out or handle specific collections differently
```

### ❌ Don't Hide Business Decisions
```csharp
// BAD: Can't see what's being updated
Context.Entry(existingExercise).UpdateFromSource(exercise);

// GOOD: Explicit about what's changing
Context.Entry(existingExercise).CurrentValues.SetValues(exercise);
existingExercise.Collections.ReplaceWith(exercise.Collections);
```

## The Complete Pattern in Action

```csharp
public async Task<Exercise> UpdateAsync(Exercise exercise)
{
    // Load existing with relationships - VISIBLE what we need
    var existingExercise = await Context.Exercises
        .Include(e => e.CoachNotes)
        .Include(e => e.ExerciseMuscleGroups)
        .Include(e => e.ExerciseEquipment)
        .Include(e => e.ExerciseMovementPatterns)
        .Include(e => e.ExerciseBodyParts)
        .FirstOrDefaultAsync(e => e.Id == exercise.Id);
    
    if (existingExercise == null)
        throw new InvalidOperationException($"Exercise with ID {exercise.Id} not found");
    
    // Update scalar properties - VISIBLE that all scalars are updated
    Context.Entry(existingExercise).CurrentValues.SetValues(exercise);
    
    // Update relationships - VISIBLE which collections are updated
    existingExercise.CoachNotes.ReplaceWith(exercise.CoachNotes);
    existingExercise.ExerciseMuscleGroups.ReplaceWith(exercise.ExerciseMuscleGroups);
    existingExercise.ExerciseEquipment.ReplaceWith(exercise.ExerciseEquipment);
    existingExercise.ExerciseMovementPatterns.ReplaceWith(exercise.ExerciseMovementPatterns);
    existingExercise.ExerciseBodyParts.ReplaceWith(exercise.ExerciseBodyParts);
    
    await Context.SaveChangesAsync();
    
    // Load navigation properties - TECHNICAL operation, OK to hide
    return await Context.Entry(existingExercise).LoadAllNavigationPropertiesAsync();
}
```

## Benefits Achieved

### Code Quality
- **Reduced boilerplate**: 60+ lines → 1 line for navigation loading
- **Improved performance**: AddRange instead of foreach loops
- **Maintained clarity**: Business logic remains visible
- **Better debugging**: Can see and control each update

### Maintainability
- **Easy to understand**: New developers see what's happening
- **Easy to modify**: Can handle special cases per collection
- **Easy to debug**: Breakpoints on specific updates
- **Easy to audit**: Can log specific field changes

### Performance
- **Optimized queries**: Single query for navigation loading
- **Batch operations**: AddRange for collections
- **Reduced allocations**: No unnecessary loops

## Guidelines for Future Extensions

Before creating an extension method, ask:

1. **Is this hiding business logic?** → Don't create it
2. **Will developers need to see this?** → Keep it explicit
3. **Is this pure technical boilerplate?** → Good candidate
4. **Does this provide performance benefits?** → Good candidate

## Key Takeaway

**The best code is not the shortest code, but the clearest code.**

Our patterns eliminate repetitive technical code while keeping business operations explicit and debuggable. This is the balance that creates maintainable, high-quality software.

## Related Documentation

- [Extension Method Philosophy](./ExtensionMethodPhilosophy.md) - When to use extensions
- [Navigation Loading Pattern](./NavigationLoadingPattern.md) - Technical pattern details
- [Collection Replace Pattern](./CollectionReplacePattern.md) - Implementation details

## Quick Reference

### Use Extensions For:
- Technical operations (LoadNavigationProperties)
- Performance optimizations (ReplaceWith)
- Complex but unchanging patterns (IncludeStandardData)
- Standard transformations (ToDto)

### Keep Explicit:
- Field updates (which fields change)
- Business logic (what gets updated when)
- Conditional operations (if/else logic)
- Context-dependent behavior