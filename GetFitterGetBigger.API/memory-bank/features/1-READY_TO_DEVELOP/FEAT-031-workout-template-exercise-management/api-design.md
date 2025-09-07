# API Design: Workout Template Exercise Management

## Database Schema

**See**: [database-schema.md](./database-schema.md) for complete schema definition including:
- ExecutionProtocol migration (First Task!)
- WorkoutTemplate modifications
- WorkoutTemplateExercise table structure
- Design decisions and relationships

## Metadata Patterns

**See**: [metadata-patterns.md](./metadata-patterns.md) for complete JSON structure definitions including:
- Time-based, reps-based, and weight-based patterns
- Advanced patterns (supersets, tempo, drop sets)
- ExecutionProtocol-specific configurations
- Validation rules and best practices

## API Endpoints

### 1. Add Exercise to Template

**Endpoint**: `POST /api/workout-templates/{templateId}/exercises`

**Request Body**:
```json
{
  "exerciseId": 125,
  "phase": "Workout", // Warmup, Workout, Cooldown
  "roundNumber": 1,
  "metadata": {
    "reps": 10,
    "weight": {
      "value": 20,
      "unit": "kg"
    }
  }
  // Note: Order is auto-determined (max + 1 in round)
}
```

**Response**:
```json
{
  "success": true,
  "data": {
    "addedExercises": [
      {
        "id": "guid-1",
        "exerciseId": 125,
        "exerciseName": "Barbell Squat",
        "phase": "Workout",
        "roundNumber": 1,
        "orderInRound": 1
      },
      {
        "id": "guid-2",
        "exerciseId": 45,
        "exerciseName": "High Knees",
        "phase": "Warmup",
        "roundNumber": 1,
        "orderInRound": 1,
      },
      {
        "id": "guid-3",
        "exerciseId": 78,
        "exerciseName": "Static Stretching",
        "phase": "Cooldown",
        "roundNumber": 1,
        "orderInRound": 1,
      }
    ]
  }
}
```

**Business Logic**:
1. Validate template exists and user has permission
2. Validate exercise exists and is active
3. If phase == "Workout":
   - Query ExerciseLinks for warmup/cooldown exercises
   - Check if they already exist in template
   - Add missing ones to appropriate phases with empty metadata
4. Add the main exercise
5. Return all added exercises

### 2. Remove Exercise from Template

**Endpoint**: `DELETE /api/workout-templates/{templateId}/exercises/{exerciseGuid}`

**Response**:
```json
{
  "success": true,
  "data": {
    "removedExercises": [
      {
        "id": "guid-1",
        "exerciseName": "Barbell Squat"
      },
      {
        "id": "guid-2",
        "exerciseName": "High Knees",
        "reason": "No longer used by any workout exercise"
      }
    ]
  }
}
```

**Business Logic**:
1. Find exercise by GUID
2. If it's a workout exercise:
   - Query ExerciseLinks for its warmup/cooldown
   - Check if ANY other workout exercise uses same warmup/cooldown
   - Remove orphaned exercises (not used by any other workout exercise)
3. Remove the main exercise
4. Re-sequence order in affected round

### 3. Reorder Exercise

**Endpoint**: `PUT /api/workout-templates/{templateId}/exercises/{exerciseGuid}/order`

**Request Body**:
```json
{
  "newOrderInRound": 3
}
```

**Response**:
```json
{
  "success": true,
  "data": {
    "updatedExercises": [
      {
        "id": "guid-1",
        "orderInRound": 3
      },
      {
        "id": "guid-2",
        "orderInRound": 1
      },
      {
        "id": "guid-3",
        "orderInRound": 2
      }
    ]
  }
}
```

### 4. Copy Round

**Endpoint**: `POST /api/workout-templates/{templateId}/rounds/copy`

**Request Body**:
```json
{
  "sourcePhase": "Workout",
  "sourceRoundNumber": 1,
  "targetPhase": "Workout",
  "targetRoundNumber": 2
}
```

**Response**:
```json
{
  "success": true,
  "data": {
    "copiedExercises": [
      {
        "id": "new-guid-1",
        "exerciseName": "Barbell Squat",
        "phase": "Workout",
        "roundNumber": 2
      }
    ]
  }
}
```

### 5. Get Template Exercises

**Endpoint**: `GET /api/workout-templates/{templateId}/exercises`

**Response**:
```json
{
  "success": true,
  "data": {
    "templateId": "123",
    "templateName": "Leg Burning I",
    "executionProtocol": {
      "id": "30000003-3000-4000-8000-300000000001",
      "value": "Reps and Sets",
      "code": "REPS_AND_SETS"
    },
    "phases": {
      "warmup": {
        "rounds": [
          {
            "roundNumber": 1,
            "exercises": [
              {
                "id": "guid-1",
                "exerciseId": 45,
                "exerciseName": "High Knees",
                "exerciseType": "CARDIO",
                "orderInRound": 1,
                "metadata": {
                  "duration": "30s"
                }
              }
            ]
          }
        ]
      },
      "workout": {
        "rounds": [
          {
            "roundNumber": 1,
            "exercises": [
              {
                "id": "guid-2",
                "exerciseId": 125,
                "exerciseName": "Barbell Squat",
                "exerciseType": "STRENGTH",
                "orderInRound": 1,
                "metadata": {
                  "reps": 10,
                  "weight": {
                    "value": 20,
                    "unit": "kg"
                  }
                }
              },
              {
                "id": "guid-3",
                "exerciseId": 999,
                "exerciseName": "Rest",
                "exerciseType": "REST",
                "orderInRound": 2,
                "metadata": {
                  "duration": "60s"
                }
              }
            ]
          }
        ]
      },
      "cooldown": {
        "rounds": []
      }
    }
  }
}
```

### 6. Update Exercise Metadata

**Endpoint**: `PUT /api/workout-templates/{templateId}/exercises/{exerciseGuid}/metadata`

**Request Body**:
```json
{
  "metadata": {
    "reps": 12,
    "weight": {
      "value": 25,
      "unit": "kg"
    }
  }
}
```

## Service Layer Design

### WorkoutTemplateExerciseService

```csharp
public interface IWorkoutTemplateExerciseService
{
    Task<ServiceResult<AddExerciseResultDto>> AddExerciseAsync(int templateId, AddExerciseDto dto);
    Task<ServiceResult<RemoveExerciseResultDto>> RemoveExerciseAsync(int templateId, Guid exerciseGuid);
    Task<ServiceResult<ReorderResultDto>> ReorderExerciseAsync(int templateId, Guid exerciseGuid, int newOrder);
    Task<ServiceResult<CopyRoundResultDto>> CopyRoundAsync(int templateId, CopyRoundDto dto);
    Task<ServiceResult<WorkoutTemplateExercisesDto>> GetTemplateExercisesAsync(int templateId);
    Task<ServiceResult<UpdateMetadataResultDto>> UpdateExerciseMetadataAsync(int templateId, Guid exerciseGuid, JsonDocument metadata);
}
```

### Key Service Methods Implementation Strategy

#### AddExerciseAsync Logic
1. Validate workout template exists and user has access
2. Validate exercise exists and is active
3. Determine order in round (max + 1)
4. If adding workout exercise:
   - Query ExerciseLinks for related exercises
   - Check existing exercises in template
   - Prepare auto-add list
5. Begin transaction
6. Add main exercise
7. Add auto-linked exercises
8. Commit transaction
9. Return all added exercises

#### RemoveExerciseAsync Logic
1. Find exercise instance by GUID
2. If workout exercise:
   - Query ExerciseLinks for its warmup/cooldown exercises
   - Check if ANY other workout exercise uses same warmup/cooldown
   - Build removal list of orphaned exercises
3. Begin transaction
4. Remove all identified exercises
5. Re-sequence remaining exercises in affected rounds
6. Commit transaction

## Validation Rules

### General Rules
- All exercises must have metadata
- Round numbers must be > 0
- Order in round must be > 0
- Phase must be valid enum value
- Template must be in Draft state to be modified
- Archived exercises cannot be added to new templates

### REPS_AND_SETS Protocol Specific
- REST exercises only accept duration
- Weight exercises require weight metadata (check ExerciseWeightType)
- Reps-based exercises require reps count
- Time-based exercises require duration

### Metadata Validation Strategy
- **Service Layer Responsibility**: The service validates metadata based on Exercise, ExecutionProtocol, and ExerciseType
- **Strategy Pattern**: Different validation strategies for different execution protocols
- **Invalid Metadata Handling**: Exercise is rejected with appropriate error message
- **Unknown Strategy**: If no strategy found for protocol/exercise combination, reject with error

### Template State Validation
- **Draft State**: Full CRUD operations allowed
- **Production State**: Only description and media updates allowed
- **Archived State**: No modifications allowed

### Exercise State Validation
- **Active Exercises**: Can be added to draft templates
- **Archived Exercises**: Cannot be added, but remain in existing templates
- **Deleted Exercises**: Soft-deleted (IsDeleted flag), never hard deleted

## Error Responses

### Standard Error Format
```json
{
  "success": false,
  "error": {
    "code": "EXERCISE_NOT_FOUND",
    "message": "Exercise with ID 999 not found",
    "details": {
      "exerciseId": 999
    }
  }
}
```

### Common Error Codes
- `TEMPLATE_NOT_FOUND`
- `EXERCISE_NOT_FOUND`
- `INVALID_METADATA`
- `MISSING_METADATA`
- `INVALID_PHASE`
- `INVALID_ROUND_NUMBER`
- `UNAUTHORIZED_ACCESS`
- `DUPLICATE_EXERCISE` (same exercise in same round)

## Performance Considerations

### Caching Strategy
- ExecutionProtocol already cached forever (existing)
- Cache Exercise data for 5 minutes
- Cache ExerciseLinks for 10 minutes
- Invalidate template cache on any modification

### Query Optimization
- Use single query with includes for GetTemplateExercises
- Batch auto-add operations in single transaction
- Index on (WorkoutTemplateId, Phase, RoundNumber, OrderInRound)

### Bulk Operations
- Support bulk add/remove in future iterations
- Limit rounds per template to prevent abuse (e.g., max 100)
- Implement pagination for very large templates

## Testing Requirements

### Unit Tests
- Service layer business logic
- Metadata validation
- Auto-linking logic
- Orphan detection

### Integration Tests
- Full endpoint testing
- Transaction rollback scenarios
- Concurrent modification handling
- Permission validation

### Test Scenarios
1. Add workout exercise with auto-linking
2. Remove workout exercise with cleanup
3. Reorder within round
4. Copy round with all metadata
5. Validate metadata for different exercise types
6. Prevent duplicate warmup/cooldown
7. Handle circular dependencies in ExerciseLinks