# BUG-002 Fix Tasks

## Bug Branch: `bugfix/exercise-multiple-muscle-groups`

### Task Categories

#### 1. Test Creation (Reproduce Bug)
- **Task 1.1:** Create failing unit test that sets up exercise with multiple muscle groups [TODO]
- **Task 1.2:** Create integration test for GET /api/Exercises with multi-muscle exercise [TODO]

#### 2. Fix Implementation
- **Task 2.1:** Investigate query logic in ExerciseRepository.GetPagedAsync [TODO]
- **Task 2.2:** Fix the issue causing exercises with multiple muscle groups to be excluded [TODO]
- **Task 2.3:** Ensure proper eager loading of related entities [TODO]

#### 3. Verification
- **Task 3.1:** Run all ExerciseRepository tests [TODO]
- **Task 3.2:** Run all ExerciseController integration tests [TODO]
- **Task 3.3:** Create manual test script for API verification [TODO]
- **Task 3.4:** Update documentation if needed [TODO]

### Test Scripts
- `test-exercise-multiple-muscles.sh` - Manual verification script
- `reproduce-bug.sh` - Script to reproduce the issue

### Notes
- This is likely related to query construction or eager loading
- May be similar to BUG-002 mentioned in exercise-management-tasks.md (EF Core In-Memory database issues)
- Need to verify if this happens with PostgreSQL or just in-memory database