# Cache Invalidation Strategy for Reference Tables

## Overview

This document outlines the cache invalidation strategy for reference table data when POST, PUT, and DELETE operations are implemented in the future.

## Current Implementation

The caching infrastructure is currently implemented for read operations only:
- GET all items
- GET by ID
- GET by value

Cache invalidation methods are already in place in the `ReferenceTablesBaseController`:
- `InvalidateTableCacheAsync()` - Removes all cache entries for a specific table

## Invalidation Strategy for Future CRUD Operations

### 1. POST (Create) Operations

When a new item is added to a reference table:
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateDto dto)
{
    // ... create logic ...
    
    // Invalidate cache after successful creation
    await InvalidateTableCacheAsync();
    
    return CreatedAtAction(...);
}
```

**Rationale**: Invalidate all cached data for the table to ensure the "GetAll" cache is refreshed with the new item.

### 2. PUT (Update) Operations

When an existing item is updated:
```csharp
[HttpPut("{id}")]
public async Task<IActionResult> Update(string id, [FromBody] UpdateDto dto)
{
    // ... update logic ...
    
    // Invalidate cache after successful update
    await InvalidateTableCacheAsync();
    
    return Ok(...);
}
```

**Rationale**: Clear all cache entries since the update might affect:
- The specific item's cache (GetById)
- The item's value-based cache (GetByValue)
- The collection cache (GetAll)

### 3. DELETE Operations

When an item is deleted or soft-deleted:
```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(string id)
{
    // ... delete/soft-delete logic ...
    
    // Invalidate cache after successful deletion
    await InvalidateTableCacheAsync();
    
    return NoContent();
}
```

**Rationale**: Remove all cached data to ensure deleted items don't appear in cached results.

## Implementation Guidelines

### 1. Consistency First
- Always invalidate cache AFTER the database operation succeeds
- Use transactions where appropriate to ensure atomicity
- If database operation fails, do NOT invalidate cache

### 2. Error Handling
```csharp
try
{
    // Perform database operation
    await _repository.SaveAsync();
    
    // Only invalidate if successful
    await InvalidateTableCacheAsync();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to update {TableName}", GetTableName());
    // Cache remains intact on failure
    throw;
}
```

### 3. Bulk Operations
For bulk operations, invalidate cache once after all operations complete:
```csharp
[HttpPost("bulk")]
public async Task<IActionResult> BulkCreate([FromBody] List<CreateDto> items)
{
    // ... bulk create logic ...
    
    // Single cache invalidation after all items are created
    await InvalidateTableCacheAsync();
    
    return Ok(...);
}
```

### 4. Special Considerations

#### Static vs Dynamic Tables
- **Static Tables** (24-hour cache): Less frequent updates expected
- **Dynamic Tables** (1-hour cache): More frequent updates, shorter cache duration minimizes stale data

#### Related Table Updates
If updating a table affects related tables (e.g., through foreign keys), consider:
```csharp
// Update primary table
await InvalidateTableCacheAsync();

// Also invalidate related table caches if needed
await _relatedController.InvalidateTableCacheAsync();
```

## Future Enhancements

### 1. Granular Cache Invalidation
Instead of clearing all cache entries for a table, implement targeted invalidation:
- Clear only specific item caches (by ID and value)
- Keep collection cache if appropriate

### 2. Cache Warming
After invalidation, optionally pre-populate cache:
```csharp
await InvalidateTableCacheAsync();
await GetAllWithCacheAsync(...); // Warm the cache
```

### 3. Event-Driven Invalidation
Implement domain events for cache invalidation across services:
```csharp
public class ReferenceDataUpdatedEvent
{
    public string TableName { get; set; }
    public string ItemId { get; set; }
    public CrudOperation Operation { get; set; }
}
```

## Testing Considerations

When implementing CRUD operations, ensure tests verify:
1. Cache is properly invalidated after successful operations
2. Cache remains intact after failed operations
3. Subsequent GET requests return updated data
4. Performance impact of cache invalidation is acceptable