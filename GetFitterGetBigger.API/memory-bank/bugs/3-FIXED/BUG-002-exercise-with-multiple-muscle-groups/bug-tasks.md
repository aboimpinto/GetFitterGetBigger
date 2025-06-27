# BUG-002 Fix Tasks

## Bug Branch: `bugfix/exercise-multiple-muscle-groups`

### Task Categories

#### 1. Test Creation (Reproduce Bug)
- **Task 1.1:** Create failing unit test that sets up exercise with multiple muscle groups [IMPLEMENTED: 55fdb946]
- **Task 1.2:** Create integration test for GET /api/Exercises with multi-muscle exercise [IMPLEMENTED: 55fdb946]

#### 2. Fix Implementation
- **Task 2.1:** Investigate query logic in ExerciseRepository.GetPagedAsync [IMPLEMENTED: 55fdb946]
- **Task 2.2:** Fix the issue causing exercises with multiple muscle groups to be excluded [IMPLEMENTED: 55fdb946]
- **Task 2.3:** Ensure proper eager loading of related entities [IMPLEMENTED: 55fdb946]

#### 3. Verification
- **Task 3.1:** Run all ExerciseRepository tests [IMPLEMENTED: 55fdb946]
- **Task 3.2:** Run all ExerciseController integration tests [IMPLEMENTED: 55fdb946]
- **Task 3.3:** Create manual test script for API verification [IMPLEMENTED: 55fdb946]
- **Task 3.4:** Update documentation if needed [IMPLEMENTED: 55fdb946]

### Test Scripts
- `test-exercise-multiple-muscles.sh` - Manual verification script
- `reproduce-bug.sh` - Script to reproduce the issue

### Notes
- This is likely related to query construction or eager loading
- May be similar to BUG-002 mentioned in exercise-management-tasks.md (EF Core In-Memory database issues)
- Need to verify if this happens with PostgreSQL or just in-memory database