# Feature Implementation Process - Admin Project

This document outlines the standard process for implementing new features in the GetFitterGetBigger Admin Blazor application.

**📌 NOTE**: This document extends the [Unified Development Process](../../API/memory-bank/UNIFIED_DEVELOPMENT_PROCESS.md) with Admin-specific implementation details. Always refer to the unified process for core principles and mandatory requirements.

## Process Overview

### File Management Rules

**⚠️ IMPORTANT: Maintain focus on REQUIRED files only! ⚠️**

When implementing features:
- **Only create files explicitly required** by the unified process
- **DO NOT add extra documentation files** like README.md, notes.md, etc.
- **feature-description.md and feature-tasks.md** contain all necessary documentation
- **Avoid redundancy** - if information exists in required files, don't duplicate it elsewhere
- **Supporting files** (like test scripts, mockups) are allowed ONLY if they add unique value

Remember: Clean, focused file structure makes features easier to track and maintain.

### 0. Feature States (Pre-Implementation)
Features progress through these workflow states:
- **0-SUBMITTED**: MANDATORY starting point for ALL features
- **1-READY_TO_DEVELOP**: Feature refined with tasks and ready to implement
- **2-IN_PROGRESS**: Feature currently being implemented
- **3-COMPLETED**: Feature done and tested
- **4-BLOCKED**: Dependencies preventing progress
- **5-SKIPPED**: Feature deferred or cancelled

#### Working with Features

**API-Originated Features** (in `0-SUBMITTED` from API propagation):
1. Review the API contract and changes
2. Analyze impact on Admin UI/UX
3. Create implementation tasks specific to Admin project
4. Add time estimates
5. Move to `1-READY_TO_DEVELOP` when the team is ready

**Admin-Originated Features** (requiring API changes):
1. Create feature in `0-SUBMITTED` with UI requirements
2. Request corresponding API feature
3. Wait for API implementation
4. Move to `1-READY_TO_DEVELOP` when API is ready

**IMPORTANT**: ALL features MUST start in 0-SUBMITTED state to ensure consistent workflow tracking.

### 1. Feature Analysis & Planning
- Feature MUST already exist in `0-SUBMITTED` state
- Review feature requirements from `feature-description.md`
- Analyze UI/UX implications for Admin interface
- Create a comprehensive implementation plan with granular tasks
- **MANDATORY**: Create `feature-tasks.md` in the existing feature folder
- Each task must be marked with status `[ReadyToDevelop]`
- Move feature folder from `0-SUBMITTED` to `1-READY_TO_DEVELOP`
- Tasks should be specific, actionable, and independently verifiable
- The task file must include:
  - Feature branch name at the top
  - Tasks organized by logical categories (Components, Services, State Management, API Integration, etc.)
  - **Unit/Component test tasks immediately following each implementation task**
  - Clear description of what each task entails
  - Space for commit hashes to be added as tasks are completed

### 2. Branch Creation
- Create a dedicated feature branch from master
- Naming convention: `feature/[descriptive-feature-name]`
- All development work occurs in this isolated branch

### 3. Baseline Health Check (MANDATORY)
Before starting ANY implementation:
1. **Run baseline health check**:
   ```bash
   dotnet build
   dotnet test
   ```
2. **Document results in feature-tasks.md**:
   ```markdown
   ## Baseline Health Check Report
   **Date/Time**: YYYY-MM-DD HH:MM
   **Branch**: feature/branch-name

   ### Build Status
   - **Build Result**: ✅ Success / ❌ Failed / ⚠️ Success with warnings
   - **Warning Count**: X warnings
   - **Warning Details**: [List specific warnings if any]

   ### Test Status
   - **Total Tests**: X
   - **Passed**: X
   - **Failed**: X (MUST be 0 to proceed)
   - **Skipped/Ignored**: X
   - **Test Execution Time**: X.XX seconds

   ### Code Analysis Status
   - **Errors**: X (MUST be 0 to proceed)
   - **Warnings**: X (should be < 10)

   ### Decision to Proceed
   - [ ] All tests passing
   - [ ] Build successful
   - [ ] No code analysis errors
   - [ ] Warnings documented and approved

   **Approval to Proceed**: Yes/No
   ```

3. **Evaluation and Action**:
   - ✅ **All Green**: Proceed to implementation
   - ❌ **Build Fails**: STOP - Create Task 0.1 to fix build
   - ❌ **Tests Fail**: STOP - Create Task 0.2 to fix failing tests
   - ❌ **Code Analysis Errors**: STOP - Create Task 0.3 to fix code analysis errors
   - ⚠️ **Warnings Exist**: Document and ask for approval
     - If approved: Create Boy Scout tasks (0.4, 0.5, etc.) to fix warnings FIRST
     - Complete Boy Scout tasks before feature tasks
     - Re-run baseline check after Boy Scout cleanup

### 4. Implementation Phase
- Execute tasks sequentially from the task tracking file
- **For EVERY task implementation:**
  1. Update task status to `[InProgress: Started: YYYY-MM-DD HH:MM]` when starting
  2. Write the implementation code
  3. **MANDATORY: Write unit/component tests for the implemented code in the immediately following task**
  4. **MANDATORY: Keep build warnings to a minimum** (address TypeScript errors, unused variables, etc.)
  5. **MANDATORY: Run `dotnet build` to ensure compilation succeeds with minimal warnings**
  6. **MANDATORY: Run `dotnet test` to ensure ALL tests pass (100% green)**
  8. Only after build succeeds and ALL tests pass, commit the changes
  9. Update the task status to `[Implemented: <hash> | Started: <timestamp> | Finished: YYYY-MM-DD HH:MM | Duration: Xh Ym]`
- **For EVERY checkpoint:**
  1. Run `dotnet build` - BUILD MUST BE SUCCESSFUL (no errors)
  2. Run `dotnet test` - ALL TESTS MUST BE GREEN (no failures)
  3. Verify no build warnings exist
  4. **MANDATORY: Update checkpoint status from 🛑 to ✅ when all conditions pass**
  5. Only proceed to next category after checkpoint passes
- Follow existing React patterns and conventions
- Use existing UI components and design system
- The task tracking file serves as both documentation and audit trail
- **CRITICAL RULES**:
  - **NO broken builds between tasks** - each task must leave the application in a working state
  - **ALL tests must be green** after implementing a task (no skipped, no failures)
  - **Tests are written immediately after implementation** (not deferred to a later phase)
  - Never move to the next task if:
    - The build is failing
    - Build warnings are excessive
    - Tests are not written
    - Any test is failing
    - Linting errors exist

### 5. Test Development & Handling
- Write unit tests for all business logic
- Write component tests for all React components
- Write integration tests for API interactions
- If a test requires complex mocking or setup:
  - Create a `[BUG: <detailed-reason>]` entry
  - Mark test with skip attribute
  - Include bug reference in test comment
  - Example: `test.skip('BUG-001: Complex API mocking requires additional setup')`

### 6. Manual Testing Phase (MANDATORY - NEVER SKIP)
- **⚠️ CRITICAL: This phase is MANDATORY and CANNOT be skipped**
- **⚠️ NEVER move a feature to COMPLETED without explicit user acceptance**
- After all implementation tasks are complete
- Provide user with:
  - Detailed step-by-step testing instructions
  - Test scenarios covering all user flows
  - Expected UI behaviors and responses
  - Sample data for testing
  - Clear indication that manual testing is required
- **MANDATORY ACTIONS**:
  1. Create a task in feature-tasks.md: `**Task X.X:** Manual testing by user [ReadyForTesting]`
  2. Update task to `[InProgress: Started: YYYY-MM-DD HH:MM]` when user begins testing
  3. Wait for EXPLICIT user acceptance (user must say "tests passed" or similar)
  4. Only update to `[Completed: YYYY-MM-DD HH:MM]` after user confirms
- **⚠️ REMINDER: The AI assistant must NEVER automatically move features to COMPLETED**

### 7. Quality Comparison Report (MANDATORY)
After all implementation is complete, add to feature-tasks.md:
```markdown
## Implementation Summary Report
**Date/Time**: YYYY-MM-DD HH:MM
**Duration**: X days/hours

### Quality Metrics Comparison
| Metric | Baseline | Final | Change |
|--------|----------|-------|--------|
| Build Warnings | X | Y | -Z |
| Test Count | X | Y | +Z |
| Test Pass Rate | X% | Y% | +Z% |
| Skipped Tests | X | Y | -Z |
| Lint Warnings | X | Y | -Z |

### Quality Improvements
- Fixed X build warnings
- Added Y new tests
- Removed Z skipped tests
- Fixed X linting warnings
- [Other improvements]

### Boy Scout Rule Applied
- ✅ All encountered issues fixed
- ✅ Code quality improved
- ✅ Documentation updated
```

### 8. Feature Finalization
After user explicitly states feature acceptance:
1. Create descriptive commit message summarizing all changes
2. Push feature branch to remote repository
3. Merge feature branch into master locally
4. **MANDATORY: Push the merged master branch to remote repository**
5. Delete the feature branch locally
6. Optionally delete the feature branch on remote

### 9. Special Conditions
- **Skipping Manual Tests**: Only when user explicitly requests during initial feature specification
- **Interrupted Implementation**: Next session can resume using existing task list with current statuses
- **Git Operations**: All git operations require explicit user approval and are not automated

## Task Status Definitions
- `[ReadyToDevelop]` - Task identified and ready to implement
- `[InProgress: Started: YYYY-MM-DD HH:MM]` - Task currently being worked on with start timestamp
- `[Implemented: <hash> | Started: YYYY-MM-DD HH:MM | Finished: YYYY-MM-DD HH:MM | Duration: Xh Ym]` - Task completed with timing data
- `[BUG: <reason>]` - Known issue requiring future resolution
- `[Skipped]` - Task deferred or determined unnecessary
- `[INCOMPLETE: <reason>]` - Task cannot be completed due to external dependency or bug
- `[BLOCKED: BUG-<id>]` - Task blocked by a specific bug that needs to be fixed first

## Important Notes
- This process ensures code quality through comprehensive testing
- User maintains control over deployment and merge decisions
- Clear audit trail through commit hashes and bug tracking
- Supports both continuous and interrupted development workflows

## Handling Blocked Features
When a feature cannot be completed due to external dependencies:
1. Mark the blocking task as `[BLOCKED: BUG-<id>]` with reference to the bug
2. Create a bug entry in the appropriate tracking location with:
   - Bug ID and description
   - Link back to the blocked feature task
   - Expected resolution approach
3. Mark the overall feature as `[INCOMPLETE]` at the end of the feature file
4. When the bug is fixed:
   - Update the blocked task to `[Implemented: <hash>]`
   - Remove the `[INCOMPLETE]` status from the feature
   - Add note about bug resolution

## Implementation Verification Checklist

Before marking any task as `[Implemented]`, verify:

- [ ] Implementation code is complete
- [ ] Unit/component tests are written for the new code (in the immediately following task)
- [ ] `dotnet build` runs without errors
- [ ] Build warnings are minimal (no C# compiler warnings, unused variables, etc.)
- [ ] `dotnet test` runs with ALL tests passing (100% green, no skipped tests)
- [ ] `npm run lint` passes without errors
- [ ] Code follows React best practices and project conventions
- [ ] No commented-out code or debug statements
- [ ] The application is in a working state (no broken functionality)
- [ ] UI is responsive and follows design guidelines

## Task Tracking File Template

```markdown
# [Feature Name] Implementation Tasks

## Feature Branch: `feature/[branch-name]`
## Estimated Total Time: [X days / Y hours]
## Actual Total Time: [To be calculated at completion]

### Category 1 (e.g., API Service Layer) - Estimated: Xh
- **Task 1.1:** Create [Name]Service for API integration `[ReadyToDevelop]` (Est: 45m)
- **Task 1.2:** Write unit tests for [Name]Service `[ReadyToDevelop]` (Est: 30m)
- **Task 1.3:** Add error handling and retry logic `[ReadyToDevelop]` (Est: 30m)

### Category 2 (e.g., State Management) - Estimated: Xh
- **Task 2.1:** Create Redux/Context state for [feature] `[ReadyToDevelop]` (Est: 1h)
- **Task 2.2:** Implement actions and reducers `[ReadyToDevelop]` (Est: 1h)
- **Task 2.3:** Write tests for state management `[ReadyToDevelop]` (Est: 45m)

### Category 3 (e.g., Components) - Estimated: Xh
- **Task 3.1:** Create [Name]List component with pagination `[ReadyToDevelop]` (Est: 2h)
- **Task 3.2:** Write component tests for [Name]List `[ReadyToDevelop]` (Est: 1h)
- **Task 3.3:** Create [Name]Form component with validation `[ReadyToDevelop]` (Est: 2.5h)
- **Task 3.4:** Write component tests for [Name]Form `[ReadyToDevelop]` (Est: 1.5h)
- **Task 3.5:** Create [Name]Detail component `[ReadyToDevelop]` (Est: 1h)
- **Task 3.6:** Write component tests for [Name]Detail `[ReadyToDevelop]` (Est: 45m)

### Category 4 (e.g., Pages & Routing) - Estimated: Xh
- **Task 4.1:** Create [Name]Page container component `[ReadyToDevelop]` (Est: 1h)
- **Task 4.2:** Set up routing for [feature] pages `[ReadyToDevelop]` (Est: 30m)
- **Task 4.3:** Write integration tests for pages `[ReadyToDevelop]` (Est: 1.5h)

### Category 5 (e.g., UI/UX Polish) - Estimated: Xh
- **Task 5.1:** Add loading states and skeletons `[ReadyToDevelop]` (Est: 1h)
- **Task 5.2:** Implement error boundaries and messages `[ReadyToDevelop]` (Est: 45m)
- **Task 5.3:** Add animations and transitions `[ReadyToDevelop]` (Est: 1h)
- **Task 5.4:** Ensure responsive design across breakpoints `[ReadyToDevelop]` (Est: 45m)

### Checkpoints
- **Checkpoint after Category 2:** All state management working 🛑
- **Checkpoint after Category 3:** All components tested 🛑
- **Checkpoint after Category 4:** Full feature flow working 🛑
- **Final Checkpoint:** All tests green, build clean 🛑

## Time Tracking Summary
- **Total Estimated Time:** [Sum of all estimates]
- **Total Actual Time:** [To be calculated from task durations]
- **AI Assistance Impact:** [% reduction in time]
- **Implementation Started:** [First task start time]
- **Implementation Completed:** [Last task finish time]

## Notes
- Each implementation task must be immediately followed by its test task
- No task is complete until build passes and all tests are green
- Keep build warnings to minimum
- Follow existing UI patterns and component library
- Time estimates are for a developer without AI assistance
```

## Example Task Status Updates

- `[ReadyToDevelop]` - Initial status for all tasks
- `[InProgress: Started: 2025-01-15 14:30]` - Task being actively worked on
- `[Implemented: a1b2c3d4 | Started: 2025-01-15 14:30 | Finished: 2025-01-15 15:15 | Duration: 0h 45m]` - Completed task
- `[BUG: Complex mock setup for API service]` - Known issue to be addressed later
- `[Skipped]` - Task determined unnecessary during implementation

## Time Calculation Guidelines

### Recording Time
- Use 24-hour format for timestamps (HH:MM)
- Record actual work time only (exclude breaks, interruptions)
- If a task spans multiple days, sum up the actual work duration
- Round to nearest 5-minute increment for consistency

### Duration Calculation
- Format: `Xh Ym` (e.g., "2h 30m", "0h 45m", "4h 0m")
- For tasks interrupted and resumed:
  - Track each work session
  - Sum total actual work time
  - Note in task if it was interrupted

### Example with Interruption
```
Task 3.1: Create UserProfile component 
[Implemented: abc123 | Started: 2025-01-15 09:00 | Finished: 2025-01-16 11:30 | Duration: 3h 15m]
Note: Work sessions: Jan 15 (09:00-10:30), Jan 16 (10:00-11:30)
```

### AI Impact Calculation
At feature completion, calculate:
- Sum all estimated times
- Sum all actual durations
- AI Impact = ((Estimated - Actual) / Estimated) × 100%
- Document any factors that affected the comparison

## Admin-Specific Considerations (Blazor)

### Component Development
- Use Blazor component lifecycle methods properly (OnInitializedAsync, OnParametersSetAsync, etc.)
- Implement proper parameter validation with `[Parameter]` attributes
- Use `@implements IDisposable` when managing resources
- Follow Blazor naming conventions (*.razor for components, *.razor.cs for code-behind)
- Use `@ref` sparingly and properly dispose references

### State Management
- Use Cascading Values and Parameters for cross-component state
- Implement proper StateHasChanged() calls only when necessary
- Use dependency injection for service state management
- Handle async operations with proper cancellation tokens
- Avoid overuse of static properties for state

### API Integration
- Use typed HttpClient with proper DI configuration
- Implement Polly for retry policies and circuit breakers
- Handle authentication with AuthenticationStateProvider
- Use proper exception handling for API calls
- Implement proper loading and error states

### Testing with bUnit
- Write component tests using bUnit framework
- Test component rendering, parameters, and event callbacks
- Mock services using dependency injection
- Test JavaScript interop when used
- Verify proper component disposal

### Performance Optimization
- Use virtualization for large lists (Virtualize component)
- Implement proper component disposal to prevent memory leaks
- Minimize unnecessary re-renders with ShouldRender()
- Use appropriate render modes (Server vs WebAssembly)
- Optimize data fetching with proper caching strategies

### Blazor-Specific Patterns
- Use EditForm with DataAnnotations for form validation
- Implement proper navigation with NavigationManager
- Handle JavaScript interop with IJSRuntime carefully
- Use AuthorizeView for role-based UI rendering
- Implement proper error boundaries with ErrorBoundary component