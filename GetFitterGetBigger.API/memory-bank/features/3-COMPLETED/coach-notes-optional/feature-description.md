# Feature: Coach Notes Optional for Exercises

## Feature ID: FEAT-016
## Created: 2025-06-29
## Status: READY_TO_DEVELOP
## Target PI: PI-2025-Q3

## Description
Make coach notes completely optional when creating or updating exercises. Currently, the system appears to require at least one coach note, but exercises should be able to exist without any coach notes. Coach notes are supplementary instructions that enhance an exercise description but are not mandatory.

## Business Value
- **Flexibility**: Allows creation of simple exercises that don't need detailed step-by-step instructions
- **User Experience**: Reduces friction when creating exercises by not forcing unnecessary data entry
- **Data Integrity**: Ensures the system accurately reflects that some exercises don't require coach notes
- **API Usability**: Makes the API more intuitive by having truly optional fields be optional

## User Stories
- As a Personal Trainer, I want to create exercises without coach notes when they are self-explanatory
- As a Personal Trainer, I want to add coach notes only when they add value to the exercise
- As an API consumer, I want optional fields to truly be optional without hidden requirements

## Acceptance Criteria
- [ ] Exercises can be created without any coach notes
- [ ] Exercises can be updated to have zero coach notes
- [ ] Existing exercises with coach notes continue to work
- [ ] API accepts empty coach notes array or null
- [ ] No validation errors when coach notes are omitted
- [ ] Documentation reflects that coach notes are optional

## Technical Specifications

### Current State
- DTOs have `CoachNotes` property as a List that defaults to empty
- Some validation or business logic appears to require at least one coach note
- Need to identify where this requirement is enforced

### Required Changes
1. **Validation**: Remove any validation requiring coach notes
2. **Service Layer**: Ensure empty coach notes collections are handled properly
3. **Database**: Verify no constraints require coach notes
4. **Testing**: Add tests for exercises without coach notes

## Dependencies
- No blocking dependencies
- Works with existing Exercise entity structure
- Related to exercise management feature

## Notes
- This is a bug fix disguised as a feature since coach notes were always intended to be optional
- Should be a quick fix once the validation requirement is located
- Important for API usability and user experience