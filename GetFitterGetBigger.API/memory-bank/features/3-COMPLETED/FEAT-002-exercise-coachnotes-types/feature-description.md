# Feature: Exercise CoachNotes and TypeOfExercise

## Feature ID: FEAT-002
## Created: June 27, 2025
## Status: COMPLETED
## Completed: June 28, 2025
## Final Commit: 9362a233
## Target PI: PI-2025-Q1

## Overview
Replace the current free-text `Instructions` field in the Exercise entity with a structured `CoachNotes` list and add `TypeOfExercise` classification to better organize and categorize exercises.

## Business Requirements

### CoachNotes
- Replace the single `Instructions` text field with a list of `CoachNote` entities
- Each CoachNote represents a single instruction step
- CoachNotes must maintain a specific order defined by the Personal Trainer
- CoachNotes are optional (exercises like Rest may not need instructions)
- When returning an Exercise, CoachNotes must be returned in the correct order
- CoachNotes are managed as part of the Exercise aggregate (no separate CRUD operations)
- Changes to CoachNotes happen through Exercise Create/Update operations

### TypeOfExercise
- Add exercise classification with types: Warmup, Workout, Cooldown, Rest
- An exercise must have at least one type
- Exercises can have multiple types (e.g., can be used for Warmup, Workout, and Cooldown)
- **Business Rule**: If an exercise is of type "Rest", it cannot have any other types
- Implement as a Reference Table following the standard ReferenceTable process

## Technical Requirements

### Database Changes
1. Create `ExerciseType` reference table with strongly-typed `ExerciseTypeId`
2. Create `CoachNote` entity with:
   - Strongly-typed `CoachNoteId` (format: "coachnote-{guid}")
   - Relationship to Exercise using `ExerciseId`
   - Order field for maintaining sequence
   - Text content for the instruction
3. Create junction table for Exercise-ExerciseType many-to-many relationship
4. Remove `Instructions` field from Exercise table

### ID Requirements
- Create `CoachNoteId` as a strongly-typed ID following the existing pattern
- IDs must be formatted as strings with prefixes (e.g., "coachnote-{guid}", "exercisetype-{guid}")
- All IDs must use the `readonly record struct` pattern in the `SpecializedIds` namespace

### API Changes
- Update Exercise Create endpoint to accept CoachNotes collection
- Update Exercise Update endpoint to handle CoachNotes changes (add/remove/modify)
- Update Exercise Get/List endpoints to return CoachNotes in order
- Update DTOs to include CoachNotes collection and ExerciseTypes
- Implement validation for the Rest exclusivity rule
- Service layer must handle CoachNotes lifecycle within Exercise operations

### Admin Application Requirements
- PT must be able to define and reorder CoachNotes
- PT must be able to select one or more ExerciseTypes
- UI must enforce the Rest exclusivity rule

## Acceptance Criteria
1. Exercise can have zero or more CoachNotes
2. CoachNotes are returned in the order specified by the PT
3. Exercise must have at least one TypeOfExercise
4. Rest type exercises cannot have other types
5. Non-Rest exercises can have any combination of Warmup, Workout, Cooldown
6. All existing tests pass with updated structure
7. New tests cover the business rules

## Out of Scope
- Migration of existing Instructions data to CoachNotes (separate migration task)
- UI implementation in Admin app (separate frontend task)