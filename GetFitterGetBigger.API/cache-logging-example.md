# Equipment Cache Logging Example

## Overview
The Equipment service now includes comprehensive cache logging that tracks all cache operations including:
- Cache hits and misses
- Cache sets (when data is stored)
- Cache invalidation (when data is removed)
- Cache eviction (when data expires)

## Log Levels
- **Debug**: Detailed cache operations (attempting to retrieve, cache key details)
- **Information**: Important cache events (hits, misses, sets, invalidation)
- **Warning**: Issues that don't prevent operation
- **Error**: Cache operation failures

## Example Log Output

### 1. First GET Request (Cache Miss)
```
[Cache] Attempting to retrieve all Equipment entities with key: Equipment:all
[Cache MISS] No cached data found for key: Equipment:all. Fetching from database...
[Cache] Fetched 10 Equipment entities from database
[Cache SET] Successfully stored value for key: Equipment:all with sliding expiration: 01:00:00
[Cache SET] Cached 10 Equipment entities with key: Equipment:all for duration: 01:00:00
```

### 2. Second GET Request (Cache Hit)
```
[Cache] Attempting to retrieve all Equipment entities with key: Equipment:all
[Cache HIT] Successfully retrieved 10 Equipment entities from cache with key: Equipment:all
```

### 3. GET by ID (Cache Miss then Hit)
```
# First request:
[Cache] Attempting to retrieve Equipment by ID with key: Equipment:id:equipment-33445566-7788-99aa-bbcc-ddeeff001122
[Cache MISS] No cached data found for key: Equipment:id:equipment-33445566-7788-99aa-bbcc-ddeeff001122. Fetching from database...
[Cache SET] Cached Equipment with ID: equipment-33445566-7788-99aa-bbcc-ddeeff001122 using key: Equipment:id:equipment-33445566-7788-99aa-bbcc-ddeeff001122 for duration: 01:00:00

# Second request:
[Cache] Attempting to retrieve Equipment by ID with key: Equipment:id:equipment-33445566-7788-99aa-bbcc-ddeeff001122
[Cache HIT] Successfully retrieved Equipment from cache with key: Equipment:id:equipment-33445566-7788-99aa-bbcc-ddeeff001122
```

### 4. Creating Equipment (Cache Invalidation)
```
[Cache] Creating new Equipment. Cache invalidation will follow after successful creation.
[Cache] Equipment created successfully. Proceeding with cache invalidation...
[Cache INVALIDATION] Starting cache invalidation for pattern: Equipment:*
[Cache PATTERN REMOVE] Removing 5 keys matching pattern: Equipment:*
[Cache REMOVE] Successfully removed key: Equipment:all
[Cache REMOVE] Successfully removed key: Equipment:id:equipment-33445566-7788-99aa-bbcc-ddeeff001122
[Cache PATTERN REMOVE] Completed removal of all keys matching pattern: Equipment:*
[Cache INVALIDATION] Successfully invalidated all cache entries for Equipment with pattern: Equipment:*
```

### 5. Updating Equipment (Cache Invalidation)
```
[Equipment Update] Starting update for ID: equipment-123
[Equipment Update] Incoming JSON: {"name":"Updated Name"}
[Cache] Updating Equipment with ID: equipment-123. Cache invalidation will follow after successful update.
[Cache] Equipment with ID: equipment-123 updated successfully. Proceeding with cache invalidation...
[Cache INVALIDATION] Starting cache invalidation for pattern: Equipment:*
[Cache INVALIDATION] Successfully invalidated all cache entries for Equipment with pattern: Equipment:*
```

### 6. Deactivating Equipment (Cache Invalidation)
```
[Cache] Deactivating equipment with ID: equipment-123. Cache invalidation will follow after successful deactivation.
[Cache] Equipment 'Barbell' (ID: equipment-123) found and is active. Checking if in use...
[Cache] Equipment 'Barbell' (ID: equipment-123) deactivated successfully. Proceeding with cache invalidation...
[Cache INVALIDATION] Starting cache invalidation for pattern: Equipment:*
[Cache INVALIDATION] Successfully invalidated all cache entries for Equipment with pattern: Equipment:*
```

### 7. Cache Eviction (After Expiration)
```
[Cache EVICTED] Key: Equipment:all was evicted. Reason: Expired
```

## Configuration

### Enable Debug Logging
To see all cache operations, ensure your `appsettings.Development.json` includes:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "GetFitterGetBigger.API.Services": "Debug",
      "GetFitterGetBigger.API.Controllers": "Debug"
    }
  }
}
```

### Production Configuration
For production, you may want to reduce logging verbosity:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "GetFitterGetBigger.API.Services": "Information",
      "GetFitterGetBigger.API.Controllers": "Warning"
    }
  }
}
```

## Benefits
1. **Debugging**: Easily identify cache-related issues
2. **Performance Monitoring**: Track cache hit/miss ratios
3. **Troubleshooting**: Understand why data might be stale or missing
4. **Optimization**: Identify opportunities to improve cache usage

## Cache Key Patterns
- Get all: `Equipment:all`
- Get by ID: `Equipment:id:{id}`
- Get by name: `Equipment:name:{name}`
- Get by value: `Equipment:value:{value}`

## Cache Duration
- Equipment uses 1-hour sliding expiration (dynamic table)
- Static tables use 24-hour expiration