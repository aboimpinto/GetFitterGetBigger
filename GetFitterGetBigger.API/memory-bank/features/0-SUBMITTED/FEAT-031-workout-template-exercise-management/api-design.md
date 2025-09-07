# API Design: Workout Template Exercise Management

## Database Schema

### New Tables

#### WorkoutType (Reference Table - Cached Forever)
```sql
CREATE TABLE WorkoutType (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    ValidationRules NVARCHAR(MAX), -- JSON with type-specific rules
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Initial Data
INSERT INTO WorkoutType (Name, Code, Description) VALUES
(1, 'Reps & Sets', 'REPS_SETS', 'Traditional workout with reps and sets'),
(2, 'EMOM', 'EMOM', 'Every Minute on the Minute'),
(3, 'Tabata', 'TABATA', '20s work, 10s rest, 8 rounds'),
(4, 'AMRAP', 'AMRAP', 'As Many Reps/Rounds As Possible'),
(5, 'Circuit', 'CIRCUIT', 'Sequential exercises with minimal rest'),
(6, 'Ladder', 'LADDER', 'Progressive rep scheme'),
(7, 'CrossFit Pattern', 'CROSSFIT', 'Patterns like 21-15-9');
```

#### WorkoutTemplate (Modified)
```sql
-- Add these columns to existing WorkoutTemplate table
ALTER TABLE WorkoutTemplate ADD
    WorkoutTypeId INT NOT NULL DEFAULT 1 FOREIGN KEY REFERENCES WorkoutType(Id),
    WorkoutTypeConfig NVARCHAR(MAX) NULL; -- JSON configuration for workout type
```

#### WorkoutTemplateExercise (Redesigned)
```sql
CREATE TABLE WorkoutTemplateExercise (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    WorkoutTemplateId INT NOT NULL FOREIGN KEY REFERENCES WorkoutTemplate(Id),
    ExerciseId INT NOT NULL FOREIGN KEY REFERENCES Exercise(Id),
    Phase NVARCHAR(20) NOT NULL CHECK (Phase IN ('Warmup', 'Workout', 'Cooldown')),
    RoundNumber INT NOT NULL CHECK (RoundNumber > 0),
    OrderInRound INT NOT NULL CHECK (OrderInRound > 0),
    Metadata NVARCHAR(MAX) NOT NULL, -- JSON structure
    AddedAutomatically BIT NOT NULL DEFAULT 0, -- Track if auto-added
    SourceExerciseId UNIQUEIDENTIFIER NULL, -- Links to workout exercise that triggered auto-add
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_WorkoutTemplate (WorkoutTemplateId, Phase, RoundNumber, OrderInRound),
    INDEX IX_Exercise (ExerciseId),
    INDEX IX_Source (SourceExerciseId)
);
```

### Metadata JSON Structure Examples

#### Time-based Exercises (Including REST)
```json
{
  "duration": 30,
  "unit": "seconds"  // or "minutes"
}
```

#### Reps-only Exercises (Bodyweight)
```json
{
  "reps": 10
}
```

#### Weight + Reps Exercises
```json
{
  "reps": 20,
  "weight": {
    "value": 10,
    "unit": "kg"  // or "lbs"
  }
}
```

#### Complex Patterns (Future)
```json
// Superset indication
{
  "reps": 10,
  "weight": {"value": 60, "unit": "kg"},
  "supersetGroup": "A"
}

// Time under tension
{
  "reps": 10,
  "weight": {"value": 60, "unit": "kg"},
  "tempo": {
    "eccentric": 3,
    "pause": 1,
    "concentric": 1,
    "rest": 0
  }
}
```

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
        "addedAutomatically": true,
        "sourceExerciseId": "guid-1"
      },
      {
        "id": "guid-3",
        "exerciseId": 78,
        "exerciseName": "Static Stretching",
        "phase": "Cooldown",
        "roundNumber": 1,
        "orderInRound": 1,
        "addedAutomatically": true,
        "sourceExerciseId": "guid-1"
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
   - Add missing ones to appropriate phases
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
   - Find all auto-added warmup/cooldown linked to it
   - Check if other workout exercises use same warmup/cooldown
   - Remove orphaned exercises
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
    "workoutType": {
      "id": 1,
      "name": "Reps & Sets",
      "code": "REPS_SETS"
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
2. If workout exercise with auto-added links:
   - Find all exercises with SourceExerciseId = this GUID
   - Check if other workout exercises link to same warmup/cooldown
   - Build removal list
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

### Reps & Sets Specific
- REST exercises only accept duration
- Weight exercises require weight metadata
- Reps-based exercises require reps count
- Time-based exercises require duration

### Metadata Validation
```csharp
public class MetadataValidator
{
    public ValidationResult Validate(ExerciseType exerciseType, WorkoutType workoutType, JsonDocument metadata)
    {
        // Type-specific validation logic
        return exerciseType switch
        {
            ExerciseType.REST => ValidateRestMetadata(metadata),
            _ => ValidateByWorkoutType(workoutType, exerciseType, metadata)
        };
    }
}
```

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
- Cache WorkoutType table forever
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