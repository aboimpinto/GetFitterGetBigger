# EntityResult Pattern

## Overview

The EntityResult pattern is a validation pattern that allows entity factory methods (smart constructors) to return validation failures without throwing exceptions. This pattern aligns with our architectural principle of avoiding exceptions for control flow and complements the existing ServiceResult pattern used at the service layer.

## Purpose

1. **No Exceptions**: Eliminate exception throwing from entity creation methods
2. **Explicit Error Handling**: Force callers to handle validation failures at compile time
3. **Consistent Patterns**: Align with the ServiceResult pattern already used in services
4. **Better Testability**: Enable testing validation failures without exception assertions

## Implementation

### Core EntityResult<T> Class

Located at: `/GetFitterGetBigger.API/Models/Results/EntityResult.cs`

```csharp
public class EntityResult<T> where T : class
{
    public T? Value { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<string> Errors { get; }
    public string FirstError => Errors.FirstOrDefault() ?? string.Empty;
    
    // Static factory methods
    public static EntityResult<T> Success(T value)
    public static EntityResult<T> Failure(params string[] errors)
    public static EntityResult<T> Failure(string error)
}
```

### Usage in Entity Handlers

#### Before (Throwing Exceptions):
```csharp
public static class Handler
{
    public static DifficultyLevel CreateNew(string value, ...)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be empty", nameof(value));
            
        return new() { ... };
    }
}
```

#### After (EntityResult Pattern):
```csharp
public static class Handler
{
    public static EntityResult<DifficultyLevel> CreateNew(string value, ...)
    {
        if (string.IsNullOrEmpty(value))
            return EntityResult<DifficultyLevel>.Failure("Value cannot be empty");
            
        return EntityResult<DifficultyLevel>.Success(new() { ... });
    }
}
```

## Test Helpers

To simplify test scenarios where entity creation is expected to succeed, we provide extension methods:

Located at: `/GetFitterGetBigger.API.Tests/TestHelpers/EntityResultExtensions.cs`

```csharp
// For tests that expect success
var entity = DifficultyLevel.Handler.CreateNew("Easy", "Description", 1).Unwrap();

// For tests that handle both success and failure
var entity = DifficultyLevel.Handler.CreateNew(value, desc, order)
    .UnwrapOr(DifficultyLevel.Empty);
```

## Service Layer Integration

Services that use Handler.CreateNew methods should convert EntityResult failures to ServiceResult failures:

```csharp
public async Task<ServiceResult<ReferenceDataDto>> CreateAsync(CreateRequest request)
{
    var entityResult = DifficultyLevel.Handler.CreateNew(
        request.Value,
        request.Description,
        request.DisplayOrder
    );
    
    if (entityResult.IsFailure)
    {
        return ServiceResult<ReferenceDataDto>.Failure(
            CreateEmptyDto(),
            ServiceError.ValidationFailed(entityResult.FirstError)
        );
    }
    
    // Continue with entity persistence...
}
```

## Migration Strategy

1. **Phase 1**: Implement EntityResult for entities using Empty pattern
   - DifficultyLevel ✅
   - BodyPart ✅
   - MovementPattern ✅

2. **Phase 2**: Extend to other reference data entities as they adopt Empty pattern

3. **Phase 3**: Consider extending to domain entities (Exercise, WorkoutLog, etc.)

## Key Benefits

1. **Type Safety**: Compile-time enforcement of error handling
2. **Consistency**: Aligned with ServiceResult pattern
3. **Clarity**: Explicit validation failures instead of hidden exceptions
4. **Testability**: Easy to test both success and failure paths
5. **Performance**: No exception overhead for validation failures

## Future Considerations

### Invalid Entity State

The EntityResult pattern sets the foundation for future enhancements like invalid entity states:

```csharp
// Potential future extension
public static class Handler
{
    public static EntityResult<DifficultyLevel> CreateNew(string value, ...)
    {
        if (string.IsNullOrEmpty(value))
            return EntityResult<DifficultyLevel>.Invalid("Value cannot be empty");
            
        if (ContainsProfanity(value))
            return EntityResult<DifficultyLevel>.Invalid("Value contains inappropriate content");
            
        return EntityResult<DifficultyLevel>.Success(new() { ... });
    }
}
```

This could enable returning special "Invalid" instances that carry validation information while still being usable in certain contexts.

## Guidelines

1. **Always validate input** in Handler.CreateNew methods
2. **Return descriptive error messages** that help identify the validation failure
3. **Don't throw exceptions** - always return EntityResult
4. **Convert to ServiceResult** at the service layer for API consistency
5. **Use test helpers** to simplify test code

## Related Patterns

- **Empty Pattern**: For handling null/missing entities
- **ServiceResult Pattern**: For service layer operations
- **Unit of Work Pattern**: For database transactions
- **Cache Result Pattern**: For cache operations