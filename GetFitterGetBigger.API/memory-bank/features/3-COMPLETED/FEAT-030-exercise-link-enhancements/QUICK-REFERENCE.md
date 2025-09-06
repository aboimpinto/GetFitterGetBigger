# FEAT-030: Exercise Link Enhancements - Quick Reference

## Key Constants/Enums

### ExerciseLinkType Enum
```csharp
public enum ExerciseLinkType
{
    WARMUP = 0,      // Warmup exercise for a workout
    COOLDOWN = 1,    // Cooldown exercise for a workout  
    WORKOUT = 2,     // Main workout exercise
    ALTERNATIVE = 3  // Alternative exercise option
}
```

### Error Messages Constants
```csharp
public static class ExerciseLinkErrorMessages
{
    // String-based (backward compatibility)
    public const string InvalidLinkType = "Link type must be either 'Warmup' or 'Cooldown'";
    
    // Enum-based (new functionality)
    public const string InvalidLinkTypeEnum = "Link type must be WARMUP, COOLDOWN, WORKOUT, or ALTERNATIVE";
    public const string WorkoutLinksAutoCreated = "WORKOUT links are automatically created as reverse links";
    public const string InvalidLinkTypeForRestExercise = "REST exercises cannot have any link types";
    public const string BidirectionalLinkExists = "A bidirectional link of this type already exists";
    public const string WarmupMustLinkToWorkout = "WARMUP links can only be created to WORKOUT exercises";
    public const string CooldownMustLinkToWorkout = "COOLDOWN links can only be created to WORKOUT exercises";
    public const string AlternativeCannotLinkToRest = "ALTERNATIVE links cannot be created to REST exercises";
}
```

## Business Rules

### Link Type Rules
- ✅ **WARMUP** links can only be created to WORKOUT exercises
- ✅ **COOLDOWN** links can only be created to WORKOUT exercises  
- ✅ **ALTERNATIVE** links can be created to any non-REST exercise
- ❌ **WORKOUT** links cannot be created manually (only as automatic reverse links)
- ❌ **REST** exercises cannot have ANY link types

### Bidirectional Creation Rules
- ✅ **WARMUP → Target**: Auto-creates Target → Source as WORKOUT
- ✅ **COOLDOWN → Target**: Auto-creates Target → Source as WORKOUT
- ✅ **ALTERNATIVE → Target**: Auto-creates Target → Source as ALTERNATIVE
- ⚠️ **Self-linking is allowed** (exercise can link to itself for valid use cases)

### Display Order Rules
- ✅ **Server-side calculation**: Display order is automatically calculated (next available sequence)
- ✅ **Per exercise/link type**: Each exercise maintains independent display order per link type
- ✅ **Sequential ordering**: Orders are assigned as 1, 2, 3... based on existing links

## API Endpoints

### Create Exercise Link
```http
POST /api/exercises/{exerciseId}/links
Authorization: Bearer {token} (PT-Tier or Admin-Tier required)
Content-Type: application/json

{
  "targetExerciseId": "exercise-456",
  "linkType": "WARMUP"  // or "Warmup" for backward compatibility
}

Response: 201 Created
{
  "primaryLink": {
    "id": "link-123",
    "sourceExerciseId": "exercise-123",
    "targetExerciseId": "exercise-456",
    "linkType": "WARMUP",
    "displayOrder": 1,
    "isActive": true,
    "createdAt": "2025-01-04T10:00:00Z"
  },
  "reverseLink": {
    "id": "link-124", 
    "sourceExerciseId": "exercise-456",
    "targetExerciseId": "exercise-123",
    "linkType": "WORKOUT",
    "displayOrder": 2,
    "isActive": true,
    "createdAt": "2025-01-04T10:00:00Z"
  }
}
```

### Get Exercise Links
```http
GET /api/exercises/{exerciseId}/links?linkType=WARMUP&includeReverse=true
Authorization: Bearer {token} (PT-Tier or Admin-Tier required)

Response: 200 OK
[
  {
    "id": "link-123",
    "sourceExerciseId": "exercise-123", 
    "targetExerciseId": "exercise-456",
    "linkType": "WARMUP",
    "displayOrder": 1,
    "targetExercise": {
      "id": "exercise-456",
      "name": "Jumping Jacks",
      "exerciseType": "WARMUP"
    }
  }
]
```

### Update Exercise Link
```http
PUT /api/exercises/{exerciseId}/links/{linkId}
Authorization: Bearer {token} (PT-Tier or Admin-Tier required)
Content-Type: application/json

{
  "displayOrder": 2  // Only displayOrder can be updated
}

Response: 200 OK
{
  "id": "link-123",
  "sourceExerciseId": "exercise-123",
  "targetExerciseId": "exercise-456", 
  "linkType": "WARMUP",
  "displayOrder": 2,
  "isActive": true
}
```

### Delete Exercise Link
```http
DELETE /api/exercises/{exerciseId}/links/{linkId}?deleteReverse=true
Authorization: Bearer {token} (PT-Tier or Admin-Tier required)

Response: 204 No Content

Note: deleteReverse=true (default) deletes both forward and reverse links
      deleteReverse=false deletes only the specified link
```

### Query Parameters
- `linkType`: Filter by specific link type (WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE)
- `includeReverse`: Include reverse links in response (default: false)
- `deleteReverse`: Delete reverse link when deleting (default: true)
- `includeExerciseDetails`: Include full exercise information (default: false)

## Common Validation Errors

### Missing Required Field
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "TargetExerciseId": ["The TargetExerciseId field is required."]
  }
}
```

### Invalid Link Type
```json
{
  "type": "ValidationFailed",
  "title": "Validation failed",
  "status": 400, 
  "detail": "Link type must be WARMUP, COOLDOWN, WORKOUT, or ALTERNATIVE"
}
```

### REST Exercise Link Attempt
```json
{
  "type": "ValidationFailed",
  "title": "Validation failed",
  "status": 400,
  "detail": "REST exercises cannot have any link types"
}
```

### Incompatible Link Type
```json
{
  "type": "ValidationFailed", 
  "title": "Validation failed",
  "status": 400,
  "detail": "WARMUP links can only be created to WORKOUT exercises"
}
```

### Duplicate Link
```json
{
  "type": "ValidationFailed",
  "title": "Validation failed", 
  "status": 400,
  "detail": "A bidirectional link of this type already exists"
}
```

## C# Usage Examples

### Service Layer Usage
```csharp
// Inject service
private readonly IExerciseLinkService _exerciseLinkService;

// Create bidirectional link
var result = await _exerciseLinkService.CreateLinkAsync(
    sourceExerciseId: "exercise-123",
    targetExerciseId: "exercise-456", 
    linkType: ExerciseLinkType.WARMUP);

if (result.IsSuccess)
{
    var createdLink = result.Data;
    // Both forward and reverse links created automatically
}
```

### Repository Usage
```csharp
// Get bidirectional links
var warmupLinks = await _repository.GetBidirectionalLinksAsync(
    exerciseId, ExerciseLinkType.WARMUP);

// Check if link exists
var exists = await _repository.ExistsBidirectionalAsync(
    sourceId, targetId, ExerciseLinkType.ALTERNATIVE);
```

### Controller Integration
```csharp
[HttpPost("exercises/{exerciseId}/links")]
public async Task<IActionResult> CreateLink(
    string exerciseId,
    [FromBody] CreateExerciseLinkRequest request)
{
    // Convert string to enum (supports both formats)
    var linkType = Enum.TryParse<ExerciseLinkType>(request.LinkType, out var enumValue) 
        ? enumValue 
        : MapStringToEnum(request.LinkType);

    var result = await _exerciseLinkService.CreateLinkAsync(
        exerciseId, 
        request.TargetExerciseId, 
        linkType);

    return result.Match(
        onSuccess: data => Created($"/api/exercises/{exerciseId}/links/{data.Id}", data),
        onFailure: error => BadRequest(error)
    );
}
```

## Migration Support

### Backward Compatibility
The system supports both string and enum formats during migration:

```csharp
// These are equivalent:
linkType: "WARMUP"      // New enum format
linkType: "Warmup"      // Legacy string format

// These are equivalent: 
linkType: "COOLDOWN"    // New enum format
linkType: "Cooldown"    // Legacy string format
```

### Data Migration Status
- **Existing Links**: Automatically converted using ActualLinkType computed property
- **Database Schema**: Dual columns (LinkType string + LinkTypeEnum nullable)
- **API Compatibility**: Both formats accepted in requests
- **Response Format**: Returns enum string format ("WARMUP", "COOLDOWN", etc.)

## Performance Notes

### Database Optimizations
- **AsNoTracking()**: Applied to all read-only repository queries
- **Strategic Indexing**: Bidirectional query indexes on (TargetExerciseId, LinkTypeEnum)
- **Reduced Database Calls**: Dual-entity validation reduces calls by 67%

### Caching Considerations
- **Cache Keys**: Include both exerciseId and linkType for efficient filtering
- **Invalidation**: Both source and target exercise caches invalidated on link changes
- **TTL**: Standard 30-minute cache expiration

## Testing Quick Reference

### Unit Test Examples
```csharp
// Test bidirectional creation
[Fact]
public async Task CreateLinkAsync_WarmupLink_ShouldCreateBidirectionalLinks()
{
    // Arrange
    var sourceId = "exercise-123";
    var targetId = "exercise-456";
    
    // Act
    var result = await _service.CreateLinkAsync(sourceId, targetId, ExerciseLinkType.WARMUP);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    // Verify both forward (WARMUP) and reverse (WORKOUT) links created
}

// Test REST exercise validation
[Fact]
public async Task CreateLinkAsync_RestExercise_ShouldFail()
{
    // Arrange - source exercise is REST type
    
    // Act
    var result = await _service.CreateLinkAsync(restExerciseId, targetId, ExerciseLinkType.WARMUP);
    
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be(ServiceErrorCode.ValidationFailed);
}
```

### Integration Test Examples
```csharp
// Test API endpoint
[Fact]
public async Task POST_ExerciseLinks_ShouldCreateBidirectionalLinks()
{
    // Arrange
    var request = new { targetExerciseId = "exercise-456", linkType = "WARMUP" };
    
    // Act
    var response = await Client.PostAsJsonAsync("/api/exercises/exercise-123/links", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var result = await response.Content.ReadFromJsonAsync<BidirectionalLinkResponseDto>();
    result.PrimaryLink.LinkType.Should().Be("WARMUP");
    result.ReverseLink.LinkType.Should().Be("WORKOUT");
}
```

## Troubleshooting

### Common Issues
1. **"WORKOUT links auto-created"**: WORKOUT links are only created as reverse links - use WARMUP or COOLDOWN instead
2. **"REST exercise links blocked"**: REST exercises cannot have any links - this is by design
3. **"Display order conflicts"**: Display orders are server-calculated - don't specify in requests
4. **"Bidirectional link exists"**: Check for existing links before creating new ones

### Debug Steps
1. Check exercise types using GET /api/exercises/{id}
2. Verify authorization token has PT-Tier or Admin-Tier claims
3. Review validation error messages for specific business rule violations
4. Test with different link types to understand compatibility matrix

---

**Need Help?** Check the TECHNICAL-SUMMARY.md for detailed implementation details or LESSONS-LEARNED.md for common pitfalls and solutions.