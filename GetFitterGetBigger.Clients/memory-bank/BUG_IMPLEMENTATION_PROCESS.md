# Bug Implementation Process

This document outlines the standard process for fixing bugs across GetFitterGetBigger client applications (Mobile, Web, Desktop).

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
  - Affected platforms clearly identified
  - Root cause analysis
  - Impact assessment
  - Tasks organized by logical categories (Test Creation, Fix Implementation, Verification)
  - **Failing test tasks MUST come before fix implementation**
  - Platform-specific tasks clearly marked
  - Test scripts for manual verification on each platform
  - Space for commit hashes to be added as tasks are completed

### 2. Branch Creation
- Create a dedicated bugfix branch from the current branch
- Naming convention: `bugfix/[descriptive-bug-name]`
- All bug fix work occurs in this isolated branch

### 3. Baseline Health Check (MANDATORY)
Before starting ANY bug fix:
1. **Run baseline health check for affected platforms**:
   ```bash
   # Mobile (if affected)
   npm run build
   npm test
   npm run lint

   # Web (if affected)
   npm run build
   npm test
   npm run lint

   # Desktop (if affected)
   dotnet build
   dotnet test
   ```
2. **Document results in bug-tasks.md**:
   ```markdown
   ## Baseline Health Check Report
   **Date/Time**: YYYY-MM-DD HH:MM
   **Branch**: bugfix/branch-name
   **Affected Platforms**: [Mobile, Web, Desktop]

   ### [Platform Name]
   #### Build Status
   - **Build Result**: ✅ Success / ❌ Failed / ⚠️ Success with warnings
   - **Warning Count**: X warnings
   - **Warning Details**: [List specific warnings if any]

   #### Test Status
   - **Total Tests**: X
   - **Passed**: X
   - **Failed**: X (excluding the bug being fixed)
   - **Skipped/Ignored**: X

   #### Linting Status (if applicable)
   - **Errors**: X (MUST be 0 to proceed)
   - **Warnings**: X

   ### Decision to Proceed
   - [ ] Builds successful on all affected platforms
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
     - Create Task 0.4+ for warning fixes per platform
     - Fix warnings in touched files

### 4. Test-First Implementation Phase
- **CRITICAL**: Write failing tests FIRST that reproduce the bug on affected platforms
- Execute tasks sequentially from the bug tracking file
- **For EVERY task implementation:**
  1. If it's a test task: Write test that fails with the exact error described
  2. If it's a fix task: Implement the minimum code to make tests pass
  3. **MANDATORY: Keep build warnings to a minimum**
  4. **MANDATORY: Run platform-specific builds:**
     - Mobile: `npm run build` or platform-specific commands
     - Web: `npm run build`
     - Desktop: `dotnet build`
  5. **MANDATORY: Run platform-specific tests:**
     - Mobile: `npm test`
     - Web: `npm test`
     - Desktop: `dotnet test`
  6. **MANDATORY: Run linting where applicable**
  7. Commit the changes with descriptive message
  8. Update task status from `[TODO]` to `[IMPLEMENTED: <git-commit-hash>]`
- Task progression states:
  - `[TODO]` - Task identified and ready to implement
  - `[PROGRESS]` - Currently working on this task
  - `[IMPLEMENTED: <hash>]` - Task completed with reference commit
  - `[BLOCKED: <reason>]` - Task cannot be completed due to dependency
  - `[N/A - Platform]` - Task not applicable to specific platform

### 5. Test Development Rules
- **First tests MUST fail** with the same error as the bug report
- Tests should cover:
  - The exact scenario that triggered the bug on each platform
  - Edge cases related to the bug
  - Platform-specific variations
  - Regression prevention
- If a test cannot be written due to technical limitations:
  - Mark as `[BLOCKED: <detailed-reason>]`
  - Create a new bug entry for the blocker
  - Link both bugs bidirectionally

### 6. Fix Verification Phase
- After fix implementation:
  - ALL tests must be GREEN on ALL affected platforms
  - Builds must succeed with minimal warnings
  - Linting must pass (where applicable)
  - Manual test scripts must execute successfully on each platform
- Only when ALL tests pass on ALL platforms, return control to user

### 7. Manual Testing Phase
- Provide user with:
  - Platform-specific test scripts from the bug file
  - Expected behaviors on each platform
  - Device/browser/OS requirements for testing
  - Steps to verify the fix
  - Any additional verification steps
- Wait for user acceptance on all affected platforms before proceeding

### 8. Quality Comparison Report (MANDATORY)
After all bug fix tasks are complete, add to bug-tasks.md:
```markdown
## Implementation Summary Report
**Date/Time**: YYYY-MM-DD HH:MM
**Duration**: X days/hours

### Quality Metrics Comparison (Per Platform)

#### [Platform Name]
| Metric | Baseline | Final | Change |
|--------|----------|-------|--------|
| Build Warnings | X | Y | -Z |
| Test Count | X | Y | +Z |
| Test Pass Rate | X% | 100% | +Z% |
| Skipped Tests | X | Y | -Z |
| Lint Warnings | X | Y | -Z |

[Repeat for each affected platform]

### Quality Improvements
- Fixed X build warnings across platforms
- Added Y new tests for bug fix
- Fixed Z unrelated failing tests
- Fixed X platform-specific issues
- [Other improvements]

### Boy Scout Rule Applied
- ✅ All issues in touched files fixed
- ✅ Bug properly tested on all platforms
- ✅ No regression introduced
```

### 9. Bug Finalization
After user confirms bug is fixed on all platforms:
1. Update bug file status to `[FIXED]`
2. Merge bugfix branch into original branch
3. Create descriptive commit message referencing BUG-ID
4. Delete the bugfix branch locally

### 10. Handling Blocked Bugs
When a bug cannot be fixed due to dependencies:
1. Mark the bug overall status as `[BLOCKED: <reason>]`
2. Create new bug entry for the blocker with:
   - Bug ID and description
   - Affected platforms
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
- `[FIXED]` - Bug completely resolved and verified on all platforms
- `[N/A - Platform]` - Task not applicable to specific platform

## Bug File Template

```markdown
# BUG-[ID]: [Short Description]

## Bug Branch: `bugfix/[descriptive-name]`
## Affected Platforms: [Mobile|Web|Desktop|All]

## Bug Description
[Detailed description of the bug including error messages, crash logs, console output]

## Platform-Specific Symptoms
### Mobile (iOS/Android)
[Mobile-specific error details]

### Web
[Web-specific error details]

### Desktop
[Desktop-specific error details]

## Root Cause
[Analysis of why the bug occurs]

## Bug Impact
- [Impact on users]
- [Impact on system]
- [Affected features]
- [Platform-specific impacts]

## Implementation Tasks

### Category 1: Test Creation (Reproduce the Bug)
- **Task 1.1:** Create unit test that reproduces the error [TODO]
- **Task 1.2:** Create Mobile test for bug reproduction [TODO]
- **Task 1.3:** Create Web test for bug reproduction [TODO]
- **Task 1.4:** Create Desktop test for bug reproduction [TODO]

### Category 2: Fix Implementation
- **Task 2.1:** [Core fix action - shared logic] [TODO]
- **Task 2.2:** [Mobile-specific fix] [TODO]
- **Task 2.3:** [Web-specific fix] [TODO]
- **Task 2.4:** [Desktop-specific fix] [TODO]

### Category 3: Boy Scout Cleanup (MANDATORY)
- **Task 3.1:** Fix any failing tests found during work [TODO]
- **Task 3.2:** Fix all linting warnings in touched files [TODO]
- **Task 3.3:** Clean up code smells in modified files [TODO]

### Category 4: Verification
- **Task 4.1:** Run ALL Mobile tests (must be 100% green) [TODO]
- **Task 4.2:** Run ALL Web tests (must be 100% green) [TODO]
- **Task 4.3:** Run ALL Desktop tests (must be 100% green) [TODO]
- **Task 4.4:** Verify zero linting warnings [TODO]
- **Task 4.5:** Create platform test scripts for manual testing [TODO]
- **Task 4.6:** Update documentation [TODO]

## Test Scripts for Manual Verification

### Mobile Test Script
```bash
#!/bin/bash
# Test script for Mobile platforms
[Mobile-specific test commands]
```

### Web Test Script
```bash
#!/bin/bash
# Test script for Web platform
[Web-specific test commands]
```

### Desktop Test Script
```powershell
# Test script for Desktop platforms
[Desktop-specific test commands]
```

## Prevention Measures
[List measures to prevent similar bugs across platforms]

## Notes
[Any additional context or platform-specific considerations]
```

## Important Rules
1. **Tests First**: Always write failing tests before implementing fixes
2. **Green Tests**: Bug is only fixed when ALL tests pass on ALL platforms
3. **Minimal Fix**: Implement the minimum code needed to fix the bug
4. **No Regression**: Ensure fix doesn't break existing functionality on any platform
5. **Documentation**: Always document the fix and prevention measures

## Verification Checklist

Before marking bug as `[FIXED]`:
- [ ] Failing tests written that reproduce the bug on affected platforms
- [ ] Fix implemented that makes tests pass
- [ ] ALL tests are GREEN on ALL platforms (no failures, no skipped)
- [ ] Platform builds run without errors:
  - [ ] Mobile build successful
  - [ ] Web build successful
  - [ ] Desktop build successful
- [ ] Build warnings are minimal
- [ ] Linting passes (where applicable)
- [ ] Manual test scripts execute successfully on each platform
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

Platform not affected:
```
- **Task 2.3:** Web-specific fix [N/A - Web]
```

Blocked task:
```
- **Task 2.1:** Fix native module [BLOCKED: Requires native SDK update]
```

## Platform-Specific Considerations

### Mobile Bug Fixes
- Test on both iOS and Android
- Consider different OS versions
- Test on various device sizes
- Check offline behavior
- Verify performance impact

### Web Bug Fixes
- Test across all supported browsers
- Check responsive breakpoints
- Verify accessibility isn't broken
- Test with different network speeds
- Check SEO impact if applicable

### Desktop Bug Fixes
- Test on Windows, macOS, and Linux
- Check different screen resolutions
- Verify native integrations
- Test keyboard shortcuts
- Check memory usage

### Cross-Platform Fixes
- Ensure consistency across platforms
- Test shared logic thoroughly
- Verify platform-specific code isolation
- Check for platform-specific side effects
- Document any platform differences