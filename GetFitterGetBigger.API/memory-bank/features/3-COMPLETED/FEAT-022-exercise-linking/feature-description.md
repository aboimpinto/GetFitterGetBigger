# FEAT-022: Exercise Linking

## Overview

This feature enables Personal Trainers to link exercises together based on their type (Warmup, Workout, Cooldown). When a workout exercise is selected during workout creation, its associated warmup and cooldown exercises will be automatically suggested or added, streamlining the workout building process.

## Business Value

- **Efficiency**: PTs can build workouts faster by having related exercises automatically suggested
- **Consistency**: Ensures appropriate warmups and cooldowns are included for specific exercises
- **Safety**: Reduces the risk of injury by ensuring proper exercise preparation and recovery
- **Knowledge Sharing**: Captures PT expertise in exercise relationships for reuse

## Technical Requirements

### Data Model

#### New Entity: ExerciseLink

```
ExerciseLink
{
    Id: ExerciseLinkId (GUID)
    SourceExerciseId: ExerciseId (GUID) - The main exercise
    TargetExerciseId: ExerciseId (GUID) - The linked exercise
    LinkType: string - "Warmup" or "Cooldown"
    DisplayOrder: int - Order in which linked exercises should appear
    IsActive: bool - Soft delete flag
    CreatedAt: DateTime
    UpdatedAt: DateTime
}
```

#### Constraints
- A source exercise can have multiple warmup and cooldown links
- The same target exercise can be linked to multiple source exercises
- Circular links are not allowed (A links to B, B cannot link back to A)
- Only exercises of type "Workout" can be source exercises
- Only exercises of type "Warmup" can be linked as warmups
- Only exercises of type "Cooldown" can be linked as cooldowns
- Rest exercises cannot be linked

### API Endpoints

#### 1. Create Exercise Link
- **POST** `/api/exercises/{exerciseId}/links`
- **Authorization**: PT-Tier, Admin-Tier
- **Request Body**:
  ```json
  {
    "targetExerciseId": "guid",
    "linkType": "Warmup|Cooldown",
    "displayOrder": 1
  }
  ```
- **Response**: 201 Created with created link
- **Validation**:
  - Source exercise must exist and be of type "Workout"
  - Target exercise must exist and match the linkType
  - No duplicate links (same source, target, and type)
  - No circular references

#### 2. Get Exercise Links
- **GET** `/api/exercises/{exerciseId}/links`
- **Authorization**: PT-Tier, Admin-Tier, Free-Tier
- **Query Parameters**:
  - `linkType`: Filter by "Warmup" or "Cooldown" (optional)
  - `includeExerciseDetails`: Include full exercise data (default: false)
- **Response**:
  ```json
  {
    "exerciseId": "guid",
    "links": [
      {
        "id": "guid",
        "targetExerciseId": "guid",
        "targetExercise": { /* if includeExerciseDetails=true */ },
        "linkType": "Warmup",
        "displayOrder": 1
      }
    ]
  }
  ```

#### 3. Update Exercise Link
- **PUT** `/api/exercises/{exerciseId}/links/{linkId}`
- **Authorization**: PT-Tier, Admin-Tier
- **Request Body**:
  ```json
  {
    "displayOrder": 2,
    "isActive": true
  }
  ```
- **Response**: 200 OK with updated link

#### 4. Delete Exercise Link
- **DELETE** `/api/exercises/{exerciseId}/links/{linkId}`
- **Authorization**: PT-Tier, Admin-Tier
- **Response**: 204 No Content
- **Note**: Soft delete (sets IsActive = false)

#### 5. Get Suggested Links
- **GET** `/api/exercises/{exerciseId}/suggested-links`
- **Authorization**: PT-Tier, Admin-Tier
- **Description**: Returns commonly linked exercises based on usage patterns
- **Response**: List of suggested exercises with link type and usage count

### Service Layer

#### IExerciseLinkService
- `CreateLinkAsync(ExerciseId sourceId, CreateExerciseLinkDto dto)`
- `GetLinksAsync(ExerciseId exerciseId, string? linkType, bool includeDetails)`
- `UpdateLinkAsync(ExerciseLinkId linkId, UpdateExerciseLinkDto dto)`
- `DeleteLinkAsync(ExerciseLinkId linkId)`
- `GetSuggestedLinksAsync(ExerciseId exerciseId)`
- `ValidateNoCircularReference(ExerciseId source, ExerciseId target)`

### Repository Layer

#### IExerciseLinkRepository
- Standard CRUD operations
- `GetBySourceExerciseAsync(ExerciseId sourceId, string? linkType)`
- `GetByTargetExerciseAsync(ExerciseId targetId)`
- `ExistsAsync(ExerciseId sourceId, ExerciseId targetId, string linkType)`
- `GetMostUsedLinksAsync(int count)`

### Database Changes

1. **New Table**: `ExerciseLinks`
   - Primary Key: `Id`
   - Foreign Keys: `SourceExerciseId`, `TargetExerciseId`
   - Indexes: 
     - `IX_ExerciseLinks_SourceExerciseId_LinkType`
     - `IX_ExerciseLinks_TargetExerciseId`
     - Unique: `SourceExerciseId_TargetExerciseId_LinkType`

2. **Migration**: `AddExerciseLinksTable`

### Validation Rules

1. **Exercise Type Validation**:
   - Source must be "Workout" type
   - Target must match linkType ("Warmup" or "Cooldown")
   - "Rest" exercises cannot be linked

2. **Circular Reference Prevention**:
   - Check if target already links back to source
   - Implement recursive check for indirect circular references

3. **Business Rules**:
   - Maximum 10 warmup links per exercise
   - Maximum 10 cooldown links per exercise
   - Display order must be unique within same source and link type

### Cache Considerations

- Cache exercise links with key pattern: `exercise-links:{exerciseId}:{linkType}`
- Invalidate on: link creation, update, deletion
- TTL: 1 hour

### Error Handling

- `ExerciseNotFoundException`: When source or target exercise not found
- `InvalidExerciseTypeException`: When exercise types don't match requirements
- `DuplicateLinkException`: When link already exists
- `CircularReferenceException`: When circular reference detected
- `MaxLinksExceededException`: When maximum links exceeded

## Testing Requirements

### Unit Tests
- Service layer validation logic
- Circular reference detection algorithm
- Exercise type validation

### Integration Tests
- Full CRUD operations
- Validation rule enforcement
- Cache invalidation
- Concurrent link creation

### Test Scenarios
1. Create valid warmup and cooldown links
2. Prevent linking non-workout exercises as source
3. Prevent linking wrong exercise types
4. Detect and prevent circular references
5. Handle concurrent link creation
6. Verify cache invalidation

## Completion Information

**Completed Date**: 2025-07-09  
**Accepted By**: Paulo Aboim Pinto (Product Owner)  
**Manual Testing**: Completed successfully  
**Status**: Ready for PI release  
**Implementation Duration**: 2h 0m  
**AI Assistance Impact**: 87.9% time reduction  
**Tests Added**: 35 comprehensive tests  
**Quality Status**: Zero defects, all tests passing  

## Future Enhancements

1. **Link Templates**: Pre-defined sets of warmup/cooldown combinations
2. **AI Suggestions**: ML-based exercise link recommendations
3. **Link Analytics**: Track most effective exercise combinations
4. **Bulk Operations**: Link multiple exercises at once
5. **Link Categories**: Group links by muscle group or movement pattern