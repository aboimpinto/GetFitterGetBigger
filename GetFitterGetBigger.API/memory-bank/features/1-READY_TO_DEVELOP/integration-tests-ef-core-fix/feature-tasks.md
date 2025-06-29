# Fix EF Core Integration Tests with PostgreSQL TestContainers - Implementation Tasks

## Feature Branch: `feature/integration-tests-postgresql`
## Estimated Total Time: 3 days (24 hours)
## Actual Total Time: [To be calculated at completion]

### Task Categories
1. **Infrastructure Setup** (8 hours)
2. **Test Migration** (12 hours)
3. **Documentation & Cleanup** (4 hours)

### Progress Tracking
- All tasks start as `[ReadyToDevelop]` with time estimate
- Update to `[InProgress: Started: YYYY-MM-DD HH:MM]` when starting
- Update to `[Implemented: <hash> | Started: <time> | Finished: <time> | Duration: Xh Ym]` when complete
- Use `[Blocked: reason]` if blocked

## Phase 1: Infrastructure Setup

### 1.1 Dependencies & Project Configuration
- [ ] `[ReadyToDevelop: 30m]` Add NuGet package `Testcontainers.PostgreSql` to test project
- [ ] `[ReadyToDevelop: 15m]` Verify Docker Desktop is installed and document requirement
- [ ] `[ReadyToDevelop: 15m]` Update test project dependencies if needed

### 1.2 Create PostgreSQL Test Fixture
- [ ] `[ReadyToDevelop: 1h]` Create `PostgreSqlTestFixture` class implementing `IAsyncLifetime`
  - Container initialization with PostgreSQL 15-alpine image
  - Connection string exposure
  - Proper disposal
- [ ] `[ReadyToDevelop: 30m]` Add container health checks and startup verification
- [ ] `[ReadyToDevelop: 30m]` Implement connection retry logic for container startup

### 1.3 Update ApiTestFixture
- [ ] `[ReadyToDevelop: 1h]` Replace `UseInMemoryDatabase` with `UseNpgsql` in `ApiTestFixture`
- [ ] `[ReadyToDevelop: 1h]` Implement database migration strategy (replace `EnsureCreated` with `MigrateAsync`)
- [ ] `[ReadyToDevelop: 30m]` Configure proper logging for SQL queries in test environment
- [ ] `[ReadyToDevelop: 30m]` Add test database connection string configuration

### 1.4 Test Data Management
- [ ] `[ReadyToDevelop: 1h]` Implement test data cleanup strategy (evaluate Transaction vs Respawn)
- [ ] `[ReadyToDevelop: 1h]` Create base test class with cleanup lifecycle
- [ ] `[ReadyToDevelop: 30m]` Ensure test isolation between test runs

## Phase 2: Migrate Tests (Proof of Concept)

### 2.1 Migrate First Test
- [ ] `[ReadyToDevelop: 1h]` Remove skip attribute from `GetExercises_ReturnsPagedListOfExercises`
- [ ] `[ReadyToDevelop: 30m]` Update test class to use new PostgreSQL fixture
- [ ] `[ReadyToDevelop: 30m]` Run test and fix any issues
- [ ] `[ReadyToDevelop: 30m]` Verify test passes consistently (run 5 times)

### 2.2 Validate Approach
- [ ] `[ReadyToDevelop: 30m]` Measure test execution time
- [ ] `[ReadyToDevelop: 30m]` Verify SQL queries are correct using logs
- [ ] `[ReadyToDevelop: 30m]` Check memory and resource usage
- [ ] `[ReadyToDevelop: 30m]` Document any issues or adjustments needed

## Phase 3: Migrate Remaining Tests

### 3.1 Migrate Filter Tests
- [ ] `[ReadyToDevelop: 45m]` Enable and fix `GetExercises_WithNameFilter_ReturnsFilteredResults`
- [ ] `[ReadyToDevelop: 45m]` Enable and fix `GetExercises_WithMuscleGroupFilter_ReturnsFilteredResults`
- [ ] `[ReadyToDevelop: 45m]` Enable and fix `GetExercise_WithValidId_ReturnsExercise`

### 3.2 Migrate Create/Update Tests
- [ ] `[ReadyToDevelop: 45m]` Enable and fix `CreateExercise_WithDuplicateName_ReturnsConflict`
- [ ] `[ReadyToDevelop: 45m]` Enable and fix `UpdateExercise_WithValidData_ReturnsUpdatedExercise`
- [ ] `[ReadyToDevelop: 45m]` Enable and fix `UpdateExercise_WithDuplicateName_ReturnsConflict`

### 3.3 Migrate Delete Test
- [ ] `[ReadyToDevelop: 30m]` Enable and fix `DeleteExercise_WithValidId_ReturnsNoContent`

### 3.4 Test Suite Validation
- [ ] `[ReadyToDevelop: 1h]` Run entire ExercisesControllerTests suite 10 times
- [ ] `[ReadyToDevelop: 30m]` Fix any flaky tests or race conditions
- [ ] `[ReadyToDevelop: 30m]` Optimize performance if needed

## Phase 4: Documentation & Cleanup

### 4.1 Developer Documentation
- [ ] `[ReadyToDevelop: 1h]` Create `INTEGRATION-TESTING.md` with:
  - Docker setup requirements
  - How to run tests locally
  - Troubleshooting guide
  - Performance considerations
- [ ] `[ReadyToDevelop: 30m]` Update README with new test requirements

### 4.2 CI/CD Configuration
- [ ] `[ReadyToDevelop: 1h]` Update GitHub Actions/CI pipeline to support Docker
- [ ] `[ReadyToDevelop: 30m]` Verify tests run in CI environment
- [ ] `[ReadyToDevelop: 30m]` Add Docker service configuration if needed

### 4.3 Code Cleanup
- [ ] `[ReadyToDevelop: 30m]` Remove any temporary debug code
- [ ] `[ReadyToDevelop: 30m]` Ensure all tests have proper assertions
- [ ] `[ReadyToDevelop: 15m]` Format and lint all modified files
- [ ] `[ReadyToDevelop: 15m]` Review and optimize imports

## Verification & Sign-off

### Final Validation
- [ ] `[ReadyToDevelop: 30m]` Run full test suite and confirm all tests pass
- [ ] `[ReadyToDevelop: 30m]` Verify test execution time is under 30 seconds
- [ ] `[ReadyToDevelop: 30m]` Check test coverage metrics improved
- [ ] `[ReadyToDevelop: 30m]` Get code review approval

### Completion Checklist
- [ ] All 8 previously skipped tests are now passing
- [ ] No regression in other tests
- [ ] Documentation is complete
- [ ] CI/CD pipeline is green
- [ ] Performance metrics are acceptable

### Bug Resolution
- [ ] `[ReadyToDevelop: 30m]` Verify BUG-002 is resolved:
  - Run all 8 previously skipped tests multiple times
  - Confirm no "Sequence contains no elements" errors
  - Verify complex queries with `.AsSplitQuery()` work correctly
  - Update BUG-002 documentation with resolution details
- [ ] `[ReadyToDevelop: 15m]` Move BUG-002 from `1-OPEN` to `3-FIXED` with resolution notes
- [ ] `[ReadyToDevelop: 15m]` Add reference to FEAT-007 in the bug resolution

## Time Tracking Summary
- **Total Estimated Time:** 25 hours (3 days)
- **Total Actual Time:** [To be calculated from task durations]
- **AI Assistance Impact:** [% reduction in time]
- **Implementation Started:** [First task start time]
- **Implementation Completed:** [Last task finish time]

## Risk Mitigation

### Potential Blockers:
1. **Docker not available in CI**: May need to configure CI environment
2. **Migration compatibility**: Some migrations might be PostgreSQL-specific
3. **Performance issues**: Container startup might be slow on some machines

### Contingency Plans:
1. **Hybrid approach**: Keep simple tests with In-Memory, complex ones with PostgreSQL
2. **Shared container**: Reuse container across test runs for performance
3. **Parallel limits**: Reduce parallel test execution if resource constrained