# Controller Pattern Matching Optimization

**Status**: ✅ Active  
**Category**: Controller Patterns  
**Related**: [ControllerPatterns.md](./ControllerPatterns.md), [SingleExitPointPattern.md](./SingleExitPointPattern.md)

## Overview

This guideline defines how to optimize pattern matching in controller methods by grouping switch cases by HTTP status code rather than business error codes. This reduces redundancy and improves maintainability.

## The Problem

Controllers often have redundant switch cases that all return the same HTTP status code:

```csharp
// ❌ ANTI-PATTERN - Redundant cases
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.ValidationFailed, StructuredErrors: var errors } => BadRequest(new { errors }),
    { PrimaryErrorCode: ServiceErrorCode.InvalidFormat, StructuredErrors: var errors } => BadRequest(new { errors }),
    { PrimaryErrorCode: ServiceErrorCode.InvalidInput, StructuredErrors: var errors } => BadRequest(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })
};
```

This violates the DRY principle and makes the code harder to maintain.

## The Solution

Group switch cases by HTTP status code, using a default case for the most common error response:

```csharp
// ✅ CORRECT - Optimized by HTTP status
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })  // Handles ALL other errors
};
```

## Key Principles

### 1. Controllers Care About HTTP Status, Not Business Logic

Controllers are part of the web layer and should map business results to HTTP responses. They don't need to differentiate between `ValidationFailed`, `InvalidFormat`, and `InvalidInput` if they all return 400 Bad Request.

### 2. One Case Per HTTP Status Code

Each switch case should return a DIFFERENT HTTP status code. If multiple error codes map to the same HTTP status, use a single case.

### 3. Use Default Case for Most Common Response

The default/catch-all case should handle the most common error response, typically BadRequest (400).

## Implementation Guidelines

### Standard Pattern

```csharp
public async Task<IActionResult> ActionMethod(string id, [FromBody] RequestDto request)
{
    // Parse IDs and create command
    var parsedId = EntityId.ParseOrEmpty(id);
    var command = request.ToCommand();
    
    // Call service
    var result = await _service.MethodAsync(parsedId, command);
    
    // Single exit point with optimized pattern matching
    var actionResult = result switch
    {
        { IsSuccess: true } => Ok(result.Data),
        { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
        { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
        { StructuredErrors: var errors } => BadRequest(new { errors })
    };
    
    return actionResult;
}
```

### HTTP Status Mapping Table

| ServiceErrorCode | HTTP Status | When to Add Specific Case |
|------------------|-------------|---------------------------|
| Success | 200 OK / 201 Created | Always (success case) |
| NotFound | 404 Not Found | Always |
| Unauthorized | 403 Forbidden | When authorization is checked |
| DependencyExists | 409 Conflict | When dependencies block operation |
| AlreadyExists | 409 Conflict | For create operations |
| ValidationFailed | 400 Bad Request | Use default case |
| InvalidFormat | 400 Bad Request | Use default case |
| InvalidInput | 400 Bad Request | Use default case |
| InternalError | 400 Bad Request | Use default case |
| Any other error | 400 Bad Request | Use default case |

### Common Patterns by Operation Type

#### GET (Single Resource)
```csharp
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.Unauthorized } => Forbid(),
    { StructuredErrors: var errors } => BadRequest(new { errors })
};
```

#### GET (List/Search)
```csharp
// Search operations ALWAYS return 200 for security
return Ok(result.Data);
```

#### POST (Create)
```csharp
return result switch
{
    { IsSuccess: true } => CreatedAtAction(nameof(GetMethod), new { id = result.Data.Id }, result.Data),
    { PrimaryErrorCode: ServiceErrorCode.AlreadyExists, StructuredErrors: var errors } => Conflict(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })
};
```

#### PUT (Update)
```csharp
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.AlreadyExists, StructuredErrors: var errors } => Conflict(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })
};
```

#### DELETE
```csharp
return result switch
{
    { IsSuccess: true } => NoContent(),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })
};
```

## Refactoring Process

When refactoring existing controllers:

1. **Identify all unique HTTP status codes** being returned
2. **Group error codes by their HTTP status**
3. **Create one case per HTTP status**
4. **Use default case for 400 Bad Request**
5. **Apply single exit point pattern**

### Before and After Example

```csharp
// ❌ BEFORE - Verbose and redundant
public async Task<IActionResult> ChangeState(string id, [FromBody] ChangeStateDto request)
{
    _logger.LogInformation("Changing state of {Id} to {StateId}", id, request.StateId);
    
    var result = await _service.ChangeStateAsync(
        WorkoutTemplateId.ParseOrEmpty(id),
        WorkoutStateId.ParseOrEmpty(request.StateId));

    return result switch
    {
        { IsSuccess: true } => Ok(result.Data),
        { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
        { PrimaryErrorCode: ServiceErrorCode.ValidationFailed, StructuredErrors: var errors } => BadRequest(new { errors }),
        { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
        { StructuredErrors: var errors } => BadRequest(new { errors })
    };
}

// ✅ AFTER - Clean and optimized
public async Task<IActionResult> ChangeState(string id, [FromBody] ChangeStateDto request)
{
    var parsedId = WorkoutTemplateId.ParseOrEmpty(id);
    var parsedStateId = WorkoutStateId.ParseOrEmpty(request.StateId);

    var result = await _service.ChangeStateAsync(parsedId, parsedStateId);

    var actionResult = result switch
    {
        { IsSuccess: true } => Ok(result.Data),
        { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
        { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
        { StructuredErrors: var errors } => BadRequest(new { errors })
    };

    return actionResult;
}
```

## Common Mistakes to Avoid

### 1. Enumerating All Error Codes

```csharp
// ❌ WRONG - Lists every possible error code
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.ValidationFailed } => BadRequest(result.Message),
    { PrimaryErrorCode: ServiceErrorCode.InvalidFormat } => BadRequest(result.Message),
    { PrimaryErrorCode: ServiceErrorCode.InvalidInput } => BadRequest(result.Message),
    { PrimaryErrorCode: ServiceErrorCode.InternalError } => BadRequest(result.Message),
    // ... more cases
};
```

### 2. Missing Default Case

```csharp
// ❌ WRONG - No catch-all for unexpected errors
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound()
    // What happens for other errors?
};
```

### 3. Logging in Controllers

```csharp
// ❌ WRONG - Controllers shouldn't log
_logger.LogInformation("Processing request for {Id}", id);
```

### 4. Multiple Return Statements

```csharp
// ❌ WRONG - Multiple exit points
if (result.IsSuccess)
    return Ok(result.Data);
    
if (result.PrimaryErrorCode == ServiceErrorCode.NotFound)
    return NotFound();
    
return BadRequest(result.StructuredErrors);
```

## Testing Considerations

When testing controllers with optimized pattern matching:

1. **Test HTTP status codes, not error codes**
   ```csharp
   // Test that validation errors return 400
   result.Should().BeOfType<BadRequestObjectResult>();
   ```

2. **Don't test specific error code handling in controllers**
   - That's the service's responsibility
   - Controllers just map to HTTP status

3. **Verify error structure is passed through**
   ```csharp
   var badRequest = result as BadRequestObjectResult;
   badRequest.Value.Should().BeEquivalentTo(new { errors = expectedErrors });
   ```

## Benefits

1. **Reduced Code**: Fewer lines, easier to read
2. **DRY Principle**: No repeated BadRequest cases
3. **Maintainability**: Adding new error codes doesn't require controller changes
4. **Clear Intent**: Shows which business errors map to which HTTP statuses
5. **Consistency**: All controllers follow the same pattern

## Related Patterns

- [ControllerPatterns.md](./ControllerPatterns.md) - General controller guidelines
- [SingleExitPointPattern.md](./SingleExitPointPattern.md) - Single return statement
- [ServiceResultPattern.md](./ServiceResultPattern.md) - Service result handling

## Quick Reference

See [ControllerPatternMatchingQuickReference.md](../PracticalGuides/ControllerPatternMatchingQuickReference.md) for a condensed version with copy-paste templates.