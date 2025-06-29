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
- **Task 3.1:** Create ExerciseTypeSelector component with checkbox interface `[ReadyToDevelop]`
- **Task 3.2:** Implement Rest type exclusivity validation (cannot combine with other types) `[ReadyToDevelop]`
- **Task 3.3:** Enforce at least one type must be selected rule `[ReadyToDevelop]`
- **Task 3.4:** Prevent selection of all four types `[ReadyToDevelop]`
- **Task 3.5:** Write component tests for ExerciseTypeSelector `[ReadyToDevelop]`
- **Task 3.6:** Test all validation rules and edge cases `[ReadyToDevelop]`

**CHECKPOINT 3:** 🛑 `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 4: Form Integration
- **Task 4.1:** Update ExerciseForm to replace instructions field with CoachNotesEditor `[ReadyToDevelop]`
- **Task 4.2:** Integrate ExerciseTypeSelector into ExerciseForm `[ReadyToDevelop]`
- **Task 4.3:** Update form validation to include new business rules `[ReadyToDevelop]`
- **Task 4.4:** Update form submission to send coachNotes and exerciseTypes in correct format `[ReadyToDevelop]`
- **Task 4.5:** Write tests for updated ExerciseForm `[ReadyToDevelop]`

**CHECKPOINT 4:** 🛑 `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 5: List & Detail Updates
- **Task 5.1:** Update ExerciseList to display exercise types as colored badges `[ReadyToDevelop]`
- **Task 5.2:** Add isActive status indicator to exercise list rows `[ReadyToDevelop]`
- **Task 5.3:** Implement isActive filter dropdown in ExerciseList `[ReadyToDevelop]`
- **Task 5.4:** Write tests for ExerciseList updates `[ReadyToDevelop]`
- **Task 5.5:** Update ExerciseDetail to display coach notes as ordered list `[ReadyToDevelop]`
- **Task 5.6:** Update ExerciseDetail to show exercise types with badges `[ReadyToDevelop]`
- **Task 5.7:** Write tests for ExerciseDetail updates `[ReadyToDevelop]`

**CHECKPOINT 5:** 🛑 `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 6: Field Name Updates
- **Task 6.1:** Update all model properties to use camelCase (isUnilateral, imageUrl, videoUrl, etc.) `[ReadyToDevelop]`
- **Task 6.2:** Update API request/response handling for camelCase conversion `[ReadyToDevelop]`
- **Task 6.3:** Search and replace any remaining snake_case references `[ReadyToDevelop]`
- **Task 6.4:** Test all CRUD operations with updated field names `[ReadyToDevelop]`

**CHECKPOINT 6:** 🛑 `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 7: State Management Updates
- **Task 7.1:** Update ExerciseStateService to handle coach notes array `[ReadyToDevelop]`
- **Task 7.2:** Update ExerciseStateService to handle exercise types `[ReadyToDevelop]`
- **Task 7.3:** Add isActive filtering to state management `[ReadyToDevelop]`
- **Task 7.4:** Write tests for updated state management `[ReadyToDevelop]`

**CHECKPOINT 7:** 🛑 `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 8: Integration Testing
- **Task 8.1:** Write end-to-end test for creating exercise with coach notes `[ReadyToDevelop]`
- **Task 8.2:** Write end-to-end test for exercise type selection and validation `[ReadyToDevelop]`
- **Task 8.3:** Write end-to-end test for editing exercise with reordering coach notes `[ReadyToDevelop]`
- **Task 8.4:** Write end-to-end test for isActive filtering `[ReadyToDevelop]`

**CHECKPOINT 8:** 🛑 `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

### Phase 9: UI/UX Polish
- **Task 9.1:** Style exercise type badges with appropriate colors `[ReadyToDevelop]`
- **Task 9.2:** Add icons for coach notes reordering buttons `[ReadyToDevelop]`
- **Task 9.3:** Ensure responsive design for new components `[ReadyToDevelop]`
- **Task 9.4:** Add helpful tooltips for business rules (Rest exclusivity, etc.) `[ReadyToDevelop]`

**FINAL CHECKPOINT:** 🛑 `dotnet build` MUST PASS | `dotnet test` ALL GREEN | NO WARNINGS

## Notes
- Coach notes must maintain their order (0-based indexing)
- Rest exercise type cannot be combined with other types
- Every exercise must have at least one type
- No exercise can have all four types
- All API fields now use camelCase naming convention
- Each implementation task must be immediately followed by its test task
- Follow existing Blazor patterns and Tailwind CSS styling
## Boy Scout Rule (Applied during Phase 1):
- Implement Builder pattern for all Exercise DTOs `[Implemented: 223fc351]`
- Enhance builders with bulk operations support `[Implemented: 50457414]`
- Move builders from test to production code `[Implemented: 14d748af]`
