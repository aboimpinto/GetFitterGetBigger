# Workout Reference Data Implementation Tasks

## Feature Branch: `feature/workout-reference-data`
## Estimated Total Time: 12 days / 96 hours
## Actual Total Time: [To be calculated at completion]

## 📚 Pre-Implementation Checklist
- [ ] Read `/memory-bank/systemPatterns.md` - Architecture rules
- [ ] Read `/memory-bank/unitOfWorkPattern.md` - Critical ReadOnly vs Writable patterns
- [ ] Read `/memory-bank/common-implementation-pitfalls.md` - Common mistakes to avoid
- [ ] Read `/memory-bank/UNIT-VS-INTEGRATION-TESTS.md` - Test separation rules
- [x] Run baseline health check (`dotnet build` and `dotnet test`)
- [ ] Define BDD scenarios for all feature endpoints

## Baseline Health Check Report
**Date/Time**: 2025-07-13 02:41
**Branch**: feature/workout-reference-data

### Build Status
- **Build Result**: ✅ Success
- **Warning Count**: 0 warnings
- **Warning Details**: None

### Test Status
- **Total Tests**: 707 (481 unit + 226 integration)
- **Passed**: 707
- **Failed**: 0 (MUST be 0 to proceed)
- **Skipped/Ignored**: 0
- **Test Execution Time**: ~8.66 seconds

### Test Coverage
- **Total Line Coverage**: 53.72%
- **Total Branch Coverage**: 45.09%
- **Total Method Coverage**: 57.8%

### Decision to Proceed
- [x] All tests passing
- [x] Build successful
- [x] Warnings documented and approved

**Approval to Proceed**: ✅ YES - All baseline requirements met

## 🧪 BDD Test Scenarios (MANDATORY)

⚠️ **CRITICAL REQUIREMENT**: Every feature MUST define comprehensive BDD scenarios during planning phase
⚠️ **INTEGRATION TESTING RULE**: All database-dependent tests MUST be BDD integration tests

### Scenario 1: Get All Workout Objectives - Success
```gherkin
Given I am authenticated as "Free-Tier"
When I send a GET request to "/api/workout-objectives"
Then the response status should be 200
And the response should contain a list of workout objectives
And each objective should have required fields: workoutObjectiveId, value, description, displayOrder, isActive
And the objectives should be ordered by displayOrder ascending
```

### Scenario 2: Get Workout Objective by ID - Success
```gherkin
Given I am authenticated as "Free-Tier"
And a workout objective with id "3fa85f64-5717-4562-b3fc-2c963f66afa6" exists
When I send a GET request to "/api/workout-objectives/3fa85f64-5717-4562-b3fc-2c963f66afa6"
Then the response status should be 200
And the response should contain the workout objective details
And the objective should have value "Muscular Strength"
```

### Scenario 3: Get Workout Objective by ID - Not Found
```gherkin
Given I am authenticated as "Free-Tier"
And no workout objective with id "00000000-0000-0000-0000-000000000000" exists
When I send a GET request to "/api/workout-objectives/00000000-0000-0000-0000-000000000000"
Then the response status should be 404
And the response should contain a not found error
```

### Scenario 4: Get All Workout Categories - Success
```gherkin
Given I am authenticated as "Free-Tier"
When I send a GET request to "/api/workout-categories"
Then the response status should be 200
And the response should contain a list of workout categories
And each category should have fields: workoutCategoryId, value, description, icon, color, primaryMuscleGroups, displayOrder, isActive
And the categories should be ordered by displayOrder ascending
```

### Scenario 5: Get All Execution Protocols - Success
```gherkin
Given I am authenticated as "Free-Tier"
When I send a GET request to "/api/execution-protocols"
Then the response status should be 200
And the response should contain a list of execution protocols
And each protocol should have fields: executionProtocolId, code, value, description, timeBase, repBase, restPattern, intensityLevel, displayOrder, isActive
And the protocols should be ordered by displayOrder ascending
```

### Scenario 6: Get Execution Protocol by Code - Success
```gherkin
Given I am authenticated as "Free-Tier"
And an execution protocol with code "STANDARD" exists
When I send a GET request to "/api/execution-protocols/by-code/STANDARD"
Then the response status should be 200
And the response should contain the execution protocol details
And the protocol should have code "STANDARD" and value "Standard"
```

### Scenario 7: Unauthorized Access
```gherkin
Given I am not authenticated
When I send a GET request to "/api/workout-objectives"
Then the response status should be 401
And the response should contain authentication error
```

### Scenario 8: Include Inactive Parameters
```gherkin
Given I am authenticated as "Free-Tier"
When I send a GET request to "/api/workout-objectives?includeInactive=true"
Then the response status should be 200
And the response should contain both active and inactive objectives
```

### Edge Cases:
- [ ] Invalid GUID format handling
- [ ] Case sensitivity for execution protocol codes
- [ ] Large payload response optimization
- [ ] Concurrent access performance
- [ ] Cache invalidation scenarios

### Test Planning Requirements (MANDATORY)
**During Feature Refinement, MUST define**:
1. **Unit Test Scope**: Service methods, repository methods, controller actions with all dependencies mocked
2. **Integration Test Scope**: Complete API workflows with database persistence and caching
3. **BDD Scenarios**: All business rules and API endpoints covered
4. **Migration Tasks**: New integration tests required for reference data endpoints

**Test Placement Rules** (See `/memory-bank/UNIT-VS-INTEGRATION-TESTS.md`):
- ❌ Database tests in API.Tests project = ARCHITECTURE VIOLATION
- ✅ Unit tests with ALL dependencies mocked = API.Tests project
- ✅ Database/workflow tests = API.IntegrationTests project in BDD format

### Category 1 (Models & DTOs) - Estimated: 4h
#### 📖 Before Starting: Review entity pattern in `/memory-bank/databaseModelPattern.md`
- **Task 1.1:** Create WorkoutObjective entity model `[Implemented: b2faedf5 | Started: 2025-07-13 02:45 | Finished: 2025-07-13 02:47 | Duration: 0h 2m | Est: 45m]`
- **Task 1.2:** Create WorkoutCategory entity model `[Implemented: 2ee8fa22 | Started: 2025-07-13 02:47 | Finished: 2025-07-13 02:48 | Duration: 0h 1m | Est: 45m]`
- **Task 1.3:** Create ExecutionProtocol entity model `[Implemented: d2f5e2c2 | Started: 2025-07-13 02:48 | Finished: 2025-07-13 02:50 | Duration: 0h 2m | Est: 45m]`
- **Task 1.4:** Create WorkoutMuscles relationship entity model `[Implemented: a88328a8 | Started: 2025-07-13 02:50 | Finished: 2025-07-13 07:22 | Duration: 0h 32m | Est: 45m]`
- **Task 1.5:** Create WorkoutObjectiveDto and response models `[InProgress: Started: 2025-07-13 07:30]` (Est: 30m)
- **Task 1.6:** Create WorkoutCategoryDto and response models `[ReadyToDevelop]` (Est: 30m)
- **Task 1.7:** Create ExecutionProtocolDto and response models `[ReadyToDevelop]` (Est: 30m)
- **Task 1.8:** Write unit tests for DTOs and mapping logic `[ReadyToDevelop]` (Est: 30m)

### Category 2 (Repository Layer) - Estimated: 6h
#### 📖 Before Starting: Review repository patterns in `/memory-bank/unitOfWorkPattern.md`
- **Task 2.1:** Create IWorkoutObjectiveRepository interface `[ReadyToDevelop]` (Est: 15m)
- **Task 2.2:** Create IWorkoutCategoryRepository interface `[ReadyToDevelop]` (Est: 15m)
- **Task 2.3:** Create IExecutionProtocolRepository interface `[ReadyToDevelop]` (Est: 15m)
- **Task 2.4:** Implement WorkoutObjectiveRepository `[ReadyToDevelop]` (Est: 1h)
- **Task 2.5:** Implement WorkoutCategoryRepository `[ReadyToDevelop]` (Est: 1h)
- **Task 2.6:** Implement ExecutionProtocolRepository with code lookup `[ReadyToDevelop]` (Est: 1.5h)
- **Task 2.7:** Write repository unit tests (WorkoutObjective) `[ReadyToDevelop]` (Est: 45m)
- **Task 2.8:** Write repository unit tests (WorkoutCategory) `[ReadyToDevelop]` (Est: 45m)
- **Task 2.9:** Write repository unit tests (ExecutionProtocol) `[ReadyToDevelop]` (Est: 45m)

### Category 3 (Service Layer) - Estimated: 9h
#### ⚠️ CRITICAL Before Starting: 
- [ ] Re-read `/memory-bank/common-implementation-pitfalls.md` Section 1
- [ ] Remember: ReadOnlyUnitOfWork for validation, WritableUnitOfWork for modifications ONLY
- **Task 3.1:** Create IWorkoutObjectiveService interface `[ReadyToDevelop]` (Est: 15m)
- **Task 3.2:** Create IWorkoutCategoryService interface `[ReadyToDevelop]` (Est: 15m)
- **Task 3.3:** Create IExecutionProtocolService interface `[ReadyToDevelop]` (Est: 15m)
- **Task 3.4:** Implement WorkoutObjectiveService with caching `[ReadyToDevelop]` (Est: 2h)
- **Task 3.5:** Implement WorkoutCategoryService with caching `[ReadyToDevelop]` (Est: 2h)
- **Task 3.6:** Implement ExecutionProtocolService with caching and code lookup `[ReadyToDevelop]` (Est: 2.5h)
- **Task 3.7:** Write service unit tests (WorkoutObjectiveService) `[ReadyToDevelop]` (Est: 1h)
- **Task 3.8:** Write service unit tests (WorkoutCategoryService) `[ReadyToDevelop]` (Est: 1h)
- **Task 3.9:** Write service unit tests (ExecutionProtocolService) `[ReadyToDevelop]` (Est: 1h)

### Category 4 (Controllers) - Estimated: 6h
#### 📖 Before Starting: Review controller rules - NO direct repository/UnitOfWork access!
- **Task 4.1:** Create WorkoutObjectivesController with GET endpoints `[ReadyToDevelop]` (Est: 1h)
- **Task 4.2:** Create WorkoutCategoriesController with GET endpoints `[ReadyToDevelop]` (Est: 1h)
- **Task 4.3:** Create ExecutionProtocolsController with GET endpoints and by-code lookup `[ReadyToDevelop]` (Est: 1.5h)
- **Task 4.4:** Add authorization and validation to all controllers `[ReadyToDevelop]` (Est: 45m)
- **Task 4.5:** Write controller unit tests (WorkoutObjectivesController) `[ReadyToDevelop]` (Est: 45m)
- **Task 4.6:** Write controller unit tests (WorkoutCategoriesController) `[ReadyToDevelop]` (Est: 45m)
- **Task 4.7:** Write controller unit tests (ExecutionProtocolsController) `[ReadyToDevelop]` (Est: 30m)

### Category 5 (BDD Integration Tests) - Estimated: 8h
#### 📖 Before Starting: Review BDD scenarios defined above
#### ⚠️ MANDATORY: Integration tests MUST be planned during feature refinement
- **Task 5.1:** Create BDD feature file for WorkoutObjectives `[ReadyToDevelop]` (Est: 30m)
  - **REQUIREMENT**: Every business rule from requirements MUST have a BDD scenario
  - **REQUIREMENT**: Every API endpoint MUST have happy path + error scenarios
- **Task 5.2:** Create BDD feature file for WorkoutCategories `[ReadyToDevelop]` (Est: 30m)
- **Task 5.3:** Create BDD feature file for ExecutionProtocols `[ReadyToDevelop]` (Est: 30m)
- **Task 5.4:** Implement step definitions for WorkoutObjectives scenarios `[ReadyToDevelop]` (Est: 2h)
- **Task 5.5:** Implement step definitions for WorkoutCategories scenarios `[ReadyToDevelop]` (Est: 2h)
- **Task 5.6:** Implement step definitions for ExecutionProtocols scenarios `[ReadyToDevelop]` (Est: 2h)
- **Task 5.7:** Implement step definitions for authentication/authorization scenarios `[ReadyToDevelop]` (Est: 30m)

### Category 6 (Database & Migrations) - Estimated: 3h
- **Task 6.1:** Add WorkoutObjective entity configuration `[ReadyToDevelop]` (Est: 30m)
- **Task 6.2:** Add WorkoutCategory entity configuration `[ReadyToDevelop]` (Est: 30m)
- **Task 6.3:** Add ExecutionProtocol entity configuration `[ReadyToDevelop]` (Est: 30m)
- **Task 6.4:** Add WorkoutMuscles entity configuration `[ReadyToDevelop]` (Est: 30m)
- **Task 6.5:** Create database migration for all entities `[ReadyToDevelop]` (Est: 15m)
- **Task 6.6:** Add comprehensive seed data for WorkoutObjective `[ReadyToDevelop]` (Est: 15m)
- **Task 6.7:** Add comprehensive seed data for WorkoutCategory `[ReadyToDevelop]` (Est: 15m)
- **Task 6.8:** Add comprehensive seed data for ExecutionProtocol `[ReadyToDevelop]` (Est: 15m)

### Category 7 (Dependency Injection & Configuration) - Estimated: 1h
- **Task 7.1:** Register repositories in DI container `[ReadyToDevelop]` (Est: 15m)
- **Task 7.2:** Register services in DI container `[ReadyToDevelop]` (Est: 15m)
- **Task 7.3:** Configure caching for reference data `[ReadyToDevelop]` (Est: 15m)
- **Task 7.4:** Add OpenAPI documentation attributes `[ReadyToDevelop]` (Est: 15m)

## 🔄 Mid-Implementation Checkpoint
- [ ] All tests still passing (`dotnet test`)
- [ ] Build has no errors (`dotnet clean && dotnet build`)
- [ ] Re-read `/memory-bank/common-implementation-pitfalls.md` if any issues
- [ ] Verify correct UnitOfWork usage in all services

## Time Tracking Summary
- **Total Estimated Time:** 37 hours
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
- **READ-ONLY FEATURE**: All operations use ReadOnlyUnitOfWork since no data modifications occur
- **CACHING STRATEGY**: All service methods implement aggressive caching with 1-hour TTL
- **AUTHORIZATION**: Minimum Free-Tier access required for all endpoints