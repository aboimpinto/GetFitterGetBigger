# Exercise Updates - Coach Notes & Exercise Types Implementation Tasks

## Feature Branch: `feature/exercise-coach-notes-types`

## MANDATORY RULES:
- **After completing each phase/category below:**
  - ✅ Run `dotnet build` - BUILD MUST BE SUCCESSFUL (no errors)
  - ✅ Run `dotnet test` - ALL TESTS MUST BE GREEN (no failures)
  - ✅ Resolve all compiler warnings (nullable references, unused variables, etc.)
  - ❌ DO NOT proceed to next phase if build fails or tests are red
  - ❌ Fix all issues before moving forward (unless user explicitly asks to skip)

### Phase 1: API Model & Service Updates
- **Task 1.1:** Update Exercise models to include coachNotes array and exerciseTypes array `[Implemented: 6d54bc51]`
- **Task 1.2:** Update ExerciseService to handle new API response structure with camelCase fields `[Implemented: 6d54bc51]`
- **Task 1.3:** Write unit tests for updated ExerciseService with new data structures `[Implemented: 862beec9]`
- **Task 1.4:** Update ReferenceDataService to fetch exercise types if needed `[Implemented: 6d54bc51]`

**CHECKPOINT 1:** ✅ `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 2: Coach Notes Editor Component
- **Task 2.1:** Create CoachNotesEditor component with add/remove/reorder functionality `[Implemented: 511fe2a6]`
- **Task 2.2:** Implement drag-and-drop or move up/down buttons for reordering `[Implemented: 511fe2a6]`
- **Task 2.3:** Add character limit (1000) validation for each note `[Implemented: 511fe2a6]`
- **Task 2.4:** Write component tests for CoachNotesEditor `[Implemented: 511fe2a6]`
- **Task 2.5:** Test reordering functionality and validation `[Implemented: 511fe2a6]`

**CHECKPOINT 2:** ✅ `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 3: Exercise Type Selector Component
- **Task 3.1:** Create ExerciseTypeSelector component with checkbox interface `[Implemented: 6cbc0e66]`
- **Task 3.2:** Implement Rest type exclusivity validation (cannot combine with other types) `[Implemented: 6cbc0e66]`
- **Task 3.3:** Enforce at least one type must be selected rule `[Implemented: 6cbc0e66]`
- **Task 3.4:** Prevent selection of all four types `[Implemented: 6cbc0e66]`
- **Task 3.5:** Write component tests for ExerciseTypeSelector `[Implemented: 6cbc0e66]`
- **Task 3.6:** Test all validation rules and edge cases `[Implemented: 6cbc0e66]`

**CHECKPOINT 3:** ✅ `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 4: Form Integration
- **Task 4.1:** Update ExerciseForm to replace instructions field with CoachNotesEditor `[Implemented: 49f67695]`
- **Task 4.2:** Integrate ExerciseTypeSelector into ExerciseForm `[Implemented: 49f67695]`
- **Task 4.3:** Update form validation to include new business rules `[Implemented: 49f67695]`
- **Task 4.4:** Update form submission to send coachNotes and exerciseTypes in correct format `[Implemented: 49f67695]`
- **Task 4.5:** Write tests for updated ExerciseForm `[Implemented: 407538ad + current commit]`

**CHECKPOINT 4:** ✅ `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 5: List & Detail Updates
- **Task 5.1:** Update ExerciseList to display exercise types as colored badges `[Implemented: 89f872a7]`
- **Task 5.2:** Add isActive status indicator to exercise list rows `[Already existed in codebase]`
- **Task 5.3:** Implement isActive filter dropdown in ExerciseList `[Implemented: 89f872a7]`
- **Task 5.4:** Write tests for ExerciseList updates `[Implemented: b0f34c2e]`
- **Task 5.5:** Update ExerciseDetail to display coach notes as ordered list `[Implemented: 89f872a7]`
- **Task 5.6:** Update ExerciseDetail to show exercise types with badges `[Implemented: 89f872a7]`
- **Task 5.7:** Write tests for ExerciseDetail updates `[Implemented: b0f34c2e]`

**CHECKPOINT 5:** ✅ `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 6: Field Name Updates
- **Task 6.1:** Update all model properties to use camelCase (isUnilateral, imageUrl, videoUrl, etc.) `[Implemented: JSON serialization config]`
- **Task 6.2:** Update API request/response handling for camelCase conversion `[Implemented: Added PropertyNamingPolicy.CamelCase to services]`
- **Task 6.3:** Search and replace any remaining snake_case references `[Completed: No snake_case found]`
- **Task 6.4:** Test all CRUD operations with updated field names `[Completed: All 156 tests passing]`

**CHECKPOINT 6:** ✅ `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 7: State Management Updates
- **Task 7.1:** Update ExerciseStateService to handle coach notes array `[Already implemented in codebase]`
- **Task 7.2:** Update ExerciseStateService to handle exercise types `[Already implemented in codebase]`
- **Task 7.3:** Add isActive filtering to state management `[Already implemented in codebase]`
- **Task 7.4:** Write tests for updated state management `[Implemented: Enhanced ExerciseStateServiceTests]`

**CHECKPOINT 7:** ✅ `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 8: Manual Testing Fixes
- **Task 8.1:** Fix ExerciseTypeSelector - Allow unselecting Rest type `[Implemented: 1f0d3a75]`
- **Task 8.2:** Enhance CoachNotesEditor - Add delete functionality for individual notes `[Fixed: CSS + FontAwesome issue: d9a8f721]`
- **Task 8.3:** Enhance CoachNotesEditor - Implement automatic order resequencing after deletion `[Fixed: CSS + FontAwesome issue: d9a8f721]`
- **Task 8.4:** Enhance CoachNotesEditor - Handle empty state when all notes deleted `[Fixed: CSS + FontAwesome issue: d9a8f721]`
- **Task 8.5:** Add up/down reordering buttons to CoachNotesEditor `[Fixed: CSS + FontAwesome issue: d9a8f721]`
- **Task 8.6:** Implement Rest type business rules - Auto-disable/clear dependent fields `[Implemented: ccb43b67]`
- **Task 8.7:** Implement Rest type business rules - Auto-select Beginner difficulty (read-only) `[Implemented: ccb43b67]`
- **Task 8.8:** Make muscle groups optional for exercise creation `[Already implemented - validation skips for Rest]`
- **Task 8.9:** Update form validation to handle Rest type special cases `[Implemented: ccb43b67]`
- **Task 8.10:** Write tests for all manual testing fixes `[Implemented: 604e42ef]`

**CHECKPOINT 8:** 🛑 `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 9: Integration Testing
- **Task 9.1:** Write end-to-end test for creating exercise with coach notes `[ReadyToDevelop]`
- **Task 9.2:** Write end-to-end test for exercise type selection and validation `[ReadyToDevelop]`
- **Task 9.3:** Write end-to-end test for editing exercise with reordering coach notes `[ReadyToDevelop]`
- **Task 9.4:** Write end-to-end test for isActive filtering `[ReadyToDevelop]`
- **Task 9.5:** Write end-to-end test for Rest type business rules `[ReadyToDevelop]`

**CHECKPOINT 9:** 🛑 `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 10: UI/UX Polish
- **Task 10.1:** Style exercise type badges with appropriate colors `[ReadyToDevelop]`
- **Task 10.2:** Add icons for coach notes reordering buttons `[ReadyToDevelop]`
- **Task 10.3:** Ensure responsive design for new components `[ReadyToDevelop]`
- **Task 10.4:** Add helpful tooltips for business rules (Rest exclusivity, etc.) `[ReadyToDevelop]`

**FINAL CHECKPOINT:** 🛑 `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

## Notes
- Coach notes must maintain their order (0-based indexing)
- Coach notes should be deletable and auto-resequence order after deletion
- Rest exercise type cannot be combined with other types but CAN be unselected
- Every exercise must have at least one type (except temporarily during selection)
- No exercise can have all four types
- **Rest Type Business Rules:**
  - When Rest is selected: automatically disable/clear Equipment, Muscle Groups, Movement Patterns, Body Parts
  - When Rest is selected: automatically set Difficulty to "Beginner" and make it read-only
  - Muscle groups should be optional (not required) for exercise creation
  - Coach notes are still allowed for Rest exercises
  - IsActive flag is still user-controlled for Rest exercises
- All API fields now use camelCase naming convention
- Each implementation task must be immediately followed by its test task
- Follow existing Blazor patterns and Tailwind CSS styling
- **FontAwesome Issue**: FontAwesome CSS library is not loaded in the project, causing icons (fas fa-*) to not display. Components use Unicode symbols instead (→, ↑, ↓, ✕, 📝) for better visibility and compatibility.
## Boy Scout Rule (Applied during Phase 1):
- Implement Builder pattern for all Exercise DTOs `[Implemented: 223fc351]`
- Enhance builders with bulk operations support `[Implemented: 50457414]`
- Move builders from test to production code `[Implemented: 14d748af]`
