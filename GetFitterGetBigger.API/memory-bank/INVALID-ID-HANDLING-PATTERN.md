# Invalid ID Handling Pattern

## Overview

This document describes the consistent pattern for handling invalid ID formats across reference data services, ensuring proper error responses and avoiding unnecessary database calls.

## Pattern Implementation

### Service Layer Pattern Matching

```csharp
public async Task<ServiceResult<TDto>> GetByIdAsync(TId id) => 
    id.IsEmpty 
        ? ServiceResult<TDto>.Failure(CreateEmptyDto(), ServiceError.ValidationFailed("Invalid {entity} ID format"))
        : await GetByIdAsync(id.ToString());
```

### ID Parsing Behavior

```csharp
// In SpecializedId types (e.g., BodyPartId, MovementPatternId)
public static TId ParseOrEmpty(string? input)
{
    if (string.IsNullOrEmpty(input))
        return Empty;
        
    return TryParse(input, out var result) ? result : Empty;
}

public override string ToString() => $"prefix-{this._value}";
```

## Flow for Invalid IDs

1. **Controller receives invalid format**: `"7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"`
2. **ParseOrEmpty called**: Returns `Empty` for invalid format
3. **Service pattern matching**: `id.IsEmpty` → true
4. **Immediate return**: `ValidationFailed` error with 400 status
5. **No database call**: Optimized - avoid unnecessary queries

## HTTP Status Code Mapping

| Scenario | ID Example | Service Error | HTTP Status |
|----------|------------|---------------|-------------|
| Invalid format | `"invalid-guid"` | ValidationFailed | 400 Bad Request |
| Empty GUID | `"prefix-00000000-0000-0000-0000-000000000000"` | ValidationFailed | 400 Bad Request |
| Valid but not found | `"prefix-11111111-1111-1111-1111-111111111111"` | NotFound | 404 Not Found |
| Valid and found | `"prefix-12345678-1234-1234-1234-123456789012"` | Success | 200 OK |

## Controller Pattern Matching

```csharp
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    _ => BadRequest(new { errors = result.StructuredErrors })
};
```

## Integration Test Scenarios

```gherkin
@reference-data @validation
Scenario: Get entity by invalid ID format returns bad request
  When I send a GET request to "/api/ReferenceTables/Entities/7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"
  Then the response status should be 400

@reference-data @validation  
Scenario: Get entity by empty GUID returns bad request
  When I send a GET request to "/api/ReferenceTables/Entities/entity-00000000-0000-0000-0000-000000000000"
  Then the response status should be 400

@reference-data @validation
Scenario: Get entity by non-existent ID returns not found
  When I send a GET request to "/api/ReferenceTables/Entities/entity-11111111-1111-1111-1111-111111111111"
  Then the response status should be 404
```

## Benefits

### 1. Performance Optimization
- No database calls for obviously invalid IDs
- Early validation prevents resource waste

### 2. Consistent Error Responses
- All reference services behave identically
- Predictable API behavior for clients

### 3. Clear Error Categories
- 400: Client provided invalid data format
- 404: Valid format but entity doesn't exist
- 500: Internal server error (rare for reference data)

### 4. Maintainable Code
- Pattern is reusable across all reference services
- Easy to test and reason about

## Implementation Checklist

For each reference service:

- [ ] Implement pattern matching in `GetByIdAsync(TId id)`
- [ ] Ensure `ToString()` always returns prefixed format
- [ ] Update unit tests for empty ID scenarios
- [ ] Update integration tests for invalid format scenarios
- [ ] Verify controller pattern matching includes ValidationFailed case

## Related Patterns

- **Empty/Null Object Pattern**: Used in ID types
- **Service Result Pattern**: Error handling structure
- **Three-Tier Architecture**: Clear separation of concerns

## Migration Notes

When implementing this pattern:

1. **Update Services First**: Implement pattern matching
2. **Update Tests**: Change expectations from NotFound to ValidationFailed
3. **Update Documentation**: Reflect new error codes in API docs
4. **Verify Integration**: Run full test suite

This pattern was implemented as part of the broader Empty/Null Object Pattern migration to eliminate null handling throughout the codebase.