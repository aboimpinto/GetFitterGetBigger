# Baseline Health Check Process

This document defines the mandatory baseline health check process that must be performed before starting any feature or bug implementation across all GetFitterGetBigger projects.

## Purpose

The baseline health check ensures:
- No existing issues are masked by new work
- Build health is maintained throughout development
- Test suite integrity is preserved
- Clear accountability for code quality
- Measurable quality improvements through Boy Scout Rule

## When to Perform

A baseline health check is **MANDATORY** before:
- Starting any feature implementation
- Starting any bug fix
- Resuming work after a break
- Starting work on a different branch

## Health Check Components

### 1. Build Status Check
```bash
# API Project
dotnet build

# Admin Project
npm run build

# Clients Project
npm run build  # Web/Mobile
dotnet build   # Desktop
```

**Requirements:**
- ✅ Build must succeed completely
- ❌ If build fails: STOP - Fix build first
- ⚠️ If warnings exist: Document count and ask for guidance

### 2. Test Suite Status
```bash
# API Project
dotnet test

# Admin Project
npm test

# Clients Project
npm test       # Web/Mobile
dotnet test    # Desktop
```

**Requirements:**
- ✅ All tests must pass (or have documented skips)
- ❌ If any test fails: STOP - Fix failing tests first
- ⚠️ Document number of skipped/ignored tests

### 3. Linting Status (Where Applicable)
```bash
# Admin Project
npm run lint

# Clients Project (Web/Mobile)
npm run lint
```

**Requirements:**
- ✅ No linting errors
- ⚠️ Document warning count if any

## Baseline Report Format

Add this report at the beginning of your task/bug file:

```markdown
## Baseline Health Check Report
**Date/Time**: YYYY-MM-DD HH:MM
**Branch**: feature/branch-name

### Build Status
- **Build Result**: ✅ Success / ❌ Failed / ⚠️ Success with warnings
- **Warning Count**: X warnings
- **Warning Details**: [List specific warnings if any]

### Test Status
- **Total Tests**: X
- **Passed**: X
- **Failed**: X (MUST be 0 to proceed)
- **Skipped/Ignored**: X
- **Test Execution Time**: X.XX seconds

### Linting Status (if applicable)
- **Errors**: X (MUST be 0 to proceed)
- **Warnings**: X

### Decision to Proceed
- [ ] All tests passing
- [ ] Build successful
- [ ] No linting errors
- [ ] Warnings documented and approved

**Approval to Proceed**: Yes/No
**Approved By**: [User/Self]
**Conditions**: [Any specific conditions for proceeding with warnings]
```

## Implementation Flow

### 1. Pre-Implementation Phase
1. Run baseline health check
2. Document results in task file
3. Evaluate results:
   - **All Green**: Proceed to implementation
   - **Build Fails**: Create priority task to fix build
   - **Tests Fail**: Create priority task to fix tests
   - **Warnings Exist**: Ask for approval, plan Boy Scout tasks

### 2. Boy Scout Rule Application
If proceeding with warnings:
1. Create Boy Scout tasks as Task 0.X in your task list
2. Complete Boy Scout tasks FIRST
3. Re-run baseline check after Boy Scout cleanup
4. Document improved baseline

### 3. Post-Implementation Comparison
At feature/bug completion, create comparison report:

```markdown
## Implementation Summary Report
**Date/Time**: YYYY-MM-DD HH:MM
**Duration**: X days/hours

### Quality Metrics Comparison
| Metric | Baseline | Final | Change |
|--------|----------|-------|--------|
| Build Warnings | X | Y | -Z |
| Test Count | X | Y | +Z |
| Test Pass Rate | X% | Y% | +Z% |
| Skipped Tests | X | Y | -Z |
| Lint Warnings | X | Y | -Z |

### Quality Improvements
- Fixed X build warnings
- Added Y new tests
- Removed Z skipped tests
- [Other improvements]

### Boy Scout Rule Applied
- ✅ All encountered issues fixed
- ✅ Code quality improved
- ✅ Documentation updated
```

## Special Cases

### Critical Production Fixes
For emergency fixes only:
1. Document "EMERGENCY FIX" in baseline report
2. Note pre-existing issues to fix later
3. Create follow-up tasks for deferred fixes
4. Schedule Boy Scout cleanup sprint

### Large Warning Counts
If baseline has many warnings:
1. Document top 10 most critical warnings
2. Create separate Boy Scout task list
3. Get approval for phased cleanup approach
4. Track progress across multiple features

## Benefits

1. **Quality Assurance**: Never start with a broken baseline
2. **Accountability**: Clear documentation of code state
3. **Continuous Improvement**: Measurable quality gains
4. **Risk Reduction**: Catch issues before they compound
5. **Team Awareness**: Shared understanding of codebase health

## Integration with Existing Processes

### Feature Implementation
- Add baseline check as Task 0.0
- Boy Scout tasks become 0.1, 0.2, etc.
- Original Task 1.1 remains first feature task

### Bug Implementation
- Baseline check before test creation
- Ensures bug isn't masked by other issues
- Boy Scout cleanup in Category 3

## Enforcement

This process is **MANDATORY** and non-negotiable because:
- Starting with failing tests makes debugging impossible
- Build failures cascade into more problems
- Warnings accumulate into technical debt
- Quality standards must be maintained

## Tools and Automation

Consider creating helper scripts:

```bash
# baseline-check.sh
#!/bin/bash
echo "=== Baseline Health Check ==="
echo "Date: $(date)"
echo ""
echo "Running build..."
# Add project-specific commands
```

## Quick Reference Checklist

Before starting ANY implementation:
- [ ] Run build - Must succeed
- [ ] Run tests - All must pass
- [ ] Check linting - No errors allowed
- [ ] Document baseline in task file
- [ ] Get approval if warnings exist
- [ ] Create Boy Scout tasks if needed
- [ ] Complete Boy Scout tasks first
- [ ] Re-run baseline after cleanup

Remember: A healthy baseline is the foundation of quality development!