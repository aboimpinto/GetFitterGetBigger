# Bug Tracking

This file tracks known bugs that are blocking feature implementation.

## Active Bugs

### BUG-001: [Authorize] Attribute Not Working Correctly
- **Status**: OPEN
- **Severity**: High
- **Discovered**: During Exercise Management feature implementation
- **Blocked Features**: 
  - Exercise Management CRUD - Task 5.3: Add [Authorize] attribute for admin-only access
- **Description**: The [Authorize] attribute is not functioning correctly in the API. Authentication/Authorization middleware may not be properly configured or there might be issues with JWT token validation.
- **Expected Behavior**: When [Authorize] attribute is applied to controllers/actions, only authenticated users with valid JWT tokens should be able to access those endpoints.
- **Actual Behavior**: [To be documented based on specific issue]
- **Reproduction Steps**: [To be documented]
- **Potential Solutions**:
  1. Review JWT middleware configuration in Program.cs
  2. Check if authentication middleware is in the correct order in the pipeline
  3. Verify JWT token generation and validation logic
  4. Ensure proper claim-based authorization is configured

### BUG-002: EF Core In-Memory Database Issues with Complex Queries
- **Status**: OPEN
- **Severity**: Medium
- **Discovered**: During Exercise Management integration tests
- **Affected Tests**: 
  - ExercisesControllerTests - 7 tests skipped
- **Description**: The EF Core In-Memory database provider has limitations when dealing with complex queries that include multiple navigation properties. When using Include() to load related entities (DifficultyLevel, MuscleGroups, Equipment, etc.), the in-memory provider fails to properly track and return the seeded data.
- **Symptoms**:
  - `SeedTestDataAsync()` creates exercises but they're not retrievable in tests
  - "Sequence contains no elements" errors when trying to access seeded data
  - Empty collections returned from queries that should have data
- **Root Cause**: Each test creates a new in-memory database instance, and the complex entity relationships with multiple includes are not properly handled by the in-memory provider.
- **Potential Solutions**:
  1. Use SQLite in-memory mode instead of EF Core In-Memory provider
  2. Use a real test database (PostgreSQL or SQL Server) for integration tests
  3. Simplify the entity model for testing
  4. Mock the repository/service layer instead of using integration tests
- **Workaround**: Tests are currently skipped with this bug reference

## Resolved Bugs

(None yet)