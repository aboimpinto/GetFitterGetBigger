# ID Validation Pattern - Consistent Approach

**🎯 PURPOSE**: Establish a consistent pattern for validating specialized IDs across all services

## Core Principle

```
┌─────────────────────────────────────────────────────────────────┐
│ 🔑 ID VALIDATION PATTERN - ALWAYS USE EnsureNotEmpty           │
├─────────────────────────────────────────────────────────────────┤
│ Use: .EnsureNotEmpty(id, EntityErrorMessages.InvalidIdFormat)  │
│ Returns: ValidationFailed error (not InvalidFormat)            │
│ Controller: Handles as BadRequest (not special InvalidFormat)  │
│ Benefit: Consistent, readable, reusable pattern               │
└─────────────────────────────────────────────────────────────────┘
```

## The Pattern

### ✅ CORRECT - Use EnsureNotEmpty Extension

```csharp
// ✅ Service method - Clean and readable
public async Task<ServiceResult<ExerciseDto>> GetByIdAsync(ExerciseId id)
{
    return await ServiceValidate.For<ExerciseDto>()
        .EnsureNotEmpty(id, ExerciseErrorMessages.InvalidIdFormat)  // ✅ Clean!
        .EnsureAsync(
            async () => await ExerciseExistsAsync(id),
            ServiceError.NotFound("Exercise", id.ToString()))
        .MatchAsync(
            whenValid: async () => await GetExerciseByIdInternalAsync(id)
        );
}

// ✅ Controller - Simple handling
public async Task<IActionResult> GetExercise(string id)
{
    var result = await _exerciseService.GetByIdAsync(ExerciseId.ParseOrEmpty(id));
    
    return result switch
    {
        { IsSuccess: true } => Ok(result.Data),
        { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
        _ => BadRequest(result.Errors.First().Message)  // ✅ No special InvalidFormat case
    };
}
```

### ❌ ANTI-PATTERN - Don't Use Lambda with Negation

```csharp
// ❌ Service method - Harder to read
public async Task<ServiceResult<ExerciseDto>> GetByIdAsync(ExerciseId id)
{
    return await ServiceValidate.For<ExerciseDto>()
        .Ensure(() => !id.IsEmpty, ServiceError.InvalidFormat(...))  // ❌ "() => !" reduces readability
        // ...
}

// ❌ Controller - Unnecessary complexity
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.InvalidFormat } => BadRequest(...),  // ❌ Extra case
    _ => BadRequest(...)
};
```

## Why This Pattern?

1. **Readability**: `EnsureNotEmpty(id, ...)` is clearer than `Ensure(() => !id.IsEmpty, ...)`
2. **Consistency**: All reference services use the same pattern
3. **Reusability**: The extension method is already available and tested
4. **Simplicity**: Controllers don't need special InvalidFormat handling
5. **Trust**: We trust the architecture - IDs are parsed at controller level

## Error Message Pattern

Each entity should define its InvalidIdFormat message:

```csharp
public static class ExerciseErrorMessages
{
    public static string InvalidIdFormat => "Invalid Exercise ID format. Expected format: exercise-{guid}";
    // ...
}

public static class BodyPartErrorMessages
{
    public static string InvalidIdFormat => "Invalid BodyPart ID format. Expected format: bodypart-{guid}";
    // ...
}
```

## The Extension Method

The `EnsureNotEmpty` extension is already available for all specialized IDs:

```csharp
// Already exists in the codebase
public static ServiceValidate<T> EnsureNotEmpty<T>(
    this ServiceValidate<T> validate, 
    ISpecializedId id, 
    string errorMessage)
{
    return validate.Ensure(() => !id.IsEmpty, ServiceError.ValidationFailed(errorMessage));
}
```

## Consistent Service Pattern

All services should follow this pattern:

```csharp
// GetById
public async Task<ServiceResult<TDto>> GetByIdAsync(TId id)
{
    return await ServiceValidate.For<TDto>()
        .EnsureNotEmpty(id, EntityErrorMessages.InvalidIdFormat)
        .EnsureAsync(/* exists check */)
        .MatchAsync(/* load logic */);
}

// Update
public async Task<ServiceResult<TDto>> UpdateAsync(TId id, UpdateCommand command)
{
    return await ServiceValidate.For<TDto>()
        .EnsureNotEmpty(id, EntityErrorMessages.InvalidIdFormat)
        // ... other validations
        .MatchAsync(/* update logic */);
}

// Delete
public async Task<ServiceResult<bool>> DeleteAsync(TId id)
{
    return await ServiceValidate.Build<bool>()
        .EnsureNotEmpty(id, EntityErrorMessages.InvalidIdFormat)
        .EnsureAsync(/* exists check */)
        .MatchAsync(/* delete logic */);
}
```

## Key Takeaways

1. **Always use** `EnsureNotEmpty(id, ErrorMessages.InvalidIdFormat)`
2. **Never use** `Ensure(() => !id.IsEmpty, ...)` for ID validation
3. **Controllers** handle all validation errors as BadRequest (no special cases)
4. **Trust** that IDs are parsed correctly at controller level
5. **Reuse** existing extensions and patterns

## Related Documentation
- [ServiceValidate Pattern](./ServiceValidatePattern.md)
- [Specialized ID Types](./SpecializedIdTypes.md)
- [No Null Command Pattern](./NoNullCommandPattern.md)
- [Clean Validation Pattern](./CleanValidationPattern.md)

## Remember

> "Trust the architecture and focus on business rule validation instead. Use the patterns that already exist - they're there for a reason."

---

*This pattern ensures consistency across all services and improves code readability by removing unnecessary lambda expressions and negations.*