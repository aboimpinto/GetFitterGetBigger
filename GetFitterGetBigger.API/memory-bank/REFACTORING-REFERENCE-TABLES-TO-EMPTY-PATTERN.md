# Refactoring Reference Tables to Empty Pattern

## Overview
This document captures the learnings from refactoring MovementPattern to use the Null Object Pattern (Empty Pattern) and provides a guide for future reference table refactoring.

## Key Changes Required

### 1. Make TryParse Private in SpecializedId Classes
```csharp
// Change from:
public static bool TryParse(string? input, out MovementPatternId result)

// To:
private static bool TryParse(string? input, out MovementPatternId result)
```

This forces all consumers to use `ParseOrEmpty` which always returns a valid instance.

### 2. Controller Pattern - Keep It Simple
Controllers should follow this pattern exactly:
```csharp
public async Task<IActionResult> GetById(string id)
{
    _logger.LogInformation("Getting {EntityType} with ID: {Id}", "EntityName", id);
    
    var result = await _service.GetByIdAsync(EntityId.ParseOrEmpty(id));
    
    return result switch
    {
        { IsSuccess: true } => Ok(result.Data),
        { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(),
        { Errors: var errors } => BadRequest(new { errors })
    };
}
```

**Key Points:**
- Parse using `ParseOrEmpty` 
- Pass directly to service without validation
- Simple pattern matching for responses
- NO business logic in controller

### 3. Service Implementation Pattern
Follow the BodyPartService pattern:
```csharp
public async Task<ServiceResult<TDto>> GetByIdAsync(TId id) => id switch
{
    { IsEmpty: true } => ServiceResult<TDto>.Failure(
        CreateEmptyDto(),
        "Invalid {entity} ID format. Expected format: '{prefix}-{guid}'"),
    _ => await GetByIdAsync(id.ToString())
};
```

### 4. LoadEntityByIdAsync Implementation
```csharp
protected override async Task<TEntity> LoadEntityByIdAsync(
    IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, 
    string id) =>
    TId.ParseOrEmpty(id) switch
    {
        { IsEmpty: true } => TEntity.Empty,
        var entityId => await unitOfWork.GetRepository<IRepository>()
            .GetByIdAsync(entityId)
    };
```

## Common Issues and Solutions

### Issue 1: Empty GUID Returns Wrong Status Code
**Problem**: ID "entity-00000000-0000-0000-0000-000000000000" returns 400 instead of 404.

**Solution**: The service's `GetByIdAsync(TId id)` method should check for empty IDs and return appropriate error message. The base class will then treat valid but non-existent IDs as "not found".

### Issue 2: Invalid Format Returns Wrong Error
**Problem**: ID without proper prefix returns 404 instead of 400.

**Solution**: Invalid formats get parsed to Empty IDs, which the service catches and returns as invalid format errors.

### Issue 3: Test Failures After Refactoring
**Problem**: Unit tests expect controller validation that no longer exists.

**Solution**: Update tests to expect service to be called and mock appropriate responses.

## Migration Checklist

When refactoring a reference table to use Empty Pattern:

- [ ] Make TryParse private in the SpecializedId class
- [ ] Update all TryParse calls to use ParseOrEmpty
- [ ] Simplify controller to just parse and delegate
- [ ] Update service to check for empty IDs in GetByIdAsync(TId)
- [ ] Ensure LoadEntityByIdAsync returns Entity.Empty for empty IDs
- [ ] Update unit tests to match new behavior
- [ ] Verify integration tests pass

## Testing Scenarios

Ensure these scenarios work correctly:
1. Valid ID that exists → 200 OK
2. Valid ID that doesn't exist → 404 Not Found
3. Empty GUID (all zeros) → 404 Not Found
4. Invalid format (wrong prefix) → 400 Bad Request
5. Completely invalid string → 400 Bad Request

## Benefits

1. **Consistency**: All reference tables behave the same way
2. **Simplicity**: Controllers are simple translators
3. **Null Safety**: No null references, always have valid objects
4. **Clear Separation**: Business logic stays in services

## Remember

> "Controllers translate HTTP to service calls. They don't think, they just translate."

The controller's only job is to:
1. Parse the input using ParseOrEmpty
2. Call the service
3. Map the result to HTTP response

All validation and business logic belongs in the service layer.