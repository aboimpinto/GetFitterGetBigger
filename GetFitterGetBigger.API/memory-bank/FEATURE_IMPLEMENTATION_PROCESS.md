# Feature Implementation Process - API Project

This document outlines the standard process for implementing new features in the GetFitterGetBigger API.

## Process Overview

### File Management Rules

**⚠️ IMPORTANT: Maintain focus on REQUIRED files only! ⚠️**

When implementing features:
- **Only create files explicitly required** by this process
- **DO NOT add extra documentation files** like README.md, notes.md, etc.
- **feature-description.md and feature-tasks.md** contain all necessary documentation
- **Avoid redundancy** - if information exists in required files, don't duplicate it elsewhere
- **Supporting files** (like test scripts, API specs) are allowed ONLY if they add unique value

Remember: Clean, focused file structure makes features easier to track and maintain.

### 0. Feature States (Pre-Implementation)
Features progress through these workflow states:
- **0-SUBMITTED**: MANDATORY starting point for ALL features
- **1-READY_TO_DEVELOP**: Feature fully planned with tasks defined
- **2-IN_PROGRESS**: Feature currently being implemented
- **3-COMPLETED**: Feature done and tested
- **4-BLOCKED**: Dependencies preventing progress
- **5-SKIPPED**: Feature deferred or cancelled

**IMPORTANT**: Every feature MUST start in 0-SUBMITTED state, even in the API project. This ensures consistent workflow tracking across all projects.

### 1. Feature Analysis & Planning
- Feature MUST already exist in `0-SUBMITTED` state
- Review feature requirements from `feature-description.md`
- Create a comprehensive implementation plan with granular tasks
- **MANDATORY**: Create `feature-tasks.md` in the existing feature folder
- Each task must be marked with status `[ReadyToDevelop]`
- Move feature folder from `0-SUBMITTED` to `1-READY_TO_DEVELOP`
- Tasks should be specific, actionable, and independently verifiable
- The task file must include:
  - Feature branch name at the top
  - Tasks organized by logical categories (Models, Repository, Service, Controller, etc.)
  - **Unit test tasks immediately following each implementation task**
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

   ### Decision to Proceed
   - [ ] All tests passing
   - [ ] Build successful
   - [ ] Warnings documented and approved

   **Approval to Proceed**: Yes/No
   ```

3. **Evaluation and Action**:
   - ✅ **All Green**: Proceed to implementation
   - ❌ **Build Fails**: STOP - Create Task 0.1 to fix build
   - ❌ **Tests Fail**: STOP - Create Task 0.2 to fix failing tests
   - ⚠️ **Warnings Exist**: Document and ask for approval
     - If approved: Create Boy Scout tasks (0.3, 0.4, etc.) to fix warnings FIRST
     - Complete Boy Scout tasks before feature tasks
     - Re-run baseline check after Boy Scout cleanup

### 4. Implementation Phase
- Execute tasks sequentially from the task tracking file
- **For EVERY task implementation:**
  1. Update task status to `[InProgress: Started: YYYY-MM-DD HH:MM]` when starting
  2. Write the implementation code
     - **⚠️ CRITICAL: Check `common-implementation-pitfalls.md` before implementing services**
     - **ALWAYS use ReadOnlyUnitOfWork for validation queries**
     - **ONLY use WritableUnitOfWork for actual data modifications**
  3. **MANDATORY: Write unit tests for the implemented code in the immediately following task**
  4. **MANDATORY: Keep build warnings to a minimum** (address nullable warnings, unused variables, etc.)
  5. **MANDATORY: Run `dotnet build` to ensure compilation succeeds with minimal warnings**
  6. **MANDATORY: Run `dotnet test` to ensure ALL tests pass (100% green)**
  7. Only after build succeeds and ALL tests pass, commit the changes
  8. Update the task status to `[Implemented: <hash> | Started: <timestamp> | Finished: YYYY-MM-DD HH:MM | Duration: Xh Ym]`
- **For EVERY checkpoint:**
  1. Run `dotnet build` - BUILD MUST BE SUCCESSFUL (no errors)
  2. Run `dotnet test` - ALL TESTS MUST BE GREEN (no failures)
  3. Verify no build warnings exist
  4. **MANDATORY: Update checkpoint status from 🛑 to ✅ when all conditions pass**
- Follow existing API patterns and conventions
- Use dependency injection and repository patterns
- The task tracking file serves as both documentation and audit trail
- **CRITICAL RULES**:
  - **NO broken builds between tasks** - each task must leave the API in a working state
  - **ALL tests must be green** after implementing a task (no skipped, no failures)
  - **Tests are written immediately after implementation** (not deferred to a later phase)
  - Never move to the next task if:
    - The build is failing
    - Compiler warnings are excessive
    - Tests are not written
    - Any test is failing

### 5. Test Development & Handling
- Write unit tests for all business logic
- Write integration tests for API endpoints
- Write repository tests for data access
- If a test requires complex mocking or setup:
  - Create a `[BUG: <detailed-reason>]` entry
  - Mark test with skip attribute
  - Include bug reference in test comment
  - Example: `[Fact(Skip = "BUG-001: Complex database mocking requires additional setup")]`

### 6. Manual Testing Phase (DEFAULT BEHAVIOR)
- After all implementation tasks are complete
- Provide user with:
  - Detailed step-by-step testing instructions
  - Test scenarios covering all endpoints
  - Expected API responses
  - Sample data for testing
- Wait for user acceptance before proceeding

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

### Quality Improvements
- Fixed X build warnings
- Added Y new tests
- Removed Z skipped tests
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
- [ ] Unit tests are written for the new code (in the immediately following task)
- [ ] `dotnet build` runs without errors
- [ ] Build warnings are minimal (no nullable warnings, unused variables, etc.)
- [ ] `dotnet test` runs with ALL tests passing (100% green, no skipped tests)
- [ ] Code follows .NET best practices and project conventions
- [ ] No commented-out code or debug statements
- [ ] The API is in a working state (no broken endpoints)

## Task Tracking File Template

```markdown
# [Feature Name] Implementation Tasks

## Feature Branch: `feature/[branch-name]`
## Estimated Total Time: [X days / Y hours]
## Actual Total Time: [To be calculated at completion]

## 📚 Pre-Implementation Checklist
- [ ] Read `/memory-bank/systemPatterns.md` - Architecture rules
- [ ] Read `/memory-bank/unitOfWorkPattern.md` - Critical ReadOnly vs Writable patterns
- [ ] Read `/memory-bank/common-implementation-pitfalls.md` - Common mistakes to avoid
- [ ] Run baseline health check (`dotnet build` and `dotnet test`)

### Category 1 (e.g., Models & DTOs) - Estimated: Xh
#### 📖 Before Starting: Review entity pattern in `/memory-bank/databaseModelPattern.md`
- **Task 1.1:** Create [Name] entity model `[ReadyToDevelop]` (Est: 30m)
- **Task 1.2:** Create [Name]Dto and request/response models `[ReadyToDevelop]` (Est: 45m)
- **Task 1.3:** Write unit tests for DTOs `[ReadyToDevelop]` (Est: 30m)

### Category 2 (e.g., Repository Layer) - Estimated: Xh
#### 📖 Before Starting: Review repository patterns in `/memory-bank/unitOfWorkPattern.md`
- **Task 2.1:** Create I[Name]Repository interface `[ReadyToDevelop]` (Est: 15m)
- **Task 2.2:** Implement [Name]Repository `[ReadyToDevelop]` (Est: 1h)
- **Task 2.3:** Write repository unit tests `[ReadyToDevelop]` (Est: 1h)

### Category 3 (e.g., Service Layer) - Estimated: Xh
#### ⚠️ CRITICAL Before Starting: 
- [ ] Re-read `/memory-bank/common-implementation-pitfalls.md` Section 1
- [ ] Remember: ReadOnlyUnitOfWork for validation, WritableUnitOfWork for modifications ONLY
- **Task 3.1:** Create I[Name]Service interface `[ReadyToDevelop]` (Est: 15m)
- **Task 3.2:** Implement [Name]Service with business logic `[ReadyToDevelop]` (Est: 2h)
- **Task 3.3:** Write service unit tests `[ReadyToDevelop]` (Est: 1.5h)

### Category 4 (e.g., Controller) - Estimated: Xh
#### 📖 Before Starting: Review controller rules - NO direct repository/UnitOfWork access!
- **Task 4.1:** Create [Name]Controller with CRUD endpoints `[ReadyToDevelop]` (Est: 1.5h)
- **Task 4.2:** Add authorization and validation `[ReadyToDevelop]` (Est: 45m)
- **Task 4.3:** Write controller unit tests `[ReadyToDevelop]` (Est: 1.5h)
- **Task 4.4:** Write integration tests for endpoints `[ReadyToDevelop]` (Est: 2h)

### Category 5 (e.g., Database & Migrations) - Estimated: Xh
- **Task 5.1:** Add entity configuration `[ReadyToDevelop]` (Est: 30m)
- **Task 5.2:** Create database migration `[ReadyToDevelop]` (Est: 15m)
- **Task 5.3:** Add seed data if needed `[ReadyToDevelop]` (Est: 30m)

## 🔄 Mid-Implementation Checkpoint
- [ ] All tests still passing (`dotnet test`)
- [ ] Build has no errors (`dotnet build`)
- [ ] Re-read `/memory-bank/common-implementation-pitfalls.md` if any issues
- [ ] Verify correct UnitOfWork usage in all services

## Time Tracking Summary
- **Total Estimated Time:** [Sum of all estimates]
- **Total Actual Time:** [To be calculated from task durations]
- **AI Assistance Impact:** [% reduction in time]

## Notes
- Each implementation task must be immediately followed by its test task
- No task is complete until build passes and all tests are green
- Keep build warnings to minimum
- Follow existing API patterns and conventions
- Time estimates are for a developer without AI assistance
```

## Example Task Status Updates

- `[ReadyToDevelop]` - Initial status for all tasks
- `[InProgress: Started: 2025-01-15 14:30]` - Task being actively worked on
- `[Implemented: a1b2c3d4 | Started: 2025-01-15 14:30 | Finished: 2025-01-15 15:15 | Duration: 0h 45m]` - Completed task
- `[BUG: Complex mock setup for external service]` - Known issue to be addressed later
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
Task 2.2: Implement UserRepository 
[Implemented: abc123 | Started: 2025-01-15 09:00 | Finished: 2025-01-16 11:30 | Duration: 3h 15m]
Note: Work sessions: Jan 15 (09:00-10:30), Jan 16 (10:00-11:30)
```

### AI Impact Calculation
At feature completion, calculate:
- Sum all estimated times
- Sum all actual durations
- AI Impact = ((Estimated - Actual) / Estimated) × 100%
- Document any factors that affected the comparison

## API-Specific Considerations

### API Standards
- Follow RESTful conventions
- Use proper HTTP status codes
- Implement consistent error responses
- Document endpoints with XML comments for Swagger

### Dependency Injection
- Register all services in Program.cs
- Use appropriate service lifetimes
- Follow interface-based design

### Database Patterns
- Use Entity Framework Core with code-first approach
- Implement repository pattern
- Use unit of work pattern where appropriate

### Security
- Implement proper authentication/authorization
- Validate all inputs
- Sanitize data before persistence
- Never expose internal implementation details in errors

### Performance
- Implement caching where appropriate
- Use async/await for all I/O operations
- Implement pagination for list endpoints
- Consider using projection for large entities