# Single Exit Point Refactoring with ServiceValidate Pattern

## Problem
Methods with multiple exit points violate Golden Rule #1 and make code harder to maintain and test.

## Solution
Use `ServiceValidate.Build<T>().MatchAsync()` pattern to ensure single exit point, even when no validations are needed.

## Pattern Structure

### Before (Multiple Exit Points)
```csharp
private async Task<ServiceResult<TDto>> MethodWithMultipleExits(...)
{
    var entity = await repository.GetByIdAsync(id);
    if (entity.IsEmpty)
    {
        return ServiceResult<TDto>.Success(TDto.Empty);  // Exit point 1
    }
    
    var result = SomeOperation(entity);
    if (!result.IsSuccess)
    {
        return ServiceResult<TDto>.Failure(...);  // Exit point 2
    }
    
    var dto = result.Value.ToDto();
    return ServiceResult<TDto>.Success(dto);  // Exit point 3
}
```

### After (Single Exit Point)
```csharp
private async Task<ServiceResult<TDto>> MethodWithSingleExit(...)
{
    return await ServiceValidate.Build<TDto>()
        .MatchAsync(
            async () =>
            {
                var entity = await repository.GetByIdAsync(id);
                
                if (entity.IsEmpty)
                {
                    return ServiceResult<TDto>.Success(TDto.Empty);
                }
                
                var result = SomeOperation(entity);
                if (!result.IsSuccess)
                {
                    return ServiceResult<TDto>.Failure(
                        TDto.Empty,
                        ServiceError.ValidationFailed($"Operation failed: {result.Error?.Message}"));
                }
                
                var dto = result.Value.ToDto();
                return ServiceResult<TDto>.Success(dto);
            },
            errors => ServiceResult<TDto>.Failure(TDto.Empty, errors.First()));
}
```

## Key Principles

### 1. Use Build<T> Even Without Validations
```csharp
// Even with no validations, use Build<T> for single exit point
return await ServiceValidate.Build<TDto>()
    .MatchAsync(
        async () => { /* all logic here */ },
        errors => { /* error handling */ });
```

### 2. Handle Business Logic Inside MatchAsync
All business logic, including repository operations and transformations, goes inside the `whenValid` callback:
```csharp
.MatchAsync(
    async () =>
    {
        // 1. Load data
        var entity = await repository.GetByIdAsync(id);
        
        // 2. Handle business cases (like empty entities)
        if (entity.IsEmpty)
            return ServiceResult<TDto>.Success(TDto.Empty);
        
        // 3. Perform operations
        var result = await PerformOperation(entity);
        
        // 4. Transform and return
        return ServiceResult<TDto>.Success(result.ToDto());
    },
    errors => ServiceResult<TDto>.Failure(TDto.Empty, errors.First()));
```

### 3. Add Validations When Needed
If you have preconditions to validate:
```csharp
return await ServiceValidate.Build<TDto>()
    .EnsureNotEmpty(id, "ID is required")
    .EnsureNotWhiteSpace(name, "Name is required")
    .EnsureNameIsUniqueAsync(
        async () => await IsNameUnique(name),
        "Entity", name)
    .MatchAsync(
        async () => { /* operations */ },
        errors => { /* error handling */ });
```

## Common Patterns

### Pattern 1: Entity Load with Empty Check
```csharp
return await ServiceValidate.Build<TDto>()
    .MatchAsync(
        async () =>
        {
            var entity = await repository.GetByIdAsync(id);
            if (entity.IsEmpty)
                return ServiceResult<TDto>.Success(TDto.Empty);
            
            // Continue with entity operations
            return ServiceResult<TDto>.Success(entity.ToDto());
        },
        errors => ServiceResult<TDto>.Failure(TDto.Empty, errors.First()));
```

### Pattern 2: Entity Creation with Result Handling
```csharp
return await ServiceValidate.Build<TDto>()
    .MatchAsync(
        async () =>
        {
            var createResult = Entity.Handler.CreateNew(...);
            if (!createResult.IsSuccess)
                return ServiceResult<TDto>.Failure(
                    TDto.Empty,
                    ServiceError.ValidationFailed($"Creation failed: {createResult.Error?.Message}"));
            
            await repository.AddAsync(createResult.Value);
            return ServiceResult<TDto>.Success(createResult.Value.ToDto());
        },
        errors => ServiceResult<TDto>.Failure(TDto.Empty, errors.First()));
```

### Pattern 3: Update Operations
```csharp
return await ServiceValidate.Build<TDto>()
    .MatchAsync(
        async () =>
        {
            var entity = await repository.GetByIdAsync(id);
            if (entity.IsEmpty)
                return ServiceResult<TDto>.Success(TDto.Empty);
            
            var updated = updateAction(entity);
            await repository.UpdateAsync(updated);
            
            var reloaded = await repository.GetByIdAsync(id);
            return ServiceResult<TDto>.Success(reloaded.ToDto());
        },
        errors => ServiceResult<TDto>.Failure(TDto.Empty, errors.First()));
```

## Methods Identified for Refactoring

The following methods in `WorkoutTemplateCommandDataService` have multiple exit points and should be refactored:

1. **UpdateWithScopeAsync** (Lines 240-263)
   - 2 exit points
   - Pattern: Entity load with empty check + update

2. **DeleteWithScopeAsync** (Lines 345-362)
   - 2 exit points
   - Pattern: Entity load with empty check + delete

3. **CreateWithScopeAsync** (Lines 206-221)
   - 1 exit point (already good, but could be cleaner)

## Benefits

1. **Single Exit Point**: All methods have exactly one return statement
2. **Consistent Pattern**: Easy to understand and maintain
3. **Testability**: Easier to mock and test with single flow
4. **Error Handling**: Centralized error handling in one place
5. **Future Extensibility**: Easy to add validations later

## When to Use This Pattern

Use this refactoring pattern when:
- Method has multiple return statements
- Method performs data operations (CRUD)
- Method needs to handle empty/null cases
- Method needs to transform Result<T> to ServiceResult<T>

## When NOT to Use

- Simple property getters
- Pure calculation methods without I/O
- Methods that already have single exit point
- Methods that don't return ServiceResult<T>

## Migration Strategy

1. Identify methods with multiple exit points
2. Wrap entire method logic in ServiceValidate.Build<T>().MatchAsync()
3. Move all business logic into whenValid callback
4. Ensure all paths return ServiceResult<T>
5. Test thoroughly to ensure behavior is preserved