# FEAT-021: Workout Template UI Implementation Tasks

## Feature Branch: `feature/FEAT-021-workout-template-ui`
## Estimated Total Time: 8 days / 64 hours
## Actual Total Time: [To be calculated at completion]

## Baseline Health Check Report
**Date/Time**: 2025-07-24 10:30
**Branch**: feature/FEAT-021-workout-template-core

### Build Status
- **Build Result**: ✅ Build succeeded
- **Warning Count**: 0
- **Warning Details**: None

### Test Status
- **Total Tests**: 826
- **Passed**: 826
- **Failed**: 0
- **Skipped/Ignored**: 0
- **Test Execution Time**: 1s
- **Code Coverage**: 72.92% Line, 59.83% Branch, 72.54% Method

### Code Analysis Status
- **Errors**: 0
- **Warnings**: 0

### Decision to Proceed
- [x] All tests passing
- [x] Build successful
- [x] No code analysis errors
- [x] Warnings documented and approved

**Approval to Proceed**: Yes

---

## Phase 1: API Service Layer - Estimated: 6h

### Core Services
- **Task 1.1:** Create IWorkoutTemplateService interface following existing patterns [ReadyToDevelop] (Est: 30m)
- **Task 1.2:** Implement WorkoutTemplateService with all CRUD operations [ReadyToDevelop] (Est: 2h)
- **Task 1.3:** Create unit tests for WorkoutTemplateService (happy path, errors, validation) [ReadyToDevelop] (Est: 1h 30m)
- **Task 1.4:** Create IWorkoutTemplateStateService interface for state management [ReadyToDevelop] (Est: 30m)
- **Task 1.5:** Implement WorkoutTemplateStateService following ExerciseStateService pattern [ReadyToDevelop] (Est: 1h)
- **Task 1.6:** Create unit tests for WorkoutTemplateStateService (state changes, error persistence, optimistic updates) [ReadyToDevelop] (Est: 30m)

## CHECKPOINT: Phase 1 Complete - API Service Layer
`[COMPLETE]` - Date: 2025-07-24 14:00

Build Report:
- Admin Project: ✅ 0 errors, 0 warnings
- Test Project: ✅ 0 errors, 0 warnings

Service Implementation Summary:
- **WorkoutTemplateService**: ✅ Complete - 18 methods implemented (CRUD, state management, filtering, reference data)
- **WorkoutTemplateStateService**: ✅ Complete - State management implemented following ExerciseStateService pattern

Test Summary:
- **Total Tests**: 851 (up from 826 baseline)
- **New Tests**: 25 tests added (13 for WorkoutTemplateService, 12 for WorkoutTemplateStateService)
- **Test Coverage**: 72.43% Line, 60% Branch, 71.27% Method
- **All Tests**: ✅ PASSING

Code Review: Phase 1 implementation follows established patterns from existing services
- Services registered in Program.cs
- DTOs created with proper validation attributes
- Builders created for test support
- Error handling and caching implemented

Git Commit: Ready for commit

Status: ✅ COMPLETE Phase 1
Notes: 
- Successfully implemented API service layer for workout templates
- All tests passing with good coverage
- Ready to proceed to Phase 2 (Data Models and DTOs)

✅ Phase 1 APPROVED - Ready to proceed to Phase 2

---

## Phase 2: Data Models and DTOs - Estimated: 4h

### DTOs and Models
- **Task 2.1:** Create WorkoutTemplateDto and related DTOs (CreateWorkoutTemplateDto, UpdateWorkoutTemplateDto) [ReadyToDevelop] (Est: 1h)
- **Task 2.2:** Create WorkoutTemplateFilterDto and PagedResultDto [ReadyToDevelop] (Est: 30m)
- **Task 2.3:** Create WorkoutStateDto, CategoryDto, DifficultyDto reference DTOs [ReadyToDevelop] (Est: 30m)
- **Task 2.4:** Create validation attributes and validation rules [ReadyToDevelop] (Est: 45m)
- **Task 2.5:** Create WorkoutTemplateDtoBuilder for test data [ReadyToDevelop] (Est: 45m)
- **Task 2.6:** Create unit tests for WorkoutTemplateDtoBuilder [ReadyToDevelop] (Est: 30m)

### Questions for Clarification
- **Task 2.7:** Clarify approach for handling missing equipment aggregation from API [BLOCKED] (Est: 15m)
  - Should UI hide equipment section or show placeholder?
  - What message to display to users?

## CHECKPOINT: Phase 2 Complete - Data Models
`[PENDING]` - Date: [YYYY-MM-DD HH:MM]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project: [STATUS] [X errors, Y warnings]

Data Model Summary:
- **DTOs Created**: [Count]
- **Validation Rules**: [Status]
- **Builder Pattern**: [Status]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-021-workout-template-ui/code-reviews/Phase_2_Data_Models/Code-Review-Phase-2-Data-Models-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 2
Notes: 
- [Key implementation notes]
- [Readiness for Phase 3]

⚠️ Cannot proceed to Phase 3 until this checkpoint is APPROVED

---

## Phase 3: State Management - Estimated: 3h

### State Services
- **Task 3.1:** Implement WorkoutTemplateStateService filter management and pagination [ReadyToDevelop] (Est: 1h)
- **Task 3.2:** Create unit tests for filter and pagination state management [ReadyToDevelop] (Est: 45m)
- **Task 3.3:** Implement template duplication and state transition logic in state service [ReadyToDevelop] (Est: 45m)
- **Task 3.4:** Create unit tests for duplication and state transitions (optimistic updates, rollback) [ReadyToDevelop] (Est: 30m)

## CHECKPOINT: Phase 3 Complete - State Management
`[PENDING]` - Date: [YYYY-MM-DD HH:MM]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project: [STATUS] [X errors, Y warnings]

State Management Summary:
- **Filter State**: [Status]
- **Pagination State**: [Status]
- **Error Persistence**: [Status]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-021-workout-template-ui/code-reviews/Phase_3_State_Management/Code-Review-Phase-3-State-Management-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 3
Notes: 
- [Key implementation notes]
- [Readiness for Phase 4]

⚠️ Cannot proceed to Phase 4 until this checkpoint is APPROVED

---

## Phase 4: Shared/Base Components - Estimated: 8h

### Shared Components
- **Task 4.1:** Create WorkoutStateIndicator component with icons and colors (add data-testid attributes) [ReadyToDevelop] (Est: 1h)
- **Task 4.2:** Create unit tests for WorkoutStateIndicator (UI interaction + logic tests) [ReadyToDevelop] (Est: 45m)
- **Task 4.3:** Create StateTransitionButton component with confirmation dialogs (internal visibility for testability) [ReadyToDevelop] (Est: 1h 30m)
- **Task 4.4:** Create unit tests for StateTransitionButton (state validation, dialog interaction) [ReadyToDevelop] (Est: 1h)
- **Task 4.5:** Create WorkoutTemplateCard component for list view display [ReadyToDevelop] (Est: 1h)
- **Task 4.6:** Create unit tests for WorkoutTemplateCard (rendering, click handlers) [ReadyToDevelop] (Est: 45m)
- **Task 4.7:** Create WorkoutTemplateFilters component with category/difficulty/state filters [ReadyToDevelop] (Est: 1h 30m)
- **Task 4.8:** Create unit tests for WorkoutTemplateFilters (filter changes, clear filters) [ReadyToDevelop] (Est: 30m)

## CHECKPOINT: Phase 4 Complete - Shared Components
`[PENDING]` - Date: [YYYY-MM-DD HH:MM]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project: [STATUS] [X errors, Y warnings]

Shared Components Summary:
- **Components Created**: [Count]
- **Test Coverage**: [Percentage]
- **Accessibility**: [Status]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-021-workout-template-ui/code-reviews/Phase_4_Shared_Components/Code-Review-Phase-4-Shared-Components-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 4
Notes: 
- [Key implementation notes]
- [Readiness for Phase 5]

⚠️ Cannot proceed to Phase 5 until this checkpoint is APPROVED

---

## Phase 5: Feature Components - List View - Estimated: 10h

### List View Implementation
- **Task 5.1:** Create WorkoutTemplateList component with grid display and sorting (add data-testid for all interactive elements) [ReadyToDevelop] (Est: 2h)
- **Task 5.2:** Create unit tests for WorkoutTemplateList (rendering, sorting, empty state) [ReadyToDevelop] (Est: 1h 30m)
- **Task 5.3:** Implement pagination controls in WorkoutTemplateList [ReadyToDevelop] (Est: 1h)
- **Task 5.4:** Create unit tests for pagination (page changes, bounds checking) [ReadyToDevelop] (Est: 45m)
- **Task 5.5:** Implement search functionality with debouncing [ReadyToDevelop] (Est: 1h)
- **Task 5.6:** Create unit tests for search (debouncing, clear search) [ReadyToDevelop] (Est: 45m)
- **Task 5.7:** Implement quick actions (edit, duplicate, archive, delete) with proper authorization [ReadyToDevelop] (Est: 1h 30m)
- **Task 5.8:** Create unit tests for quick actions (permissions, confirmations) [ReadyToDevelop] (Est: 1h)
- **Task 5.9:** Create integration test for complete list view workflow [ReadyToDevelop] (Est: 30m)

## CHECKPOINT: Phase 5 Complete - List View
`[PENDING]` - Date: [YYYY-MM-DD HH:MM]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project: [STATUS] [X errors, Y warnings]

List View Summary:
- **Features Implemented**: [List key features]
- **Test Coverage**: [Percentage]
- **Performance**: [Notes on pagination/search]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-021-workout-template-ui/code-reviews/Phase_5_List_View/Code-Review-Phase-5-List-View-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 5
Notes: 
- [Key implementation notes]
- [Readiness for Phase 6]

⚠️ Cannot proceed to Phase 6 until this checkpoint is APPROVED

---

## Phase 6: Feature Components - Create/Edit Forms - Estimated: 12h

### Form Implementation
- **Task 6.1:** Create WorkoutTemplateForm component with all metadata fields (ensure internal visibility) [ReadyToDevelop] (Est: 2h 30m)
- **Task 6.2:** Create unit tests for WorkoutTemplateForm (field rendering, validation display) [ReadyToDevelop] (Est: 1h 30m)
- **Task 6.3:** Implement client-side validation matching API rules [ReadyToDevelop] (Est: 1h 30m)
- **Task 6.4:** Create unit tests for validation logic (all validation rules) [ReadyToDevelop] (Est: 1h)
- **Task 6.5:** Implement auto-save functionality for draft templates [ReadyToDevelop] (Est: 1h 30m)
- **Task 6.6:** Create unit tests for auto-save (timer, dirty state detection) [ReadyToDevelop] (Est: 1h)
- **Task 6.7:** Implement unsaved changes warning and recovery [ReadyToDevelop] (Est: 1h)
- **Task 6.8:** Create unit tests for unsaved changes (navigation blocking, recovery) [ReadyToDevelop] (Est: 45m)
- **Task 6.9:** Implement name uniqueness check with async validation [ReadyToDevelop] (Est: 45m)
- **Task 6.10:** Create unit tests for async validation (debouncing, error states) [ReadyToDevelop] (Est: 30m)
- **Task 6.11:** Create integration test for complete form workflow (create and edit) [ReadyToDevelop] (Est: 45m)

### Questions for Clarification
- **Task 6.12:** Clarify UI approach for postponed exercise suggestions feature [BLOCKED] (Est: 15m)
  - Should we show disabled "Get suggestions" button?
  - Or completely hide the suggestions UI?

## CHECKPOINT: Phase 6 Complete - Forms
`[PENDING]` - Date: [YYYY-MM-DD HH:MM]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project: [STATUS] [X errors, Y warnings]

Form Implementation Summary:
- **Validation Rules**: [Count implemented]
- **Auto-save**: [Status]
- **User Experience**: [Notes on form usability]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-021-workout-template-ui/code-reviews/Phase_6_Forms/Code-Review-Phase-6-Forms-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 6
Notes: 
- [Key implementation notes]
- [Readiness for Phase 7]

⚠️ Cannot proceed to Phase 7 until this checkpoint is APPROVED

---

## Phase 7: Feature Components - Detail View - Estimated: 6h

### Detail View Implementation
- **Task 7.1:** Create WorkoutTemplateDetail component with read-only display [ReadyToDevelop] (Est: 1h 30m)
- **Task 7.2:** Create unit tests for WorkoutTemplateDetail (rendering all sections) [ReadyToDevelop] (Est: 1h)
- **Task 7.3:** Implement state transition controls in detail view [ReadyToDevelop] (Est: 1h)
- **Task 7.4:** Create unit tests for state transitions (permission checks, confirmations) [ReadyToDevelop] (Est: 45m)
- **Task 7.5:** Implement duplicate functionality from detail view [ReadyToDevelop] (Est: 45m)
- **Task 7.6:** Create unit tests for duplication (name modification, state reset) [ReadyToDevelop] (Est: 30m)
- **Task 7.7:** Add placeholder for exercise list (read-only, per API docs) [ReadyToDevelop] (Est: 30m)
- **Task 7.8:** Create integration test for detail view workflows [ReadyToDevelop] (Est: 30m)

## CHECKPOINT: Phase 7 Complete - Detail View
`[PENDING]` - Date: [YYYY-MM-DD HH:MM]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project: [STATUS] [X errors, Y warnings]

Detail View Summary:
- **Features Implemented**: [List key features]
- **State Transitions**: [Status]
- **Exercise Display**: [Placeholder status]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-021-workout-template-ui/code-reviews/Phase_7_Detail_View/Code-Review-Phase-7-Detail-View-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 7
Notes: 
- [Key implementation notes]
- [Readiness for Phase 8]

⚠️ Cannot proceed to Phase 8 until this checkpoint is APPROVED

---

## Phase 8: Navigation and Integration - Estimated: 4h

### Routing and Navigation
- **Task 8.1:** Configure routes for all workout template pages [ReadyToDevelop] (Est: 30m)
- **Task 8.2:** Add navigation menu items with PT-Tier authorization [ReadyToDevelop] (Est: 30m)
- **Task 8.3:** Create unit tests for authorization checks [ReadyToDevelop] (Est: 30m)
- **Task 8.4:** Implement breadcrumb navigation for template pages [ReadyToDevelop] (Est: 45m)
- **Task 8.5:** Create unit tests for breadcrumb navigation [ReadyToDevelop] (Est: 30m)
- **Task 8.6:** Register all services in Program.cs [ReadyToDevelop] (Est: 15m)
- **Task 8.7:** Create E2E test for navigation flows [ReadyToDevelop] (Est: 1h)

## CHECKPOINT: Phase 8 Complete - Navigation
`[PENDING]` - Date: [YYYY-MM-DD HH:MM]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project: [STATUS] [X errors, Y warnings]

Navigation Summary:
- **Routes Configured**: [Count]
- **Authorization**: [Status]
- **Menu Integration**: [Status]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-021-workout-template-ui/code-reviews/Phase_8_Navigation/Code-Review-Phase-8-Navigation-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 8
Notes: 
- [Key implementation notes]
- [Readiness for Phase 9]

⚠️ Cannot proceed to Phase 9 until this checkpoint is APPROVED

---

## Phase 9: UI/UX Polish - Estimated: 6h

### User Experience Enhancements
- **Task 9.1:** Implement loading states with skeletons for all async operations [ReadyToDevelop] (Est: 1h 30m)
- **Task 9.2:** Create unit tests for loading states [ReadyToDevelop] (Est: 45m)
- **Task 9.3:** Implement error handling with user-friendly messages and retry actions [ReadyToDevelop] (Est: 1h)
- **Task 9.4:** Create unit tests for error handling (display, retry) [ReadyToDevelop] (Est: 45m)
- **Task 9.5:** Add success notifications for all CRUD operations [ReadyToDevelop] (Est: 45m)
- **Task 9.6:** Implement responsive design for mobile/tablet views [ReadyToDevelop] (Est: 1h)
- **Task 9.7:** Add keyboard navigation support and accessibility attributes [ReadyToDevelop] (Est: 30m)
- **Task 9.8:** Create accessibility tests (ARIA labels, keyboard nav) [ReadyToDevelop] (Est: 30m)

## CHECKPOINT: Phase 9 Complete - UX Polish
`[PENDING]` - Date: [YYYY-MM-DD HH:MM]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project: [STATUS] [X errors, Y warnings]

UX Polish Summary:
- **Loading States**: [Status]
- **Error Handling**: [Status]
- **Accessibility**: [Status]
- **Responsive Design**: [Status]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-021-workout-template-ui/code-reviews/Phase_9_UX_Polish/Code-Review-Phase-9-UX-Polish-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] Phase 9
Notes: 
- [Key implementation notes]
- [Readiness for final phase]

⚠️ Cannot proceed to Final Phase until this checkpoint is APPROVED

---

## Final Phase: Manual Testing - Estimated: 2h

### Testing and Documentation
- **Task 10.1:** Create comprehensive manual testing guide following existing patterns [ReadyToDevelop] (Est: 1h)
- **Task 10.2:** Document placeholder features (equipment, suggestions) for future implementation [ReadyToDevelop] (Est: 30m)
- **Task 10.3:** Manual testing by user [ReadyForTesting] (Est: 30m)

### Manual Testing Scenarios
1. **Template Creation Flow**
   - Create new template with all fields
   - Verify validation messages
   - Test auto-save functionality
   - Confirm template appears in list

2. **Template Editing**
   - Edit existing template
   - Verify unsaved changes warning
   - Test field validation
   - Confirm updates are saved

3. **State Transitions**
   - Move template from DRAFT to PRODUCTION
   - Verify edit restrictions on PRODUCTION templates
   - Archive template and verify restrictions
   - Test invalid state transitions

4. **Search and Filter**
   - Search by template name
   - Filter by category, difficulty, state
   - Verify pagination works correctly
   - Test combined filters

5. **Duplication**
   - Duplicate existing template
   - Verify name modification
   - Confirm new template is in DRAFT state

6. **Error Scenarios**
   - Test network error handling
   - Verify duplicate name prevention
   - Test concurrent edit scenarios

7. **Responsive Design**
   - Test on mobile viewport
   - Verify tablet layout
   - Check touch interactions

8. **Accessibility**
   - Navigate with keyboard only
   - Test with screen reader
   - Verify ARIA labels

## FINAL CHECKPOINT: Feature Complete
`[PENDING]` - Date: [YYYY-MM-DD HH:MM]

Final Build Report:
- Admin Project: ✅ 0 errors, [X] warnings
- Test Project: ✅ 0 errors, [X] warnings
- All Tests: ✅ [Count] tests passing

Final Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-021-workout-template-ui/code-reviews/Final_Code_Review/Final-Code-Review-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Manual Testing: ✅ User acceptance received
Feature Status: ✅ COMPLETE and ready for production

---

## Implementation Summary Report
**Date/Time**: [To be completed]
**Duration**: [X days/hours]

### Quality Metrics Comparison
| Metric | Baseline | Final | Change |
|--------|----------|-------|--------|
| Build Warnings | X | Y | -Z |
| Test Count | X | Y | +Z |
| Test Pass Rate | X% | Y% | +Z% |
| Test Coverage | X% | Y% | +Z% |

### Key Achievements
- [List major accomplishments]
- [Test coverage improvements]
- [Performance optimizations]

### Time Tracking Summary
- **Total Estimated Time:** 64 hours
- **Total Actual Time:** [To be calculated from task durations]
- **AI Assistance Impact:** [% reduction in time]
- **Implementation Started:** [First task start time]
- **Implementation Completed:** [Last task finish time]

## Notes on Postponed Features
Based on the API documentation, the following features are postponed and require placeholder UI:
1. **Equipment Aggregation**: Show "Equipment information coming soon" message
2. **Exercise Suggestions**: Hide suggestions UI or show disabled state with tooltip
3. **Fine-grained Authorization**: Use mock auth context with PT-Tier role

## Code Quality Standards References
- Pattern: State management follows ExerciseStateService pattern (per CODE_QUALITY_STANDARDS.md)
- Pattern: Service layer follows IExerciseService pattern
- Pattern: Component testing follows bUnit patterns from COMPREHENSIVE-TESTING-GUIDE.md
- ⚠️ No existing WorkoutTemplate UI examples found - setting new patterns for this feature

## Test Coverage Requirements (per COMPREHENSIVE-TESTING-GUIDE.md)
- Unit tests immediately after each implementation task
- Both UI interaction and logic tests for components
- Service tests covering happy path, errors, and edge cases
- Integration tests after complete workflows
- Aim for 80%+ coverage on new code