# Controller Pattern for Reference Tables

## Key Principle: Controllers are DUMB

Controllers in our architecture have ONE job: **Parse input and delegate to services**. They do NOT:
- Validate business rules
- Check if IDs are empty
- Decide what constitutes an invalid format
- Make any business decisions

## The Pattern

### Correct Implementation (from BodyPartsController)
```csharp
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

## Why This Pattern?

1. **Separation of Concerns**: Controllers handle HTTP, Services handle business logic
2. **Consistency**: All controllers behave the same way
3. **Testability**: Easy to test controllers without complex business logic
4. **Flexibility**: Services can change validation rules without touching controllers

## Common Mistakes to Avoid

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

❌ **DON'T have complex error matching**
```csharp
// WRONG - Too specific
{ Errors: var errors } when errors.Any(e => e.Contains("ID cannot be empty")) && movementPatternId.IsEmpty => NotFound("MovementPattern not found"),
{ Errors: var errors } when errors.Any(e => e.Contains("Invalid") || e.Contains("format")) => BadRequest(new { errors }),
```

## The Service's Responsibility

The SERVICE decides:
- What constitutes a valid ID
- Whether an empty ID should return "not found" or "invalid format"
- What error messages to return
- All business logic and validation

## Implementation Checklist

When implementing a controller GetById method:
- [ ] Parse the ID using `ParseOrEmpty`
- [ ] Call the service method
- [ ] Use simple pattern matching for response
- [ ] No business logic in the controller
- [ ] No ID validation in the controller

## Remember

> "The controller is just a translator between HTTP and your service. It doesn't think, it just translates."