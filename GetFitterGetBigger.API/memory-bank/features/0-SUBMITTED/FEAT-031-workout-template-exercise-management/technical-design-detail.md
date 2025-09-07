# Technical Design Detail: Workout Template Exercise Management

## Overview

This document provides the complete technical design for the Workout Template Exercise Management system, demonstrating how a flexible metadata-driven approach can support any workout type while maintaining a simple, consistent database structure.

## Core Design Principles

1. **WorkoutTemplate** defines the RULES and STRUCTURE
2. **WorkoutTemplateExercise** defines the CONTENT 
3. **Metadata JSON** provides infinite flexibility
4. **No schema changes** needed for new workout types

## Database Schema

### WorkoutType (Reference Table - Cached Forever)
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
```

### WorkoutTemplate (Modified)
```sql
-- Add these columns to existing WorkoutTemplate table
ALTER TABLE WorkoutTemplate ADD
    WorkoutTypeId INT NOT NULL DEFAULT 1 FOREIGN KEY REFERENCES WorkoutType(Id),
    WorkoutTypeConfig NVARCHAR(MAX) NULL; -- JSON configuration for workout type
```

### WorkoutTemplateExercise (New Design)
```sql
CREATE TABLE WorkoutTemplateExercise (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    WorkoutTemplateId INT NOT NULL FOREIGN KEY REFERENCES WorkoutTemplate(Id),
    ExerciseId INT NOT NULL FOREIGN KEY REFERENCES Exercise(Id),
    Phase NVARCHAR(20) NOT NULL CHECK (Phase IN ('Warmup', 'Workout', 'Cooldown')),
    RoundNumber INT NOT NULL CHECK (RoundNumber > 0),
    OrderInRound INT NOT NULL CHECK (OrderInRound > 0),
    Metadata NVARCHAR(MAX) NOT NULL, -- JSON structure
    AddedAutomatically BIT NOT NULL DEFAULT 0,
    SourceExerciseId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_WorkoutTemplate (WorkoutTemplateId, Phase, RoundNumber, OrderInRound),
    INDEX IX_Exercise (ExerciseId),
    INDEX IX_Source (SourceExerciseId)
);
```

## Complete Example: "Leg Burning I" Workout

### WorkoutTemplate Table
```sql
Id: 1001
Name: 'Leg Burning I'
WorkoutTypeId: 1 (Reps & Sets)
WorkoutTypeConfig: NULL -- Reps & Sets doesn't need special config
```

### WorkoutTemplateExercise Table Rows

Let's assume these Exercise IDs:
- High Knees Running in Place = 101
- Squats (Bodyweight) = 102  
- Lunges (Bodyweight) = 103
- Barbell Squat = 104
- Barbell Front Lunges = 105
- Walking Lunges = 106
- Pigeon Pose Left = 107
- Pigeon Pose Right = 108
- Rest = 999

#### WARMUP PHASE
```sql
-- Round 1
INSERT INTO WorkoutTemplateExercise VALUES
('a1b2c3d4-1111-1111-1111-111111111111', 1001, 101, 'Warmup', 1, 1, 
 '{"duration": 20, "unit": "seconds"}', 0, NULL),
('a1b2c3d4-2222-2222-2222-222222222222', 1001, 102, 'Warmup', 1, 2, 
 '{"reps": 10}', 0, NULL),
('a1b2c3d4-3333-3333-3333-333333333333', 1001, 103, 'Warmup', 1, 3, 
 '{"reps": 10}', 0, NULL),

-- Round 2 (copied from Round 1, different GUIDs)
('b1b2c3d4-1111-1111-1111-111111111111', 1001, 101, 'Warmup', 2, 1, 
 '{"duration": 20, "unit": "seconds"}', 0, NULL),
('b1b2c3d4-2222-2222-2222-222222222222', 1001, 102, 'Warmup', 2, 2, 
 '{"reps": 10}', 0, NULL),
('b1b2c3d4-3333-3333-3333-333333333333', 1001, 103, 'Warmup', 2, 3, 
 '{"reps": 10}', 0, NULL);
```

#### WORKOUT PHASE
```sql
-- Round 1: Barbell Squat 0kg x20 + Rest
('c1c2c3d4-1111-1111-1111-111111111111', 1001, 104, 'Workout', 1, 1,
 '{"reps": 20, "weight": {"value": 0, "unit": "kg"}}', 0, NULL),
('c1c2c3d4-2222-2222-2222-222222222222', 1001, 999, 'Workout', 1, 2,
 '{"duration": 60, "unit": "seconds"}', 0, NULL),

-- Round 2: Barbell Squat 10kg x10 + Rest
('d1d2d3d4-1111-1111-1111-111111111111', 1001, 104, 'Workout', 2, 1,
 '{"reps": 10, "weight": {"value": 10, "unit": "kg"}}', 0, NULL),
('d1d2d3d4-2222-2222-2222-222222222222', 1001, 999, 'Workout', 2, 2,
 '{"duration": 60, "unit": "seconds"}', 0, NULL),

-- Round 3: Barbell Squat 20kg x10 + Rest
('e1e2e3e4-1111-1111-1111-111111111111', 1001, 104, 'Workout', 3, 1,
 '{"reps": 10, "weight": {"value": 20, "unit": "kg"}}', 0, NULL),
('e1e2e3e4-2222-2222-2222-222222222222', 1001, 999, 'Workout', 3, 2,
 '{"duration": 60, "unit": "seconds"}', 0, NULL),

-- Round 4: Barbell Squat 30kg x10 + Long Rest
('f1f2f3f4-1111-1111-1111-111111111111', 1001, 104, 'Workout', 4, 1,
 '{"reps": 10, "weight": {"value": 30, "unit": "kg"}}', 0, NULL),
('f1f2f3f4-2222-2222-2222-222222222222', 1001, 999, 'Workout', 4, 2,
 '{"duration": 180, "unit": "seconds"}', 0, NULL),

-- Round 5: Barbell Front Lunges 0kg x20 + Rest
('g1g2g3g4-1111-1111-1111-111111111111', 1001, 105, 'Workout', 5, 1,
 '{"reps": 20, "weight": {"value": 0, "unit": "kg"}}', 0, NULL),
('g1g2g3g4-2222-2222-2222-222222222222', 1001, 999, 'Workout', 5, 2,
 '{"duration": 60, "unit": "seconds"}', 0, NULL),

-- Round 6: Barbell Front Lunges 10kg x10 + Rest
('h1h2h3h4-1111-1111-1111-111111111111', 1001, 105, 'Workout', 6, 1,
 '{"reps": 10, "weight": {"value": 10, "unit": "kg"}}', 0, NULL),
('h1h2h3h4-2222-2222-2222-222222222222', 1001, 999, 'Workout', 6, 2,
 '{"duration": 60, "unit": "seconds"}', 0, NULL),

-- Round 7: Barbell Front Lunges 10kg x10 + Rest
('i1i2i3i4-1111-1111-1111-111111111111', 1001, 105, 'Workout', 7, 1,
 '{"reps": 10, "weight": {"value": 10, "unit": "kg"}}', 0, NULL),
('i1i2i3i4-2222-2222-2222-222222222222', 1001, 999, 'Workout', 7, 2,
 '{"duration": 60, "unit": "seconds"}', 0, NULL),

-- Round 8: Barbell Front Lunges 10kg x10 + Long Rest
('j1j2j3j4-1111-1111-1111-111111111111', 1001, 105, 'Workout', 8, 1,
 '{"reps": 10, "weight": {"value": 10, "unit": "kg"}}', 0, NULL),
('j1j2j3j4-2222-2222-2222-222222222222', 1001, 999, 'Workout', 8, 2,
 '{"duration": 180, "unit": "seconds"}', 0, NULL),

-- Round 9: Walking Lunges x50 + Rest
('k1k2k3k4-1111-1111-1111-111111111111', 1001, 106, 'Workout', 9, 1,
 '{"reps": 50}', 0, NULL),
('k1k2k3k4-2222-2222-2222-222222222222', 1001, 999, 'Workout', 9, 2,
 '{"duration": 180, "unit": "seconds"}', 0, NULL);
```

#### COOLDOWN PHASE
```sql
-- Round 1 (Note: Round numbering restarts per phase)
('l1l2l3l4-1111-1111-1111-111111111111', 1001, 107, 'Cooldown', 1, 1,
 '{"duration": 30, "unit": "seconds"}', 0, NULL),
('l1l2l3l4-2222-2222-2222-222222222222', 1001, 108, 'Cooldown', 1, 2,
 '{"duration": 30, "unit": "seconds"}', 0, NULL);
```

## Metadata Patterns by Exercise Type

### 1. Time-Based Exercises
```json
{
  "duration": 30,
  "unit": "seconds"  // or "minutes"
}
```

### 2. Reps-Only Exercises
```json
{
  "reps": 10
}
```

### 3. Weight + Reps Exercises
```json
{
  "reps": 20,
  "weight": {
    "value": 10,
    "unit": "kg"  // or "lbs"
  }
}
```

### 4. Distance-Based Exercises (future)
```json
{
  "distance": 400,
  "unit": "meters",  // or "miles", "km"
  "targetTime": 90,
  "timeUnit": "seconds"
}
```

## All Workout Types Supported

### 1. Reps & Sets (Traditional)
```sql
-- WorkoutTemplate
WorkoutTypeId: 1
WorkoutTypeConfig: NULL  -- No special config needed

-- WorkoutTemplateExercise
-- As shown in the complete example above
```

### 2. EMOM (Every Minute on the Minute)
```sql
-- WorkoutTemplate
WorkoutTypeId: 2
WorkoutTypeConfig: {
  "interval": 60,
  "intervalUnit": "seconds",
  "totalDuration": 10,
  "durationUnit": "minutes"
}

-- WorkoutTemplateExercise (10 rounds auto-created)
Round 1-10: Each contains same exercises
  Exercise: Burpees | Metadata: {"reps": 5}
  Exercise: Air Squats | Metadata: {"reps": 10}
  Exercise: Push-ups | Metadata: {"reps": 5}
-- User must complete all within the minute, rest remainder
```

### 3. Tabata
```sql
-- WorkoutTemplate
WorkoutTypeId: 3
WorkoutTypeConfig: {
  "workDuration": 20,
  "restDuration": 10,
  "rounds": 8,
  "unit": "seconds"
}

-- WorkoutTemplateExercise
Round 1: Burpees | Metadata: {}  -- Just do max reps for 20s
Round 2: Mountain Climbers | Metadata: {}
Round 3: Jump Squats | Metadata: {}
Round 4: High Knees | Metadata: {}
(Rounds 5-8 repeat exercises 1-4)
```

### 4. AMRAP (As Many Reps/Rounds As Possible)
```sql
-- WorkoutTemplate
WorkoutTypeId: 4
WorkoutTypeConfig: {
  "timeCap": 15,
  "unit": "minutes"
}

-- WorkoutTemplateExercise (defines one round to repeat)
Round 1:
  Exercise: Thrusters | Metadata: {"reps": 10, "weight": {"value": 40, "unit": "kg"}}
  Exercise: Pull-ups | Metadata: {"reps": 10}
  Exercise: Box Jumps | Metadata: {"reps": 10, "height": {"value": 24, "unit": "inches"}}
-- User repeats this round as many times as possible in 15 minutes
```

### 5. Supersets
```sql
-- Option A: As metadata flag
Round 1:
  Exercise: Bench Press | Metadata: {"reps": 10, "weight": {"value": 60, "unit": "kg"}, "supersetGroup": "A"}
  Exercise: Bent Over Row | Metadata: {"reps": 10, "weight": {"value": 40, "unit": "kg"}, "supersetGroup": "A"}
  Exercise: Rest | Metadata: {"duration": 90, "unit": "seconds"}

-- Option B: As WorkoutType
WorkoutTypeId: 5
WorkoutTypeConfig: {
  "restBetweenSupersets": 90,
  "restUnit": "seconds"
}
```

### 6. Circuit Training
```sql
-- WorkoutTemplate
WorkoutTypeId: 6
WorkoutTypeConfig: {
  "rounds": 3,
  "restBetweenRounds": 60,
  "restUnit": "seconds"
}

-- WorkoutTemplateExercise
Round 1:
  Exercise: Burpees | Metadata: {"duration": 30, "unit": "seconds"}
  Exercise: Mountain Climbers | Metadata: {"duration": 30, "unit": "seconds"}
  Exercise: Plank | Metadata: {"duration": 30, "unit": "seconds"}
  Exercise: Jumping Jacks | Metadata: {"duration": 30, "unit": "seconds"}
  Exercise: Rest | Metadata: {"duration": 60, "unit": "seconds"}
(Rounds 2-3 are copies)
```

### 7. Pyramid Sets
```sql
-- WorkoutTemplate
WorkoutTypeId: 1  -- Just use Reps & Sets
WorkoutTypeConfig: NULL

-- WorkoutTemplateExercise
Round 1: Squats | Metadata: {"reps": 2, "weight": {"value": 100, "unit": "kg"}}
Round 2: Squats | Metadata: {"reps": 4, "weight": {"value": 90, "unit": "kg"}}
Round 3: Squats | Metadata: {"reps": 6, "weight": {"value": 80, "unit": "kg"}}
Round 4: Squats | Metadata: {"reps": 8, "weight": {"value": 70, "unit": "kg"}}
Round 5: Squats | Metadata: {"reps": 6, "weight": {"value": 80, "unit": "kg"}}
Round 6: Squats | Metadata: {"reps": 4, "weight": {"value": 90, "unit": "kg"}}
Round 7: Squats | Metadata: {"reps": 2, "weight": {"value": 100, "unit": "kg"}}
```

### 8. 21-15-9 (CrossFit)
```sql
-- WorkoutTemplate
WorkoutTypeId: 7
WorkoutTypeConfig: {
  "pattern": "21-15-9",
  "forTime": true
}

-- WorkoutTemplateExercise
Round 1:
  Exercise: Thrusters | Metadata: {"reps": 21, "weight": {"value": 40, "unit": "kg"}}
  Exercise: Pull-ups | Metadata: {"reps": 21}
Round 2:
  Exercise: Thrusters | Metadata: {"reps": 15, "weight": {"value": 40, "unit": "kg"}}
  Exercise: Pull-ups | Metadata: {"reps": 15}
Round 3:
  Exercise: Thrusters | Metadata: {"reps": 9, "weight": {"value": 40, "unit": "kg"}}
  Exercise: Pull-ups | Metadata: {"reps": 9}
```

### 9. Cluster Sets
```sql
-- WorkoutTemplate
WorkoutTypeId: 1  -- Reps & Sets with special metadata

-- WorkoutTemplateExercise
Round 1:
  Exercise: Deadlift | Metadata: {"reps": 3, "weight": {"value": 140, "unit": "kg"}, "miniRest": 15}
  Exercise: Deadlift | Metadata: {"reps": 3, "weight": {"value": 140, "unit": "kg"}, "miniRest": 15}
  Exercise: Deadlift | Metadata: {"reps": 3, "weight": {"value": 140, "unit": "kg"}}
  Exercise: Rest | Metadata: {"duration": 180, "unit": "seconds"}
```

### 10. Drop Sets
```sql
-- WorkoutTemplate
WorkoutTypeId: 1  -- Reps & Sets with special metadata

-- WorkoutTemplateExercise
Round 1:
  Exercise: Bicep Curls | Metadata: {"reps": 8, "weight": {"value": 15, "unit": "kg"}, "dropSet": 1}
  Exercise: Bicep Curls | Metadata: {"reps": 8, "weight": {"value": 12, "unit": "kg"}, "dropSet": 2}
  Exercise: Bicep Curls | Metadata: {"reps": 8, "weight": {"value": 10, "unit": "kg"}, "dropSet": 3}
  Exercise: Rest | Metadata: {"duration": 90, "unit": "seconds"}
```

### 11. Time Under Tension
```sql
-- WorkoutTemplate
WorkoutTypeId: 1  -- Reps & Sets with tempo metadata

-- WorkoutTemplateExercise
Round 1:
  Exercise: Bench Press | Metadata: {
    "reps": 10,
    "weight": {"value": 60, "unit": "kg"},
    "tempo": {
      "eccentric": 3,    // 3 seconds lowering
      "pause": 1,        // 1 second pause at bottom
      "concentric": 1,   // 1 second lifting
      "rest": 0          // 0 second pause at top
    }
  }
```

### 12. Ladder Workouts
```sql
-- WorkoutTemplate
WorkoutTypeId: 8
WorkoutTypeConfig: {
  "type": "ascending",  // or "descending" or "pyramid"
  "start": 1,
  "end": 10,
  "step": 1
}

-- WorkoutTemplateExercise (system generates 10 rounds)
Round 1: Burpees | Metadata: {"reps": 1}
Round 2: Burpees | Metadata: {"reps": 2}
Round 3: Burpees | Metadata: {"reps": 3}
...
Round 10: Burpees | Metadata: {"reps": 10}
```

## Complex Scenarios

### Superset within EMOM
```sql
-- WorkoutTemplate
WorkoutTypeId: 2 (EMOM)
WorkoutTypeConfig: {"interval": 60, "totalDuration": 10, "durationUnit": "minutes"}

-- WorkoutTemplateExercise
Each Round (1-10):
  Exercise: Deadlift | Metadata: {"reps": 5, "weight": {"value": 100, "unit": "kg"}, "supersetGroup": "A"}
  Exercise: Box Jumps | Metadata: {"reps": 5, "height": {"value": 24, "unit": "inches"}, "supersetGroup": "A"}
-- Perform both back-to-back at the start of each minute
```

### Mixed Workout (Different types per phase)
```sql
-- Warmup: Traditional reps
Warmup Round 1:
  Exercise: Jumping Jacks | Metadata: {"reps": 20}
  Exercise: Arm Circles | Metadata: {"reps": 10}

-- Workout: EMOM style
Workout Rounds 1-10:
  Exercise: Thrusters | Metadata: {"reps": 10, "weight": {"value": 40, "unit": "kg"}}
  Exercise: Burpees | Metadata: {"reps": 5}

-- Cooldown: Time-based stretching
Cooldown Round 1:
  Exercise: Hamstring Stretch | Metadata: {"duration": 30, "unit": "seconds", "side": "left"}
  Exercise: Hamstring Stretch | Metadata: {"duration": 30, "unit": "seconds", "side": "right"}
```

## Auto-Add Logic Example

When PT adds a Barbell Squat to the workout:

```sql
-- 1. User adds Barbell Squat to Workout
INSERT WorkoutTemplateExercise (Id, ExerciseId, Phase, Round, Order, Metadata)
VALUES ('main-guid', 104, 'Workout', 1, 1, '{"reps": 10, "weight": {"value": 60, "unit": "kg"}}')

-- 2. System queries ExerciseLinks
SELECT * FROM ExerciseLinks WHERE ExerciseId = 104

-- 3. Finds linked exercises:
-- WarmupExerciseId: 102 (Bodyweight Squats)
-- CooldownExerciseId: 107 (Pigeon Pose Left)
-- CooldownExerciseId: 108 (Pigeon Pose Right)

-- 4. Checks if they already exist in template
SELECT * FROM WorkoutTemplateExercise 
WHERE WorkoutTemplateId = 1001 AND ExerciseId IN (102, 107, 108)

-- 5. Auto-adds missing ones
INSERT WorkoutTemplateExercise VALUES
('auto-guid-1', 1001, 102, 'Warmup', 1, 4, '{}', 1, 'main-guid'),  -- Empty metadata, PT must fill
('auto-guid-2', 1001, 107, 'Cooldown', 1, 1, '{}', 1, 'main-guid'),
('auto-guid-3', 1001, 108, 'Cooldown', 1, 2, '{}', 1, 'main-guid')
```

## API Response Example

### GET /api/workout-templates/1001/exercises

```json
{
  "success": true,
  "data": {
    "templateId": 1001,
    "templateName": "Leg Burning I",
    "workoutType": {
      "id": 1,
      "name": "Reps & Sets",
      "code": "REPS_SETS"
    },
    "workoutTypeConfig": null,
    "phases": {
      "warmup": {
        "rounds": [
          {
            "roundNumber": 1,
            "exercises": [
              {
                "id": "a1b2c3d4-1111-1111-1111-111111111111",
                "exerciseId": 101,
                "exerciseName": "High Knees Running in Place",
                "exerciseType": "CARDIO",
                "orderInRound": 1,
                "metadata": {
                  "duration": 20,
                  "unit": "seconds"
                },
                "addedAutomatically": false
              },
              {
                "id": "a1b2c3d4-2222-2222-2222-222222222222",
                "exerciseId": 102,
                "exerciseName": "Squats",
                "exerciseType": "BODYWEIGHT",
                "orderInRound": 2,
                "metadata": {
                  "reps": 10
                },
                "addedAutomatically": false
              },
              {
                "id": "a1b2c3d4-3333-3333-3333-333333333333",
                "exerciseId": 103,
                "exerciseName": "Lunges",
                "exerciseType": "BODYWEIGHT",
                "orderInRound": 3,
                "metadata": {
                  "reps": 10
                },
                "addedAutomatically": false
              }
            ]
          },
          {
            "roundNumber": 2,
            "exercises": [
              // Same structure, different GUIDs
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
                "id": "c1c2c3d4-1111-1111-1111-111111111111",
                "exerciseId": 104,
                "exerciseName": "Barbell Squat",
                "exerciseType": "STRENGTH",
                "orderInRound": 1,
                "metadata": {
                  "reps": 20,
                  "weight": {
                    "value": 0,
                    "unit": "kg"
                  }
                },
                "addedAutomatically": false
              },
              {
                "id": "c1c2c3d4-2222-2222-2222-222222222222",
                "exerciseId": 999,
                "exerciseName": "Rest",
                "exerciseType": "REST",
                "orderInRound": 2,
                "metadata": {
                  "duration": 60,
                  "unit": "seconds"
                },
                "addedAutomatically": false
              }
            ]
          }
          // ... rounds 2-9
        ]
      },
      "cooldown": {
        "rounds": [
          {
            "roundNumber": 1,
            "exercises": [
              {
                "id": "l1l2l3l4-1111-1111-1111-111111111111",
                "exerciseId": 107,
                "exerciseName": "Pigeon Pose Left",
                "exerciseType": "STRETCHING",
                "orderInRound": 1,
                "metadata": {
                  "duration": 30,
                  "unit": "seconds"
                },
                "addedAutomatically": false
              },
              {
                "id": "l1l2l3l4-2222-2222-2222-222222222222",
                "exerciseId": 108,
                "exerciseName": "Pigeon Pose Right",
                "exerciseType": "STRETCHING",
                "orderInRound": 2,
                "metadata": {
                  "duration": 30,
                  "unit": "seconds"
                },
                "addedAutomatically": false
              }
            ]
          }
        ]
      }
    }
  }
}
```

## Key Benefits of This Design

1. **Infinite Flexibility**: Any workout type can be supported without schema changes
2. **Simple Core Structure**: Just two main tables drive everything
3. **Consistent API**: Same endpoints work for all workout types
4. **Future-Proof**: New workout types just need a new WorkoutType row and config
5. **Performance**: Efficient queries with proper indexing
6. **Maintainable**: Clear separation between structure (template) and content (exercises)

## Implementation Priority

### Phase 1: Foundation
1. Create WorkoutType table with initial data
2. Add WorkoutTypeId and Config to WorkoutTemplate
3. Create new WorkoutTemplateExercise table
4. Migrate existing data

### Phase 2: Core Features (Reps & Sets)
1. Implement Add Exercise with auto-linking
2. Implement Remove Exercise with cleanup
3. Implement Reorder functionality
4. Implement Copy Round

### Phase 3: Advanced Workout Types
1. EMOM support
2. Tabata support
3. AMRAP support
4. Circuit training

### Phase 4: Complex Features
1. Supersets
2. Drop sets
3. Pyramid sets
4. Custom patterns

## Validation Examples

### Reps & Sets Validation
```csharp
// REST exercise must have duration
if (exercise.Type == ExerciseType.REST && !metadata.ContainsKey("duration"))
    throw new ValidationException("REST exercise requires duration");

// Weight exercise must have weight if ExerciseWeightType != BODYWEIGHT
if (exercise.WeightType != ExerciseWeightType.BODYWEIGHT && !metadata.ContainsKey("weight"))
    throw new ValidationException("This exercise requires weight specification");
```

### EMOM Validation
```csharp
// Total exercise time must fit within interval
var estimatedTime = CalculateExerciseTime(exercises);
if (estimatedTime > config.Interval)
    return Warning("Exercises may not fit within {interval} second window");
```

## Conclusion

This design provides a robust, flexible foundation that can handle any workout type imaginable while maintaining simplicity and consistency. The metadata-driven approach ensures we can adapt to new requirements without database migrations, and the clear separation of concerns makes the system easy to understand and maintain.