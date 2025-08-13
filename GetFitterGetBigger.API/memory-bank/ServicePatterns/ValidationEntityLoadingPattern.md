# ServiceValidation Entity Loading Pattern

## Overview
A fluent validation pattern that enables loading and validating entities as part of the validation chain, carrying them through for use in subsequent operations. This eliminates redundant null checks and centralizes validation logic.

## Problem Solved
Previously, validation and entity loading were split across multiple methods:
```csharp
// ❌ OLD: Split validation and loading
public async Task<ServiceResult<T>> OperationAsync(Id id)
{
    // Basic validation
    var validation = ServiceValidate.Build<T>()
        .EnsureNotEmpty(id, "Invalid ID")
        .WhenValidAsync(async () => await PerformOperationAsync(id));
}

private async Task<ServiceResult<T>> PerformOperationAsync(Id id)
{
    // Load entity
    var result = await service.GetByIdAsync(id);
    
    // Redundant validation
    if (!result.IsSuccess || result.Data == null || result.Data.IsEmpty)
    {
        return ServiceResult<T>.Failure(T.Empty, ServiceError.NotFound("Not found"));
    }
    
    // Process...
}
```

## Solution
The new pattern combines validation and entity loading in a single fluent chain:
```csharp
// ✅ NEW: Combined validation and loading
public async Task<ServiceResult<T>> OperationAsync(Id id, string name)
{
    return await ServiceValidate.Build<T>()
        .EnsureNotEmpty(id, "Invalid ID")
        .EnsureNotWhiteSpace(name, "Name required")
        .EnsureAsync(async () => await IsNameUniqueAsync(name), "Name exists")
        .WhenValidAsync(async () => 
        {
            return await ServiceValidate.Build<T>()
                .Validation
                .AsAsync()
                .EnsureEntityExists(service, id)
                .ThenWithEntity(async entity => 
                    await ProcessEntity(entity, name));
        });
}
```

## Implementation Components

### 1. Generic Entity Loading Extensions
Located in: `/Services/Validation/ServiceValidationEntityExtensions.cs`

#### Core Methods:
- `EnsureEntityExists<TResult, TEntity>()` - Loads and validates entity exists
- `ThenWithEntity<TResult, TEntity>()` - Terminal operation with loaded entity
- `EnsureWithEntity()` - Additional validation using loaded entity
- `EnsureAsyncWithEntity()` - Async validation using loaded entity
- `Transform()` - Transform entity to different type
- `EnsureEntityExistsIf()` - Conditional entity loading

### 2. Domain-Specific Extensions
Located in: `/Services/[Domain]/Validation/[Domain]ValidationExtensions.cs`

Example for WorkoutTemplate:
- `EnsureWorkoutTemplateExists()` - Load and validate workout template
- `ThenWithWorkoutTemplate()` - Continue with loaded template
- `EnsureNameIsUniqueForDuplication()` - Domain-specific validation
- `EnsureTemplateInModifiableState()` - State-based validation

### 3. ServiceValidationWithData
Carries validation state and loaded entity data through the chain.

## Usage Examples

### Basic Entity Loading
```csharp
return await validation.AsAsync()
    .EnsureWorkoutTemplateExists(queryService, templateId)
    .ThenWithWorkoutTemplate(async template => 
        await ProcessTemplate(template));
```

### With Additional Validations
```csharp
return await validation.AsAsync()
    .EnsureWorkoutTemplateExists(queryService, templateId)
    .EnsureNameIsUniqueForDuplication(queryService, newName)
    .EnsureTemplateInModifiableState(["DRAFT", "REVIEW"])
    .ThenWithWorkoutTemplate(async template => 
        await UpdateTemplate(template, changes));
```

### Conditional Loading
```csharp
return await validation.AsAsync()
    .EnsureEntityExistsIf(
        condition: !isNewTemplate,
        loadFunc: async () => await queryService.GetByIdAsync(id),
        errorMessage: "Template not found")
    .ThenWithEntity(async template => 
        await ProcessTemplate(template ?? Template.Empty));
```

## Benefits
1. **Single Responsibility**: All validation logic in one place
2. **No Redundant Checks**: Entity existence validated once
3. **Type Safety**: Compile-time checking of entity types
4. **Readable**: Fluent chain reads like requirements
5. **Testable**: Each step can be tested independently
6. **Reusable**: Generic pattern works for all entities
7. **Efficient**: Fail-fast approach stops at first validation failure

## Error Handling
- Validation errors use `ServiceError.ValidationFailed()`
- Entity not found uses `ServiceError.NotFound()`
- Service failures propagate through the chain
- All errors collected and returned in ServiceResult

## Testing Considerations
When testing handlers using this pattern:
1. Mock the query service to return success/empty for not found scenarios
2. Mock the exists checks separately from entity loading
3. Verify the order of operations (validations before loading)
4. Test both success and failure paths

## Migration Guide
To migrate existing code:
1. Identify split validation/loading patterns
2. Remove redundant null/empty checks from inner methods
3. Create domain-specific extensions if needed
4. Update method to use new pattern
5. Update tests to match new behavior

## Related Patterns
- ServiceValidation Builder Pattern
- Empty Object Pattern (IEmptyDto)
- ServiceResult Pattern
- Repository Pattern with UnitOfWork