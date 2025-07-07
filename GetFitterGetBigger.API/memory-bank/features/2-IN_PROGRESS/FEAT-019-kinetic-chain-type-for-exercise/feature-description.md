# Feature: Add Kinetic Chain Type to Exercise

## Feature ID: FEAT-019
## Created: 2025-01-07
## Status: SUBMITTED
## Target PI: PI-2025-Q1

## Description
Add a Kinetic Chain Type field to the Exercise entity, following the same pattern as the Difficulty Level field. This field will indicate whether an exercise uses open or closed kinetic chain movement patterns.

## Business Value
- Provides important biomechanical information about exercises
- Helps trainers and clients understand the functional characteristics of movements
- Enables better exercise selection based on rehabilitation or training goals
- Completes the exercise categorization system

## User Stories
- As a Personal Trainer, I want to specify the kinetic chain type when creating exercises so that I can categorize movements by their biomechanical properties
- As a Personal Trainer, I want to see the kinetic chain type in exercise lists so that I can select appropriate exercises for specific training goals
- As a Client, I want to understand whether an exercise is open or closed chain so that I can better understand the movement patterns

## Acceptance Criteria
- [ ] Exercise entity has a KineticChainType field that follows the same pattern as DifficultyLevel
- [ ] KineticChainType is required for non-rest exercises and must be empty for rest exercises
- [ ] Only one KineticChainType can be assigned per exercise
- [ ] All existing Exercise endpoints (GET, POST, PUT) handle the KineticChainType field
- [ ] Database migration adds the new column with appropriate foreign key constraint
- [ ] All existing tests pass and new tests cover the KineticChainType functionality
- [ ] API documentation is updated to reflect the new field

## Technical Specifications
- Add `KineticChainTypeId KineticChainId` property to Exercise entity
- Add `KineticChainType? KineticChain` navigation property
- Update ExerciseDto to include `ReferenceDataDto KineticChain`
- Update CreateExerciseRequest and UpdateExerciseRequest to include `string KineticChainId`
- Configure one-to-many relationship in FitnessDbContext
- Update ExerciseService mapping logic
- Add validation: required for non-rest exercises, must be null for rest exercises

## Dependencies
- KineticChainType reference table already exists in the system
- Exercise entity and related infrastructure are already implemented

## Notes
- Follow the exact same pattern as DifficultyLevel implementation
- Ensure consistency with existing validation patterns
- The KineticChainType reference data already has seed data in the database