# FEAT-030: Exercise Link Enhancements - Four-Way Linking System

## Overview

This feature enhances the existing ExerciseLink system to support a comprehensive four-way linking system with WARMUP, COOLDOWN, WORKOUT, and ALTERNATIVE link types. The system will support bidirectional relationships, where creating a link automatically creates the corresponding reverse link.

## Business Value

- **Flexibility**: Supports more complex exercise relationships beyond simple warmup/cooldown
- **Alternative Options**: Allows PTs to specify alternative exercises for clients who cannot perform certain movements
- **Smart Suggestions**: Bidirectional linking ensures exercises can be discovered from both directions
- **Improved Workflow**: PTs can build more comprehensive workout plans with proper exercise alternatives

## Technical Requirements

### Link Type Definitions

#### 1. WARMUP
- **Purpose**: Exercises that prepare the body for a specific workout exercise
- **Rule**: Can only be added to a WORKOUT exercise
- **Suggestion**: Will be suggested as warmup when the user chooses the exercise for a workout
- **Bidirectional**: When A is linked as WARMUP to B (workout), B is automatically linked as WORKOUT to A

#### 2. COOLDOWN
- **Purpose**: Exercises that help recovery after a specific workout exercise
- **Rule**: Can only be added to a WORKOUT exercise
- **Suggestion**: Will be suggested as cooldown when the user chooses the exercise for a workout
- **Bidirectional**: When A is linked as COOLDOWN to B (workout), B is automatically linked as WORKOUT to A

#### 3. WORKOUT
- **Purpose**: Main workout exercises that can have warmups and cooldowns
- **Rule**: Can be added to COOLDOWN and WARMUP exercises
- **Suggestion**: Will be suggested as the main workout when selecting warmup/cooldown exercises
- **Bidirectional**: Automatically created when WARMUP or COOLDOWN links are established

#### 4. ALTERNATIVE
- **Purpose**: Alternative exercises that can replace the original exercise
- **Rule**: Any exercise can have an alternative version (except REST)
- **Suggestion**: Will be offered as alternatives when the original exercise is selected
- **Bidirectional**: When A is linked as ALTERNATIVE to B, B is automatically linked as ALTERNATIVE to A

### Exercise Type Constraints

#### REST Exercise Type
- **Cannot have**: WARMUP, COOLDOWN, WORKOUT, or ALTERNATIVE links
- **Validation**: System must reject any attempt to create links for REST exercises

### Example Scenarios

#### Example 1: Standard Linking
**Given:**
- Workout Exercise: Burpee (ExerciseType: WORKOUT)
- Warmup Exercise: Jumping-Jacks (ExerciseType: WARMUP or WORKOUT)
- Cooldown Exercise: Pigeon-Pose (ExerciseType: COOLDOWN)

**When linking:**
1. Link Jumping-Jacks as WARMUP to Burpee
   - Creates: Jumping-Jacks → Burpee (WARMUP)
   - Auto-creates: Burpee → Jumping-Jacks (WORKOUT)

2. Link Pigeon-Pose as COOLDOWN to Burpee
   - Creates: Pigeon-Pose → Burpee (COOLDOWN)
   - Auto-creates: Burpee → Pigeon-Pose (WORKOUT)

**Result:**
- Listing warmups for Burpee returns: Jumping-Jacks
- Listing workouts for Jumping-Jacks returns: Burpee
- Zero, one, or multiple warmup/cooldown exercises can be linked to a workout

#### Example 2: Self-Linking
**Given:**
- Exercise: Burpees (can serve as both WORKOUT and WARMUP)

**When linking:**
- Link Burpees as WARMUP to Burpees (self-reference)
   - Creates: Burpees → Burpees (WARMUP)
   - Auto-creates: Burpees → Burpees (WORKOUT)

**Result:**
- Burpees can be its own warmup exercise
- This is valid as some exercises can serve multiple purposes

### Data Model Changes

#### Modified ExerciseLink Entity

```csharp
// New file: /Models/Enums/ExerciseLinkType.cs
public enum ExerciseLinkType
{
    WARMUP = 0,      // Warmup exercise for a workout
    COOLDOWN = 1,    // Cooldown exercise for a workout
    WORKOUT = 2,     // Main workout exercise
    ALTERNATIVE = 3  // Alternative exercise option
}

// Updated: /Models/Entities/ExerciseLink.cs
public record ExerciseLink : IEmptyEntity<ExerciseLink>
{
    public ExerciseLinkId Id { get; init; }
    public ExerciseId SourceExerciseId { get; init; }
    public ExerciseId TargetExerciseId { get; init; }
    public ExerciseLinkType LinkType { get; init; } // Changed from string to enum
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    
    // Navigation properties
    public Exercise? SourceExercise { get; init; }
    public Exercise? TargetExercise { get; init; }
    
    // Empty pattern implementation (unchanged)
    public static ExerciseLink Empty => new()
    {
        Id = ExerciseLinkId.Empty,
        SourceExerciseId = ExerciseId.Empty,
        TargetExerciseId = ExerciseId.Empty,
        LinkType = ExerciseLinkType.WARMUP, // Default enum value
        DisplayOrder = 0,
        IsActive = false,
        CreatedAt = DateTime.MinValue,
        UpdatedAt = DateTime.MinValue
    };
}
```

### API Endpoints (Enhanced)

#### 1. Create Exercise Link (Enhanced)
- **POST** `/api/exercises/{exerciseId}/links`
- **Authorization**: PT-Tier, Admin-Tier
- **Request Body**:
  ```json
  {
    "targetExerciseId": "exercise-{guid}",
    "linkType": "WARMUP|COOLDOWN|WORKOUT|ALTERNATIVE",
    "displayOrder": 1
  }
  ```
- **Response**: 201 Created with created link(s)
- **Behavior**: 
  - Creates the requested link
  - Automatically creates the reverse link based on type
  - Returns both created links in response

#### 2. Get Exercise Links (Enhanced)
- **GET** `/api/exercises/{exerciseId}/links`
- **Query Parameters**:
  - `linkType`: Filter by link type (optional)
  - `includeExerciseDetails`: Include full exercise data
  - `includeReverse`: Include reverse links (default: false)
- **Response**: Links including bidirectional relationships

#### 3. Delete Exercise Link (Enhanced)
- **DELETE** `/api/exercises/{exerciseId}/links/{linkId}`
- **Query Parameters**:
  - `deleteReverse`: Also delete the reverse link (default: true)
- **Behavior**: Deletes both directions of the link by default

### Service Layer Enhancements

#### IExerciseLinkService (Enhanced)
```csharp
public interface IExerciseLinkService
{
    // Enhanced to handle bidirectional creation
    Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(
        ExerciseId sourceId, 
        CreateExerciseLinkDto dto);
    
    
    // Enhanced validation
    Task<ServiceResult<bool>> ValidateLinkTypeCompatibilityAsync(
        ExerciseId sourceId, 
        ExerciseId targetId, 
        ExerciseLinkType linkType);
}
```

### Validation Rules

#### 1. Link Type Compatibility Matrix

| Source Exercise Type | Target Exercise Type | Allowed Link Types |
|---------------------|---------------------|-------------------|
| WORKOUT | WARMUP | Source→Target: none, Auto: Target→Source: WARMUP |
| WORKOUT | COOLDOWN | Source→Target: none, Auto: Target→Source: COOLDOWN |
| WORKOUT | WORKOUT | ALTERNATIVE |
| WARMUP | WORKOUT | WARMUP (creates reverse WORKOUT) |
| WARMUP | WARMUP | ALTERNATIVE |
| COOLDOWN | WORKOUT | COOLDOWN (creates reverse WORKOUT) |
| COOLDOWN | COOLDOWN | ALTERNATIVE |
| REST | ANY | none |
| ANY | REST | none |

#### 2. Bidirectional Link Rules

- **WARMUP → WORKOUT**: Auto-creates WORKOUT → WARMUP
- **COOLDOWN → WORKOUT**: Auto-creates WORKOUT → COOLDOWN
- **ALTERNATIVE → ANY**: Auto-creates ANY → ALTERNATIVE
- **Deletion**: Deleting a link deletes its reverse by default

#### 3. Business Constraints

- No duplicate links (same source, target, and type)
- Self-linking is allowed (exercise can link to itself for valid use cases)

### Database Changes

#### 1. Migration: UpdateExerciseLinksForFourWaySystem
**Code-First Migration Approach:**
- Change LinkType column from `nvarchar` (string) to `int` (enum)
- Data conversion during migration:
  - Existing "Warmup" → 0 (WARMUP)
  - Existing "Cooldown" → 1 (COOLDOWN)
- **NO reverse links created during migration** - these will be created via application logic
- Add necessary indexes for performance

#### 2. New Indexes
- `IX_ExerciseLinks_TargetExerciseId_LinkType` for reverse lookups
- `IX_ExerciseLinks_SourceTarget` composite for bidirectional queries

#### 3. Display Order Logic for Reverse Links
- When creating a reverse link, calculate DisplayOrder based on existing links
- Example: If exercise B already has 2 WORKOUT links, the new reverse link gets DisplayOrder = 3
- Each exercise maintains its own DisplayOrder sequence per link type

### Cache Strategy

- **Key Pattern**: `exercise-links:{exerciseId}:{linkType}:{direction}`
- **Invalidation**: 
  - Invalidate both source and target caches on any link operation
  - Invalidate related exercise caches
- **TTL**: 30 minutes (reduced due to bidirectional complexity)

### Error Handling

- `InvalidLinkTypeException`: When link type is not compatible with exercise types
- `RestExerciseLinkException`: When attempting to link REST exercises
- `ReverseLinkCreationException`: When reverse link creation fails

## Implementation Categories

### Category 1: Entity and Domain Model Updates
- Update ExerciseLink entity with new enum
- Create link type compatibility validator
- Update entity handlers for new rules

### Category 2: Repository Layer Updates
- Enhance repository with bidirectional queries
- Update existing queries for new link types

### Category 3: Service Layer Implementation
- Implement bidirectional link creation logic
- Add link type validation
- Enhance existing methods for new types

### Category 4: API Controller Updates
- Update DTOs for new link types
- Enhance endpoints with new parameters

### Category 5: Database Migration
- Create and run migration
- Migrate existing data
- Create reverse links for existing data

### Category 6: Testing
- Unit tests for all new logic
- Integration tests for bidirectional operations
- Test REST exercise constraints
- Test self-linking scenarios

## Success Criteria

1. All four link types (WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE) are functional
2. Bidirectional links are created/deleted automatically
3. REST exercises cannot have any links
4. Existing links are migrated successfully
5. All validation rules are enforced
6. Performance is not degraded (< 100ms for link operations)
7. 100% test coverage for new code

## Estimated Time

- Planning & Analysis: 1h
- Implementation: 6h
- Testing: 2h
- Documentation: 1h
- **Total**: 10h

## Dependencies

- Existing ExerciseLink implementation (FEAT-022)
- Exercise entity and service
- Current database schema

## Risks

1. **Data Migration**: Existing links need careful migration
2. **Performance**: Bidirectional operations double the database operations
3. **Complexity**: Four-way linking adds significant business logic complexity
4. **Cache Invalidation**: More complex due to bidirectional nature

## Future Enhancements

1. **Bulk Operations**: Add ability to create/delete multiple links at once
2. **Link Strength**: Add weight/strength to links based on usage
3. **Smart Suggestions**: AI-based link recommendations
4. **Link Templates**: Pre-defined link sets for common exercises
5. **Analytics**: Track most effective link combinations
6. **Conditional Links**: Links based on user fitness level or goals