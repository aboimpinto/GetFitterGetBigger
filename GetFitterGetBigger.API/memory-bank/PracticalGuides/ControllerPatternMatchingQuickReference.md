# Controller Pattern Matching Quick Reference

**Purpose**: Quick guide to optimize pattern matching in controller methods by grouping cases by HTTP status code, not error code.

## 🚨 RED FLAGS - Watch for These

1. **Multiple cases returning same HTTP status**
   ```csharp
   // ❌ BAD - Three different cases all return BadRequest
   { PrimaryErrorCode: ServiceErrorCode.ValidationFailed } => BadRequest(...),
   { PrimaryErrorCode: ServiceErrorCode.InvalidFormat } => BadRequest(...),
   { PrimaryErrorCode: ServiceErrorCode.InvalidInput } => BadRequest(...),
   ```

2. **Listing specific error codes that all map to 400**
   ```csharp
   // ❌ BAD - Unnecessarily specific
   { PrimaryErrorCode: ServiceErrorCode.ValidationFailed, StructuredErrors: var errors } => BadRequest(new { errors }),
   { StructuredErrors: var errors } => BadRequest(new { errors })  // This catches ValidationFailed anyway!
   ```

3. **Not using default case for common responses**
   ```csharp
   // ❌ BAD - No catch-all for unexpected errors
   return result switch
   {
       { IsSuccess: true } => Ok(result.Data),
       { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound()
       // Missing default case!
   };
   ```

## ✅ PATTERN TEMPLATES

### Standard CRUD Operation (90% of cases)
```csharp
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })
};
```

### Create Operation with Conflict
```csharp
return result switch
{
    { IsSuccess: true } => CreatedAtAction(nameof(GetMethod), new { id = result.Data.Id }, result.Data),
    { PrimaryErrorCode: ServiceErrorCode.AlreadyExists, StructuredErrors: var errors } => Conflict(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })
};
```

### Delete Operation
```csharp
return result switch
{
    { IsSuccess: true } => NoContent(),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })
};
```

### Search/List Operation (Always returns 200)
```csharp
// Search operations ALWAYS return Ok for security reasons
return Ok(result.Data);  // Even if failed, Data contains empty collection
```

### Operations with Authorization
```csharp
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.Unauthorized } => Forbid(),
    { StructuredErrors: var errors } => BadRequest(new { errors })
};
```

## 🎯 Decision Guide

### When to Add a Specific Case

Ask: **"Does this error code require a DIFFERENT HTTP status than 400?"**

| Error Code | HTTP Status | Add Specific Case? |
|------------|-------------|-------------------|
| NotFound | 404 | ✅ YES |
| Unauthorized | 403 | ✅ YES |
| DependencyExists | 409 | ✅ YES |
| AlreadyExists | 409 | ✅ YES |
| ValidationFailed | 400 | ❌ NO (use default) |
| InvalidFormat | 400 | ❌ NO (use default) |
| InvalidInput | 400 | ❌ NO (use default) |
| InternalError | 400 | ❌ NO (use default) |

### Quick Consolidation Check

If you see this pattern:
```csharp
// Multiple cases → Same HTTP status
case A: return BadRequest(x);
case B: return BadRequest(y);
case C: return BadRequest(z);
```

Consolidate to:
```csharp
// Single default case
default: return BadRequest(result.StructuredErrors);
```

## 📝 Before/After Examples

### Example 1: State Change Operation
```csharp
// ❌ BEFORE - Redundant ValidationFailed case
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.ValidationFailed, StructuredErrors: var errors } => BadRequest(new { errors }),
    { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })
};

// ✅ AFTER - Consolidated
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })  // Handles ValidationFailed and all other 400s
};
```

### Example 2: Update Operation
```csharp
// ❌ BEFORE - Multiple validation error types
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.ValidationFailed } => BadRequest(result.Message),
    { PrimaryErrorCode: ServiceErrorCode.InvalidFormat } => BadRequest(result.Message),
    { PrimaryErrorCode: ServiceErrorCode.InvalidInput } => BadRequest(result.Message),
    _ => BadRequest("Unknown error")
};

// ✅ AFTER - Single default for all 400s
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    _ => BadRequest(result.Message)
};
```

## 🔑 Key Principle

> **Controllers care about HTTP status codes, not business error codes.**
> 
> Group by what matters at the HTTP layer!

## 🚀 Copy-Paste Ready

Most controller methods can use this template:
```csharp
var result = await _service.MethodAsync(command);

return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
    { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
    { StructuredErrors: var errors } => BadRequest(new { errors })
};
```

Adjust only if you have different HTTP status requirements!