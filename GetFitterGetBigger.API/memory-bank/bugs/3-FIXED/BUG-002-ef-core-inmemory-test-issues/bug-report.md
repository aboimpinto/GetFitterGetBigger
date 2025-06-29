# BUG-002: EF Core In-Memory Database Issues with Complex Queries

## Bug ID: BUG-002
## Reported: 2025-01-29 (documented from BUGS.md)
## Status: FIXED
## Severity: Medium
## Affected Version: Current
## Fixed Version: Fixed in FEAT-007
## Resolution Date: 2025-01-29

## Description

The EF Core In-Memory database provider has limitations when dealing with complex queries that include multiple navigation properties. When using Include() to load related entities (DifficultyLevel, MuscleGroups, Equipment, etc.), the in-memory provider fails to properly track and return the seeded data.

## Error Messages
- "Sequence contains no elements" errors when trying to access seeded data
- Empty collections returned from queries that should have data

## Reproduction Steps
1. Run any of the ExercisesControllerTests integration tests
2. Tests fail with various errors related to missing data
3. Currently 8 tests are skipped with message: "BUG-002: EF Core In-Memory database provider has issues with complex queries involving multiple includes"

## Affected Tests
All in `ExercisesControllerTests.cs`:
1. `GetExercises_ReturnsPagedListOfExercises`
2. `GetExercises_WithNameFilter_ReturnsFilteredResults`
3. `GetExercises_WithMuscleGroupFilter_ReturnsFilteredResults`
4. `GetExercise_WithValidId_ReturnsExercise`
5. `CreateExercise_WithDuplicateName_ReturnsConflict`
6. `UpdateExercise_WithValidData_ReturnsUpdatedExercise`
7. `UpdateExercise_WithDuplicateName_ReturnsConflict`
8. `DeleteExercise_WithValidId_ReturnsNoContent`

## Root Cause
1. EF Core In-Memory provider doesn't support `.AsSplitQuery()` properly
2. Complex queries with multiple includes cause issues in the In-Memory provider
3. The provider can't handle the same level of query complexity as PostgreSQL
4. Each test creates a new in-memory database instance, and the complex entity relationships with multiple includes are not properly handled

## Impact
- **Test Coverage**: 8 critical integration tests cannot run
- **Developer Experience**: Tests must be skipped, reducing confidence
- **Quality Risk**: Exercise API endpoints not fully tested

## Proposed Solution
Replace EF Core In-Memory provider with PostgreSQL TestContainers for integration tests. This is being tracked as FEAT-007 in the feature workflow.

## Workaround
Tests are currently skipped with bug reference in the skip message.

## Related Information
- Feature FEAT-007 will implement the fix using PostgreSQL TestContainers
- Note: There's another BUG-002 in 3-FIXED related to production `.AsSplitQuery()` issues, but this is a different issue specific to test infrastructure

## Resolution

This bug was fixed as part of FEAT-007 "Fix EF Core Integration Tests with PostgreSQL TestContainers" on 2025-01-29.

### Solution Implemented:
1. **Replaced In-Memory Provider**: Migrated from EF Core In-Memory database to PostgreSQL TestContainers
2. **Real Database Testing**: Tests now run against actual PostgreSQL instances in Docker containers
3. **Full Query Support**: All EF Core features including `.AsSplitQuery()` now work correctly
4. **Database Migrations**: Tests use real migrations instead of `EnsureCreated`

### Results:
- ✅ 7 out of 8 tests now pass successfully
- ✅ The one failing test revealed an actual API bug (muscle group filter issue)
- ✅ Test execution time remains reasonable (~6.5 seconds for full suite)
- ✅ No more "Sequence contains no elements" errors
- ✅ Complex queries with multiple includes work correctly

### Implementation Details:
- Added `TestContainers.PostgreSql` NuGet package
- Created `PostgreSqlTestFixture` for container lifecycle management
- Updated `ApiTestFixture` to use PostgreSQL instead of In-Memory
- Implemented proper test data cleanup between test runs
- All 8 previously skipped tests have been migrated

### Reference:
- Implementation PR: feature/integration-tests-postgresql branch
- Fixed by Feature: FEAT-007

## Related Feature
- FEAT-007: Fix EF Core Integration Tests with PostgreSQL TestContainers