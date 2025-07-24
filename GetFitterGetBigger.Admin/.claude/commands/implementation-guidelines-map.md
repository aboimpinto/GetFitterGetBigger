# Implementation Guidelines Map

This document maps task types to their relevant documentation and guidelines. Used by start-implementing and continue-implementing commands.

## Task Type Detection Patterns

### API Service Tasks
**Pattern**: Tasks containing "Service", "API integration", "API calls"
**Guidelines**:
- @memory-bank/CODE_QUALITY_STANDARDS.md
- @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md#api-service-testing-xunit
- @memory-bank/ADMIN-CODE_QUALITY_STANDARDS.md#api-integration
- Look for existing service patterns in `/Services/` directory
- Reference: ExerciseService.cs (81% coverage) as good example

### Component Tasks
**Pattern**: Tasks containing "component", "Create", "View", "Form", "Panel"
**Guidelines**:
- @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md#blazor-component-testing-bunit
- @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md#component-design-for-testability
- @memory-bank/ADMIN-CODE_QUALITY_STANDARDS.md#component-development
- Add `data-testid` attributes to all interactive elements
- Make key methods `internal` for test access
- Reference: ExerciseWeightTypeBadge.razor (100% coverage) as example

### State Management Tasks
**Pattern**: Tasks containing "StateService", "state management", "state"
**Guidelines**:
- @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md#state-service-testing
- @memory-bank/ADMIN-CODE_QUALITY_STANDARDS.md#state-management
- Pattern: Optimistic updates with rollback on error
- Pattern: Error message persistence during rollback
- Reference: ExerciseWeightTypeStateService.cs (100% coverage)

### Data Model/DTO Tasks
**Pattern**: Tasks containing "DTO", "model", "type definitions"
**Guidelines**:
- @memory-bank/CODE_QUALITY_STANDARDS.md
- NO unit tests required for DTOs (per testing guidelines)
- Use builders for test data setup
- Reference: ExerciseListDtoBuilder.cs for builder pattern

### Testing Tasks
**Pattern**: Tasks containing "test", "unit test", "integration test"
**Guidelines**:
- @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md (full document)
- @memory-bank/TEST-COVERAGE-IMPROVEMENT-STRATEGY.md
- Boy Scout Rule: Improve coverage when touching existing code
- Target 80%+ coverage for new code
- Use FluentAssertions for all assertions
- Mock at appropriate level (not too deep)

### Routing/Navigation Tasks
**Pattern**: Tasks containing "routing", "navigation", "routes"
**Guidelines**:
- @memory-bank/ADMIN-CODE_QUALITY_STANDARDS.md#blazor-specific-patterns
- Use NavigationManager properly
- Implement proper authorization checks
- Test navigation guards

### UI/UX Polish Tasks
**Pattern**: Tasks containing "loading", "skeleton", "responsive", "accessibility"
**Guidelines**:
- Existing UI patterns from completed features
- Use Tailwind CSS classes consistently
- Implement proper ARIA labels
- Test responsive breakpoints
- Reference: Equipment Management feature for UI patterns

### Checkpoint Tasks
**Pattern**: Lines starting with "## CHECKPOINT" or "CHECKPOINT:"
**Guidelines**:
- @memory-bank/FEATURE_CHECKPOINT_TEMPLATE.md
- @memory-bank/CODE_REVIEW_PROCESS.md
- Run: `dotnet build` (must succeed with minimal warnings)
- Run: `dotnet test` (must be 100% green)
- Create code review in: `/code-reviews/Phase_X_[Name]/`
- Cannot proceed until checkpoint APPROVED

### Integration/E2E Test Tasks
**Pattern**: Tasks containing "E2E", "integration test", "workflow test"
**Guidelines**:
- Test complete user workflows
- Use realistic test data
- Test error scenarios
- Reference: Exercise Management integration tests

## Common Requirements for All Tasks

### Before Starting Any Task
1. Ensure previous task is complete and tested
2. Check build status (must be green)
3. Review any code review feedback

### After Completing Any Task
1. Update task status in feature-tasks.md
2. Run build and tests
3. Commit with descriptive message
4. Add commit hash to task

### Quality Standards Apply to All
- Single exit point per method
- Methods < 20 lines
- No code duplication
- Proper error handling
- Clear naming conventions

## Phase-Specific Context

### Phase 1: API Service Layer
Focus on foundation - these services will be used by all other layers.
Critical to get error handling and retry logic right.

### Phase 2: Data Models
Keep simple, no business logic. Focus on clean data structures.

### Phase 3: State Management
This is where business logic lives. Implement proper state transitions.

### Phase 4+: UI Components
Build from shared/base components up to specific features.
Always think about reusability.

### Final Phases
Polish, performance, and user experience. Don't skip these!