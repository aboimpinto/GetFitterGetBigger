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

### 3. Test-First Implementation Phase
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

### 4. Test Development Rules
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

### 5. Fix Verification Phase
- After fix implementation:
  - ALL tests must be GREEN on ALL affected platforms
  - Builds must succeed with minimal warnings
  - Linting must pass (where applicable)
  - Manual test scripts must execute successfully on each platform
- Only when ALL tests pass on ALL platforms, return control to user

### 6. Manual Testing Phase
- Provide user with:
  - Platform-specific test scripts from the bug file
  - Expected behaviors on each platform
  - Device/browser/OS requirements for testing
  - Steps to verify the fix
  - Any additional verification steps
- Wait for user acceptance on all affected platforms before proceeding

### 7. Bug Finalization
After user confirms bug is fixed on all platforms:
1. Update bug file status to `[FIXED]`
2. Merge bugfix branch into original branch
3. Create descriptive commit message referencing BUG-ID
4. Delete the bugfix branch locally

### 8. Handling Blocked Bugs
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

### Category 3: Verification Tests
- **Task 3.1:** Run all Mobile tests [TODO]
- **Task 3.2:** Run all Web tests [TODO]
- **Task 3.3:** Run all Desktop tests [TODO]
- **Task 3.4:** Create platform test scripts for manual testing [TODO]

### Category 4: Documentation
- **Task 4.1:** Document the fix and prevention [TODO]
- **Task 4.2:** Update platform-specific documentation [TODO]

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