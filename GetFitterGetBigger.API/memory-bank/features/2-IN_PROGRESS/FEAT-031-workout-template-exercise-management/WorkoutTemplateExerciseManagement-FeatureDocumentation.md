# Workout Template Exercise Management - Feature Documentation

## Overview

The Workout Template Exercise Management system is a comprehensive redesign of how exercises are organized within workout templates. It introduces a multi-phase, round-based structure with intelligent auto-linking and flexible JSON metadata support.

## Key Features

### 1. Multi-Phase Organization
- **Warmup Phase**: Preparatory exercises before the main workout
- **Workout Phase**: Main training exercises
- **Cooldown Phase**: Recovery exercises after the main workout

### 2. Round-Based Structure
- Unlimited rounds per phase
- Exercises organized within rounds with specific order
- Supports complex workout structures (supersets, circuits, progressive rounds)

### 3. Intelligent Auto-Linking
- Automatic addition of warmup exercises when adding workout exercises
- Automatic addition of cooldown exercises when adding workout exercises
- Based on ExerciseLink relationships stored in the system

### 4. Flexible JSON Metadata
- ExecutionProtocol-specific metadata support
- Supports all execution protocols: REPS_AND_SETS, SUPERSET, DROP_SET, AMRAP
- Flexible structure accommodates future protocol additions

### 5. Orphan Cleanup
- Intelligent removal of warmup/cooldown exercises when their related workout exercises are removed
- Preserves shared warmup/cooldown exercises used by multiple workout exercises

### 6. Round Management
- Copy entire rounds with new GUIDs generated
- Reorder exercises within rounds
- Maintain proper sequencing automatically

## API Endpoints

### Enhanced API (v2)
Base URL: `/api/v2/workout-templates/{templateId}/exercises`

#### POST / - Add Exercise
Adds an exercise with automatic warmup/cooldown linking.

**Request Example:**
```json
{
  "exerciseId": "exercise-550e8400-e29b-41d4-a716-446655440000",
  "phase": "Workout",
  "roundNumber": 1,
  "metadata": {
    "reps": 10,
    "weight": {
      "value": 60,
      "unit": "kg"
    }
  }
}
```

**Response Example:**
```json
{
  "success": true,
  "data": {
    "addedExercises": [
      {
        "id": "workouttemplateexercise-guid-1",
        "exerciseId": "exercise-550e8400-e29b-41d4-a716-446655440000",
        "phase": "Workout",
        "roundNumber": 1,
        "orderInRound": 1
      },
      {
        "id": "workouttemplateexercise-guid-2",
        "exerciseId": "exercise-warmup-guid",
        "phase": "Warmup",
        "roundNumber": 1,
        "orderInRound": 1
      }
    ],
    "autoLinkedCount": 1
  },
  "message": "Successfully added 2 exercise(s) with auto-linking"
}
```

#### DELETE /{exerciseId} - Remove Exercise
Removes an exercise with orphan cleanup.

**Response Example:**
```json
{
  "success": true,
  "data": {
    "removedExercises": [
      {
        "id": "workouttemplateexercise-guid-1",
        "exerciseId": "exercise-550e8400-e29b-41d4-a716-446655440000",
        "phase": "Workout"
      },
      {
        "id": "workouttemplateexercise-guid-2",
        "exerciseId": "exercise-warmup-guid",
        "phase": "Warmup"
      }
    ],
    "orphansRemoved": 1
  },
  "message": "Successfully removed 2 exercise(s) with orphan cleanup"
}
```

#### GET / - Get Template Exercises
Retrieves all exercises organized by phase and round.

**Response Example:**
```json
{
  "success": true,
  "data": {
    "templateId": "workouttemplate-550e8400-e29b-41d4-a716-446655440000",
    "phases": {
      "Warmup": {
        "rounds": {
          "1": [
            {
              "id": "workouttemplateexercise-guid",
              "exerciseId": "exercise-guid",
              "exerciseName": "High Knees",
              "phase": "Warmup",
              "roundNumber": 1,
              "orderInRound": 1,
              "metadata": {
                "duration": 30,
                "unit": "seconds"
              }
            }
          ]
        }
      },
      "Workout": {
        "rounds": {
          "1": [
            {
              "id": "workouttemplateexercise-guid-2",
              "exerciseId": "exercise-guid-2",
              "exerciseName": "Barbell Squat",
              "phase": "Workout",
              "roundNumber": 1,
              "orderInRound": 1,
              "metadata": {
                "reps": 10,
                "weight": {
                  "value": 60,
                  "unit": "kg"
                }
              }
            }
          ],
          "2": [...]
        }
      },
      "Cooldown": {
        "rounds": {
          "1": [...]
        }
      }
    }
  }
}
```

#### PUT /{exerciseId}/metadata - Update Exercise Metadata
Updates only the metadata field of an exercise.

**Request Example:**
```json
{
  "metadata": {
    "reps": 12,
    "weight": {
      "value": 70,
      "unit": "kg"
    }
  }
}
```

#### PUT /{exerciseId}/order - Reorder Exercise
Changes the order of an exercise within its round.

**Request Example:**
```json
{
  "newOrderInRound": 2
}
```

#### POST /rounds/copy - Copy Round
Copies all exercises from one round to another with new GUIDs.

**Request Example:**
```json
{
  "sourcePhase": "Workout",
  "sourceRoundNumber": 1,
  "targetPhase": "Workout",
  "targetRoundNumber": 2
}
```

## Metadata Structure Examples

### REPS_AND_SETS with Weight
```json
{
  "reps": 10,
  "weight": {
    "value": 60,
    "unit": "kg"
  }
}
```

### REPS_AND_SETS Bodyweight
```json
{
  "reps": 15
}
```

### Time-based Exercise
```json
{
  "duration": 30,
  "unit": "seconds"
}
```

### REST Exercise
```json
{
  "duration": 90,
  "unit": "seconds"
}
```

### SUPERSET (Future Enhancement)
```json
{
  "exercises": [
    {
      "exerciseId": "exercise-1",
      "reps": 10
    },
    {
      "exerciseId": "exercise-2",
      "reps": 12
    }
  ],
  "restBetween": 30
}
```

## Auto-Linking Behavior

### How Auto-Linking Works
1. When adding a WORKOUT exercise, the system checks for ExerciseLink relationships
2. WARMUP-type links are followed to find warmup exercises
3. COOLDOWN-type links are followed to find cooldown exercises
4. Linked exercises are automatically added to their respective phases
5. All exercises receive unique GUIDs and proper ordering

### Example Scenario
```
Database State:
- Exercise "Barbell Squat" (WORKOUT) linked to "High Knees" (WARMUP)
- Exercise "Barbell Squat" (WORKOUT) linked to "Standing Quad Stretch" (COOLDOWN)

Action: Add "Barbell Squat" to Workout phase, round 1

Result:
1. "Barbell Squat" added to Workout phase, round 1, order 1
2. "High Knees" automatically added to Warmup phase, round 1, order 1  
3. "Standing Quad Stretch" automatically added to Cooldown phase, round 1, order 1
```

## Orphan Cleanup Behavior

### How Orphan Cleanup Works
1. When removing a WORKOUT exercise, the system checks linked warmup/cooldown exercises
2. For each linked exercise, it counts how many other workout exercises also use it
3. If a warmup/cooldown exercise is only used by the removed workout exercise, it's marked as orphan
4. All orphaned exercises are removed automatically
5. Exercise ordering is recalculated for remaining exercises

### Example Scenarios

#### Scenario 1: Simple Orphan Cleanup
```
Template State:
- "Barbell Squat" (Workout) + "High Knees" (Warmup, auto-linked)

Action: Remove "Barbell Squat"

Result: Both exercises removed (High Knees becomes orphan)
```

#### Scenario 2: Shared Warmup Preserved
```
Template State:
- "Barbell Squat" (Workout) + "High Knees" (Warmup, linked to Barbell Squat)
- "Leg Press" (Workout) + "High Knees" (Warmup, linked to Leg Press)

Action: Remove "Barbell Squat"

Result: Only "Barbell Squat" removed (High Knees still used by Leg Press)
```

## Round Management

### Round Copying
- Copies all exercises from source round to target round
- Generates new GUIDs for all copied exercises
- Preserves all metadata exactly as-is
- Maintains OrderInRound within the target round
- Can copy within same phase or across different phases

### Exercise Reordering
- Changes OrderInRound of target exercise
- Automatically adjusts order of other exercises in the round
- Maintains proper sequence without gaps
- Only affects exercises within the same round

## Migration from Legacy System

### Key Differences from Legacy System

| Aspect | Legacy System | New System |
|--------|---------------|------------|
| Organization | Zone-based (Warmup, Main, Cooldown) | Phase-based with rounds |
| Metadata | SetConfiguration entities | JSON metadata |
| Auto-linking | Manual | Automatic via ExerciseLinks |
| Structure | Flat list per zone | Hierarchical (Phase → Round → Exercise) |
| GUIDs | Sequential IDs | True GUIDs with uniqueness |

### Migration Steps
1. **Data Migration**: Convert existing zones to phases, create initial rounds
2. **Metadata Migration**: Convert SetConfiguration to JSON format
3. **API Migration**: Update client applications to use v2 endpoints
4. **Link Creation**: Establish ExerciseLink relationships for auto-linking
5. **Testing**: Verify all functionality with comprehensive test suite

## Performance Considerations

### Database Optimizations
- Proper indexing on templateId, phase, roundNumber, orderInRound
- JSON metadata indexing for common query patterns
- AsNoTracking() for read-only operations
- Bulk operations for round copying

### Caching Strategy
- Template exercise lists cached per template
- Cache invalidation on template modifications
- ExecutionProtocol and ExerciseLink data cached eternally

### API Performance
- Hierarchical response structure reduces client-side processing
- Single endpoint for complete template exercise data
- Efficient auto-linking with minimal database queries

## Error Handling

### Validation Errors
- Invalid phase names (must be Warmup, Workout, or Cooldown)
- Invalid round numbers (must be positive)
- Invalid JSON metadata format
- Metadata incompatible with ExecutionProtocol

### Business Logic Errors
- Template not in Draft state for modifications
- Exercise not found or not active
- Duplicate exercise in same round
- Source round not found for copying
- Target round already exists for copying

### Auto-linking Errors
- Failed to add linked exercises
- Circular dependency detection
- Missing ExecutionProtocol information

All errors provide clear, actionable messages stored in WorkoutTemplateExerciseErrorMessages constants.

## Testing Strategy

### Unit Tests
- Service layer logic with mocked dependencies
- Handler classes tested in isolation
- Validation logic comprehensive coverage
- Extension methods and utility functions

### Integration Tests
- End-to-end API testing with real database
- Auto-linking behavior verification
- Orphan cleanup logic validation
- Round management operations

### BDD Tests
- Feature-level acceptance testing
- Real-world scenario coverage
- Cross-functional requirement validation
- User story verification

## Future Enhancements

### Planned Features
1. **Advanced ExecutionProtocol Support**: SUPERSET, DROP_SET, AMRAP metadata
2. **Exercise Dependencies**: Prerequisites and progressions
3. **Template Versioning**: Track changes over time
4. **Advanced Reordering**: Drag-and-drop interface support
5. **Bulk Operations**: Multiple exercise additions/removals
6. **Template Analytics**: Usage patterns and effectiveness metrics

### Extensibility Points
- JSON metadata allows new ExecutionProtocol types
- Handler pattern enables easy feature additions
- Event system for workflow triggers
- Plugin architecture for custom behaviors

## Conclusion

The Workout Template Exercise Management system provides a robust, flexible foundation for complex workout organization. With intelligent auto-linking, orphan cleanup, and comprehensive JSON metadata support, it addresses all current requirements while providing extensibility for future enhancements.

The phase/round structure accommodates any workout style, from simple linear progressions to complex circuit training and superset configurations. The v2 API provides a clean, intuitive interface for client applications while maintaining backward compatibility through the legacy v1 endpoints.