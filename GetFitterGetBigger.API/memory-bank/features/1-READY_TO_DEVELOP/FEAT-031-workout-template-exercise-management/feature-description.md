# Feature: Workout Template Exercise Management System

## Feature ID: FEAT-031
**Title**: Workout Template Exercise Management with Multi-Type Support  
**Status**: SUBMITTED  
**Priority**: High  
**Created**: 2025-01-07  
**Author**: Paulo Aboim Pinto  

## Description

Complete redesign of the WorkoutTemplate exercise management system to support multiple execution protocols (REPS_AND_SETS, SUPERSET, AMRAP, etc.) with intelligent exercise linking, round-based organization, and automatic warmup/cooldown management. Integrates with existing ExecutionProtocol entity while using flexible JSON metadata for infinite extensibility.

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

### 1. ExecutionProtocol Integration
- [ ] Link WorkoutTemplate to existing ExecutionProtocol entity
- [ ] Add ExecutionProtocolConfig field for protocol-specific settings
- [ ] Support existing protocols: REPS_AND_SETS (renamed from STANDARD), SUPERSET, DROP_SET, AMRAP
- [ ] Plan for future protocols: EMOM, TABATA, CIRCUIT, LADDER

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

#### For REPS_AND_SETS ExecutionProtocol:
- Support weight (based on ExerciseWeightTypes)
- Support reps count
- Support time duration
- Support rest periods (REST as special exercise)
- All exercises must have metadata (no empty parameters)
- No validation of exercise capabilities (handled by future FEAT-032)

### 5. Validation Rules
- [ ] No exercise without metadata
- [ ] REST exercises only accept time parameter
- [ ] Validate based on WorkoutType rules
- [ ] Templates can be saved without exercises (draft state)

## Business Rules

### Template Lifecycle States
1. **Draft**: Template can be fully edited, exercises added/removed/modified
2. **Production**: Template is locked, only description and media can be updated
3. **Archived**: Template cannot be modified, can still be referenced by historical data

### Exercise Lifecycle Rules
1. **Active Exercises**: Can be added to draft templates
2. **Archived Exercises**: Cannot be added to new templates, but remain in existing ones
3. **Soft Delete**: Exercises are never hard deleted, only marked as IsDeleted
4. **Production Lock**: Once in production, exercise core properties cannot change

### Round Management Rules
1. **Empty Rounds**: Allowed during template customization
2. **Round Limit**: No hard limit on number of rounds (UI may impose soft limits)
3. **Round Renumbering**: When deleting a round, subsequent rounds are renumbered sequentially
   - Example: Delete round 2 → round 3 becomes round 2, round 4 becomes round 3, etc.
4. **Round Numbering**: Always sequential starting from 1, no gaps allowed

### Permissions (Current Phase)
- Single PT has full access to all templates
- Future phases will introduce role-based permissions

## Technical Design

### Database Schema Approach

**Adopted Design: Flexible Single Table**
```sql
WorkoutTemplateExercise
- Id (GUID) - Unique identifier for each instance
- WorkoutTemplateId
- Phase (Warmup/Workout/Cooldown)
- RoundNumber
- OrderInRound
- ExerciseId
- Metadata (JSON) - Flexible structure per ExecutionProtocol
- CreatedAt
- UpdatedAt
```

**Integration Points**:
- ExecutionProtocol (existing) - Defines workout structure
- ExerciseLinks (existing) - Manages warmup/cooldown relationships

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
  "executionProtocol": "REPS_AND_SETS",
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
1. **Rename "Standard" to "Reps and Sets" in ExecutionProtocol** (First task!)
2. Add ExecutionProtocolId and Config to WorkoutTemplate
3. Design and implement flexible exercise storage
4. Implement basic CRUD operations

### Phase 2: Core Features (REPS_AND_SETS Protocol)
1. Add exercise with auto-linking via ExerciseLinks
2. Remove exercise with orphan cleanup
3. Reorder functionality
4. Round copying

### Phase 3: Validation & Polish
1. Implement ExecutionProtocol-specific validation
2. Add comprehensive error handling
3. Optimize queries for performance

## Dependencies
- ExecutionProtocol entity (existing, fully integrated)
- ExerciseLinks table (existing, for auto-linking)
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
- REST is a normal exercise with ExerciseType = REST (not a hardcoded special ID)
- All exercises must have metadata
- Focus on REPS_AND_SETS protocol for initial implementation
- Leverage existing SUPERSET, DROP_SET, AMRAP protocols
- Future protocols (EMOM, TABATA) will follow same pattern
- Integration with ExecutionProtocol from FEAT-028
- Exercise metric validation moved to separate FEAT-032