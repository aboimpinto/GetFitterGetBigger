# Cache Configuration Documentation

## Overview

The GetFitterGetBigger API implements server-side caching for reference table data to improve performance and reduce database load. This document describes the cache configuration and usage.

## Configuration

### appsettings.json

The cache configuration is defined in `appsettings.json`:

```json
"CacheConfiguration": {
  "StaticTables": {
    "DurationInHours": 24,
    "Tables": ["DifficultyLevels", "KineticChainTypes", "BodyParts", "MuscleRoles"]
  },
  "DynamicTables": {
    "DurationInHours": 1,
    "Tables": ["Equipment", "MetricTypes", "MovementPatterns", "MuscleGroups"]
  }
}
```

### Table Categories

#### Static Tables (24-hour cache)
These tables contain data that rarely changes:
- **DifficultyLevels**: Exercise difficulty classifications
- **KineticChainTypes**: Open/Closed chain categorization
- **BodyParts**: Anatomical body part references
- **MuscleRoles**: Primary/Secondary muscle role classifications

#### Dynamic Tables (1-hour cache)
These tables may be updated more frequently:
- **Equipment**: Exercise equipment that gyms may add/remove
- **MetricTypes**: Measurement types that may be expanded
- **MovementPatterns**: Movement classifications that may evolve
- **MuscleGroups**: Muscle group definitions that may be refined

## Implementation Details

### Service Registration

The caching infrastructure is registered in `Program.cs`:
```csharp
// Add Memory Cache
builder.Services.AddMemoryCache();

// Configure cache settings
builder.Services.Configure<CacheConfiguration>(
    builder.Configuration.GetSection("CacheConfiguration"));

// Register cache service
builder.Services.AddSingleton<ICacheService, CacheService>();
```

### Cache Keys

Cache keys follow a consistent pattern:
- **Pattern**: `ReferenceTable:{TableName}:{Operation}:{Parameters}`
- **Examples**:
  - `ReferenceTable:DifficultyLevels:GetAll`
  - `ReferenceTable:Equipment:GetById:equipment-12345`
  - `ReferenceTable:MuscleGroups:GetByValue:biceps`

### Cache Operations

The `ReferenceTablesBaseController` provides three main caching methods:

1. **GetAllWithCacheAsync**: Caches entire table results
2. **GetByIdWithCacheAsync**: Caches individual items by ID
3. **GetByValueWithCacheAsync**: Caches items retrieved by value (case-insensitive)

### Cache Service Features

- **Sliding Expiration**: Cache entries use sliding expiration based on table type
- **Key Tracking**: Maintains internal tracking of cache keys for pattern-based removal
- **Error Resilience**: Falls back to database queries if caching fails
- **Logging**: Comprehensive logging for cache hits, misses, and errors

## Usage Examples

### Controller Implementation
```csharp
public async Task<IActionResult> GetDifficultyLevels()
{
    var difficultyLevels = await GetAllWithCacheAsync(async () =>
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        return await repository.GetAllActiveAsync();
    });
    
    return Ok(difficultyLevels.Select(MapToDto));
}
```

### Manual Cache Invalidation
```csharp
// Invalidate all cache entries for a table
await InvalidateTableCacheAsync();
```

## Performance Considerations

### Benefits
- Reduces database queries by up to 95% for reference data
- Improves API response times for reference table endpoints
- Reduces database connection pool usage
- Scales better under high load

### Trade-offs
- Slight memory overhead (minimal for reference data)
- Potential for stale data within cache duration window
- Additional complexity in data modification operations

## Monitoring

### Logging
The cache service logs:
- Cache hits/misses with Debug level
- Errors with Error level
- Cache invalidation events with Information level

### Metrics to Track
- Cache hit ratio
- Memory usage by cache
- Cache eviction frequency
- Response time improvements

## Future Enhancements

1. **Distributed Caching**: Implement Redis for multi-instance deployments
2. **Cache Warming**: Pre-populate cache on application startup
3. **Granular Invalidation**: Invalidate specific items rather than entire tables
4. **Cache Statistics Endpoint**: Expose cache metrics via admin API
5. **Configuration Hot Reload**: Update cache durations without restart