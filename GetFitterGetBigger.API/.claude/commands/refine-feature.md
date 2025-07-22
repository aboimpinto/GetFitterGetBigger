# Refine Feature Command

This command analyzes a feature description and breaks it down into detailed implementation tasks following GetFitterGetBigger's established patterns and best practices.

## Usage

`/refine-feature [FEAT-ID]` - Refine a feature in the 0-SUBMITTED state

Example: `/refine-feature FEAT-026`

**Prerequisites**:
- Feature must exist in `/memory-bank/features/0-SUBMITTED/FEAT-XXX-feature-name/`
- Feature must have a `feature-description.md` file

## Process Overview

The command will:
1. Analyze the feature description AND **all other files** in the `0-SUBMITTED` folder
   - `feature-description.md` - Main feature specification
   - `implementation-plan.md` - Technical implementation details (if exists)
   - `api-endpoints-spec.md` - API contract definitions (if exists)
   - Any mockups, diagrams, or supporting documents
2. **Study the codebase** to identify reusable patterns and similar implementations
3. Review memory-bank for relevant patterns, pitfalls, and lessons learned
4. Generate a comprehensive `feature-tasks.md` with proper task ordering
5. Include references to existing code examples
6. Incorporate lessons learned from completed features
7. **Move the feature folder** from `0-SUBMITTED` to `1-READY_TO_DEVELOP`

## Key Requirements

### 1. Codebase Study Task (MANDATORY)
Every feature refinement MUST include an initial task to study the codebase:
```
Task X.X: Study codebase for similar implementations
- Search for similar entities/services/controllers
- Identify patterns to follow
- Note code that can be reused or adapted
- Document findings with specific file references
```

### 2. Task Ordering Validation
- Database migrations MUST come early (to keep integration tests green)
- Follow logical dependency order: Models → Database → Repository → Service → Controller
- Each category requires a checkpoint before proceeding
- **IMPORTANT**: Tests are NOT a separate category - they must be included with each implementation task
- **MANDATORY CHECKPOINTS**: Each checkpoint must be GREEN before proceeding:
  - Build: 0 errors, 0 warnings
  - Tests: All green/pass
  - Code Review: APPROVED (see `/memory-bank/UNIFIED_DEVELOPMENT_PROCESS.md`)
  - **Report Format**: Every checkpoint must include:
    ```
    Build Report: X errors, Y warnings
    Test Report: A passed, B failed (Total: C)
    Code Review: [filename] - [STATUS]
    ```
- **Final Task**: The last task must include a final checkpoint that verifies:
  - All code reviews are in APPROVED state
  - Complete test suite passes
  - Zero build errors and warnings
  - Feature is ready for deployment

### 3. Reference Key Documents
The generated tasks should reference (not copy):
- `/memory-bank/FEATURE_IMPLEMENTATION_PROCESS.md` - For task structure template
- `/memory-bank/common-implementation-pitfalls.md` - Before service implementation
- `/memory-bank/service-implementation-checklist.md` - Quick reference during coding
- `/memory-bank/SERVICE-RESULT-PATTERN.md` - For error handling
- `/memory-bank/TESTING-QUICK-REFERENCE.md` - For test implementation

### 4. Include Lessons Learned
Review and incorporate lessons from:
- `/memory-bank/features/3-COMPLETED/*/LESSONS-LEARNED.md`
- Relevant bug fixes in `/memory-bank/bugs/3-FIXED/*/`
- Recent code reviews in completed features

### 5. Task Detail Requirements
Each task should include:
- Clear description of what to implement
- Time estimate based on complexity (including time for tests)
- **Specific code examples** from the codebase (with file paths)
- References to relevant patterns or guidelines
- Any critical warnings (e.g., "Use ReadOnlyUnitOfWork for validation")
- **Unit tests and/or integration tests** as part of the same task
- Test file locations and specific test scenarios to implement

## Implementation Steps

1. **Read and analyze ALL files** in the feature folder:
   - `feature-description.md` - Primary feature specification
   - Any additional documentation files (implementation plans, API specs, etc.)
   - Mockups, diagrams, or example files
   - **IMPORTANT**: These files often contain critical implementation details not in the main description
2. **Search the codebase** for similar implementations
3. **Review memory-bank** for:
   - Completed features with similar patterns
   - Relevant lessons learned
   - Common pitfalls to avoid
4. **Generate feature-tasks.md** following the structure in:
   - `/memory-bank/FEATURE_IMPLEMENTATION_PROCESS.md` (Section 2)
   - Incorporate details from ALL analyzed files
5. **Validate task order** to ensure:
   - Database changes come early
   - Dependencies are respected
   - Tests can stay green throughout
6. **Include TDD approach**:
   - Create test builders early in planning phase
   - Each implementation task includes its tests
   - No separate testing category at the end

## Output Format

The command generates a `feature-tasks.md` file with:
- Pre-implementation checklist (referencing key documents)
- Note to create test builders early (in planning phase)
- BDD scenarios defined upfront (to be implemented throughout)
- Tasks organized by category with checkpoints
- Each task includes:
  - Description and time estimate (implementation + tests)
  - Implementation steps
  - Unit/Integration test requirements
  - Test file locations and scenarios
  - References to similar code in the codebase
  - Critical patterns to follow
  - Warnings about common pitfalls
- **CHECKPOINT sections between categories** with:
  - Build verification (0 errors, 0 warnings)
  - Test verification (all green)
  - Code review requirement (APPROVED status)
  - Reference to `/memory-bank/UNIFIED_DEVELOPMENT_PROCESS.md`
  - **Checkpoint Report Template**:
    ```
    ## CHECKPOINT: [Category Name]
    `[Status]` - Date: YYYY-MM-DD
    
    Build Report: 0 errors, 0 warnings
    Test Report: X passed, 0 failed (Total: X)
    Code Review: code-review/code-review-YYYY-MM-DD-HHMMSS.md - APPROVED
    
    Notes: [Any relevant observations]
    ```
  - **Code Review Iterations**: If first review is not APPROVED:
    - Keep initial report unchanged
    - After refactoring, create new code review file
    - Add to checkpoint report:
      ```
      Code Review History:
      - code-review/code-review-YYYY-MM-DD-HHMMSS.md - NEED_CHANGES
      - code-review/code-review-YYYY-MM-DD-HHMMSS-v2.md - APPROVED_WITH_NOTES
      - code-review/code-review-YYYY-MM-DD-HHMMSS-v3.md - APPROVED
      ```
- BOY SCOUT RULE section for improvements found during implementation

## Quality Checks

The generated task list must:
- ✅ Analyze ALL files in the feature folder (not just feature-description.md)
- ✅ Include initial codebase study task
- ✅ Create test builders early (planning phase)
- ✅ Include tests with each implementation task (TDD approach)
- ✅ Reference (not duplicate) guidelines
- ✅ Include specific code examples with file paths
- ✅ Incorporate relevant lessons learned
- ✅ Follow proper task ordering for green tests
- ✅ Include checkpoints between categories
- ✅ Reference critical patterns (ServiceResult, UnitOfWork, etc.)
- ✅ NO separate testing category - tests are part of each task
- ✅ Incorporate details from supporting documents (API specs, implementation plans, etc.)

## Code Review Status Values

- **NEED_CHANGES**: Code requires modifications before approval
- **APPROVED_WITH_NOTES**: Code is approved but has suggestions for improvement
- **APPROVED**: Code meets all standards and is ready for merge

## Example Task Format

```markdown
### Task 3.2: Create WorkoutTemplateService with tests
`[Pending]` (Est: 5h)

**Implementation (3h):**
- Create service following pattern from `Services/ExerciseService.cs`
- All methods must return ServiceResult<T> (see SERVICE-RESULT-PATTERN.md)
- Use ReadOnlyUnitOfWork for validation queries
- Use WritableUnitOfWork ONLY for Create/Update/Delete
- **WARNING**: No exceptions for control flow!

**Unit Tests (2h):**
- Create `Tests/Services/WorkoutTemplateServiceTests.cs`
- Mock UnitOfWorkProvider and repositories
- Test CreateAsync with valid/invalid data
- Test ownership validation scenarios
- Test public/private visibility rules
- Reference: `/memory-bank/TESTING-QUICK-REFERENCE.md`
```

## Example Checkpoint with Reports

```markdown
## CHECKPOINT: Models & Database
`[Completed]` - Date: 2025-01-15

Build Report: 0 errors, 0 warnings
Test Report: 156 passed, 0 failed (Total: 156)
Code Review: code-review/code-review-2025-01-15-143022.md - APPROVED

Notes: All database migrations applied successfully. Entity configurations verified.
```

## Success Criteria

The refined feature tasks are ready when:
- Every task has clear implementation guidance
- Every implementation task includes corresponding tests
- Test builders are created early for TDD approach
- Critical patterns and pitfalls are highlighted
- Existing code examples are referenced
- Task order ensures tests stay green
- Lessons from similar features are incorporated
- NO separate testing category exists
- Feature folder is moved from `0-SUBMITTED` to `1-READY_TO_DEVELOP`

## Final Step: Move Feature to Ready State

After successfully generating feature-tasks.md:
1. Verify the feature folder is currently in `/memory-bank/features/0-SUBMITTED/`
2. Move the entire feature folder to `/memory-bank/features/1-READY_TO_DEVELOP/`
3. This indicates the feature is fully planned and ready for implementation
4. **Reference**: `/memory-bank/FEATURE_WORKFLOW_PROCESS.md` and `/memory-bank/FEATURE_IMPLEMENTATION_PROCESS.md`