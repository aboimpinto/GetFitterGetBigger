# Fix Build Errors Command

This command orchestrates a comprehensive build error and warning fix process that intelligently determines whether errors are due to production code refactoring (requiring test adaptation) or actual regressions (requiring code fixes).

## Command Flow

1. **Execute csharp-build-error-fixer agent** to analyze and categorize all build errors
2. **Determine root cause** for each error (refactoring vs regression)
3. **ASK for clarification** when the cause is unclear
4. **Apply appropriate fixes** based on the analysis
5. **Run tests** to verify fixes don't introduce new issues
6. **Handle warnings** after all errors are resolved

## Agent Task

### Phase 1: Error Analysis and Categorization

Launch the csharp-build-error-fixer agent with these instructions:

**Primary Mission**: Analyze all build errors, determine their root cause (refactoring vs regression), and fix them appropriately.

**Critical Analysis Protocol**:

1. **Initial Build Analysis**:
   - Run `dotnet clean && dotnet build` to get fresh error list
   - Categorize errors by location (production vs test code)
   - Identify patterns suggesting refactoring vs regression

2. **Root Cause Determination** (MOST CRITICAL STEP):
   For EACH error, classify as:
   
   **A. Refactoring-Related** (tests need adaptation):
   - Namespace changes (e.g., services moved to new folders)
   - Architecture pattern changes (e.g., Repository → DataService)
   - Method signature changes in production code
   - Consistent pattern across multiple test files
   
   **B. Regression** (production code is wrong):
   - Missing implementations
   - Logic errors
   - Broken contracts
   - Inconsistent behavior
   
   **C. Unclear** (ASK FOR CLARIFICATION):
   - When intent cannot be determined
   - When multiple interpretations are valid
   - When fix could go either direction

3. **User Interaction Protocol**:
   When unclear, MUST ASK:
   ```
   ❓ CLARIFICATION NEEDED:
   
   Error: [Specific error message]
   File: [Location]
   
   Possible Interpretations:
   1. REFACTORING: [Why this might be intentional]
   2. REGRESSION: [Why this might be a bug]
   
   Which is correct? Should I:
   A) Update tests to match new production code pattern
   B) Fix production code to maintain previous behavior
   ```

4. **Fix Application**:
   - **For Refactoring**: Adapt tests to new patterns
   - **For Regression**: Fix production code
   - **Follow quality standards** from `/memory-bank/CODE_QUALITY_STANDARDS.md`

### Phase 2: Test Validation

After fixing all errors:

1. Run `dotnet test` to ensure no regressions
2. If tests fail, analyze whether:
   - Test needs migration to AutoMocker pattern
   - Test assertions need updating
   - Production code has actual bugs

### Phase 3: Warning Cleanup

Once all errors are fixed and tests pass:

1. Check for build warnings
2. Fix non-obsolete warnings following quality standards
3. Report obsolete warnings count (don't fix)

## Success Criteria

✅ **Complete Success When**:
- Build shows 0 errors
- All root causes correctly identified
- User confirmed all unclear cases
- Appropriate fixes applied (test adaptation OR code fixes)
- All tests passing
- Build warnings addressed (non-obsolete fixed, obsolete reported)

## Execution Strategy

```
1. ANALYZE:
   - Run dotnet clean && dotnet build
   - Categorize all errors
   - Determine root cause for each

2. CLARIFY:
   - Present unclear cases to user
   - Wait for confirmation on each
   - Document decisions

3. FIX:
   - Apply fixes based on analysis + user input
   - Group similar fixes for efficiency
   - Validate after each group

4. VERIFY:
   - Run dotnet build (must be 0 errors)
   - Run dotnet test (must all pass)
   - Check and fix warnings

5. REPORT:
   - Summary of all changes
   - Explanation of decisions
   - Final status
```

## Report Format

After completion, provide a detailed report:

```
=== BUILD ERROR FIX REPORT ===

📊 Initial Analysis:
- Total Errors: X
- Production Code Errors: Y
- Test Code Errors: Z

🔍 Root Cause Analysis:
✅ Refactoring-Related (A errors):
  - [Specific pattern, e.g., "IExerciseService namespace changed"]
  - [Another pattern]

⚠️ Regressions Fixed (B errors):
  - [Specific regression]

❓ User Clarifications (C cases):
  - [Decision 1]: User confirmed refactoring
  - [Decision 2]: User confirmed regression

📝 Fixes Applied:

Test Adaptations:
- Updated X test files with new namespaces
- Migrated Y test classes to AutoMocker
- Updated Z mock setups for DataService pattern

Production Code Fixes:
- [Any regression fixes]

⚠️ Warnings:
- Fixed: M non-obsolete warnings
- Reported: N obsolete warnings (not fixed)

✅ Final State:
- Build Errors: 0
- Build Warnings: N (all obsolete)
- Test Results: All passing
```

## Key Principles

1. **NEVER ASSUME** - When unclear, always ASK
2. **UNDERSTAND INTENT** - Distinguish refactoring from regression
3. **PRESERVE BEHAVIOR** - Don't change functionality without confirmation
4. **FOLLOW PATTERNS** - Respect project conventions
5. **DOCUMENT DECISIONS** - Explain every fix applied

## Common Scenarios

### Scenario 1: Service Refactoring
**Pattern**: Service moved from `Services.Implementations` to `Services.Domain`
**Action**: Update all test references, no production code changes

### Scenario 2: Architecture Change
**Pattern**: Direct repository access replaced with DataService pattern
**Action**: Update test mocks from repositories to DataServices

### Scenario 3: Missing Implementation
**Pattern**: Interface method not implemented
**Action**: ASK - could be intentional removal or accidental deletion

### Scenario 4: Test Pattern Migration
**Pattern**: Old test pattern with class-level mocks
**Action**: Migrate entire test class to AutoMocker pattern

## Implementation Notes

- **Ask early and often** - Better to clarify than fix wrong
- **Batch similar changes** - Fix all namespace issues together
- **Incremental validation** - Build after each batch of fixes
- **Preserve git history** - Make changes traceable
- **Maximum clarity** - Over-communicate rather than under-communicate