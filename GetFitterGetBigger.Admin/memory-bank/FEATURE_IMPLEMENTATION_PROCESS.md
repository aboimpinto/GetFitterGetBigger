# Feature Implementation Process - Admin Project

This document outlines the standard process for implementing new features in the GetFitterGetBigger Admin web application.

## Process Overview

### 1. Feature Analysis & Planning
- User provides detailed feature requirements and UI/UX specifications
- Create a comprehensive implementation plan with granular tasks
- **MANDATORY**: Create a task tracking file at `memory-bank/features/[feature-name]-tasks.md`
- Each task must be marked with status `[ReadyToDevelop]`
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

### 3. Implementation Phase
- Execute tasks sequentially from the task tracking file
- **For EVERY task implementation:**
  1. Write the implementation code
  2. **MANDATORY: Write unit/component tests for the implemented code in the immediately following task**
  3. **MANDATORY: Keep build warnings to a minimum** (address TypeScript errors, unused variables, etc.)
  4. **MANDATORY: Run `npm run build` to ensure compilation succeeds with minimal warnings**
  5. **MANDATORY: Run `npm test` to ensure ALL tests pass (100% green)**
  6. **MANDATORY: Run `npm run lint` to ensure code follows project standards**
  7. Only after build succeeds and ALL tests pass, commit the changes
  8. Update the task status in the tracking file from `[ReadyToDevelop]` to `[Implemented: <git-commit-hash>]`
- **For EVERY checkpoint:**
  1. Run `dotnet build` - BUILD MUST BE SUCCESSFUL (no errors)
  2. Run `dotnet test` - ALL TESTS MUST BE GREEN (no failures)
  3. Verify no compiler warnings exist
  4. **MANDATORY: Update checkpoint status from 🛑 to ✅ when all conditions pass**
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

### 4. Test Development & Handling
- Write unit tests for all business logic
- Write component tests for all React components
- Write integration tests for API interactions
- If a test requires complex mocking or setup:
  - Create a `[BUG: <detailed-reason>]` entry
  - Mark test with skip attribute
  - Include bug reference in test comment
  - Example: `test.skip('BUG-001: Complex API mocking requires additional setup')`

### 5. Manual Testing Phase (DEFAULT BEHAVIOR)
- After all implementation tasks are complete
- Provide user with:
  - Detailed step-by-step testing instructions
  - Test scenarios covering all user flows
  - Expected UI behaviors and responses
  - Sample data for testing
- Wait for user acceptance before proceeding

### 6. Feature Finalization
After user explicitly states feature acceptance:
1. Merge feature branch into master
2. Create descriptive commit message summarizing all changes
3. Push to remote repository
4. Delete the feature branch locally

### 7. Special Conditions
- **Skipping Manual Tests**: Only when user explicitly requests during initial feature specification
- **Interrupted Implementation**: Next session can resume using existing task list with current statuses
- **Git Operations**: All git operations require explicit user approval and are not automated

## Task Status Definitions
- `[ReadyToDevelop]` - Task identified and ready to implement
- `[Implemented: <hash>]` - Task completed with reference commit
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
- [ ] `npm run build` runs without errors
- [ ] Build warnings are minimal (no TypeScript errors, unused variables, etc.)
- [ ] `npm test` runs with ALL tests passing (100% green, no skipped tests)
- [ ] `npm run lint` passes without errors
- [ ] Code follows React best practices and project conventions
- [ ] No commented-out code or debug statements
- [ ] The application is in a working state (no broken functionality)
- [ ] UI is responsive and follows design guidelines

## Task Tracking File Template

```markdown
# [Feature Name] Implementation Tasks

## Feature Branch: `feature/[branch-name]`

### Category 1 (e.g., API Service Layer)
- **Task 1.1:** Create [Name]Service for API integration `[ReadyToDevelop]`
- **Task 1.2:** Write unit tests for [Name]Service `[ReadyToDevelop]`
- **Task 1.3:** Add error handling and retry logic `[ReadyToDevelop]`

### Category 2 (e.g., State Management)
- **Task 2.1:** Create Redux/Context state for [feature] `[ReadyToDevelop]`
- **Task 2.2:** Implement actions and reducers `[ReadyToDevelop]`
- **Task 2.3:** Write tests for state management `[ReadyToDevelop]`

### Category 3 (e.g., Components)
- **Task 3.1:** Create [Name]List component with pagination `[ReadyToDevelop]`
- **Task 3.2:** Write component tests for [Name]List `[ReadyToDevelop]`
- **Task 3.3:** Create [Name]Form component with validation `[ReadyToDevelop]`
- **Task 3.4:** Write component tests for [Name]Form `[ReadyToDevelop]`
- **Task 3.5:** Create [Name]Detail component `[ReadyToDevelop]`
- **Task 3.6:** Write component tests for [Name]Detail `[ReadyToDevelop]`

### Category 4 (e.g., Pages & Routing)
- **Task 4.1:** Create [Name]Page container component `[ReadyToDevelop]`
- **Task 4.2:** Set up routing for [feature] pages `[ReadyToDevelop]`
- **Task 4.3:** Write integration tests for pages `[ReadyToDevelop]`

### Category 5 (e.g., UI/UX Polish)
- **Task 5.1:** Add loading states and skeletons `[ReadyToDevelop]`
- **Task 5.2:** Implement error boundaries and messages `[ReadyToDevelop]`
- **Task 5.3:** Add animations and transitions `[ReadyToDevelop]`
- **Task 5.4:** Ensure responsive design across breakpoints `[ReadyToDevelop]`

## Notes
- Each implementation task must be immediately followed by its test task
- No task is complete until build passes and all tests are green
- Keep build warnings to minimum
- Follow existing UI patterns and component library
```

## Example Task Status Updates

- `[ReadyToDevelop]` - Initial status for all tasks
- `[Implemented: a1b2c3d4]` - Task completed with commit hash
- `[BUG: Complex mock setup for API service]` - Known issue to be addressed later
- `[Skipped]` - Task determined unnecessary during implementation

## Admin-Specific Considerations

### UI/UX Standards
- Follow existing Tailwind CSS patterns
- Use existing component library
- Ensure accessibility (ARIA labels, keyboard navigation)
- Test on different screen sizes

### API Integration
- Use centralized API service
- Handle authentication tokens properly
- Implement proper error handling
- Show loading states during API calls

### State Management
- Use existing state management pattern (Redux/Context)
- Keep state normalized
- Implement optimistic updates where appropriate

### Performance
- Implement code splitting for large features
- Use React.memo for expensive components
- Lazy load images and heavy components
- Implement proper pagination for lists