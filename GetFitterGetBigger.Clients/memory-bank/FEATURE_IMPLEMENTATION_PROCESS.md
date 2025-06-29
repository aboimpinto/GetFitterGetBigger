# Feature Implementation Process - Client Projects

This document outlines the standard process for implementing new features across GetFitterGetBigger client applications (Mobile, Web, Desktop).

## Process Overview

### 0. Feature States (Pre-Implementation)
Features progress through these workflow states:
- **0-SUBMITTED**: Features propagated from API, needs project-specific refinement
- **1-READY_TO_DEVELOP**: Feature refined with tasks and ready to implement
- **2-IN_PROGRESS**: Feature currently being implemented
- **3-COMPLETED**: Feature done and tested
- **4-BLOCKED**: Dependencies preventing progress
- **5-SKIPPED**: Feature deferred or cancelled

#### Working with 0-SUBMITTED Features
When a feature arrives in `0-SUBMITTED` from API propagation:
1. Review the API contract and changes
2. Analyze impact on each client platform (Mobile, Web, Desktop)
3. Create platform-specific implementation tasks
4. Consider user experience differences across platforms
5. Add time estimates per platform
6. Move to `1-READY_TO_DEVELOP` when the team is ready

### 1. Feature Analysis & Planning
- Feature MUST already exist in `0-SUBMITTED` state
- Review feature requirements from `feature-description.md`
- Analyze platform-specific UI/UX implications (Mobile, Web, Desktop)
- Create a comprehensive implementation plan with tasks for each platform
- **MANDATORY**: Create `feature-tasks.md` in the existing feature folder
- Each task must be marked with status `[ReadyToDevelop]`
- Move feature folder from `0-SUBMITTED` to `1-READY_TO_DEVELOP`
- Tasks should be specific, actionable, and independently verifiable
- The task file must include:
  - Feature branch name at the top
  - Tasks organized by platform (Mobile, Web, Desktop) and then by logical categories
  - **Unit/Component test tasks immediately following each implementation task**
  - Clear description of what each task entails
  - Platform-specific considerations
  - Space for commit hashes to be added as tasks are completed

### 2. Branch Creation
- Create a dedicated feature branch from master
- Naming convention: `feature/[descriptive-feature-name]`
- All development work occurs in this isolated branch

### 3. Baseline Health Check (MANDATORY)
Before starting ANY implementation:
1. **Run baseline health check for each platform**:
   ```bash
   # Mobile
   npm run build
   npm test
   npm run lint

   # Web
   npm run build
   npm test
   npm run lint

   # Desktop
   dotnet build
   dotnet test
   ```
2. **Document results in feature-tasks.md**:
   ```markdown
   ## Baseline Health Check Report
   **Date/Time**: YYYY-MM-DD HH:MM
   **Branch**: feature/branch-name

   ### Mobile Platform
   #### Build Status
   - **Build Result**: ✅ Success / ❌ Failed / ⚠️ Success with warnings
   - **Warning Count**: X warnings
   - **Warning Details**: [List specific warnings if any]

   #### Test Status
   - **Total Tests**: X
   - **Passed**: X
   - **Failed**: X (MUST be 0 to proceed)
   - **Skipped/Ignored**: X

   #### Linting Status
   - **Errors**: X (MUST be 0 to proceed)
   - **Warnings**: X

   ### Web Platform
   [Similar structure as Mobile]

   ### Desktop Platform
   #### Build Status
   - **Build Result**: ✅ Success / ❌ Failed / ⚠️ Success with warnings
   - **Warning Count**: X warnings

   #### Test Status
   - **Total Tests**: X
   - **Passed**: X
   - **Failed**: X (MUST be 0 to proceed)

   ### Decision to Proceed
   - [ ] All platforms build successfully
   - [ ] All tests passing on all platforms
   - [ ] No linting errors
   - [ ] Warnings documented and approved

   **Approval to Proceed**: Yes/No
   ```

3. **Evaluation and Action**:
   - ✅ **All Green**: Proceed to implementation
   - ❌ **Build Fails**: STOP - Create Task 0.1 to fix build on affected platform
   - ❌ **Tests Fail**: STOP - Create Task 0.2 to fix failing tests
   - ❌ **Lint Errors**: STOP - Create Task 0.3 to fix linting errors
   - ⚠️ **Warnings Exist**: Document and ask for approval
     - If approved: Create Boy Scout tasks (0.4, 0.5, etc.) per platform
     - Complete Boy Scout tasks before feature tasks
     - Re-run baseline check after Boy Scout cleanup

### 4. Implementation Phase
- Execute tasks sequentially from the task tracking file
- **For EVERY task implementation:**
  1. Update task status to `[InProgress: Started: YYYY-MM-DD HH:MM]` when starting
  2. Write the implementation code
  3. **MANDATORY: Write unit/component tests for the implemented code in the immediately following task**
  4. **MANDATORY: Keep build warnings to a minimum** (address platform-specific warnings)
  5. **MANDATORY: Run platform-specific build commands:**
     - Mobile: `npm run build` or `npx react-native run-android/ios`
     - Web: `npm run build`
     - Desktop: `dotnet build`
  6. **MANDATORY: Run platform-specific tests:**
     - Mobile: `npm test`
     - Web: `npm test`
     - Desktop: `dotnet test`
  7. **MANDATORY: Ensure code follows platform standards (linting, formatting)**
  8. Only after build succeeds and ALL tests pass, commit the changes
  9. Update the task status to `[Implemented: <hash> | Started: <timestamp> | Finished: YYYY-MM-DD HH:MM | Duration: Xh Ym]`
- **For EVERY checkpoint:**
  1. Run platform-specific builds - BUILD MUST BE SUCCESSFUL (no errors)
  2. Run platform-specific tests - ALL TESTS MUST BE GREEN (no failures)
  3. Verify no excessive warnings exist
  4. **MANDATORY: Update checkpoint status from 🛑 to ✅ when all conditions pass**
- Follow platform-specific patterns and conventions
- The task tracking file serves as both documentation and audit trail
- **CRITICAL RULES**:
  - **NO broken builds between tasks** - each task must leave all platforms in a working state
  - **ALL tests must be green** after implementing a task (no skipped, no failures)
  - **Tests are written immediately after implementation** (not deferred to a later phase)
  - Never move to the next task if:
    - Any platform build is failing
    - Build warnings are excessive
    - Tests are not written
    - Any test is failing

### 5. Test Development & Handling
- Write unit tests for all business logic
- Write component/UI tests for all platforms
- Write platform-specific integration tests
- If a test requires complex setup:
  - Create a `[BUG: <detailed-reason>]` entry
  - Mark test appropriately per platform
  - Include bug reference in test comment

### 6. Manual Testing Phase (DEFAULT BEHAVIOR)
- After all implementation tasks are complete
- Provide user with platform-specific testing instructions:
  - **Mobile**: Device/emulator setup, gestures, navigation
  - **Web**: Browser compatibility, responsive breakpoints
  - **Desktop**: OS-specific behaviors, window management
- Include test scenarios covering all user flows
- Wait for user acceptance on all platforms before proceeding

### 7. Quality Comparison Report (MANDATORY)
After all implementation is complete, add to feature-tasks.md:
```markdown
## Implementation Summary Report
**Date/Time**: YYYY-MM-DD HH:MM
**Duration**: X days/hours

### Quality Metrics Comparison (Per Platform)

#### Mobile Platform
| Metric | Baseline | Final | Change |
|--------|----------|-------|--------|
| Build Warnings | X | Y | -Z |
| Test Count | X | Y | +Z |
| Test Pass Rate | X% | Y% | +Z% |
| Skipped Tests | X | Y | -Z |
| Lint Warnings | X | Y | -Z |

#### Web Platform
[Similar table]

#### Desktop Platform
[Similar table]

### Quality Improvements
- Fixed X build warnings across platforms
- Added Y new tests
- Removed Z skipped tests
- Fixed X platform-specific issues
- [Other improvements]

### Boy Scout Rule Applied
- ✅ All encountered issues fixed
- ✅ Code quality improved on all platforms
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
- **Platform-Specific Implementation**: Some features may not apply to all platforms
- **Skipping Manual Tests**: Only when user explicitly requests
- **Interrupted Implementation**: Next session can resume using existing task list
- **Git Operations**: All git operations require explicit user approval

## Task Status Definitions
- `[ReadyToDevelop]` - Task identified and ready to implement
- `[InProgress: Started: YYYY-MM-DD HH:MM]` - Task currently being worked on with start timestamp
- `[Implemented: <hash> | Started: YYYY-MM-DD HH:MM | Finished: YYYY-MM-DD HH:MM | Duration: Xh Ym]` - Task completed with timing data
- `[BUG: <reason>]` - Known issue requiring future resolution
- `[Skipped]` - Task deferred or determined unnecessary
- `[N/A - Platform]` - Task not applicable to specific platform
- `[INCOMPLETE: <reason>]` - Task cannot be completed due to external dependency
- `[BLOCKED: BUG-<id>]` - Task blocked by a specific bug

## Important Notes
- This process ensures code quality across all client platforms
- User maintains control over deployment and merge decisions
- Clear audit trail through commit hashes and bug tracking
- Supports both continuous and interrupted development workflows
- Platform-specific code should be clearly isolated

## Handling Blocked Features
When a feature cannot be completed due to external dependencies:
1. Mark the blocking task as `[BLOCKED: BUG-<id>]` with reference to the bug
2. Create a bug entry with platform information
3. Mark the overall feature as `[INCOMPLETE]` at the end of the feature file
4. When the bug is fixed, update statuses accordingly

## Implementation Verification Checklist

Before marking any task as `[Implemented]`, verify:

### All Platforms
- [ ] Implementation code is complete
- [ ] Unit/component tests are written (in the immediately following task)
- [ ] Build runs without errors
- [ ] Build warnings are minimal
- [ ] All tests pass (100% green)
- [ ] Code follows platform conventions
- [ ] No commented-out code or debug statements

### Mobile-Specific
- [ ] Works on both iOS and Android
- [ ] Handles different screen sizes
- [ ] Offline functionality works as expected
- [ ] Performance is acceptable on older devices

### Web-Specific
- [ ] Works across target browsers
- [ ] Responsive design is implemented
- [ ] Accessibility standards are met
- [ ] SEO considerations addressed

### Desktop-Specific
- [ ] Works on Windows, macOS, Linux
- [ ] Window management behaves correctly
- [ ] Native features integrate properly
- [ ] Performance meets desktop standards

## Task Tracking File Template

```markdown
# [Feature Name] Implementation Tasks

## Feature Branch: `feature/[branch-name]`
## Estimated Total Time: [X days / Y hours]
## Actual Total Time: [To be calculated at completion]

## Mobile Tasks (React Native) - Estimated: Xh

### API Integration - Estimated: Xh
- **Task M1.1:** Create [Name]Service for API calls `[ReadyToDevelop]` (Est: 45m)
- **Task M1.2:** Write unit tests for [Name]Service `[ReadyToDevelop]` (Est: 30m)
- **Task M1.3:** Implement offline caching `[ReadyToDevelop]` (Est: 1h)

### Components - Estimated: Xh
- **Task M2.1:** Create [Name]Screen component `[ReadyToDevelop]` (Est: 1.5h)
- **Task M2.2:** Write component tests for [Name]Screen `[ReadyToDevelop]` (Est: 1h)
- **Task M2.3:** Create [Name]Card component `[ReadyToDevelop]` (Est: 1h)
- **Task M2.4:** Write component tests for [Name]Card `[ReadyToDevelop]` (Est: 45m)

### Navigation - Estimated: Xh
- **Task M3.1:** Add navigation routes `[ReadyToDevelop]` (Est: 30m)
- **Task M3.2:** Implement deep linking `[ReadyToDevelop]` (Est: 45m)

### Platform-Specific - Estimated: Xh
- **Task M4.1:** iOS-specific styling adjustments `[ReadyToDevelop]` (Est: 30m)
- **Task M4.2:** Android-specific permissions `[ReadyToDevelop]` (Est: 45m)

## Web Tasks (React) - Estimated: Xh

### API Integration - Estimated: Xh
- **Task W1.1:** Create [Name]Service for API calls `[ReadyToDevelop]` (Est: 45m)
- **Task W1.2:** Write unit tests for [Name]Service `[ReadyToDevelop]` (Est: 30m)

### Components - Estimated: Xh
- **Task W2.1:** Create [Name]Page component `[ReadyToDevelop]` (Est: 1.5h)
- **Task W2.2:** Write component tests for [Name]Page `[ReadyToDevelop]` (Est: 1h)
- **Task W2.3:** Create responsive [Name]Grid `[ReadyToDevelop]` (Est: 2h)
- **Task W2.4:** Write component tests for [Name]Grid `[ReadyToDevelop]` (Est: 1h)

### State Management - Estimated: Xh
- **Task W3.1:** Implement Redux/Context state `[ReadyToDevelop]` (Est: 1h)
- **Task W3.2:** Write tests for state management `[ReadyToDevelop]` (Est: 45m)

## Desktop Tasks (Avalonia) - Estimated: Xh

### Services - Estimated: Xh
- **Task D1.1:** Create [Name]Service for API integration `[ReadyToDevelop]` (Est: 45m)
- **Task D1.2:** Write unit tests for [Name]Service `[ReadyToDevelop]` (Est: 30m)

### ViewModels - Estimated: Xh
- **Task D2.1:** Create [Name]ViewModel with ReactiveUI `[ReadyToDevelop]` (Est: 1h)
- **Task D2.2:** Write tests for [Name]ViewModel `[ReadyToDevelop]` (Est: 45m)

### Views - Estimated: Xh
- **Task D3.1:** Create [Name]View XAML layout `[ReadyToDevelop]` (Est: 1h)
- **Task D3.2:** Implement data bindings `[ReadyToDevelop]` (Est: 30m)

### Platform Integration - Estimated: Xh
- **Task D4.1:** Implement native menu integration `[ReadyToDevelop]` (Est: 45m)
- **Task D4.2:** Add keyboard shortcuts `[ReadyToDevelop]` (Est: 30m)

## Shared Tasks (If Applicable) - Estimated: Xh

### Shared Models/Logic - Estimated: Xh
- **Task S1.1:** Create shared data models `[ReadyToDevelop]` (Est: 30m)
- **Task S1.2:** Write tests for shared models `[ReadyToDevelop]` (Est: 30m)

## Checkpoints
- **Mobile Checkpoint:** After Mobile tasks - All mobile tests green 🛑
- **Web Checkpoint:** After Web tasks - All web tests green 🛑
- **Desktop Checkpoint:** After Desktop tasks - All desktop tests green 🛑
- **Final Checkpoint:** All platforms tested, builds clean 🛑

## Time Tracking Summary
- **Total Estimated Time:** [Sum of all estimates]
- **Total Actual Time:** [To be calculated from task durations]
- **AI Assistance Impact:** [% reduction in time]
- **Implementation Started:** [First task start time]
- **Implementation Completed:** [Last task finish time]

## Notes
- Tasks prefixed with M (Mobile), W (Web), D (Desktop), S (Shared)
- Each implementation task must be immediately followed by its test task
- Platform-specific tasks may be marked [N/A - Platform] if not applicable
- Time estimates are for a developer without AI assistance
```

## Example Task Status Updates

- `[ReadyToDevelop]` - Initial status for all tasks
- `[InProgress: Started: 2025-01-15 14:30]` - Task being actively worked on
- `[Implemented: a1b2c3d4 | Started: 2025-01-15 14:30 | Finished: 2025-01-15 15:15 | Duration: 0h 45m]` - Completed task
- `[BUG: React Native navigation issue on Android]` - Platform-specific bug
- `[N/A - Desktop]` - Task not applicable to desktop platform
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
Task M2.1: Create UserProfile screen component 
[Implemented: abc123 | Started: 2025-01-15 09:00 | Finished: 2025-01-16 11:30 | Duration: 3h 15m]
Note: Work sessions: Jan 15 (09:00-10:30), Jan 16 (10:00-11:30)
```

### AI Impact Calculation
At feature completion, calculate:
- Sum all estimated times
- Sum all actual durations
- AI Impact = ((Estimated - Actual) / Estimated) × 100%
- Document any factors that affected the comparison
- Track platform-specific efficiency differences

## Platform-Specific Considerations

### Mobile (React Native)
- Test on both iOS and Android devices/emulators
- Handle platform differences (navigation, permissions, styling)
- Implement proper offline support
- Optimize for battery and performance
- Handle different screen sizes and orientations

### Web (React)
- Test across major browsers
- Implement responsive design for all breakpoints
- Ensure accessibility compliance
- Optimize for SEO where applicable
- Handle browser-specific features gracefully

### Desktop (Avalonia)
- Test on Windows, macOS, and Linux
- Implement native OS integrations
- Handle window management properly
- Provide keyboard shortcuts
- Ensure performance for desktop usage patterns

### Code Sharing
- Identify opportunities for shared logic
- Keep platform-specific code isolated
- Use dependency injection for platform services
- Maintain consistent API contracts across platforms