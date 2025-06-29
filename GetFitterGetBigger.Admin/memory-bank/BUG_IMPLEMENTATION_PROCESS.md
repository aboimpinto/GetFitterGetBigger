# Bug Implementation Process

This document outlines the standard process for fixing bugs in the GetFitterGetBigger Admin web application.

## Process Overview

### 1. Bug Analysis & Planning
- User provides bug description with error details and reproduction steps
- Create a comprehensive bug fix plan with granular tasks
- **MANDATORY**: Create a bug tracking file at `memory-bank/bugs/BUG-[ID]-[descriptive-name]/bug-tasks.md`
- Each task must be marked with status `[TODO]`
- Tasks should be specific, actionable, and independently verifiable
- The bug file must include:
  - Bug branch name at the top
  - Bug description with full error details
  - Root cause analysis
  - Impact assessment
  - Tasks organized by logical categories (Test Creation, Fix Implementation, Verification)
  - **Failing test tasks MUST come before fix implementation**
  - Clear description of what each task entails
  - Test script for manual verification
  - Space for commit hashes to be added as tasks are completed

### 2. Branch Creation
- Create a dedicated bugfix branch from the current branch
- Naming convention: `bugfix/[descriptive-bug-name]`
- All bug fix work occurs in this isolated branch

### 3. Baseline Health Check (MANDATORY)
Before starting ANY bug fix:
1. **Run baseline health check**:
   ```bash
   npm run build
   npm test
   npm run lint
   ```
2. **Document results in bug-tasks.md**:
   ```markdown
   ## Baseline Health Check Report
   **Date/Time**: YYYY-MM-DD HH:MM
   **Branch**: bugfix/branch-name

   ### Build Status
   - **Build Result**: ✅ Success / ❌ Failed / ⚠️ Success with warnings
   - **Warning Count**: X warnings
   - **Warning Details**: [List specific warnings if any]

   ### Test Status
   - **Total Tests**: X
   - **Passed**: X
   - **Failed**: X (excluding the bug being fixed)
   - **Skipped/Ignored**: X
   - **Test Execution Time**: X.XX seconds

   ### Linting Status
   - **Errors**: X (MUST be 0 to proceed)
   - **Warnings**: X

   ### Decision to Proceed
   - [ ] Build successful
   - [ ] No unrelated test failures
   - [ ] No linting errors
   - [ ] Warnings documented and approved

   **Approval to Proceed**: Yes/No
   ```

3. **Evaluation and Action**:
   - ✅ **Build OK, Tests OK**: Proceed to bug fix
   - ❌ **Build Fails**: STOP - Fix build first (Task 0.1)
   - ❌ **Unrelated Tests Fail**: STOP - Fix other tests first (Task 0.2)
   - ❌ **Lint Errors**: STOP - Fix linting errors (Task 0.3)
   - ⚠️ **Warnings Exist**: Document and apply Boy Scout Rule
     - Create Task 0.4+ for warning fixes
     - Fix warnings in touched files

### 4. Test-First Implementation Phase
- **CRITICAL**: Write failing tests FIRST that reproduce the bug
- Execute tasks sequentially from the bug tracking file
- **For EVERY task implementation:**
  1. If it's a test task: Write test that fails with the exact error described
  2. If it's a fix task: Implement the minimum code to make tests pass
  3. **MANDATORY: Keep linting warnings to a minimum**
  4. **MANDATORY: Run `npm run build` to ensure compilation succeeds**
  5. **MANDATORY: Run `npm test` to verify test status**
  6. **MANDATORY: Run `npm run lint` to ensure code quality**
  7. Commit the changes with descriptive message
  8. Update task status from `[TODO]` to `[IMPLEMENTED: <git-commit-hash>]`
- Task progression states:
  - `[TODO]` - Task identified and ready to implement
  - `[PROGRESS]` - Currently working on this task
  - `[IMPLEMENTED: <hash>]` - Task completed with reference commit
  - `[BLOCKED: <reason>]` - Task cannot be completed due to dependency

### 5. Test Development Rules
- **First tests MUST fail** with the same error as the bug report
- Tests should cover:
  - The exact scenario that triggered the bug
  - Edge cases related to the bug
  - Regression prevention
- If a test cannot be written due to technical limitations:
  - Mark as `[BLOCKED: <detailed-reason>]`
  - Create a new bug entry for the blocker
  - Link both bugs bidirectionally

### 6. Fix Verification Phase
- After fix implementation:
  - ALL tests must be GREEN (no failures, no skipped)
  - Build must succeed with minimal warnings
  - Linting must pass
  - Manual test script must execute successfully
- Only when ALL tests pass, return control to user

### 7. Manual Testing Phase
- Provide user with:
  - The test script from the bug file
  - Expected UI behaviors
  - Steps to verify the fix
  - Any additional verification steps
- Wait for user acceptance before proceeding

### 8. Quality Comparison Report (MANDATORY)
After all bug fix tasks are complete, add to bug-tasks.md:
```markdown
## Implementation Summary Report
**Date/Time**: YYYY-MM-DD HH:MM
**Duration**: X days/hours

### Quality Metrics Comparison
| Metric | Baseline | Final | Change |
|--------|----------|-------|--------|
| Build Warnings | X | Y | -Z |
| Test Count | X | Y | +Z |
| Test Pass Rate | X% | 100% | +Z% |
| Skipped Tests | X | Y | -Z |
| Lint Warnings | X | Y | -Z |

### Quality Improvements
- Fixed X build warnings in touched files
- Added Y new tests for bug fix
- Fixed Z unrelated failing tests
- Fixed X linting warnings
- [Other improvements]

### Boy Scout Rule Applied
- ✅ All issues in touched files fixed
- ✅ Bug properly tested
- ✅ No regression introduced
```

### 9. Bug Finalization
After user confirms bug is fixed:
1. Update bug file status to `[FIXED]`
2. Merge bugfix branch into original branch
3. Create descriptive commit message referencing BUG-ID
4. Delete the bugfix branch locally

### 10. Handling Blocked Bugs
When a bug cannot be fixed due to dependencies:
1. Mark the bug overall status as `[BLOCKED: <reason>]`
2. Create new bug entry for the blocker with:
   - Bug ID and description
   - Link to the blocked bug
   - Expected resolution approach
3. Update the blocking bug/feature to reference this blocked bug
4. When blocker is resolved:
   - Resume from the blocked task
   - Update status to continue implementation

## Bug Status Definitions
- `[TODO]` - Task identified and ready to implement
- `[PROGRESS]` - Currently working on this task
- `[IMPLEMENTED: <hash>]` - Task completed with reference commit
- `[BLOCKED: <reason>]` - Task blocked by external dependency
- `[FIXED]` - Bug completely resolved and verified

## Bug File Template

```markdown
# BUG-[ID]: [Short Description]

## Bug Branch: `bugfix/[descriptive-name]`

## Bug Description
[Detailed description of the bug including error message or console output]

## Root Cause
[Analysis of why the bug occurs]

## Bug Impact
- [Impact on users]
- [Impact on system]
- [Affected features]

## Implementation Tasks

### Category 1: Test Creation (Reproduce the Bug)
- **Task 1.1:** Create unit test that reproduces the error [TODO]
- **Task 1.2:** Create component test for affected UI [TODO]

### Category 2: Fix Implementation
- **Task 2.1:** [Specific fix action] [TODO]
- **Task 2.2:** [Additional fix if needed] [TODO]

### Category 3: Boy Scout Cleanup (MANDATORY)
- **Task 3.1:** Fix any failing tests found during work [TODO]
- **Task 3.2:** Fix all build warnings in touched files [TODO]
- **Task 3.3:** Clean up code smells in modified files [TODO]
- **Task 3.4:** Update outdated documentation encountered [TODO]

### Category 4: Verification Tests
- **Task 4.1:** Run all related unit tests [TODO]
- **Task 4.2:** Run all component tests [TODO]
- **Task 4.3:** Create test script for manual testing [TODO]

### Category 5: Documentation
- **Task 5.1:** Document the fix and prevention [TODO]

## Test Script for Manual Verification
```bash
#!/bin/bash
# Test script for [bug description]
[Actual test commands or manual steps]
```

## Prevention Measures
[List measures to prevent similar bugs]

## Notes
[Any additional context or considerations]
```

## Important Rules
1. **Tests First**: Always write failing tests before implementing fixes
2. **Green Tests**: Bug is only fixed when ALL tests pass
3. **Minimal Fix**: Implement the minimum code needed to fix the bug
4. **No Regression**: Ensure fix doesn't break existing functionality
5. **Documentation**: Always document the fix and prevention measures

## Verification Checklist

Before marking bug as `[FIXED]`:
- [ ] Failing tests written that reproduce the bug
- [ ] Fix implemented that makes tests pass
- [ ] ALL tests are GREEN (no failures, no skipped)
- [ ] `npm run build` runs without errors
- [ ] Build warnings are minimal
- [ ] `npm run lint` passes without errors
- [ ] Manual test script executes successfully
- [ ] No regression in existing functionality
- [ ] Documentation updated with fix details

## Example Bug Status Updates

Initial state:
```
- **Task 1.1:** Create failing test [TODO]
```

Working on task:
```
- **Task 1.1:** Create failing test [PROGRESS]
```

Task complete:
```
- **Task 1.1:** Create failing test [IMPLEMENTED: a1b2c3d4]
```

Blocked task:
```
- **Task 2.1:** Fix API integration [BLOCKED: API endpoint not available]
```

## Admin-Specific Considerations

### UI/UX Bug Fixes
- Test across different browsers
- Verify responsive behavior
- Check accessibility impact
- Test with different user roles

### Component Testing
- Use React Testing Library
- Test user interactions
- Verify error states
- Check loading states

### Integration Testing
- Mock API responses appropriately
- Test error handling
- Verify authentication flows
- Check state management

### Performance
- Ensure fix doesn't introduce performance issues
- Check component re-renders
- Verify no memory leaks
- Test with realistic data volumes