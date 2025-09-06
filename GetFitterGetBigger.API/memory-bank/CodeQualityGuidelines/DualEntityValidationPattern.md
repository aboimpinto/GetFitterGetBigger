# Dual-Entity Validation Pattern

**🎯 PURPOSE**: Optimize validation chains that need to validate relationships between two entities by loading each entity ONCE and carrying them through the validation chain.

## Problem Statement

When validating relationships between entities (like ExerciseLinks, Prerequisites, Dependencies), naive implementations make multiple database calls:

```csharp
// ❌ ANTI-PATTERN - 6+ database calls for 2 entities!
.EnsureAsync(async () => await IsSourceExerciseValidAsync(sourceId), ...) // DB call 1
.EnsureAsync(async () => await IsTargetExerciseValidAsync(targetId), ...) // DB call 2
.EnsureAsync(async () => await IsNotRestExerciseAsync(sourceId), ...)     // DB call 3
.EnsureAsync(async () => await IsNotRestExerciseAsync(targetId), ...)     // DB call 4
.EnsureAsync(async () => await IsLinkTypeCompatibleAsync(sourceId, targetId, linkType), ...) // DB calls 5-6
```

## Solution: ServiceValidationWithEntities Pattern

Create a specialized validation class that carries loaded entities through the validation chain:

### 1. Create the Validation Carrier Class

```csharp
/// <summary>
/// Carries two entities through validation chain to avoid redundant DB calls
/// </summary>
public class ServiceValidationWithExercises<T>
    where T : class, IEmptyDto<T>
{
    public ServiceValidation<T> Validation { get; }
    public ExerciseDto? SourceExercise { get; }
    public ExerciseDto? TargetExercise { get; }
    
    public bool HasErrors => Validation.HasErrors;
    
    public ServiceValidationWithExercises(ServiceValidation<T> validation)
    {
        Validation = validation;
    }
    
    // Immutable update methods
    public ServiceValidationWithExercises<T> WithSourceExercise(ExerciseDto? source)
        => new(Validation, source, TargetExercise);
        
    public ServiceValidationWithExercises<T> WithTargetExercise(ExerciseDto? target)
        => new(Validation, SourceExercise, target);
}
```

### 2. Create Semantic Extension Methods

Replace lambda expressions with semantic, readable extension methods:

```csharp
public static class ExerciseLinkValidationExtensions
{
    /// <summary>
    /// Transition from builder to dual-entity validation
    /// </summary>
    public static ServiceValidationWithExercises<T> AsExerciseLinkValidation<T>(
        this ServiceValidationBuilder<T> builder)
        where T : class, IEmptyDto<T>
    {
        return new ServiceValidationWithExercises<T>(builder.Validation);
    }
    
    /// <summary>
    /// Load source entity ONCE and carry through chain
    /// </summary>
    public static async Task<ServiceValidationWithExercises<T>> EnsureSourceExerciseExists<T>(
        this ServiceValidationWithExercises<T> validation,
        IExerciseService service,
        ExerciseId sourceId,
        string errorMessage)
        where T : class, IEmptyDto<T>
    {
        if (validation.HasErrors) return validation;
        
        var result = await service.GetByIdAsync(sourceId); // ONE DB call
        
        if (!result.IsSuccess || result.Data?.IsEmpty != false)
        {
            validation.Validation.Ensure(() => false, errorMessage);
            return validation;
        }
        
        return validation.WithSourceExercise(result.Data); // Carry entity forward
    }
    
    /// <summary>
    /// Validate using ALREADY LOADED entity (NO DB call)
    /// </summary>
    public static async Task<ServiceValidationWithExercises<T>> EnsureSourceExerciseIsNotRest<T>(
        this Task<ServiceValidationWithExercises<T>> validationTask,
        string errorMessage)
        where T : class, IEmptyDto<T>
    {
        var validation = await validationTask;
        
        if (validation.HasErrors || validation.SourceExercise == null)
            return validation;
        
        var isRest = validation.SourceExercise.ExerciseTypes.Any(et => 
            string.Equals(et.Value, "Rest", StringComparison.OrdinalIgnoreCase));
        
        validation.Validation.Ensure(() => !isRest, errorMessage);
        return validation;
    }
}
```

### 3. Usage in Service

```csharp
// ✅ CORRECT - Only 2 DB calls total!
return await ServiceValidate.Build<ExerciseLinkDto>()
    .EnsureNotEmpty(sourceId, "Invalid source ID")
    .EnsureNotEmpty(targetId, "Invalid target ID")
    .AsExerciseLinkValidation() // Transition to dual-entity validation
    .EnsureSourceExerciseExists(exerciseService, sourceId, "Source not found") // DB call 1
    .EnsureSourceExerciseIsNotRest("REST exercises cannot have links")        // No DB call
    .EnsureTargetExerciseExists(exerciseService, targetId, "Target not found") // DB call 2
    .EnsureTargetExerciseIsNotRest("Cannot link to REST exercises")           // No DB call
    .EnsureExercisesAreCompatibleForLinkType(linkType, "Incompatible link")    // No DB call
    .MatchAsyncWithExercises(
        whenValidWithExercises: async (source, target) => 
            await CreateLinkBetweenExercisesAsync(source, target, linkType));
```

## Key Principles

### 1. Load Once, Use Many Times
- Load each entity EXACTLY ONCE
- Carry loaded entities through the validation chain
- All subsequent validations use the loaded entities

### 2. Semantic Extension Methods
```csharp
// ❌ AVOID - Lambda with business logic
.EnsureAsync(async () => !IsRestExerciseType(await LoadExercise(id)), ...)

// ✅ PREFER - Semantic method name
.EnsureSourceExerciseIsNotRest(errorMessage)
```

### 3. Progressive Enhancement
- Start with standard ServiceValidate
- Transition to specialized validation when needed
- Maintain type safety throughout

### 4. Fail Fast
- Check `HasErrors` at each step
- Skip loading if validation already failed
- Return immediately on first failure

## When to Use This Pattern

✅ **USE when:**
- Validating relationships between 2+ entities
- Multiple validations need the same entity data
- Current implementation makes redundant DB calls
- Business rules depend on properties of multiple entities

❌ **DON'T USE when:**
- Validating a single entity
- Simple CRUD operations
- Validations don't need entity data (just IDs)

## Implementation Checklist

- [ ] Create `ServiceValidationWith[Entities]<T>` class
- [ ] Implement carrier properties for entities
- [ ] Create transition method (`As[Feature]Validation`)
- [ ] Implement load-once methods (`Ensure[Entity]Exists`)
- [ ] Implement validate-without-loading methods
- [ ] Create terminal operations (`MatchAsyncWith[Entities]`)
- [ ] Replace all redundant DB calls
- [ ] Verify performance improvement

## Performance Benefits

### Before (Naive Implementation)
- 6+ database round-trips
- Same entity loaded multiple times
- Network latency multiplied
- Database connection pool pressure

### After (Dual-Entity Pattern)
- 2 database round-trips (one per entity)
- Each entity loaded exactly once
- 67% reduction in database calls
- Improved response time

## Common Scenarios

### 1. Link/Relationship Validation
```csharp
.AsLinkValidation()
.EnsureSourceEntityExists(service, sourceId, error)
.EnsureTargetEntityExists(service, targetId, error)
.EnsureLinkIsValid(linkType, error)
```

### 2. Prerequisite Validation
```csharp
.AsPrerequisiteValidation()
.EnsureMainCourseExists(service, courseId, error)
.EnsurePrerequisiteExists(service, prereqId, error)
.EnsureNoCircularDependency(error)
```

### 3. Parent-Child Validation
```csharp
.AsHierarchyValidation()
.EnsureParentExists(service, parentId, error)
.EnsureChildExists(service, childId, error)
.EnsureValidHierarchy(error)
```

## Testing Considerations

### Mock Setup
```csharp
// Setup returns entity ONCE
exerciseServiceMock
    .SetupExerciseById(sourceId, sourceExercise)
    .SetupExerciseById(targetId, targetExercise);

// Verify called exactly once per entity
exerciseServiceMock.Verify(x => x.GetByIdAsync(sourceId), Times.Once);
exerciseServiceMock.Verify(x => x.GetByIdAsync(targetId), Times.Once);
```

### Performance Testing
```csharp
[Fact]
public async Task Should_LoadEachEntityOnlyOnce()
{
    // Arrange
    var callCount = new Dictionary<string, int>();
    
    // Act
    await service.ValidateRelationshipAsync(sourceId, targetId);
    
    // Assert
    callCount["source"].Should().Be(1);
    callCount["target"].Should().Be(1);
}
```

## Migration Guide

### Step 1: Identify Redundant Calls
```csharp
// Look for patterns like:
await IsEntityValidAsync(id);        // Loads entity
await HasPropertyAsync(id);          // Loads same entity
await MeetsConditionAsync(id);       // Loads same entity again
```

### Step 2: Create Validation Carrier
```csharp
public class ServiceValidationWithYourEntities<T> { ... }
```

### Step 3: Convert Helper Methods to Extensions
```csharp
// Before: Helper returns bool
private async Task<bool> IsEntityValidAsync(EntityId id) { ... }

// After: Extension validates with loaded entity
public static async Task<ServiceValidationWithEntities<T>> EnsureEntityIsValid<T>(
    this ServiceValidationWithEntities<T> validation, ...) { ... }
```

### Step 4: Update Service Method
```csharp
// Remove helper methods that loaded entities
// Use new extension methods with semantic names
// Verify reduction in DB calls
```

## Related Patterns

- [ServiceValidatePattern.md](./ServiceValidatePattern.md) - Base validation pattern
- [ServiceValidationExtensionPatterns.md](./ServiceValidationExtensionPatterns.md) - Extension method patterns
- [PositiveValidationPattern.md](./PositiveValidationPattern.md) - Positive assertion patterns
- [CleanValidationPattern.md](./CleanValidationPattern.md) - Simplified validation approach

## Key Takeaway

> **"Load once, validate many"** - Each entity should be loaded from the database exactly ONCE per request, then passed through the validation chain for all subsequent checks.