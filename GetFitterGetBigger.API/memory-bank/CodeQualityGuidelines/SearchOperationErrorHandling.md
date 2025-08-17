# Search Operation Error Handling Pattern

## Overview

This document defines the error handling strategy for search/list operations in the GetFitterGetBigger API. This pattern prioritizes **security and user experience** over detailed error reporting.

## Core Principle

> **Search and list operations ALWAYS return HTTP 200 OK with data structure, even on internal failures.**

## Rationale

### 1. Security Through Obscurity
- **No Information Leakage**: External clients cannot determine if the system is experiencing failures
- **Attack Surface Reduction**: Prevents attackers from probing system health through API responses
- **Internal State Protection**: Database outages, service failures, or infrastructure issues remain hidden

### 2. User Experience
- **Graceful Degradation**: Users see an empty result rather than error messages
- **Consistent Behavior**: API always returns the expected data structure
- **No Client-Side Error Handling**: Simplifies client implementation

### 3. Operational Model
- **Server-Side Debugging**: All errors are logged on the server for investigation
- **Help Desk Workflow**: Users report issues → Support checks server logs → Engineers debug
- **Centralized Monitoring**: All failures tracked in server logs, not distributed across clients

## Implementation Pattern

### Controller Layer (Correct)
```csharp
[HttpGet]
public async Task<IActionResult> GetWorkoutTemplates(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? namePattern = null,
    // ... other parameters
    [FromQuery] string sortBy = "name",
    [FromQuery] string sortOrder = "asc")
{
    // Parse and validate inputs
    var parsedCategoryId = WorkoutCategoryId.ParseOrEmpty(categoryId);
    // ... other parsing

    // Call service
    var result = await _workoutTemplateService.SearchAsync(
        page, pageSize, namePattern ?? string.Empty,
        parsedCategoryId, /* ... other params ... */
        sortBy, sortOrder);

    // ALWAYS return OK with data (empty list on any failure)
    // This prevents information leakage about system state
    return Ok(result.Data);
}
```

### Service Layer Pattern
```csharp
public async Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> SearchAsync(...)
{
    // Validation using ServiceValidate
    return await ServiceValidate.Build<PagedResponse<WorkoutTemplateDto>>()
        .EnsureNumberBetween(page, 1, int.MaxValue, "Invalid page")
        .EnsureNumberBetween(pageSize, 1, 100, "Invalid page size")
        .WhenValidAsync(async () => await ExecuteSearchAsync(...))
        .OnFailure(() => PagedResponse<WorkoutTemplateDto>.Empty);
}
```

### Data Service Layer Pattern
```csharp
public async Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> SearchAsync(...)
{
    try
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        // Execute query
        var query = repository.GetWorkoutTemplatesQueryable()
            .FilterByNamePattern(namePattern)
            // ... other filters
            
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var dtos = items.Select(x => x.ToDto()).ToList();
        
        return ServiceResult<PagedResponse<WorkoutTemplateDto>>.Success(
            new PagedResponse<WorkoutTemplateDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            });
    }
    catch (Exception ex)
    {
        // Log the error internally (critical for debugging)
        _logger.LogError(ex, "Failed to search workout templates");
        
        // Return empty success result (hides failure from client)
        return ServiceResult<PagedResponse<WorkoutTemplateDto>>.Success(
            PagedResponse<WorkoutTemplateDto>.Empty);
    }
}
```

## What NOT to Do

### ❌ Never Expose Internal Errors in Search Operations
```csharp
// WRONG - Exposes system state
return result switch
{
    { IsSuccess: true } => Ok(result.Data),
    { StructuredErrors: var errors } => BadRequest(new { errors })  // This exposes failures!
};
```

### ❌ Never Return Different Status Codes for Search Failures
```csharp
// WRONG - Reveals system issues
catch (DbConnectionException ex)
{
    return StatusCode(503, "Database unavailable");  // Information leakage!
}
```

### ❌ Never Skip Logging
```csharp
// WRONG - No way to debug issues
catch (Exception ex)
{
    // Silently return empty - NO LOGGING!
    return ServiceResult<PagedResponse<T>>.Success(PagedResponse<T>.Empty);
}
```

## Debugging Workflow

### 1. User Experience
- User searches for workout templates
- Receives empty list (even if database is down)
- User reports: "I can't see any workout templates"

### 2. Support Process
- Help desk receives report
- Checks server logs for the time period
- Finds: `[ERROR] Failed to search workout templates: PostgreSQL connection timeout`
- Escalates to engineering with specific error details

### 3. Engineering Resolution
- Engineers have full error context from logs
- Can identify: connection issues, query timeouts, data problems
- Fix issue without ever exposing details to external users

## When This Pattern Applies

### ✅ Use for:
- **GET list/search endpoints**: `/api/exercises`, `/api/workout-templates`
- **Public endpoints**: No authentication required
- **Discovery operations**: Finding available options
- **Non-critical queries**: Where empty results are acceptable

### ❌ Don't Use for:
- **Mutations**: POST, PUT, DELETE should return appropriate errors
- **Authenticated operations**: Users performing actions need feedback
- **Single entity lookups**: GET by ID should return 404 if not found
- **Admin endpoints**: Internal tools need full error visibility

## Benefits

1. **Security**: No information about system internals leaked
2. **Simplicity**: Clients don't need complex error handling
3. **Reliability**: Consistent API behavior regardless of internal state
4. **Debugging**: Centralized logging provides full visibility for engineers

## Example Log Entry

```log
2024-01-15 10:23:45 [ERROR] WorkoutTemplateQueryDataService - Failed to search workout templates
User: anonymous
Parameters: page=1, pageSize=20, namePattern="strength"
Exception: Npgsql.NpgsqlException: Connection timeout expired
Stack Trace: [full stack trace for debugging]
```

## Summary

This pattern ensures that:
- **Clients** receive consistent, simple responses
- **Attackers** cannot probe system health
- **Engineers** have full debugging information in logs
- **Support** can effectively triage issues

The key insight: **Debugging happens on the server with logs, never on the client with error messages.**