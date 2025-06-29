# Feature: REST Exercise Optional Muscle Groups

## Feature ID: FEAT-015
## Created: 2025-06-29
## Status: READY_TO_DEVELOP
## Target PI: PI-2025-Q3

## Description
Enable REST exercises to be created and updated without requiring muscle groups, equipment, body parts, or movement patterns. REST exercises represent recovery periods in workout plans and are fundamentally different from other exercise types (Warmup, Workout, Cooldown) as they don't target specific muscles or require equipment.

## Business Value
- **Accurate Workout Representation**: Allows proper modeling of rest periods in workout plans without forcing artificial muscle group assignments
- **User Experience**: Eliminates confusion when creating REST exercises in the Admin application
- **Data Integrity**: Maintains clean separation between active exercises and rest periods
- **API Consistency**: Provides logical and intuitive API behavior for different exercise types

## User Stories
- As a Personal Trainer, I want to create REST exercises without specifying muscle groups so that I can accurately represent rest periods in workout plans
- As a Personal Trainer, I want to update existing exercises to REST type without being forced to remove muscle groups manually
- As an API consumer, I want consistent validation rules where REST exercises don't require attributes that don't apply to rest periods
- As a developer, I want the API to validate muscle groups only when they are conceptually relevant to the exercise type

## Acceptance Criteria
- [ ] REST exercises can be created without muscle groups, equipment, body parts, or movement patterns
- [ ] Non-REST exercises still require muscle groups (existing validation preserved)
- [ ] Existing REST exclusivity rule continues to work (REST cannot be combined with other types)
- [ ] API returns proper validation errors when non-REST exercises lack muscle groups
- [ ] All existing exercise functionality remains intact
- [ ] 100% test coverage for new REST exercise scenarios
- [ ] Documentation reflects REST exercise special behavior

## Technical Specifications

### Current State
- All exercises currently require muscle groups through DTO validation: `[Required]` and `[MinLength(1)]`
- REST exercises have special business rule: must be exclusive (cannot combine with other types)
- ExerciseService.ValidateRestExclusivityAsync() already handles REST exclusivity

### Required Changes
1. **DTO Validation**: Replace `[Required]` with conditional validation for muscle groups
2. **Custom Validation Attribute**: Create `ConditionalRequiredMuscleGroupsAttribute`
3. **Service Layer**: Update validation logic to allow empty muscle groups for REST exercises
4. **Testing**: Comprehensive test coverage for all scenarios

### API Behavior
- `POST /exercises` with REST type and no muscle groups: Success
- `PUT /exercises/{id}` with REST type and no muscle groups: Success  
- `POST /exercises` with non-REST type and no muscle groups: 400 Bad Request
- `PUT /exercises/{id}` with non-REST type and no muscle groups: 400 Bad Request

## Dependencies
- No blocking dependencies
- Leverages existing REST exclusivity validation logic
- Works with current Exercise entity structure

## Notes
- This is a new feature, not a bug fix, as optional muscle groups for REST exercises was never implemented
- Implementation must maintain backward compatibility
- Feature will immediately benefit the Admin application's exercise CRUD functionality
- Should be tested with existing exercise test data to ensure no regressions