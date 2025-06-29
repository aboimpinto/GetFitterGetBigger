# Feature: Fix CacheService Nullable Type Test

## Feature ID: FEAT-008
## Created: 2025-01-29
## Status: SUBMITTED
## Target PI: PI-2025-Q1

## Description

This feature addresses a single skipped test in the CacheServiceTests suite. The test `GetOrCreateAsync_WhenFactoryReturnsNull_DoesNotCache` is currently skipped because the GetOrCreateAsync method doesn't support nullable return types, causing compiler warning CS8634.

## Business Value

- **Code Coverage**: Enables complete test coverage for the CacheService
- **Edge Case Handling**: Ensures proper behavior when cache factories return null
- **Code Quality**: Removes compiler warnings and technical debt

## User Stories

- As a developer, I want to test all edge cases of the cache service so that I can ensure robust caching behavior
- As a maintainer, I want all tests to pass without warnings so that the codebase remains clean

## Current Problem

### Affected Test:
- File: `CacheServiceTests.cs`
- Method: `GetOrCreateAsync_WhenFactoryReturnsNull_DoesNotCache`
- Line: 163

### Root Cause:
The GetOrCreateAsync method signature doesn't support nullable return types, which prevents testing the scenario where the factory function returns null.

## Solution Analysis

### Option 1: Modify GetOrCreateAsync to Support Nullable Types
**Approach**: Update the method signature to accept nullable return types

**Pros**:
- ✅ Enables testing of null scenarios
- ✅ More flexible API
- ✅ Handles edge cases gracefully

**Cons**:
- ❌ Breaking change to existing API
- ❌ May require updates to all callers
- ❌ Increases API complexity

### Option 2: Create Separate Nullable Method
**Approach**: Add GetOrCreateNullableAsync alongside existing method

**Pros**:
- ✅ No breaking changes
- ✅ Explicit nullable support
- ✅ Clear API intent

**Cons**:
- ❌ API duplication
- ❌ Maintenance of two similar methods
- ❌ Potential confusion for consumers

### Option 3: Use Wrapper Type
**Approach**: Use Option<T> or similar wrapper for nullable values

**Pros**:
- ✅ Type-safe nullable handling
- ✅ No API changes needed
- ✅ Explicit null handling

**Cons**:
- ❌ Additional complexity
- ❌ Learning curve for team
- ❌ Performance overhead

### Option 4: Keep Test Skipped
**Approach**: Accept that null factory returns are not supported

**Pros**:
- ✅ No code changes
- ✅ Maintains current API

**Cons**:
- ❌ Incomplete test coverage
- ❌ Untested edge case
- ❌ Technical debt

## Recommended Solution: Option 2 - Create Separate Nullable Method

### Implementation Overview:

```csharp
public async Task<T?> GetOrCreateNullableAsync<T>(
    string key, 
    Func<Task<T?>> factory,
    TimeSpan? expiration = null) where T : class
{
    var cached = await GetAsync<T>(key);
    if (cached != null)
        return cached;
    
    var value = await factory();
    if (value != null)
    {
        await SetAsync(key, value, expiration);
    }
    
    return value;
}
```

## Acceptance Criteria

- [ ] New GetOrCreateNullableAsync method is implemented
- [ ] Original GetOrCreateAsync remains unchanged
- [ ] Skipped test is enabled and passes
- [ ] Documentation is updated
- [ ] No regression in existing functionality

## Dependencies

None - this is a self-contained change

## Notes

This is a minor enhancement that improves test coverage and API completeness. The separate method approach maintains backward compatibility while addressing the testing need.