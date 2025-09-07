# Implementation Notes: FEAT-031

This document consolidates all clarifications and implementation guidance for FEAT-031.

## Key Clarifications from Requirements Review

### 1. No Database Migration Needed - Fresh Start
- **IMPORTANT**: The existing WorkoutTemplateExercise table was never used (initial implementation was wrong)
- We will DROP the existing table and CREATE a new one - NO MIGRATION NEEDED
- No data exists in the current table, so no data loss concerns
- This is a fresh implementation to replace the flawed initial attempt
- No rollback needed since we're starting fresh

### 2. ExecutionProtocolId Addition
- **This is one of the implementation tasks** - not a prerequisite
- WorkoutTemplate entity doesn't have ExecutionProtocolId yet - we will add it
- Part of Phase 1 implementation work
- Also adding ExecutionProtocolConfig field for protocol-specific settings

### 3. WorkoutState IS the Template Lifecycle
- WorkoutStateId in WorkoutTemplate entity provides the lifecycle states
- States: Draft, Production, Archived (via WorkoutState entity)
- No separate lifecycle mechanism needed
- Templates can only move to Production if all exercises are Active

### 4. REST Exercise Handling
- REST is a NORMAL exercise with ExerciseType = REST
- Implementation will find or create a REST exercise in the Exercise table
- NOT a hardcoded special ID (examples use 999 for illustration only)
- Only accepts duration in metadata
- Treated like any other exercise in the system

### 5. Auto-Linking Clarifications
- Warmup exercises CANNOT have their own warmups (no recursive chains)
- No circular dependency issues possible
- A workout exercise CAN appear as its own warmup (valid scenario)
- Auto-linking is one-directional: Workout → Warmup/Cooldown only

### 6. Round Copy Operation
- Copy creates a NEW round at the target number
- Target round doesn't exist before the copy operation
- All exercises get new GUIDs in the copied round

### 7. Validation Timing
- Metadata validation happens at Controller/Service layer
- Validation occurs on save operations (add/update)
- Different validation strategies per ExecutionProtocol
- Empty metadata `{}` is valid for auto-added exercises

### 8. Single PT Constraints
- No concurrent editing concerns (single PT, single session)
- No need for optimistic concurrency control yet
- Future feature will handle multi-PT scenarios

### 9. Performance Scope
- Large template support (100+ exercises) is a future feature
- Current focus: Core exercise management functionality
- Pagination will be added when needed

## Implementation Priorities

### Phase 1: Database Setup (Fresh Start)
1. Rename ExecutionProtocol "Standard" to "Reps and Sets" (if needed)
2. Add ExecutionProtocolId and ExecutionProtocolConfig to WorkoutTemplate entity
3. DROP existing WorkoutTemplateExercise table and CREATE new one with:
   - Phase string (instead of WorkoutZone enum)
   - RoundNumber and OrderInRound (instead of SequenceOrder)
   - JSON Metadata field (instead of SetConfiguration collection)

### Phase 2: Core Functionality
1. Implement Add Exercise with auto-linking
2. Implement Remove Exercise with orphan cleanup
3. Implement Reorder within rounds
4. Implement Copy Round functionality
5. Implement Update Metadata endpoint

### Phase 3: Validation & Polish
1. Create validation strategies per ExecutionProtocol
2. Implement WorkoutState enforcement
3. Add comprehensive error handling
4. Optimize queries with proper indexes

## Technical Decisions

### Entity Changes
- Create NEW WorkoutTemplateExercise entity (drop the old one)
- Use Phase string (not WorkoutZone enum)
- Add RoundNumber and OrderInRound properties
- Use JSON Metadata field (not SetConfiguration collection)

### Service Layer
- Create IWorkoutTemplateExerciseService interface
- Implement auto-linking logic in service layer
- Use strategy pattern for metadata validation
- Handle all business rules in service, not repository

### API Design
- Individual endpoints for each operation (not bulk)
- Return detailed results showing all affected exercises
- Use consistent error codes and messages
- Follow existing API patterns in codebase

## Testing Strategy

### Unit Tests
- Test auto-linking logic thoroughly
- Test orphan detection scenarios
- Test metadata validation per protocol
- Test round management operations

### Integration Tests
- Test full exercise addition flow
- Test cascading deletions
- Test state transitions
- Test concurrent operations (future)

### Test Scenarios (Gherkin Format)
```gherkin
Feature: Auto-linking warmup exercises

Scenario: Adding workout exercise triggers warmup addition
  Given a workout template in Draft state
  And "Barbell Squat" has "High Knees" as warmup in ExerciseLinks
  When I add "Barbell Squat" to the workout phase
  Then "High Knees" should be added to warmup phase
  And both exercises should have unique GUIDs

Scenario: Removing workout exercise cleans up orphaned warmups
  Given a workout template with "Barbell Squat" in workout phase
  And "High Knees" in warmup phase (auto-added)
  And no other workout exercise uses "High Knees" as warmup
  When I remove "Barbell Squat"
  Then "High Knees" should also be removed
```

## Migration/Rollback Considerations
- **Not Applicable**: This is a fresh implementation, not a migration
- No existing data to migrate (table was never used)
- No rollback needed (nothing to roll back to)
- No backward compatibility concerns (fresh start)

## Future Enhancements (Out of Scope)
- Multiple PT support and permissions
- Concurrent editing handling
- Large template pagination
- Bulk operations
- Advanced caching strategies
- Template versioning

## Important Note on Documentation Level

This documentation is created from a **Product Owner perspective**, focusing on:
- **WHAT** needs to be built (business requirements)
- **WHY** it's needed (business value)
- **WHO** will use it (Personal Trainers)

Implementation details like DTOs, service interfaces, repository patterns, etc., are **developer responsibilities** to be determined during implementation based on these requirements.

## Summary
This feature is ready for implementation with all clarifications addressed. The documentation provides sufficient business requirements to create implementation tasks. Technical details (HOW to implement) will be determined by developers during the implementation phase.