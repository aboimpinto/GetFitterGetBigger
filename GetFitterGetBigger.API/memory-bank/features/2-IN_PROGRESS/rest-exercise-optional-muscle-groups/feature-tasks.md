# REST Exercise Optional Muscle Groups Implementation Tasks

## Feature Branch: `feature/exercise-management` (already exists)

### Task Categories

#### Phase 1: Core Validation Logic
#### Phase 2: Database and Entity Updates  
#### Phase 3: Comprehensive Testing
#### Phase 4: Validation and Documentation

### Progress Tracking
- All tasks start as `[ReadyToDevelop]`
- Update to `[Implemented: <commit-hash>]` when complete
- Use `[Blocked: reason]` if blocked

### Tasks

#### Phase 1: Core Validation Logic

1. **[Implemented: 622577e0] Create Custom Validation Attribute**
   - Create `ConditionalRequiredMuscleGroupsAttribute` for REST exercise validation
   - Implement logic to check if "Rest" is in exercise types before requiring muscle groups
   - Handle case-insensitive "Rest" detection
   - **Files**: `Attributes/ConditionalRequiredMuscleGroupsAttribute.cs`
   - **Tests**: `Tests/Unit/Attributes/ConditionalRequiredMuscleGroupsAttributeTests.cs`

2. **[Implemented: 622577e0] Update CreateExerciseRequest DTO**
   - Remove `[Required]` and `[MinLength(1)]` from MuscleGroups property
   - Add `[ConditionalRequiredMuscleGroups]` attribute
   - Ensure other validations remain intact
   - **Files**: `DTOs/CreateExerciseRequest.cs`

3. **[Implemented: 622577e0] Update UpdateExerciseRequest DTO**
   - Remove `[Required]` and `[MinLength(1)]` from MuscleGroups property
   - Add `[ConditionalRequiredMuscleGroups]` attribute
   - Ensure other validations remain intact
   - **Files**: `DTOs/UpdateExerciseRequest.cs`

4. **[ReadyToDevelop] Update Service Layer Validation**
   - Modify `ExerciseService.CreateAsync()` to allow empty muscle groups for REST exercises
   - Modify `ExerciseService.UpdateAsync()` to allow empty muscle groups for REST exercises
   - Ensure REST exclusivity rule still applies (REST can only be REST)
   - Add validation messages for clarity
   - **Files**: `Services/Implementations/ExerciseService.cs`

#### Phase 2: Database and Entity Updates

5. **[ReadyToDevelop] Review Database Constraints**
   - Verify no database-level constraints prevent REST exercises without muscle groups
   - Check junction table relationships allow zero records for REST exercises
   - Ensure ExerciseMuscleGroup table allows no records for specific exercises
   - **Files**: Database migration files (if needed), Entity configurations

6. **[ReadyToDevelop] Update Entity Validation (if needed)**
   - Review Exercise entity Handler methods for any hardcoded muscle group requirements
   - Ensure entity creation allows optional muscle groups
   - Verify navigation properties handle empty collections correctly
   - **Files**: `Models/Entities/Exercise.cs`

#### Phase 3: Comprehensive Testing

7. **[Implemented: 3dd0bd5f] Unit Tests - Custom Validation Attribute**
   - Test `ConditionalRequiredMuscleGroupsAttribute` with REST exercise types
   - Test attribute with non-REST exercise types  
   - Test various exercise type combinations
   - Test case sensitivity for "Rest" type detection
   - Test null/empty exercise types scenarios
   - **Files**: `Tests/Unit/Attributes/ConditionalRequiredMuscleGroupsAttributeTests.cs`

8. **[ReadyToDevelop] Unit Tests - DTO Validation**
   - Test `CreateExerciseRequest` allows REST exercises without muscle groups
   - Test `CreateExerciseRequest` requires muscle groups for non-REST exercises
   - Test `UpdateExerciseRequest` allows REST exercises without muscle groups
   - Test `UpdateExerciseRequest` requires muscle groups for non-REST exercises
   - Test edge cases with mixed exercise types
   - **Files**: `Tests/Unit/DTOs/CreateExerciseRequestTests.cs`, `Tests/Unit/DTOs/UpdateExerciseRequestTests.cs`

9. **[ReadyToDevelop] Unit Tests - Service Layer**
   - Test `ExerciseService.CreateAsync()` with REST exercise and no muscle groups
   - Test `ExerciseService.UpdateAsync()` with REST exercise and no muscle groups
   - Test error cases: non-REST exercises still require muscle groups
   - Test REST exclusivity rule still works
   - Test service-level validation messages
   - **Files**: `Tests/Unit/Services/ExerciseServiceTests.cs`

10. **[ReadyToDevelop] Integration Tests - API Endpoints**
    - Test POST /exercises with REST exercise and no muscle groups (success)
    - Test PUT /exercises/{id} with REST exercise and no muscle groups (success)
    - Test POST /exercises with non-REST exercise and no muscle groups (400 error)
    - Test PUT /exercises/{id} with non-REST exercise and no muscle groups (400 error)
    - Verify GET endpoints return REST exercises correctly
    - Test complete REST exercise lifecycle
    - **Files**: `Tests/Integration/Controllers/ExerciseControllerTests.cs`

#### Phase 4: Validation and Documentation

11. **[ReadyToDevelop] Manual Testing Workflows**
    - Test complete workflow: create REST exercise via API without muscle groups
    - Test complete workflow: update existing exercise to REST type  
    - Test complete workflow: update REST exercise to non-REST type (should require muscle groups)
    - Verify Swagger documentation reflects optional muscle groups for REST
    - Test edge cases and error scenarios manually

12. **[ReadyToDevelop] Regression Testing**
    - Run full test suite to ensure no existing functionality broken: `dotnet test`
    - Test existing exercise creation/update still works for non-REST exercises
    - Test existing muscle group validation still works for non-REST exercises
    - Verify no performance regressions in exercise operations
    - Test with existing exercise test data

13. **[ReadyToDevelop] Documentation Updates**
    - Update API documentation to reflect REST exercise special behavior
    - Update memory bank with REST exercise implementation details
    - Document the conditional validation pattern for future reference
    - Update Swagger/OpenAPI specs if needed
    - **Files**: `/api-docs/exercises.md`, memory bank files

## Technical Implementation Notes

### Key Files to Modify
- `Attributes/ConditionalRequiredMuscleGroupsAttribute.cs` - New custom validation
- `Models/DTOs/CreateExerciseRequest.cs` - Replace [Required] with conditional validation
- `Models/DTOs/UpdateExerciseRequest.cs` - Replace [Required] with conditional validation  
- `Services/Implementations/ExerciseService.cs` - Update validation logic

### Validation Strategy
The solution will use a custom validation attribute that:
1. Checks if "Rest" is present in the exercise types (case-insensitive)
2. If REST is present, muscle groups become optional
3. If REST is not present, muscle groups remain required
4. Leverages existing REST exclusivity rule (REST cannot be combined with other types)
5. Provides clear validation error messages

### Testing Priority
1. **Critical**: REST exercises can be created without muscle groups
2. **Critical**: Non-REST exercises still require muscle groups  
3. **Critical**: Existing REST exclusivity rule continues to work
4. **Important**: All existing exercise functionality remains intact
5. **Important**: Comprehensive error message validation

## Definition of Done
- [ ] All 13 tasks implemented with commit hashes
- [ ] REST exercises can be created/updated without muscle groups
- [ ] Non-REST exercises still require muscle groups (no regression)
- [ ] All existing tests pass (100% green): `dotnet test`
- [ ] New comprehensive test coverage for REST exercise scenarios
- [ ] Manual testing confirms expected API behavior
- [ ] Swagger documentation updated
- [ ] Memory bank documentation updated
- [ ] No breaking changes to existing exercise functionality

## Dependencies and Blockers
- **No blocking dependencies**
- **Leverages existing**: REST exclusivity validation logic in ExerciseService
- **Benefits from**: Current exercise entity structure and relationships
- **Compatible with**: Existing Admin project exercise CRUD implementation