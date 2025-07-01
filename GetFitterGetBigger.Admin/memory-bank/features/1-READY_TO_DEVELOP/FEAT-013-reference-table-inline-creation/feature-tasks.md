# FEAT-013 Reference Table Inline Creation - Implementation Tasks

## Feature Branch: `feature/reference-table-inline-creation`
## Estimated Total Time: 2 days / 16 hours
## Actual Total Time: [To be calculated at completion]

## Baseline Health Check Report
[To be completed before implementation starts]

### Category 1: Reusable Modal Component - Estimated: 3h
- **Task 1.1:** Create reusable AddReferenceItemModal component with props for entity type `[ReadyToDevelop]` (Est: 1.5h)
- **Task 1.2:** Write unit tests for AddReferenceItemModal component `[ReadyToDevelop]` (Est: 1h)
- **Task 1.3:** Add modal animation and accessibility features `[ReadyToDevelop]` (Est: 30m)

### Category 2: Service Layer Extensions - Estimated: 2h
- **Task 2.1:** Extend EquipmentService with inline creation method and cache invalidation `[ReadyToDevelop]` (Est: 45m)
- **Task 2.2:** Write unit tests for EquipmentService inline creation `[ReadyToDevelop]` (Est: 30m)
- **Task 2.3:** Extend MuscleGroupService with inline creation method and cache invalidation `[ReadyToDevelop]` (Est: 45m)

### Category 3: Form Components Enhancement - Estimated: 4h
- **Task 3.1:** Create EnhancedReferenceSelect component with "+" button for CRUD-enabled dropdowns `[ReadyToDevelop]` (Est: 1.5h)
- **Task 3.2:** Write component tests for EnhancedReferenceSelect `[ReadyToDevelop]` (Est: 1h)
- **Task 3.3:** Integrate EnhancedReferenceSelect into Exercise form for Equipment field `[ReadyToDevelop]` (Est: 45m)
- **Task 3.4:** Integrate EnhancedReferenceSelect into Exercise form for Muscle Groups field `[ReadyToDevelop]` (Est: 45m)

### Category 4: State Management & Data Flow - Estimated: 3h
- **Task 4.1:** Implement optimistic UI updates for newly created reference items `[ReadyToDevelop]` (Est: 1h)
- **Task 4.2:** Write tests for state management and data flow `[ReadyToDevelop]` (Est: 45m)
- **Task 4.3:** Add error handling and rollback for failed creations `[ReadyToDevelop]` (Est: 45m)
- **Task 4.4:** Implement proper cache invalidation across all dropdowns `[ReadyToDevelop]` (Est: 30m)

### Category 5: UI/UX Polish & Integration Testing - Estimated: 4h
- **Task 5.1:** Add loading states and error messages to inline creation flow `[ReadyToDevelop]` (Est: 1h)
- **Task 5.2:** Implement keyboard shortcuts (e.g., Ctrl+N to open modal) `[ReadyToDevelop]` (Est: 45m)
- **Task 5.3:** Write integration tests for complete inline creation flow `[ReadyToDevelop]` (Est: 1.5h)
- **Task 5.4:** Add visual indicators to differentiate CRUD vs read-only dropdowns `[ReadyToDevelop]` (Est: 45m)

### Checkpoints
- **Checkpoint after Category 1:** Modal component fully tested and accessible 🛑
- **Checkpoint after Category 2:** Service layer ready with cache management 🛑
- **Checkpoint after Category 3:** Form components integrated and working 🛑
- **Checkpoint after Category 4:** State management and error handling complete 🛑
- **Final Checkpoint:** All tests green, build clean, feature fully working 🛑

## Implementation Notes
- Focus only on Equipment and Muscle Groups for this implementation
- Ensure proper authorization checks (PT-Tier) are in place
- Modal should be reusable for future reference table types
- Cache invalidation must update all instances of the dropdown across the app
- Follow existing modal patterns in the codebase
- Maintain form state when modal is opened/closed

## Success Criteria
- Personal Trainers can add new equipment without leaving the exercise form
- Personal Trainers can add new muscle groups without leaving the exercise form
- Newly created items appear immediately in the dropdown and are auto-selected
- Error handling prevents data loss in the main form
- Visual indicators clearly show which dropdowns support inline creation

## Time Tracking Summary
- **Total Estimated Time:** 16 hours
- **Total Actual Time:** [To be calculated from task durations]
- **AI Assistance Impact:** [% reduction in time]
- **Implementation Started:** [First task start time]
- **Implementation Completed:** [Last task finish time]