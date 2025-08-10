# Controller Patterns - Thin Pass-Through Layer

**🎯 PURPOSE**: This document defines **MANDATORY** controller patterns that maintain thin, pure pass-through controllers in the GetFitterGetBigger API.

## Overview

Controllers are thin pass-through layers with **NO business logic**. They exist purely to:
- Map HTTP requests to service calls
- Map service results to HTTP responses
- Use pattern matching for clean, single-expression handling

## 🚨 CRITICAL Rules

```
┌──────────────────────────────────────────────────────────────┐
│ 🔴 CRITICAL: Controller Rules - MUST be followed            │
├──────────────────────────────────────────────────────────────┤
│ 1. NO business logic or validation in controllers           │
│ 2. NO error message interpretation or translation           │
│ 3. Single expression bodies when possible                    │
│ 4. Pattern matching for ServiceResult handling              │
│ 5. Pure pass-through of service layer errors                │
└──────────────────────────────────────────────────────────────┘
```

## Basic Controller Pattern

### ❌ BAD - Business Logic in Controller

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateEquipmentDto dto)
{
    // VIOLATION: Validation logic in controller
    if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 100)
        return BadRequest(new { errors = new[] { new { code = 2, message = "Invalid name" } } });
    
    // VIOLATION: Business logic in controller
    if (await _service.ExistsByNameAsync(dto.Name))
        return Conflict(new { errors = new[] { new { code = 3, message = "Name exists" } } });
    
    var command = new CreateEquipmentCommand(dto.Name, dto.Description);
    var result = await _service.CreateAsync(command);
    
    return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Errors);
}
```

### ✅ GOOD - Single Expression, Pattern Matching

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateEquipmentDto dto) =>
    await _service.CreateAsync(dto.ToCommand()) switch
    {
        { IsSuccess: true, Data: var data } => CreatedAtAction(
            nameof(GetById), 
            new { id = data.Id }, 
            data),
        { PrimaryErrorCode: ServiceErrorCode.Conflict } => Conflict(
            new { errors = result.StructuredErrors }),
        { StructuredErrors: var errors } => BadRequest(new { errors })
    };
```

## Pure Pass-Through Pattern

**MANDATORY**: Controllers must NEVER interpret, translate, or modify service error messages.

### ❌ VIOLATION - Controller Interpreting Errors

```csharp
[HttpGet]
public async Task<IActionResult> GetWorkoutTemplates([FromQuery] int page = 1, ...)
{
    var result = await _service.SearchAsync(page, ...);
    
    return result switch
    {
        { IsSuccess: true } => Ok(result.Data),
        // VIOLATION: Interpreting and replacing error messages
        { Errors: var errors } when errors.Any(e => e.Contains("Invalid page")) => 
            BadRequest(new { errors = new[] { "Invalid page number or page size" } }),
        // VIOLATION: Creating custom error messages
        { Errors: var errors } when errors.Any(e => e.Contains("not found")) =>
            NotFound(new { errors = new[] { "Resource not found" } }),
        { Errors: var errors } => BadRequest(new { errors })
    };
}
```

### ✅ CORRECT - Pure Pass-Through

```csharp
[HttpGet]
public async Task<IActionResult> GetWorkoutTemplates([FromQuery] int page = 1, ...)
{
    var result = await _service.SearchAsync(page, ...);
    
    // Single exit point - no business logic, just pass through
    return result switch
    {
        { IsSuccess: true } => Ok(result.Data),
        { Errors: var errors } => BadRequest(new { errors })
    };
}
```

## HTTP Status Code Mapping

Controllers can map ServiceErrorCodes to HTTP status codes, but MUST pass through original error messages.

### ✅ ACCEPTABLE - Status Code Mapping

```csharp
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(new { errors = result.Errors }),
    { PrimaryErrorCode: ServiceErrorCode.Conflict } => Conflict(new { errors = result.Errors }),
    { PrimaryErrorCode: ServiceErrorCode.Unauthorized } => Unauthorized(new { errors = result.Errors }),
    { PrimaryErrorCode: ServiceErrorCode.Forbidden } => Forbid(),
    { Errors: var errors } => BadRequest(new { errors })
};
```

## Common Controller Patterns

### GET by ID

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(string id) =>
    await _service.GetByIdAsync(EquipmentId.ParseOrEmpty(id)) switch
    {
        { IsSuccess: true, Data: var data } => Ok(data),
        { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(new { errors = result.Errors }),
        { Errors: var errors } => BadRequest(new { errors })
    };
```

### GET All/List

```csharp
[HttpGet]
public async Task<IActionResult> GetAll() =>
    await _service.GetAllActiveAsync() switch
    {
        { IsSuccess: true, Data: var data } => Ok(data),
        { Errors: var errors } => BadRequest(new { errors })
    };
```

### POST Create

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateEquipmentDto dto) =>
    await _service.CreateAsync(dto.ToCommand()) switch
    {
        { IsSuccess: true, Data: var data } => CreatedAtAction(
            nameof(GetById), 
            new { id = data.Id }, 
            data),
        { PrimaryErrorCode: ServiceErrorCode.Conflict } => Conflict(new { errors = result.Errors }),
        { Errors: var errors } => BadRequest(new { errors })
    };
```

### PUT Update

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> Update(string id, [FromBody] UpdateEquipmentDto dto) =>
    await _service.UpdateAsync(EquipmentId.ParseOrEmpty(id), dto.ToCommand()) switch
    {
        { IsSuccess: true, Data: var data } => Ok(data),
        { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(new { errors = result.Errors }),
        { PrimaryErrorCode: ServiceErrorCode.Conflict } => Conflict(new { errors = result.Errors }),
        { Errors: var errors } => BadRequest(new { errors })
    };
```

### DELETE

```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(string id) =>
    await _service.DeleteAsync(EquipmentId.ParseOrEmpty(id)) switch
    {
        { IsSuccess: true } => NoContent(),
        { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(new { errors = result.Errors }),
        { Errors: var errors } => BadRequest(new { errors })
    };
```

## ID Parsing Pattern

Always use `ParseOrEmpty` for ID parameters:

```csharp
// ❌ BAD - Validation in controller
[HttpGet("{id}")]
public async Task<IActionResult> GetById(string id)
{
    if (!EquipmentId.TryParse(id, out var equipmentId))
        return BadRequest(new { errors = new[] { "Invalid ID format" } });
    
    // ...
}

// ✅ GOOD - Use ParseOrEmpty
[HttpGet("{id}")]
public async Task<IActionResult> GetById(string id) =>
    await _service.GetByIdAsync(EquipmentId.ParseOrEmpty(id)) switch
    {
        // Service handles invalid ID (Empty) validation
        { IsSuccess: true, Data: var data } => Ok(data),
        { Errors: var errors } => BadRequest(new { errors })
    };
```

## Authorization Attributes

```csharp
[HttpPost]
[Authorize(Policy = ClaimAuthorizations.PTTier)]  // Admin operations
public async Task<IActionResult> Create([FromBody] CreateEquipmentDto dto) =>
    await _service.CreateAsync(dto.ToCommand()) switch
    {
        { IsSuccess: true, Data: var data } => CreatedAtAction(
            nameof(GetById), 
            new { id = data.Id }, 
            data),
        { Errors: var errors } => BadRequest(new { errors })
    };

[HttpGet]
[Authorize(Policy = ClaimAuthorizations.FreeTier)]  // Client read operations
public async Task<IActionResult> GetAll() =>
    await _service.GetAllActiveAsync() switch
    {
        { IsSuccess: true, Data: var data } => Ok(data),
        { Errors: var errors } => BadRequest(new { errors })
    };
```

## Why This Pattern Is Critical

1. **Clear Responsibility Boundaries**: Service layer owns ALL business logic
2. **Testability**: Controllers have no logic to test
3. **Debugging**: Clear source of truth for error messages
4. **Maintenance**: Business logic changes happen only in services
5. **Consistency**: Uniform error handling across all endpoints

## Reference Table Controller Pattern

### Key Principle: Controllers are DUMB

Controllers in our architecture have ONE job: **Parse input and delegate to services**. They do NOT:
- Validate business rules
- Check if IDs are empty
- Decide what constitutes an invalid format
- Make any business decisions

### Correct Implementation
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetBodyPartById(string id)
{
    _logger.LogInformation("Getting body part with ID: {Id}", id);
    
    var result = await _bodyPartService.GetByIdAsync(BodyPartId.ParseOrEmpty(id));
    
    return result switch
    {
        { IsSuccess: true } => Ok(result.Data),
        { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(),
        { Errors: var errors } => BadRequest(new { errors })
    };
}
```

### What This Does:
1. **Parse the ID** - Use `ParseOrEmpty` to get a valid ID object (even if empty)
2. **Call the service** - Pass the parsed ID without any validation
3. **Map the response**:
   - Success → 200 OK
   - Error contains "not found" → 404 Not Found
   - Any other error → 400 Bad Request

### Common Reference Table Mistakes

❌ **DON'T check if ID is empty in controller**
```csharp
// WRONG
if (movementPatternId.IsEmpty)
{
    return NotFound("MovementPattern not found");
}
```

❌ **DON'T validate format in controller**
```csharp
// WRONG
if (!id.StartsWith("movementpattern-"))
{
    return BadRequest("Invalid format");
}
```

### The Service's Responsibility

The SERVICE decides:
- What constitutes a valid ID
- Whether an empty ID should return "not found" or "invalid format"
- What error messages to return
- All business logic and validation

## Key Principles

> "Controllers are dumb pipes. They know nothing about business logic, they just map HTTP to services and back."

> "The controller is just a translator between HTTP and your service. It doesn't think, it just translates."

## Common Violations to Avoid

- ❌ Validating request data beyond model binding
- ❌ Checking if entities exist before calling service
- ❌ Formatting or transforming service error messages
- ❌ Making multiple service calls to compose a response
- ❌ Adding business logic based on user claims
- ❌ Caching responses in the controller
- ❌ Logging business events (services handle this)

## Testing Controllers

Since controllers are pure pass-through, testing focuses on:
- HTTP status code mapping
- Route configuration
- Authorization attributes
- Model binding

No business logic testing should exist at the controller level.

## Related Documentation

- `/memory-bank/API-CODE_QUALITY_STANDARDS.md` - Main quality standards
- `/memory-bank/CodeQualityGuidelines/ServiceResultPattern.md` - ServiceResult handling
- `/memory-bank/CodeQualityGuidelines/SpecializedIdTypes.md` - ID parsing patterns