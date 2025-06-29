# Feature Implementation Process - API Project

This document outlines the standard process for implementing new features in the GetFitterGetBigger API.

## Process Overview

### 1. Feature Analysis & Planning
- User provides detailed feature requirements and API specifications
- Create a comprehensive implementation plan with granular tasks
- **MANDATORY**: Create a task tracking file at `memory-bank/features/[feature-name]-tasks.md`
- Each task must be marked with status `[ReadyToDevelop]`
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

### 3. Implementation Phase
- Execute tasks sequentially from the task tracking file
- **For EVERY task implementation:**
  1. Write the implementation code
  2. **MANDATORY: Write unit tests for the implemented code in the immediately following task**
  3. **MANDATORY: Keep build warnings to a minimum** (address nullable warnings, unused variables, etc.)
  4. **MANDATORY: Run `dotnet build` to ensure compilation succeeds with minimal warnings**
  5. **MANDATORY: Run `dotnet test` to ensure ALL tests pass (100% green)**
  6. Only after build succeeds and ALL tests pass, commit the changes
  7. Update the task status in the tracking file from `[ReadyToDevelop]` to `[Implemented: <git-commit-hash>]`
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

### 4. Test Development & Handling
- Write unit tests for all business logic
- Write integration tests for API endpoints
- Write repository tests for data access
- If a test requires complex mocking or setup:
  - Create a `[BUG: <detailed-reason>]` entry
  - Mark test with skip attribute
  - Include bug reference in test comment
  - Example: `[Fact(Skip = "BUG-001: Complex database mocking requires additional setup")]`

### 5. Manual Testing Phase (DEFAULT BEHAVIOR)
- After all implementation tasks are complete
- Provide user with:
  - Detailed step-by-step testing instructions
  - Test scenarios covering all endpoints
  - Expected API responses
  - Sample data for testing
- Wait for user acceptance before proceeding

### 6. Feature Finalization
After user explicitly states feature acceptance:
1. Create descriptive commit message summarizing all changes
2. Push feature branch to remote repository
3. Merge feature branch into master locally
4. **MANDATORY: Push the merged master branch to remote repository**
5. Delete the feature branch locally
6. Optionally delete the feature branch on remote

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
- [ ] Build warnings are minimal (no nullable warnings, unused variables, etc.)
- [ ] `dotnet test` runs with ALL tests passing (100% green, no skipped tests)
- [ ] Code follows .NET best practices and project conventions
- [ ] No commented-out code or debug statements
- [ ] The API is in a working state (no broken endpoints)

## Task Tracking File Template

```markdown
# [Feature Name] Implementation Tasks

## Feature Branch: `feature/[branch-name]`

### Category 1 (e.g., Models & DTOs)
- **Task 1.1:** Create [Name] entity model `[ReadyToDevelop]`
- **Task 1.2:** Create [Name]Dto and request/response models `[ReadyToDevelop]`
- **Task 1.3:** Write unit tests for DTOs `[ReadyToDevelop]`

### Category 2 (e.g., Repository Layer)
- **Task 2.1:** Create I[Name]Repository interface `[ReadyToDevelop]`
- **Task 2.2:** Implement [Name]Repository `[ReadyToDevelop]`
- **Task 2.3:** Write repository unit tests `[ReadyToDevelop]`

### Category 3 (e.g., Service Layer)
- **Task 3.1:** Create I[Name]Service interface `[ReadyToDevelop]`
- **Task 3.2:** Implement [Name]Service with business logic `[ReadyToDevelop]`
- **Task 3.3:** Write service unit tests `[ReadyToDevelop]`

### Category 4 (e.g., Controller)
- **Task 4.1:** Create [Name]Controller with CRUD endpoints `[ReadyToDevelop]`
- **Task 4.2:** Add authorization and validation `[ReadyToDevelop]`
- **Task 4.3:** Write controller unit tests `[ReadyToDevelop]`
- **Task 4.4:** Write integration tests for endpoints `[ReadyToDevelop]`

### Category 5 (e.g., Database & Migrations)
- **Task 5.1:** Add entity configuration `[ReadyToDevelop]`
- **Task 5.2:** Create database migration `[ReadyToDevelop]`
- **Task 5.3:** Add seed data if needed `[ReadyToDevelop]`

## Notes
- Each implementation task must be immediately followed by its test task
- No task is complete until build passes and all tests are green
- Keep build warnings to minimum
- Follow existing API patterns and conventions
```

## Example Task Status Updates

- `[ReadyToDevelop]` - Initial status for all tasks
- `[Implemented: a1b2c3d4]` - Task completed with commit hash
- `[BUG: Complex mock setup for external service]` - Known issue to be addressed later
- `[Skipped]` - Task determined unnecessary during implementation

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