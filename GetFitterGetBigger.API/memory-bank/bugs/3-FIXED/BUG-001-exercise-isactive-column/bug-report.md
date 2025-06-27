# BUG-001: Exercise IsActive Column Missing in PostgreSQL

## Bug Branch: `bugfix/exercise-isactive-column-missing`
## Status: [FIXED]

## Bug Description
When calling the endpoint `http://localhost:5214/api/Exercises`, the application throws a PostgreSQL exception:
```
Npgsql.PostgresException (0x80004005): 42703: column e.IsActive does not exist
```

This error occurs because the migration that adds the `IsActive` column to the Exercise table has not been applied to the database, causing a mismatch between the Entity Framework model and the actual database schema.

## Root Cause
The migration `20250626223647_AddExerciseEntityWithRelationships` which adds the `IsActive` column is in a pending state and needs to be applied to the database.

## Bug Impact
- Users cannot retrieve exercise list via the API
- All operations that query exercises with the IsActive filter will fail
- Exercise soft-delete functionality is broken

## Implementation Tasks

### Category 1: Test Creation (Reproduce the Bug)
- **Task 1.1:** Create unit test that reproduces the PostgreSQL column error [IMPLEMENTED: Added test to ExerciseRepositoryTests.cs]
- **Task 1.2:** Create integration test that verifies the Exercise API endpoint filtering [IMPLEMENTED: Added GetPagedAsync_WithIncludeInactive test]

### Category 2: Fix Implementation
- **Task 2.1:** Apply pending migration to add IsActive column [IMPLEMENTED: Applied migration 20250626223647_AddExerciseEntityWithRelationships]
- **Task 2.2:** Verify database schema matches Entity model [IMPLEMENTED: Verified via dotnet ef migrations list]

### Category 3: Verification Tests
- **Task 3.1:** Run all ExerciseRepository unit tests to verify fix [IMPLEMENTED: All 11 tests passing]
- **Task 3.2:** Create test script for manual API endpoint testing [IMPLEMENTED: Script provided below]

### Category 4: Documentation
- **Task 4.1:** Document the fix and prevention measures [IMPLEMENTED: Created BUG_IMPLEMENTATION_PROCESS.md]

## Test Script for Manual Verification
```bash
#!/bin/bash
# Test script for Exercise API endpoint

echo "Testing Exercise API endpoint..."

# Test 1: Get all active exercises (default)
echo "Test 1: Get all active exercises"
curl -X GET "http://localhost:5214/api/Exercises" \
  -H "accept: application/json" | jq

# Test 2: Get all exercises including inactive
echo "Test 2: Get all exercises including inactive"
curl -X GET "http://localhost:5214/api/Exercises?IncludeInactive=true" \
  -H "accept: application/json" | jq

# Test 3: Filter exercises by name
echo "Test 3: Filter exercises by name 'press'"
curl -X GET "http://localhost:5214/api/Exercises?Name=press" \
  -H "accept: application/json" | jq
```

## Prevention Measures
1. Always check for pending migrations before running the application
2. Include migration status check in deployment scripts
3. Add integration tests that verify database schema matches Entity models

## Notes
- This bug was discovered during normal API usage
- The fix is straightforward: apply the pending migration
- Future consideration: Add automated migration checks in startup