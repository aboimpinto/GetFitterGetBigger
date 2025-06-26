# Feature Implementation Process

This document outlines the standard process for implementing new features in the GetFitterGetBigger API project.

## Process Overview

### 1. Feature Analysis & Planning
- User provides detailed feature requirements and specifications
- Create a comprehensive implementation plan with granular tasks
- **MANDATORY**: Create a task tracking file at `memory-bank/features/[feature-name]-tasks.md`
- Each task must be marked with status `[ReadyToDevelop]`
- Tasks should be specific, actionable, and independently verifiable
- The task file must include:
  - Feature branch name at the top
  - Tasks organized by logical categories (Database, Repository, Service, Controller, etc.)
  - **Unit test tasks immediately following each implementation task** (e.g., Task 2.1: Create Repository, Task 2.2: Write tests for Repository)
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
  2. **MANDATORY: Write unit tests for the implemented code in the immediately following task**
  3. **MANDATORY: Keep build warnings to a minimum** (address nullable reference warnings, unused variables, etc.)
  4. **MANDATORY: Run `dotnet build` to ensure compilation succeeds with minimal warnings**
  5. **MANDATORY: Run `dotnet test` to ensure ALL tests pass (100% green)**
  6. Only after build succeeds and ALL tests pass, commit the changes
  7. Update the task status in the tracking file from `[ReadyToDevelop]` to `[Implemented: <git-commit-hash>]`
- Follow existing code conventions and patterns
- The task tracking file serves as both documentation and audit trail
- **CRITICAL RULES**:
  - **NO broken builds between tasks** - each task must leave the codebase in a working state
  - **ALL tests must be green** after implementing a task (no skipped, no failures)
  - **Tests are written immediately after implementation** (not deferred to a later phase)
  - Never move to the next task if:
    - The build is failing
    - Build warnings are excessive
    - Tests are not written
    - Any test is failing

### 4. Test Development & Handling
- Write unit tests for all new functionality
- If a test requires more than 2-3 interactions to implement:
  - Create a `[BUG: <detailed-reason>]` entry
  - Mark test with Skip/Ignore attribute
  - Include bug reference in test comment
  - Example: `[Skip("BUG-001: Complex mocking scenario requires additional framework setup")]`

### 5. Manual Testing Phase (DEFAULT BEHAVIOR)
- After all implementation tasks are complete
- Provide user with:
  - Detailed step-by-step testing instructions
  - Expected outcomes for each test scenario
  - Sample data or commands if applicable
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
- [ ] Unit tests are written for the new code (in the immediately following task)
- [ ] `dotnet build` runs without errors
- [ ] Build warnings are minimal (no nullable reference warnings, unused variables, etc.)
- [ ] `dotnet test` runs with ALL tests passing (100% green, no skipped tests)
- [ ] Code follows project conventions
- [ ] No commented-out code or debug statements
- [ ] The codebase is in a working state (no broken functionality)

## Task Tracking File Template

```markdown
# [Feature Name] Implementation Tasks

## Feature Branch: `feature/[branch-name]`

### Category 1 (e.g., Database & Entity Setup)
- **Task 1.1:** Create [Entity] with required fields `[ReadyToDevelop]`
- **Task 1.2:** Create [Entity] unit tests `[ReadyToDevelop]`
- **Task 1.3:** Update DbContext configuration `[ReadyToDevelop]`

### Category 2 (e.g., Repository Layer)
- **Task 2.1:** Create I[Name]Repository interface `[ReadyToDevelop]`
- **Task 2.2:** Implement [Name]Repository with:
  - Method1 implementation
  - Method2 implementation `[ReadyToDevelop]`
- **Task 2.3:** Write unit tests for [Name]Repository including:
  - Test scenario 1
  - Test scenario 2 `[ReadyToDevelop]`

### Category 3 (e.g., Service Layer)
- **Task 3.1:** Create I[Name]Service interface `[ReadyToDevelop]`
- **Task 3.2:** Implement [Name]Service with business logic `[ReadyToDevelop]`
- **Task 3.3:** Write unit tests for [Name]Service `[ReadyToDevelop]`

### Category 4 (e.g., Controller)
- **Task 4.1:** Create [Name]Controller with endpoints `[ReadyToDevelop]`
- **Task 4.2:** Write integration tests for [Name]Controller `[ReadyToDevelop]`
- **Task 4.3:** Add authorization and Swagger docs `[ReadyToDevelop]`

## Notes
- Each implementation task must be immediately followed by its test task
- No task is complete until build passes and all tests are green
- Keep build warnings to minimum
```

## Example Task Status Updates

- `[ReadyToDevelop]` - Initial status for all tasks
- `[Implemented: a1b2c3d4]` - Task completed with commit hash
- `[BUG: Complex mocking scenario]` - Known issue to be addressed later
- `[Skipped]` - Task determined unnecessary during implementation