# Feature: Workout Template Exercise Management System

## Feature ID: FEAT-031
**Title**: Workout Template Exercise Management with Multi-Type Support  
**Status**: SUBMITTED  
**Priority**: High  
**Created**: 2025-01-07  
**Author**: Paulo Aboim Pinto  

## Description

Complete redesign of the WorkoutTemplate exercise management system to support multiple workout types (Reps & Sets, EMOM, Tabata, etc.) with intelligent exercise linking, round-based organization, and automatic warmup/cooldown management.

## User Stories

### As a Personal Trainer
- I want to create workout templates with different workout types (Reps & Sets, EMOM, Tabata, etc.)
- I want to organize exercises into rounds within warmup, workout, and cooldown phases
- I want the system to automatically add appropriate warmup/cooldown exercises when I add workout exercises
- I want to copy rounds to quickly create progressive workouts
- I want to reorder exercises within rounds for optimal flow
- I want to remove exercises with automatic cleanup of orphaned warmup/cooldowns

### As an API Developer
- I need a flexible data structure that supports multiple workout types
- I need individual endpoints for each operation (add, remove, reorder)
- I need automatic management of exercise relationships via ExerciseLinks

## Acceptance Criteria

### 1. WorkoutType Reference Table
- [ ] Create WorkoutType reference table (cached forever)
- [ ] Initial type: "Reps & Sets" with appropriate validation
- [ ] Each type defines its own data structure and validation rules

### 2. Round-Based Organization
- [ ] Support unlimited rounds per phase (warmup, workout, cooldown)
- [ ] Each round maintains its own exercise order
- [ ] Rounds are performed sequentially
- [ ] Each exercise instance has a unique GUID for identification

### 3. Exercise Management Operations

#### Add Exercise Endpoint
- [ ] Accept: phase, round number, exercise ID, metadata (reps/sets/weight/time)
- [ ] For workout exercises: auto-add linked warmup/cooldown from ExerciseLinks
- [ ] For warmup/cooldown: add only the specified exercise
- [ ] Prevent duplicate warmup/cooldown exercises across rounds
- [ ] Return success with list of all added exercises

#### Remove Exercise Endpoint
- [ ] Remove exercise by GUID
- [ ] Auto-remove orphaned warmup/cooldown exercises
- [ ] Only remove if no other workout exercise uses them

#### Reorder Exercise Endpoint
- [ ] Reorder within a round
- [ ] Update order for all affected exercises

#### Copy Round Endpoint
- [ ] Copy all exercises with metadata
- [ ] Generate new GUIDs for copied instances
- [ ] Allow copying between phases

### 4. Data Structure Requirements

#### For "Reps & Sets" WorkoutType:
- Support weight (based on ExerciseWeightTypes)
- Support reps count
- Support time duration
- Support rest periods (REST exercise type)
- All exercises must have metadata (no empty parameters)

### 5. Validation Rules
- [ ] No exercise without metadata
- [ ] REST exercises only accept time parameter
- [ ] Validate based on WorkoutType rules
- [ ] Templates can be saved without exercises (draft state)

## Technical Design

### Database Schema Approach

**Option 1: Flexible Single Table** (Recommended)
```sql
WorkoutTemplateExercises
- Id (GUID) - Unique identifier for each instance
- WorkoutTemplateId
- Phase (Warmup/Workout/Cooldown)
- RoundNumber
- OrderInRound
- ExerciseId
- Metadata (JSON) - Flexible structure per WorkoutType
- CreatedAt
- UpdatedAt
```

**Option 2: Multiple Tables per WorkoutType**
- More complex but strongly typed
- Requires new tables for each WorkoutType

### API Endpoints

```
POST /api/workout-templates/{templateId}/exercises
- Add exercise with auto-linking

DELETE /api/workout-templates/{templateId}/exercises/{exerciseGuid}
- Remove exercise with cleanup

PUT /api/workout-templates/{templateId}/exercises/{exerciseGuid}/order
- Reorder exercise within round

POST /api/workout-templates/{templateId}/rounds/copy
- Copy round with all exercises

GET /api/workout-templates/{templateId}/exercises
- Get all exercises organized by phase and round
```

### Exercise Auto-Linking Logic

1. When adding workout exercise:
   - Query ExerciseLinks for warmup/cooldown
   - Check if already exists in template
   - Add only missing exercises
   - Place in appropriate phase

2. When removing workout exercise:
   - Check if warmup/cooldown used by others
   - Remove orphaned exercises
   - Maintain order integrity

### Example JSON Structure

```json
{
  "templateId": "guid",
  "name": "Leg Burning I",
  "workoutType": "Reps & Sets",
  "phases": {
    "warmup": {
      "rounds": [
        {
          "roundNumber": 1,
          "exercises": [
            {
              "id": "guid1",
              "exerciseId": 123,
              "name": "High Knees Running In Place",
              "order": 1,
              "metadata": {
                "time": "20s"
              }
            },
            {
              "id": "guid2",
              "exerciseId": 124,
              "name": "Squats",
              "order": 2,
              "metadata": {
                "reps": 10
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
              "id": "guid3",
              "exerciseId": 125,
              "name": "Barbell Squat",
              "order": 1,
              "metadata": {
                "weight": "0kg",
                "reps": 20
              }
            },
            {
              "id": "guid4",
              "exerciseId": 999,
              "name": "Rest",
              "order": 2,
              "metadata": {
                "time": "1m"
              }
            }
          ]
        }
      ]
    }
  }
}
```

## Implementation Strategy

### Phase 1: Foundation
1. Create WorkoutType reference table
2. Design and implement flexible exercise storage
3. Implement basic CRUD operations

### Phase 2: Core Features
1. Add exercise with auto-linking
2. Remove exercise with cleanup
3. Reorder functionality
4. Round copying

### Phase 3: Validation & Polish
1. Implement WorkoutType-specific validation
2. Add comprehensive error handling
3. Optimize queries for performance

## Dependencies
- ExerciseLinks table (existing)
- Exercise table (existing)
- WorkoutTemplate table (existing)
- Will require refactoring existing WorkoutTemplateExercise code

## Migration Considerations
- Existing WorkoutTemplateExercise data needs migration
- Backward compatibility during transition
- Admin app will need updates to support new structure

## Estimated Effort: XL
- Complex business logic
- Multiple endpoints
- Significant refactoring required
- Database schema changes

## Success Metrics
- PT can create templates 50% faster
- Zero duplicate warmup/cooldown exercises
- All operations complete in <200ms
- 100% test coverage for auto-linking logic

## Questions Resolved
All design questions have been clarified and documented above.

## Notes
- REST is treated as a special exercise type
- All exercises must have metadata
- Focus on "Reps & Sets" for initial implementation
- Other WorkoutTypes (EMOM, Tabata) will follow same pattern