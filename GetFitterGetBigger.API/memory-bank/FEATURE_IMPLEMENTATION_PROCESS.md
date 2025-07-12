# Feature Implementation Process - API Project

This document outlines the standard process for implementing new features in the GetFitterGetBigger Minimal API.

**📌 NOTE**: This document extends the [Unified Development Process](UNIFIED_DEVELOPMENT_PROCESS.md) with API-specific implementation details. Always refer to the unified process for core principles and mandatory requirements.

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

#### Working with Features

**API-Originated Features** (new business functionality):
1. Create feature in `0-SUBMITTED` with business requirements
2. Design API contract and endpoints
3. Create implementation tasks
4. Move to `1-READY_TO_DEVELOP` when ready
5. After implementation, propagate to Admin and Clients projects

**UI-Requested Features** (from Admin or Clients):
1. Feature arrives in `0-SUBMITTED` from UI project request
2. Analyze data requirements and design endpoints
3. Create API-specific implementation tasks
4. Move to `1-READY_TO_DEVELOP` when ready
5. Notify requesting project when implementation complete

**IMPORTANT**: Every feature MUST start in 0-SUBMITTED state. This ensures consistent workflow tracking and proper feature propagation.

### 1. Feature Analysis & Planning
- Feature MUST already exist in `0-SUBMITTED` state
- Review feature requirements from `feature-description.md`
- Create a comprehensive implementation plan with granular tasks
- **MANDATORY**: Create `feature-tasks.md` in the existing feature folder
- **MANDATORY**: Define BDD scenarios for all API endpoints and business rules
- Each task must be marked with status `[ReadyToDevelop]`
- Move feature folder from `0-SUBMITTED` to `1-READY_TO_DEVELOP`
- Tasks should be specific, actionable, and independently verifiable
- The task file must include:
  - Feature branch name at the top
  - **BDD Test Scenarios section** (MANDATORY - see template below)
  - Tasks organized by logical categories (Models, Repository, Service, Controller, etc.)
  - **Unit test tasks immediately following each implementation task**
  - **BDD integration test tasks for each scenario defined**
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
   dotnet clean
   dotnet build
   dotnet test
   ```
   **Important**: Always run `dotnet clean` first to ensure all warnings are visible
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
  5. **MANDATORY: Run `dotnet clean && dotnet build` to ensure compilation succeeds with minimal warnings**
     - Always run `dotnet clean` first to catch all warnings that might be hidden by incremental builds
  6. **MANDATORY: Run `dotnet test` to ensure ALL tests pass (100% green)**
  7. Only after build succeeds and ALL tests pass, commit the changes
  8. Update the task status to `[Implemented: <hash> | Started: <timestamp> | Finished: YYYY-MM-DD HH:MM | Duration: Xh Ym | Est: Xh Ym]`
     - Calculate duration immediately when marking as finished
     - Include original estimate for comparison
- **For EVERY checkpoint:**
  1. Run `dotnet clean && dotnet build` - BUILD MUST BE SUCCESSFUL (no errors)
     - The `clean` ensures all warnings are visible (incremental builds can hide warnings)
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
- **Unit Tests (API.Tests project)**:
  - **CRITICAL**: Test methods in complete isolation with EVERYTHING else mocked
  - **MANDATORY**: Mock ALL dependencies (repositories, services, validators, loggers, utilities)
  - **RULE**: If testing class A, only class A is real - EVERYTHING else is mocked
  - Focus on business logic verification of single methods only
  - Verify method interactions with mocks
- **Integration Tests (API.IntegrationTests project)**:
  - **MANDATORY**: Use BDD format with Gherkin scenarios
  - Test complete API workflows with real database
  - Verify database persistence and transactions
  - Test with real implementations and TestContainers
- **Test Separation Rules**:
  - ANY test requiring database → API.IntegrationTests
  - ANY test using real services → API.IntegrationTests
  - ANY test testing multiple classes together → API.IntegrationTests
  - ONLY isolated unit tests with everything mocked → API.Tests
- If a test requires complex mocking or setup:
  - Create a `[BUG: <detailed-reason>]` entry
  - Mark test with skip attribute
  - Include bug reference in test comment
  - Example: `[Fact(Skip = "BUG-001: Complex database mocking requires additional setup")]`

### 6. Manual Testing Phase (MANDATORY)
**🚨 MANDATORY for ALL features - NO EXCEPTIONS 🚨**

- After all implementation tasks are complete
- Provide user with:
  - Detailed step-by-step testing instructions
  - Test scenarios covering all endpoints
  - Expected API responses
  - Sample data for testing
  - Postman collection or curl commands
- Wait for explicit user acceptance before proceeding

**Note**: Manual testing can ONLY be skipped if the user explicitly requests it at feature start AND the feature is purely technical with comprehensive automated test coverage.

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

### 9. Feature Completion Reports (MANDATORY)
After user explicitly accepts the feature, create the following reports in the feature folder:

1. **COMPLETION-REPORT.md** - Comprehensive feature summary
2. **TECHNICAL-SUMMARY.md** - Technical implementation details
3. **LESSONS-LEARNED.md** - Insights and recommendations
4. **QUICK-REFERENCE.md** - Quick usage guide

These reports are MANDATORY before moving the feature to COMPLETED status. See the Completion Report Templates section below for detailed templates.

### 10. Special Conditions
- **Skipping Manual Tests**: Only when user explicitly requests during initial feature specification
- **Interrupted Implementation**: Next session can resume using existing task list with current statuses
- **Git Operations**: All git operations require explicit user approval and are not automated

## Task Status Definitions
- `[ReadyToDevelop]` - Task identified and ready to implement
- `[InProgress: Started: YYYY-MM-DD HH:MM]` - Task currently being worked on with start timestamp
- `[Implemented: <hash> | Started: YYYY-MM-DD HH:MM | Finished: YYYY-MM-DD HH:MM | Duration: Xh Ym | Est: Xh Ym]` - Task completed with timing data
  - Duration: Actual time taken
  - Est: Original estimate for comparison
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
- [ ] Read `/memory-bank/UNIT-VS-INTEGRATION-TESTS.md` - Test separation rules
- [ ] Run baseline health check (`dotnet build` and `dotnet test`)
- [ ] Define BDD scenarios for all feature endpoints

## 🧪 BDD Test Scenarios (MANDATORY)

⚠️ **CRITICAL REQUIREMENT**: Every feature MUST define comprehensive BDD scenarios during planning phase
⚠️ **INTEGRATION TESTING RULE**: All database-dependent tests MUST be BDD integration tests

### Scenario 1: Create [Resource] - Happy Path
```gherkin
Given I am authenticated as "PT-Tier"
And I have valid [resource] data
When I send a POST request to "/api/[resources]"
Then the response status should be 201
And the response should contain the created [resource]
And the [resource] should be persisted in the database
```

### Scenario 2: Create [Resource] - Validation Error
```gherkin
Given I am authenticated as "PT-Tier"
And I have invalid [resource] data with missing required field
When I send a POST request to "/api/[resources]"
Then the response status should be 400
And the response should contain validation errors
```

### Scenario 3: Update [Resource] - Success
```gherkin
Given I am authenticated as "PT-Tier"
And a [resource] with id "{id}" exists
When I send a PUT request to "/api/[resources]/{id}"
Then the response status should be 200
And the [resource] should be updated in the database
```

### Scenario 4: Delete [Resource] - Success
```gherkin
Given I am authenticated as "PT-Tier"
And a [resource] with id "{id}" exists
When I send a DELETE request to "/api/[resources]/{id}"
Then the response status should be 204
And the [resource] should be deactivated in the database
```

### Scenario 5: Get [Resource] - Unauthorized
```gherkin
Given I am not authenticated
When I send a GET request to "/api/[resources]"
Then the response status should be 401
```

### Edge Cases:
- [ ] Concurrent creation with same unique field
- [ ] Update non-existent resource
- [ ] Delete already deleted resource
- [ ] Invalid data formats
- [ ] Permission boundaries

### Test Planning Requirements (MANDATORY)
**During Feature Refinement, MUST define**:
1. **Unit Test Scope**: Which methods will be unit tested with mocked dependencies
2. **Integration Test Scope**: Which workflows require database testing
3. **BDD Scenarios**: Every business rule and API endpoint MUST have BDD coverage
4. **Migration Tasks**: Any existing integration tests to migrate to BDD format

**Test Placement Rules** (See `/memory-bank/UNIT-VS-INTEGRATION-TESTS.md`):
- ❌ Database tests in API.Tests project = ARCHITECTURE VIOLATION
- ✅ Unit tests with ALL dependencies mocked = API.Tests project
- ✅ Database/workflow tests = API.IntegrationTests project in BDD format

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
- **Task 4.3:** Write controller unit tests (ALL dependencies mocked) `[ReadyToDevelop]` (Est: 1.5h)

### Category 5 (e.g., BDD Integration Tests) - Estimated: Xh
#### 📖 Before Starting: Review BDD scenarios defined above
#### ⚠️ MANDATORY: Integration tests MUST be planned during feature refinement
- **Task 5.1:** Create BDD feature file for [Name] `[ReadyToDevelop]` (Est: 30m)
  - **REQUIREMENT**: Every business rule from requirements MUST have a BDD scenario
  - **REQUIREMENT**: Every API endpoint MUST have happy path + error scenarios
- **Task 5.2:** Implement step definitions for happy path scenarios `[ReadyToDevelop]` (Est: 1.5h)
- **Task 5.3:** Implement step definitions for error scenarios `[ReadyToDevelop]` (Est: 1h)
- **Task 5.4:** Implement step definitions for edge cases `[ReadyToDevelop]` (Est: 1h)
- **Task 5.5:** Migrate any existing integration tests to BDD format `[ReadyToDevelop]` (Est: varies)

### Category 6 (e.g., Database & Migrations) - Estimated: Xh
- **Task 6.1:** Add entity configuration `[ReadyToDevelop]` (Est: 30m)
- **Task 6.2:** Create database migration `[ReadyToDevelop]` (Est: 15m)
- **Task 6.3:** Add seed data if needed `[ReadyToDevelop]` (Est: 30m)

## 🔄 Mid-Implementation Checkpoint
- [ ] All tests still passing (`dotnet test`)
- [ ] Build has no errors (`dotnet clean && dotnet build`)
- [ ] Re-read `/memory-bank/common-implementation-pitfalls.md` if any issues
- [ ] Verify correct UnitOfWork usage in all services

## Time Tracking Summary
- **Total Estimated Time:** [Sum of all estimates]
- **Total Actual Time:** [To be calculated from task durations]
- **AI Assistance Impact:** [% reduction in time]

## Notes
- Each implementation task must be immediately followed by its test task
- **MANDATORY**: BDD scenarios MUST be defined during planning phase
- **CRITICAL**: Unit tests go in API.Tests with EVERYTHING mocked (NO exceptions!)
- **CRITICAL**: Integration tests go in API.IntegrationTests in BDD format ONLY
- **RULE**: Any test requiring database = Integration test = API.IntegrationTests
- **RULE**: Unit tests test ONLY ONE method with ALL dependencies mocked
- No task is complete until build passes and all tests are green
- Keep build warnings to minimum
- Follow existing API patterns and conventions
- Time estimates are for a developer without AI assistance
- **Architecture Enforcement**: Database tests in API.Tests = feature blocked until fixed
```

## Example Task Status Updates

- `[ReadyToDevelop]` - Initial status for all tasks
- `[InProgress: Started: 2025-01-15 14:30]` - Task being actively worked on
- `[Implemented: a1b2c3d4 | Started: 2025-01-15 14:30 | Finished: 2025-01-15 15:15 | Duration: 0h 45m | Est: 2h 0m]` - Completed task
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
[Implemented: abc123 | Started: 2025-01-15 09:00 | Finished: 2025-01-16 11:30 | Duration: 3h 15m | Est: 1h 0m]
Note: Work sessions: Jan 15 (09:00-10:30), Jan 16 (10:00-11:30)
```

### AI Impact Calculation
At feature completion, calculate:
- Sum all estimated times
- Sum all actual durations
- AI Impact = ((Estimated - Actual) / Estimated) × 100%
- Document any factors that affected the comparison

## API-Specific Considerations (Minimal API)

### Minimal API Patterns
- Define endpoints using MapGet, MapPost, MapPut, MapDelete in Program.cs
- Use endpoint filters for cross-cutting concerns
- Implement proper route grouping with MapGroup
- Use TypedResults for consistent responses
- Apply proper endpoint metadata with OpenAPI attributes

### Endpoint Organization
- Group related endpoints using route groups
- Use extension methods to organize endpoint registration
- Implement endpoint filters for validation and authorization
- Use proper parameter binding (FromRoute, FromQuery, FromBody)

### Dependency Injection
- Register all services in Program.cs
- Use appropriate service lifetimes (Scoped for DbContext, etc.)
- Follow interface-based design for testability
- Use IServiceCollection extension methods for clean registration

### Database Patterns
- Use Entity Framework Core with code-first approach
- Implement repository pattern with interfaces
- Use Unit of Work pattern for transaction management
- **CRITICAL**: ReadOnlyUnitOfWork for queries, WritableUnitOfWork for modifications

### Request/Response Patterns
- Use record types for DTOs
- Implement proper validation with FluentValidation or DataAnnotations
- Return consistent error responses using ProblemDetails
- Use proper HTTP status codes (201 for created, 204 for no content, etc.)

### Security
- Implement JWT authentication with proper configuration
- Use authorization policies for role-based access
- Validate all inputs with proper validation attributes
- Sanitize data before persistence
- Never expose internal implementation details in errors

### Performance
- Implement memory caching with IMemoryCache
- Use async/await for all database operations
- Implement pagination using Skip/Take patterns
- Use projection (Select) to minimize data transfer
- Consider response compression for large payloads

### Testing Considerations
- Use WebApplicationFactory for integration tests
- Mock external dependencies properly
- Test all HTTP status code scenarios
- Verify proper authorization on protected endpoints

## Completion Report Templates

When moving a feature to COMPLETED, the following reports MUST be created:

### 1. COMPLETION-REPORT.md Template
```markdown
# [Feature Name] - Completion Report

## Feature Overview
**Feature ID**: FEAT-XXX  
**Feature Name**: [Full feature name]  
**Start Date**: [Date implementation started]  
**Completion Date**: [Date user accepted]  
**Status**: ✅ COMPLETE

## Summary
[Brief description of what was accomplished]

## Implementation Details

### API Changes
1. **Models & Entities**
   - [What was added/changed]
   - [Key implementation details]

2. **Repository Layer**
   - [What was added/changed]
   - [Key implementation details]

3. **Service Layer**
   - [What was added/changed]
   - [Business logic implemented]

4. **Controller/Endpoints**
   - [New endpoints created]
   - [Validation rules applied]

## Issues Resolved During Testing

### Issue 1: [Issue Name]
- **Problem**: [Description]
- **Solution**: [How it was fixed]
- **User Feedback**: [What the user reported]

[Repeat for each issue]

## Test Coverage Improvements
- **Before**: [Coverage %], [Number] tests
- **After**: [Coverage %], [Number] tests
- **New Tests Added**: [Number]

### Specific Improvements
1. **[Component]**: [Before]% → [After]%
2. **[Component]**: [Before]% → [After]%

## Technical Debt Addressed
[List any refactoring or improvements made]

## Files Changed
- **Total Files**: [Number]
- **Lines Added**: [Number]
- **Lines Removed**: [Number]

## Key Learnings
[Important insights gained during implementation]

## Deployment Notes
- [Database migration requirements]
- [Configuration changes needed]
- [Breaking changes]

## Documentation Created
[List all documentation files created]

## Next Steps
[Any follow-up work or future enhancements]

## Sign-off
- ✅ All acceptance criteria met
- ✅ Manual testing completed successfully
- ✅ Automated tests passing
- ✅ Documentation complete
- ✅ Code review ready

**Feature Status**: COMPLETE and ready for production deployment
```

### 2. TECHNICAL-SUMMARY.md Template
```markdown
# [Feature Name] Technical Implementation Summary

## Architecture Changes

### 1. Data Flow
```
[Diagram or description of data flow]
```

### 2. Key Components Created

#### Models & Entities
```
/Models/
  └── Entities/
      └── [EntityName].cs    # Description of purpose
  └── DTOs/
      └── [DtoName].cs       # Description of purpose
```

#### Repository Layer
```
/Repositories/
  └── Interfaces/
      └── I[Name]Repository.cs
  └── Implementations/
      └── [Name]Repository.cs
```

#### Service Layer
```
/Services/
  └── Interfaces/
      └── I[Name]Service.cs
  └── Implementations/
      └── [Name]Service.cs
```

### 3. Critical Implementation Details
```csharp
// Example of key implementation pattern
public async Task<ServiceResult<T>> CreateAsync(CreateRequest request)
{
    // Show critical validation or business logic
}
```

### 4. Validation Rules
- [Business rule 1]
- [Business rule 2]
- [Validation pattern used]

### 5. Database Schema Changes
```sql
-- Migration summary
CREATE TABLE [TableName] (
    -- Show key schema
);
```

## Integration Points

### 1. Dependencies
- [External services used]
- [Internal services integrated]

### 2. API Endpoints
```
GET    /api/[resource]
POST   /api/[resource]
PUT    /api/[resource]/{id}
DELETE /api/[resource]/{id}
```

## Testing Strategy

### 1. Unit Tests
- Repository tests: [approach]
- Service tests: [approach]
- Controller tests: [approach]

### 2. Integration Tests
- API endpoint tests: [approach]
- Database integration: [approach]

### 3. Test Data
- Seed data approach
- Test fixtures used

## Performance Considerations
- [Caching strategy]
- [Query optimization]
- [Async patterns]

## Security Considerations
- [Authorization rules]
- [Data validation]
- [Input sanitization]

## Breaking Changes
[List any breaking changes]

## Configuration
```json
// Any configuration added
{
  "FeatureName": {
    "Setting": "value"
  }
}
```

## Deployment
1. Run database migrations
2. Update configuration
3. Deploy API changes
4. Verify endpoints

## Monitoring
- [Key metrics to monitor]
- [Expected performance baseline]
- [Error patterns to watch]
```

### 3. LESSONS-LEARNED.md Template
```markdown
# [Feature Name] - Lessons Learned

## What Went Well ✅

### 1. [Success Area]
[Description of what worked well and why]

### 2. [Technical Success]
[What technical approach worked particularly well]

## Challenges Faced 🔧

### 1. [Challenge Name]
**Issue**: [Description of the problem]
```csharp
// Example code that caused issues
```
**Solution**: [How it was resolved]
```csharp
// Fixed code
```
**Learning**: [Key takeaway]

### 2. [Another Challenge]
**Issue**: [Description]
**Solution**: [Resolution]
**Learning**: [Takeaway]

## Technical Insights 💡

### 1. [Pattern/Approach]
[Technical learning about patterns, architecture, or implementation]

### 2. [Performance Discovery]
[Any performance insights gained]

## Process Improvements 📈

### 1. Development Process
[How the development process could be improved]

### 2. Testing Strategy
[Testing improvements discovered]

## Recommendations for Future Features 🚀

### 1. Before Starting
- [ ] [Recommendation based on learnings]
- [ ] [Pre-implementation consideration]

### 2. During Development
- [ ] [Development practice recommendation]
- [ ] [Code quality tip]

### 3. Testing Phase
- [ ] [Testing recommendation]
- [ ] [Quality assurance tip]

### 4. Documentation
- [ ] [Documentation best practice]
- [ ] [Knowledge sharing tip]

## Key Takeaways 🎯

1. **[Topic]**: [Key learning]
2. **[Technical Topic]**: [Important insight]
3. **[Process Topic]**: [Process improvement]

## Time Investment
- Initial implementation: ~[X] hours
- Bug fixing and testing: ~[X] hours
- Documentation: ~[X] hours
- **Total**: ~[X] hours

## ROI Analysis
- **Time Saved with AI**: [X]% reduction
- **Quality Improvements**: [List improvements]
- **Technical Debt Reduced**: [What was cleaned up]
- **Future Development Impact**: [How this helps future work]

## Quote of the Feature
"[Memorable quote or insight from the implementation]"
```

### 4. QUICK-REFERENCE.md Template
```markdown
# [Feature Name] - Quick Reference

## Key Constants/Enums
```csharp
public enum [EnumName]
{
    Value1 = 1,  // Description
    Value2 = 2,  // Description
}
```

## Business Rules
- ❌ [Rule about what's NOT allowed]
- ✅ [Rule about what IS required]
- ⚠️ [Important consideration]

## API Endpoints

### Get All [Resources]
```
GET /api/[resources]
Authorization: Bearer {token}

Response: 200 OK
[
  {
    "id": "guid",
    "field": "value"
  }
]
```

### Create [Resource]
```
POST /api/[resources]
Authorization: Bearer {token}
Content-Type: application/json

{
  "field": "value",
  "requiredField": "value"
}

Response: 201 Created
{
  "id": "guid",
  "field": "value"
}
```

### Update [Resource]
```
PUT /api/[resources]/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "field": "updated value"
}

Response: 200 OK
```

### Delete [Resource]
```
DELETE /api/[resources]/{id}
Authorization: Bearer {token}

Response: 204 No Content
```

## Common Validation Errors

### Missing Required Field
```json
{
  "errors": {
    "FieldName": ["The FieldName field is required."]
  }
}
```

### Invalid Format
```json
{
  "errors": {
    "FieldName": ["The field must match pattern X."]
  }
}
```

## C# Usage Examples

### Service Usage
```csharp
// Inject service
private readonly I[Name]Service _service;

// Use in controller
var result = await _service.CreateAsync(request);
if (!result.IsSuccess)
{
    return BadRequest(result.Error);
}
```

### Repository Pattern
```csharp
// Query example
var items = await _repository.GetAllAsync();

// With includes
var item = await _repository.GetByIdWithIncludesAsync(
    id, 
    x => x.Include(i => i.RelatedEntity)
);
```

## Testing
- Unit tests: `/[Feature].Tests/[Component]Tests.cs`
- Integration tests: `/IntegrationTests/[Feature]Tests.cs`
- Test data: Use builders in `/Tests/Builders/[Name]Builder.cs`

## Troubleshooting

### Issue: [Common Issue]
**Symptom**: [What user sees]
**Cause**: [Root cause]
**Solution**: [How to fix]

### Issue: [Another Issue]
**Symptom**: [Description]
**Solution**: [Fix]

## Related Documentation
- Entity model: `/Models/Entities/[Name].cs`
- Service implementation: `/Services/Implementations/[Name]Service.cs`
- Database configuration: `/Data/Configurations/[Name]Configuration.cs`
```