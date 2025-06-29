# Coach Notes Optional Implementation Tasks

## Feature Branch: `feature/exercise-management` (already exists)

### Task Categories
#### Phase 1: Investigation and Analysis
#### Phase 2: Implementation
#### Phase 3: Testing

### Progress Tracking
- All tasks start as `[ReadyToDevelop]`
- Update to `[Implemented: <commit-hash>]` when complete
- Use `[Blocked: reason]` if blocked

### Tasks

#### Phase 1: Investigation and Analysis

1. **[ReadyToDevelop] Investigate Coach Notes Validation**
   - Search for any validation requiring coach notes in DTOs
   - Check ExerciseService for coach notes requirements
   - Review Exercise entity for any constraints
   - Identify where the requirement is enforced
   - **Files**: DTOs, Services, Entities

2. **[ReadyToDevelop] Review Database Schema**
   - Check if database has any constraints on coach notes
   - Verify junction table allows zero records
   - Ensure no NOT NULL constraints exist
   - **Files**: Migrations, Entity configurations

#### Phase 2: Implementation

3. **[ReadyToDevelop] Remove Coach Notes Validation (if found)**
   - Remove any [Required] or [MinLength] attributes
   - Remove any service-level validation
   - Ensure null and empty collections are handled
   - **Files**: Identified during investigation

4. **[ReadyToDevelop] Update Service Layer Handling**
   - Ensure CreateAsync handles null/empty coach notes
   - Ensure UpdateAsync handles null/empty coach notes
   - Verify coach notes synchronization logic works with empty collections
   - **Files**: `Services/Implementations/ExerciseService.cs`

#### Phase 3: Testing

5. **[ReadyToDevelop] Unit Tests - Create Exercise Without Coach Notes**
   - Test creating exercise with null coach notes
   - Test creating exercise with empty coach notes array
   - Test service accepts exercises without coach notes
   - **Files**: `Tests/Unit/Services/ExerciseServiceTests.cs`

6. **[ReadyToDevelop] Unit Tests - Update Exercise Coach Notes**
   - Test updating exercise to remove all coach notes
   - Test updating exercise from having coach notes to none
   - Test edge cases with coach notes updates
   - **Files**: `Tests/Unit/Services/ExerciseServiceTests.cs`

7. **[ReadyToDevelop] Integration Tests - API Endpoints**
   - Test POST /exercises without coach notes
   - Test PUT /exercises/{id} removing coach notes
   - Test GET /exercises returns exercises without coach notes correctly
   - **Files**: `Tests/Integration/Controllers/ExerciseControllerTests.cs`

8. **[ReadyToDevelop] Manual Testing**
   - Create exercise via API without coach notes
   - Update existing exercise to remove coach notes
   - Verify no validation errors occur
   - Test with various combinations

## Technical Notes

### Expected Behavior
- `CoachNotes` property can be:
  - `null` (treated as empty collection)
  - Empty array `[]`
  - Array with coach notes

### Common Validation Patterns to Look For
- `[Required]` attribute on CoachNotes
- `[MinLength(1)]` attribute
- Service-level checks like `if (!request.CoachNotes.Any())`
- Entity validation in handlers

## Definition of Done
- [ ] All tasks implemented with commit hashes
- [ ] Exercises can be created without coach notes
- [ ] Exercises can be updated to have no coach notes
- [ ] All existing tests pass (100% green)
- [ ] New tests cover coach notes optional scenarios
- [ ] Manual testing confirms expected behavior
- [ ] No breaking changes to existing functionality

## Dependencies
- None - this is a standalone fix
- Part of the exercise management feature set