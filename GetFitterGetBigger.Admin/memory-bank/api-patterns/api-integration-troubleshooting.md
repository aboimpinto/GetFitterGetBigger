# API Integration Troubleshooting Guide

## Overview
This document captures common API integration issues and their solutions, serving as a reference for developers working with the GetFitterGetBigger API.

## Common Issues and Solutions

### 1. Cache Key Collisions Between Services

**Problem**: When multiple services use similar cache key patterns, data from one service may overwrite or be retrieved by another service, leading to type mismatches and runtime errors.

**Example**: Equipment and MuscleGroup services both using simple cache keys like `"getall"` or `"getactive"`.

**Solution**:
- Use service-specific prefixes in all cache keys
- Include the service name or entity type in the key pattern
- Example patterns:
  ```csharp
  // Good
  private const string CACHE_KEY_ALL = "equipment-all";
  private const string CACHE_KEY_ACTIVE = "equipment-active";
  
  // Bad
  private const string CACHE_KEY_ALL = "all";
  private const string CACHE_KEY_ACTIVE = "active";
  ```

**Prevention**:
- Establish a project-wide cache key naming convention
- Consider using a cache key builder utility that enforces prefixes
- Document cache keys used by each service

### 2. API Endpoint URL Format Issues

**Problem**: Incorrectly formatted API endpoint URLs can lead to 404 errors or failed requests.

**Common Mistakes**:
1. Missing leading slash in endpoint paths
2. Double slashes in URL construction
3. Incorrect base URL configuration

**Example**:
```csharp
// Bad - Missing leading slash
private const string ENDPOINT = "api/musclegroups";

// Good - Includes leading slash
private const string ENDPOINT = "/api/musclegroups";
```

**Solution**:
- Always include leading slashes in endpoint constants
- Use URL builder utilities when constructing complex URLs
- Validate URL format in service constructors

**Best Practices**:
```csharp
public class MuscleGroupService : IMuscleGroupService
{
    private readonly string _apiBaseUrl;
    private const string ENDPOINT = "/api/musclegroups";
    
    public MuscleGroupService(IConfiguration configuration)
    {
        _apiBaseUrl = configuration["ApiSettings:BaseUrl"] 
            ?? throw new InvalidOperationException("API Base URL not configured");
        
        // Ensure base URL doesn't end with slash to avoid double slashes
        _apiBaseUrl = _apiBaseUrl.TrimEnd('/');
    }
    
    private string GetUrl(string path = "")
    {
        return $"{_apiBaseUrl}{ENDPOINT}{path}";
    }
}
```

### 3. ID Format Handling

**Problem**: The API uses prefixed string IDs (e.g., `"equipment-123"`) which can cause issues if treated as GUIDs.

**Solution**:
- Always use `string` type for ID properties
- Never attempt to parse IDs as GUIDs
- See [ID Format Pattern](./id-format.md) for detailed guidelines

### 4. Error Response Handling

**Problem**: API error responses may have different formats depending on the error type.

**Solution**:
- Implement robust error parsing that handles multiple formats
- Always check response status before deserializing
- Log full error responses for debugging

**Example**:
```csharp
if (!response.IsSuccessStatusCode)
{
    var errorContent = await response.Content.ReadAsStringAsync();
    _logger.LogError("API request failed: {StatusCode} - {Content}", 
        response.StatusCode, errorContent);
    
    // Handle different error formats
    throw new ApiException($"Request failed with status {response.StatusCode}");
}
```

## Testing Recommendations

### 1. Cache Testing
- Write tests that verify cache isolation between services
- Test cache invalidation scenarios
- Verify cache key uniqueness

### 2. API Integration Testing
- Use test fixtures that validate URL construction
- Test with various API response scenarios
- Mock HTTP clients for unit tests

### 3. Error Scenario Testing
- Test 404, 500, and timeout scenarios
- Verify error logging and user feedback
- Test retry logic where applicable

## Debugging Tips

1. **Enable Detailed Logging**:
   - Log full request URLs
   - Log request and response bodies
   - Log cache hits and misses

2. **Use Development Tools**:
   - Browser developer tools for network inspection
   - API testing tools (Postman, Swagger UI)
   - Application Insights or similar for production debugging

3. **Common Checkpoints**:
   - Verify base URL configuration
   - Check authentication headers
   - Validate request format matches API expectations
   - Ensure proper content-type headers

## Related Documentation
- [ID Format Pattern](./id-format.md)
- [System Patterns](../systemPatterns.md)
- [Testing Guidelines](../TestingGuidelines.md)