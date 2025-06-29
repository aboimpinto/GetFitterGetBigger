# Feature: Fix EF Core Integration Tests with PostgreSQL TestContainers

## Feature ID: FEAT-007
## Created: 2025-01-29
## Status: SUBMITTED
## Target PI: PI-2025-Q1

## Description

This feature addresses the issue of 8 skipped integration tests in the ExercisesController test suite. These tests are currently skipped due to limitations of the EF Core In-Memory database provider when handling complex queries with multiple includes and the `.AsSplitQuery()` method.

The solution involves replacing the In-Memory provider with a real PostgreSQL database instance using TestContainers, ensuring test behavior matches production exactly.

## Business Value

- **Quality Assurance**: Enables full test coverage for critical Exercise API endpoints
- **Production Parity**: Tests run against the same database engine as production (PostgreSQL)
- **Confidence**: Reduces risk of database-related bugs reaching production
- **Developer Experience**: Removes need to skip tests, improving development workflow

## User Stories

- As a developer, I want all integration tests to run successfully so that I can have confidence in the codebase
- As a QA engineer, I want tests to accurately reflect production behavior so that bugs are caught early
- As a team lead, I want complete test coverage metrics so that I can assess code quality

## Current Problem

### Affected Tests (All in ExercisesControllerTests.cs):
1. `GetExercises_ReturnsPagedListOfExercises`
2. `GetExercises_WithNameFilter_ReturnsFilteredResults`
3. `GetExercises_WithMuscleGroupFilter_ReturnsFilteredResults`
4. `GetExercise_WithValidId_ReturnsExercise`
5. `CreateExercise_WithDuplicateName_ReturnsConflict`
6. `UpdateExercise_WithValidData_ReturnsUpdatedExercise`
7. `UpdateExercise_WithDuplicateName_ReturnsConflict`
8. `DeleteExercise_WithValidId_ReturnsNoContent`

### Root Cause:
- EF Core In-Memory provider cannot handle complex queries with multiple includes
- `.AsSplitQuery()` is not properly supported by the In-Memory provider
- Exercise entity has relationships to 8+ other entities (muscle groups, equipment, body parts, movement patterns, exercise types, coach notes, etc.)

## Solution Analysis

### Option 1: TestContainers with PostgreSQL (RECOMMENDED)
**Approach**: Use TestContainers to spin up real PostgreSQL instances for integration tests

**Pros**:
- ✅ Exact same database engine as production
- ✅ Full support for all EF Core features including `.AsSplitQuery()`
- ✅ Accurate SQL generation and execution
- ✅ Tests PostgreSQL-specific features (ILIKE, arrays, JSON)
- ✅ Catches database-specific bugs early
- ✅ Works in CI/CD pipelines

**Cons**:
- ❌ Slower test execution (1-2 seconds container startup)
- ❌ Requires Docker to be installed
- ❌ More complex test infrastructure
- ❌ Higher resource usage

**Implementation Complexity**: Medium

### Option 2: SQLite In-Memory Database
**Approach**: Replace EF Core In-Memory provider with SQLite in-memory mode

**Pros**:
- ✅ Better SQL compatibility than In-Memory provider
- ✅ Still runs in memory (fast)
- ✅ No external dependencies

**Cons**:
- ❌ Different SQL dialect than PostgreSQL
- ❌ `.AsSplitQuery()` may still have issues
- ❌ No support for PostgreSQL-specific features
- ❌ Custom ID types might behave differently
- ❌ Case sensitivity differences
- ❌ Would need separate migrations for tests

**Implementation Complexity**: Low-Medium

### Option 3: Mock Repository Layer
**Approach**: Mock the repository instead of using actual database

**Pros**:
- ✅ Very fast execution
- ✅ Complete control over test data
- ✅ No database dependencies
- ✅ Simple setup

**Cons**:
- ❌ Not true integration tests
- ❌ Doesn't test actual database queries
- ❌ Doesn't catch SQL generation issues
- ❌ High maintenance burden
- ❌ Tests become brittle

**Implementation Complexity**: High (due to extensive mocking)

### Option 4: Conditional Query Building
**Approach**: Detect test environment and avoid `.AsSplitQuery()`

**Pros**:
- ✅ Minimal code changes
- ✅ Tests can run immediately

**Cons**:
- ❌ Production and test code diverge
- ❌ Not testing actual production queries
- ❌ Adds complexity to repository code
- ❌ False sense of security

**Implementation Complexity**: Low

### Option 5: Keep Tests Skipped
**Approach**: Accept the limitation and keep tests skipped

**Pros**:
- ✅ No work required
- ✅ Other tests still run

**Cons**:
- ❌ No test coverage for critical endpoints
- ❌ Risk of bugs in production
- ❌ Poor developer experience
- ❌ Technical debt accumulation

**Implementation Complexity**: None

## Recommended Solution: TestContainers with PostgreSQL

### Why This Solution?

1. **Production Parity**: Tests run against actual PostgreSQL, matching production exactly
2. **Future Proof**: Supports all current and future PostgreSQL features
3. **Industry Standard**: TestContainers is widely adopted and well-maintained
4. **CI/CD Compatible**: Works in containerized build environments
5. **Debugging**: Can inspect actual SQL queries and database state

### Technical Implementation Overview

1. **Dependencies**:
   - NuGet: `Testcontainers.PostgreSql`
   - Docker: Required on development machines and CI/CD

2. **Test Infrastructure**:
   - Create `PostgreSqlTestFixture` for container lifecycle
   - Update `ApiTestFixture` to use PostgreSQL
   - Configure proper test data cleanup

3. **Migration Strategy**:
   - Phase 1: Infrastructure setup
   - Phase 2: Migrate one test as proof of concept
   - Phase 3: Migrate remaining tests
   - Phase 4: Documentation and cleanup

## Acceptance Criteria

- [ ] All 8 skipped tests can be enabled and pass successfully
- [ ] Tests run in under 30 seconds total
- [ ] Clear documentation for developers on setup requirements
- [ ] CI/CD pipeline continues to work without issues
- [ ] No regression in other tests
- [ ] Test data is properly isolated between test runs

## Dependencies

- Docker must be available on developer machines
- CI/CD environment must support Docker containers
- Team agreement on moving from In-Memory to PostgreSQL for tests

## Success Metrics

- 100% of ExercisesController integration tests passing
- 0 skipped tests in the ExercisesControllerTests file
- Test execution time remains reasonable (< 30 seconds for suite)
- No increase in test flakiness

## Notes

- This change aligns with industry best practices for integration testing
- Consider applying the same pattern to other integration tests in the future
- The slight performance cost is justified by the increased confidence in tests
- This solution has been successfully used in many production systems