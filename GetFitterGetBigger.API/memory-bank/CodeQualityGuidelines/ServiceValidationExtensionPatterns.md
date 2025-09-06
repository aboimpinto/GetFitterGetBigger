# Service Validation Extension Patterns

**🎯 PURPOSE**: Define patterns for creating clean, readable validation extensions that replace complex inline expressions with natural language methods.

## Core Principle: Natural Language Over Symbols

**Replace symbolic expressions with semantic extension methods that read like natural language.**

## 🚨 CRITICAL RULES

```
┌─────────────────────────────────────────────────────────────────┐
│ VALIDATION EXTENSION RULES - MANDATORY                          │
├─────────────────────────────────────────────────────────────────┤
│ 1. NO ServiceError.ValidationFailed wrapper in Ensure methods  │
│ 2. Create local extensions for ALL symbolic expressions        │
│ 3. Use positive assertions - avoid negations in predicates     │
│ 4. Parse IDs ONCE at method entry, pass specialized types      │
│ 5. Extension methods use natural language, not symbols         │
└─────────────────────────────────────────────────────────────────┘
```

## Pattern 1: No ServiceError.ValidationFailed Wrapper

### ❌ WRONG - Redundant wrapper
```csharp
return await ServiceValidate.Build<ExerciseLinkDto>()
    .EnsureNotEmpty(
        sourceId,
        ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidSourceExerciseId))
    .Ensure(
        () => IsValidLinkType(linkType),
        ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidLinkTypeEnum))
```

### ✅ CORRECT - Pass message directly
```csharp
return await ServiceValidate.Build<ExerciseLinkDto>()
    .EnsureNotEmpty(sourceId, ExerciseLinkErrorMessages.InvalidSourceExerciseId)
    .Ensure(() => IsValidLinkType(linkType), ExerciseLinkErrorMessages.InvalidLinkTypeEnum)
```

**Exception**: Use ServiceError for non-ValidationFailed errors:
```csharp
// ✅ CORRECT - NotFound is not ValidationFailed
.EnsureAsync(
    async () => await ExistsAsync(id),
    ServiceError.NotFound("Exercise", id.ToString()))

// ✅ CORRECT - AlreadyExists is not ValidationFailed
.EnsureNameIsUniqueAsync(
    async () => await IsNameUniqueAsync(name),
    "Exercise", name)  // Creates ServiceError.AlreadyExists internally
```

## Pattern 2: Local Extension Methods for Symbolic Expressions

### Replace ALL symbolic expressions with semantic extension methods

### ❌ WRONG - Symbolic expressions in validations
```csharp
.Ensure(() => !AreSameExercise(sourceId, targetId), errorMessage)
.Ensure(() => linkType != ExerciseLinkType.WORKOUT, errorMessage)
.Ensure(() => command.DisplayOrder >= 0, errorMessage)
.Ensure(() => count > 0 && count <= 20, errorMessage)
```

### ✅ CORRECT - Natural language extensions
```csharp
// Define local extension methods at bottom of service file
private static class ValidationExtensions
{
    public static ServiceValidationBuilder<T> EnsureExercisesAreDifferent<T>(
        this ServiceValidationBuilder<T> builder,
        string sourceId, 
        string targetId,
        string errorMessage)
    {
        return builder.Ensure(
            () => sourceId != targetId,
            errorMessage);
    }
    
    public static ServiceValidationBuilder<T> EnsureLinkTypeIsNotWorkout<T>(
        this ServiceValidationBuilder<T> builder,
        ExerciseLinkType linkType,
        string errorMessage)
    {
        return builder.Ensure(
            () => linkType != ExerciseLinkType.WORKOUT,
            errorMessage);
    }
    
    public static ServiceValidationBuilder<T> EnsureDisplayOrderIsNotNegative<T>(
        this ServiceValidationBuilder<T> builder,
        int displayOrder,
        string errorMessage)
    {
        return builder.Ensure(
            () => displayOrder >= 0,
            errorMessage);
    }
    
    public static ServiceValidationBuilder<T> EnsureCountIsInRange<T>(
        this ServiceValidationBuilder<T> builder,
        int count,
        int min,
        int max,
        string errorMessage)
    {
        return builder.Ensure(
            () => count >= min && count <= max,
            errorMessage);
    }
}

// Usage in validation chain
return await ServiceValidate.Build<ExerciseLinkDto>()
    .EnsureExercisesAreDifferent(sourceId, targetId, ErrorMessages.CannotLinkToSelf)
    .EnsureLinkTypeIsNotWorkout(linkType, ErrorMessages.WorkoutLinksAutoCreated)
    .EnsureDisplayOrderIsNotNegative(command.DisplayOrder, ErrorMessages.DisplayOrderMustBeNonNegative)
    .EnsureCountIsInRange(count, 1, 20, ErrorMessages.CountMustBeBetween1And20)
```

## Pattern 3: Positive Assertions in Helper Methods

### ❌ WRONG - Negative helper methods
```csharp
.Ensure(() => !AreSameExercise(sourceId, targetId), errorMessage)

private static bool AreSameExercise(string source, string target)
{
    return source == target;  // Returns true when they ARE same
}
```

### ✅ CORRECT - Positive helper methods
```csharp
.Ensure(() => AreDifferentExercises(sourceId, targetId), errorMessage)

private static bool AreDifferentExercises(string source, string target)
{
    return source != target;  // Returns true for positive state
}
```

Or use extension method directly:
```csharp
.EnsureExercisesAreDifferent(sourceId, targetId, errorMessage)
```

## Pattern 4: Parse IDs Once and Pass Consistently

### ❌ WRONG - Multiple parsing
```csharp
public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(
    string sourceExerciseId,
    string targetExerciseId,
    ExerciseLinkType linkType)
{
    // First parse
    var sourceId = ExerciseId.ParseOrEmpty(sourceExerciseId);
    var targetId = ExerciseId.ParseOrEmpty(targetExerciseId);
    
    return await ServiceValidate.Build<ExerciseLinkDto>()
        .EnsureNotEmpty(sourceId, errorMessage)
        .EnsureAsync(
            // Passing string version, will parse again inside!
            async () => await IsNotRestExerciseAsync(sourceExerciseId),
            errorMessage)
}

private async Task<bool> IsNotRestExerciseAsync(string exerciseId)
{
    // Second parse - REDUNDANT!
    var result = await exerciseService.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId));
}
```

### ✅ CORRECT - Parse once, use everywhere
```csharp
public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(
    string sourceExerciseId,
    string targetExerciseId,
    ExerciseLinkType linkType)
{
    // Parse once at the top
    var sourceId = ExerciseId.ParseOrEmpty(sourceExerciseId);
    var targetId = ExerciseId.ParseOrEmpty(targetExerciseId);
    
    return await ServiceValidate.Build<ExerciseLinkDto>()
        .EnsureNotEmpty(sourceId, errorMessage)
        .EnsureAsync(
            // Pass the parsed ID
            async () => await IsNotRestExerciseAsync(sourceId),
            errorMessage)
}

private async Task<bool> IsNotRestExerciseAsync(ExerciseId exerciseId)
{
    // Use the already-parsed ID directly
    var result = await exerciseService.GetByIdAsync(exerciseId);
}
```

## Pattern 5: Commands Should Contain Specialized IDs

### ❌ WRONG - Commands with string IDs
```csharp
public class CreateExerciseLinkCommand
{
    public string SourceExerciseId { get; init; }  // String ID
    public string TargetExerciseId { get; init; }  // String ID
}

// Service has to parse
public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(CreateExerciseLinkCommand command)
{
    var sourceId = ExerciseId.ParseOrEmpty(command.SourceExerciseId);  // Parsing in service
    var targetId = ExerciseId.ParseOrEmpty(command.TargetExerciseId);  // Parsing in service
}
```

### ✅ CORRECT - Commands with specialized IDs
```csharp
public class CreateExerciseLinkCommand
{
    public ExerciseId SourceExerciseId { get; init; }  // Specialized ID
    public ExerciseId TargetExerciseId { get; init; }  // Specialized ID
}

// Controller does the parsing
[HttpPost]
public async Task<IActionResult> CreateLink([FromBody] CreateLinkRequest request)
{
    var command = new CreateExerciseLinkCommand
    {
        SourceExerciseId = ExerciseId.ParseOrEmpty(request.SourceExerciseId),  // Parse in controller
        TargetExerciseId = ExerciseId.ParseOrEmpty(request.TargetExerciseId),  // Parse in controller
    };
    
    return await _service.CreateLinkAsync(command) switch { ... };
}

// Service uses IDs directly - no parsing
public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(CreateExerciseLinkCommand command)
{
    return await ServiceValidate.Build<ExerciseLinkDto>()
        .EnsureNotEmpty(command.SourceExerciseId, errorMessage)  // Direct use
        .EnsureNotEmpty(command.TargetExerciseId, errorMessage)  // Direct use
}
```

## Pattern 6: Method Consistency Within Service

### ❌ WRONG - Inconsistent patterns between methods
```csharp
// Method 1: Parses IDs at top
public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(
    string sourceExerciseId,
    string targetExerciseId,
    ExerciseLinkType linkType)
{
    var sourceId = ExerciseId.ParseOrEmpty(sourceExerciseId);
    var targetId = ExerciseId.ParseOrEmpty(targetExerciseId);
    // ...
}

// Method 2: Doesn't parse IDs at top
public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(CreateExerciseLinkCommand command)
{
    return await ServiceValidate.Build<ExerciseLinkDto>()
        .EnsureNotEmpty(
            ExerciseId.ParseOrEmpty(command.SourceExerciseId),  // Inline parsing
            errorMessage)
    // ...
}
```

### ✅ CORRECT - Consistent patterns
```csharp
// Method 1: Parse at top
public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(
    string sourceExerciseId,
    string targetExerciseId,
    ExerciseLinkType linkType)
{
    var sourceId = ExerciseId.ParseOrEmpty(sourceExerciseId);
    var targetId = ExerciseId.ParseOrEmpty(targetExerciseId);
    // ...
}

// Method 2: Also parse at top
public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(CreateExerciseLinkCommand command)
{
    var sourceId = ExerciseId.ParseOrEmpty(command.SourceExerciseId);
    var targetId = ExerciseId.ParseOrEmpty(command.TargetExerciseId);
    
    return await ServiceValidate.Build<ExerciseLinkDto>()
        .EnsureNotEmpty(sourceId, errorMessage)
    // ...
}
```

## Benefits of These Patterns

1. **Readability**: Code reads like natural language
2. **Maintainability**: Business rules are explicit and named
3. **Testability**: Each validation extension can be tested independently
4. **Performance**: IDs parsed only once
5. **Consistency**: Same patterns across all methods
6. **Type Safety**: Specialized IDs prevent wrong ID usage

## Implementation Checklist

When reviewing or writing validation code:

- [ ] No ServiceError.ValidationFailed wrapper for ValidationFailed errors
- [ ] All symbolic expressions replaced with extension methods
- [ ] Helper methods use positive assertions
- [ ] IDs parsed once at method entry
- [ ] Specialized IDs passed to helper methods
- [ ] Commands contain specialized IDs, not strings
- [ ] Consistent patterns across all methods in service

## Example: Complete Refactored Method

```csharp
public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(
    string sourceExerciseId,
    string targetExerciseId,
    ExerciseLinkType linkType)
{
    // Parse once at the top
    var sourceId = ExerciseId.ParseOrEmpty(sourceExerciseId);
    var targetId = ExerciseId.ParseOrEmpty(targetExerciseId);
    
    return await ServiceValidate.Build<ExerciseLinkDto>()
        // Use direct error messages, no wrapper
        .EnsureNotEmpty(sourceId, ExerciseLinkErrorMessages.InvalidSourceExerciseId)
        .EnsureNotEmpty(targetId, ExerciseLinkErrorMessages.InvalidTargetExerciseId)
        // Use semantic extension methods
        .EnsureExercisesAreDifferent(sourceId, targetId, ExerciseLinkErrorMessages.CannotLinkToSelf)
        .EnsureValidLinkType(linkType, ExerciseLinkErrorMessages.InvalidLinkTypeEnum)
        .EnsureLinkTypeIsNotWorkout(linkType, ExerciseLinkErrorMessages.WorkoutLinksAutoCreated)
        // Pass specialized IDs to async validations
        .EnsureAsync(
            async () => await IsSourceExerciseValidAsync(sourceId),
            ExerciseLinkErrorMessages.SourceExerciseNotFound)
        .EnsureAsync(
            async () => await IsTargetExerciseValidAsync(targetId),
            ExerciseLinkErrorMessages.TargetExerciseNotFound)
        .EnsureAsync(
            async () => await IsNotRestExerciseAsync(sourceId),
            ExerciseLinkErrorMessages.RestExercisesCannotHaveLinks)
        .EnsureAsync(
            async () => await IsNotRestExerciseAsync(targetId),
            ExerciseLinkErrorMessages.RestExercisesCannotBeLinked)
        .EnsureAsync(
            async () => await IsLinkTypeCompatibleAsync(sourceId, targetId, linkType),
            GetLinkTypeCompatibilityError(linkType))
        .MatchAsync(
            whenValid: async () => await CreateBidirectionalLinkAsync(sourceId, targetId, linkType)
        );
}

// Helper methods use specialized IDs
private async Task<bool> IsNotRestExerciseAsync(ExerciseId exerciseId)
{
    var result = await exerciseService.GetByIdAsync(exerciseId);
    return result.IsSuccess 
        ? !result.Data.ExerciseTypes.Any(et => et.Value == "Rest")
        : false;
}

// Local extension methods at bottom of file
private static class ValidationExtensions
{
    public static ServiceValidationBuilder<T> EnsureExercisesAreDifferent<T>(
        this ServiceValidationBuilder<T> builder,
        ExerciseId sourceId,
        ExerciseId targetId,
        string errorMessage)
    {
        return builder.Ensure(() => sourceId != targetId, errorMessage);
    }
    
    public static ServiceValidationBuilder<T> EnsureValidLinkType<T>(
        this ServiceValidationBuilder<T> builder,
        ExerciseLinkType linkType,
        string errorMessage)
    {
        return builder.Ensure(
            () => Enum.IsDefined(typeof(ExerciseLinkType), linkType),
            errorMessage);
    }
    
    public static ServiceValidationBuilder<T> EnsureLinkTypeIsNotWorkout<T>(
        this ServiceValidationBuilder<T> builder,
        ExerciseLinkType linkType,
        string errorMessage)
    {
        return builder.Ensure(
            () => linkType != ExerciseLinkType.WORKOUT,
            errorMessage);
    }
}
```

## Related Documentation

- [ServiceValidatePattern.md](./ServiceValidatePattern.md) - Core validation patterns
- [ValidationExtensionsCatalog.md](./ValidationExtensionsCatalog.md) - Available extensions
- [SpecializedIdTypes.md](./SpecializedIdTypes.md) - ID parsing patterns
- [RequestCommandSeparation.md](./ArchitecturalPatterns/RequestCommandSeparation.md) - Command patterns