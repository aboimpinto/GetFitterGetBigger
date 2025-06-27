# Bug Workflow Process

This document defines the complete lifecycle of bugs, from discovery to resolution, using a state-based folder structure.

## Bug States and Folder Structure

```
memory-bank/bugs/
├── 1-OPEN/          # Newly reported bugs awaiting triage
├── 2-IN_PROGRESS/   # Bugs currently being fixed
├── 3-FIXED/         # Bugs that have been resolved
├── 4-BLOCKED/       # Bugs blocked by dependencies
└── 5-WONT_FIX/     # Bugs that won't be addressed
```

## Bug Lifecycle

### 1. Bug Discovery (OPEN)
When a bug is reported:
1. Create a folder: `memory-bank/bugs/1-OPEN/BUG-[ID]-[short-description]/`
2. Create required files in the folder:
   - `bug-report.md` - Detailed bug description and reproduction steps
   - `bug-tasks.md` - Fix implementation task list
3. Optional: Add error logs, screenshots, or reproduction scripts

**Folder Structure Example:**
```
1-OPEN/
└── BUG-001-exercise-isactive-column/
    ├── bug-report.md
    ├── bug-tasks.md
    ├── error-log.txt
    └── reproduction-steps.md
```

### 2. Bug Fix Development (IN_PROGRESS)
When fix development begins:
1. Move entire bug folder from `1-OPEN` to `2-IN_PROGRESS`
2. Create bugfix branch as specified in tasks file
3. Write failing tests FIRST (test-first approach)
4. Implement fix to make tests pass
5. Add all test scripts to the folder
6. Update task statuses with commit hashes

**Folder Structure Example:**
```
2-IN_PROGRESS/
└── BUG-001-exercise-isactive-column/
    ├── bug-report.md
    ├── bug-tasks.md (with commit hashes)
    ├── error-log.txt
    ├── test-exercise-api.sh
    └── unit-test-results.txt
```

### 3. Bug Resolution (FIXED)
When bug is completely fixed:
1. Ensure ALL tests are green
2. Run manual verification scripts
3. Get user confirmation
4. Move entire folder from `2-IN_PROGRESS` to `3-FIXED`
5. Add fix date and version to bug-report.md
6. Bug is now included in next release notes

### 4. Blocked Bugs (BLOCKED)
If a bug cannot be fixed:
1. Move folder from current location to `4-BLOCKED`
2. Add `BLOCKED_REASON.md` file containing:
   - Detailed blocking reason
   - Dependencies required
   - Links to blocking issues
   - Workaround (if any)
3. Update blocking issue to reference this bug
4. When unblocked, move back to `2-IN_PROGRESS`

### 5. Won't Fix (WONT_FIX)
If bug won't be addressed:
1. Move folder to `5-WONT_FIX`
2. Add `WONT_FIX_REASON.md` file explaining:
   - Decision rationale
   - Workarounds available
   - Risk assessment
   - User impact

## File Templates

### bug-report.md Template
```markdown
# BUG-[ID]: [Bug Title]

## Bug ID: BUG-[XXX]
## Reported: [Date]
## Status: [Current State]
## Severity: [Critical|High|Medium|Low]
## Affected Version: [Version]
## Fixed Version: [Version when fixed]

## Description
[Clear description of the bug]

## Error Message
```
[Full error message or stack trace]
```

## Reproduction Steps
1. Step 1
2. Step 2
3. Expected: [What should happen]
4. Actual: [What actually happens]

## Root Cause
[Analysis of why the bug occurs]

## Impact
- Users affected: [Who/How many]
- Features affected: [List features]
- Business impact: [Description]

## Workaround
[Any temporary workaround if available]

## Test Data
[Any specific test data needed to reproduce]

## Fix Summary
[Brief description of how it was fixed - added after resolution]
```

### bug-tasks.md Template
```markdown
# BUG-[ID] Fix Tasks

## Bug Branch: `bugfix/[descriptive-name]`

### Task Categories

#### 1. Test Creation (Reproduce Bug)
- **Task 1.1:** Create failing unit test [TODO]
- **Task 1.2:** Create failing integration test [TODO]

#### 2. Fix Implementation
- **Task 2.1:** [Specific fix action] [TODO]
- **Task 2.2:** [Additional fix if needed] [TODO]

#### 3. Boy Scout Cleanup (MANDATORY)
- **Task 3.1:** Fix any failing tests found during work [TODO]
- **Task 3.2:** Fix all build warnings in touched files [TODO]
- **Task 3.3:** Clean up code smells in modified files [TODO]

#### 4. Verification
- **Task 4.1:** Run ALL tests (must be 100% green) [TODO]
- **Task 4.2:** Verify zero build warnings [TODO]
- **Task 4.3:** Create manual test script [TODO]
- **Task 4.4:** Update documentation [TODO]

### Test Scripts
- `test-[bug-name].sh` - Manual verification script
- `reproduce-bug.sh` - Script to reproduce the issue

### Notes
[Any implementation notes or considerations]
```

## State Transition Rules

1. **To IN_PROGRESS**: Only when developer starts working on fix
2. **To FIXED**: Only when ALL tests pass and user confirms
3. **To BLOCKED**: Immediately when blocking issue identified
4. **To WONT_FIX**: Only with explicit decision and justification
5. **From BLOCKED**: Only when blocking issue is resolved

## Bug Prioritization

Bugs in `1-OPEN` should be triaged based on:
- **Critical**: System down, data loss, security issue
- **High**: Major feature broken, no workaround
- **Medium**: Feature impaired, workaround exists
- **Low**: Minor issue, cosmetic, edge case

## Testing Requirements

### Test-First Approach
1. **Failing Test Required**: Must write test that reproduces bug
2. **Test Must Fail**: Verify test fails with exact error
3. **Fix Implementation**: Write minimum code to pass test
4. **All Tests Green**: No test should be skipped or failing
5. **Regression Tests**: Add tests to prevent recurrence

### Boy Scout Rule - MANDATORY
When fixing ANY bug, you MUST:
1. **Fix ALL failing tests** in the codebase, not just bug-related ones
2. **Fix ALL build warnings** encountered during your work
3. **Clean up code** in any files you touch
4. **Update outdated documentation** you encounter
5. **Remove dead code** and improve code quality

**NO EXCEPTIONS**: If you find a failing test or warning while fixing a bug, it becomes YOUR responsibility to fix it. The codebase must be in a better state after your bug fix than before.

### Test Scripts
Each bug folder MUST contain:
- Automated test that reproduces the bug
- Manual verification script
- Test results documentation

## Integration with Release Process

- Only bugs in `3-FIXED` are included in release notes
- Fixed bugs are grouped by severity in release notes
- Each bug folder contains all info needed for release documentation
- Critical/High severity fixes may trigger hotfix releases

## Metrics and Monitoring

Track bug metrics:
- Time in each state
- Fix cycle time (OPEN → FIXED)
- Bugs by severity
- Blocked bug count and reasons
- Won't fix ratios

## Best Practices

1. **One Bug Per Folder**: Keep bugs isolated and focused
2. **Comprehensive Documentation**: Include all artifacts
3. **Test Evidence**: Keep test results and scripts
4. **Clear Communication**: Document all decisions
5. **Fast Triage**: Move from OPEN quickly

## Cleanup Policy

- FIXED bugs older than 2 PIs can be archived
- WONT_FIX bugs older than 1 PI can be archived
- Archive location: `memory-bank/archive/bugs/[year]/`
- Never delete - always archive for historical reference

## Bug ID Sequence

- Use sequential numbering: BUG-001, BUG-002, etc.
- Maintain a `NEXT_BUG_ID.txt` file in bugs folder
- Never reuse bug IDs
- Format: BUG-[3-digit-number]