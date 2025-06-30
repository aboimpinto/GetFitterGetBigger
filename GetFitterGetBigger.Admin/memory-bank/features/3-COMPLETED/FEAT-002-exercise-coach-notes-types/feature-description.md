# Feature: Exercise Coach Notes and Exercise Types

## Feature ID: FEAT-002
## Created: 2025-01-28
## Status: COMPLETED
## Completed: 2025-06-29
## Target PI: PI-2025-Q1

## Description
Add support for Coach Notes (ordered instruction steps) and Exercise Types to the exercise management system. This replaces the single instructions field with an array of ordered coach notes and adds exercise type categorization with specific business rules.

## Business Value
- Provides better structured guidance for exercises with step-by-step instructions
- Enables categorization of exercises by type (Warmup, Workout, Cooldown, Rest)
- Improves user experience by allowing more detailed and organized exercise instructions
- Supports future workout planning features by categorizing exercise types

## User Stories
- As a Personal Trainer, I want to provide step-by-step instructions for exercises so that clients can follow them more easily
- As a Personal Trainer, I want to categorize exercises by type so that I can build better structured workout plans
- As a Personal Trainer, I want to enforce that Rest exercises cannot be combined with other types so that the categorization remains clear

## Acceptance Criteria
- [ ] Coach Notes replace the single instructions field
- [ ] Coach Notes can be reordered with drag-and-drop
- [ ] Each Coach Note has text content and an order value
- [ ] Exercise Types include: Warmup, Workout, Cooldown, Rest
- [ ] Rest type is mutually exclusive with other types
- [ ] At least one exercise type must be selected
- [ ] All four types cannot be selected simultaneously
- [ ] Existing exercises migrate instructions to first coach note
- [ ] API returns coach notes and exercise types in camelCase format

## Technical Specifications
### API Changes
- `instructions` field deprecated in favor of `coachNotes` array
- Each coach note has: `id`, `text`, `order`
- New `exerciseTypes` array field
- Business rule validation for exercise types

### UI Components
- CoachNotesEditor: Drag-and-drop editor for managing coach notes
- ExerciseTypeSelector: Checkbox group with validation rules
- Updated ExerciseForm, ExerciseList, and ExerciseDetail components

### State Management
- Updated DTOs to support new fields
- Builder pattern for all DTO creation
- Updated services to handle new data structure

## Dependencies
- API must be updated to support new fields
- Existing exercises need data migration

## Notes
- Phase 1 (API integration and models) completed with Boy Scout Rule refactoring
- Builder pattern implemented for improved maintainability
- Currently working on Phase 2 (UI components)