# Fix Build Warnings Command

This command orchestrates a comprehensive build warnings fix process that ensures both warnings are resolved and tests continue to pass.

## Command Flow

1. **Execute csharp-build-warnings-fixer agent** to identify and fix non-obsolete warnings
2. **Run tests** to verify no regressions
3. **If tests fail**, execute csharp-build-test-fixer agent to fix them
4. **Re-check for warnings** after test fixes
5. **Repeat until** zero non-obsolete warnings and all tests pass

## Agent Task

### Phase 1: Initial Warning Fix

Launch the csharp-build-warnings-fixer agent with these instructions:

**Primary Mission**: Fix all build warnings except obsolete warnings, following strict code quality standards.

**Critical Guidelines**:
1. **Code Quality Standards Compliance**: 
   - Strictly follow `/memory-bank/CODE_QUALITY_STANDARDS.md`
   - Strictly follow `/memory-bank/API-CODE_QUALITY_STANDARDS.md`
   
2. **Warning Fix Strategy**:
   - Run `dotnet clean && dotnet build` to identify all warnings
   - Categorize warnings by type
   - **SKIP all CS0618/CS0619 obsolete warnings** - report count only
   - Fix all other warnings following quality standards
   
3. **Implementation Standards** (from API-CODE_QUALITY_STANDARDS.md):
   - Use ServiceValidate fluent API for all validation
   - Implement proper Empty pattern with IEmptyDto<T>
   - Use collection expressions `[]` for empty collections
   - Use primary constructors for dependency injection
   - NO try-catch anti-patterns
   - Single exit point per method using pattern matching
   - Follow Single Repository Rule - services only access their own repositories

4. **After Fixing Warnings**:
   - Run `dotnet build` to verify warnings are resolved
   - Run `dotnet test` to check if any tests were broken
   - Report results with clear summary

### Phase 2: Test Fix (If Needed)

If tests fail after warning fixes, automatically launch csharp-build-test-fixer agent:

**Mission**: Fix any tests that broke due to warning fixes.

**Guidelines**:
1. Follow all quality standards from Phase 1
2. Analyze why tests failed after warning fixes
3. Update tests to match the improved code patterns
4. Ensure all tests pass

### Phase 3: Verification Loop

After test fixes:
1. Re-run `dotnet build` to check for any new warnings
2. If new warnings appeared (non-obsolete), return to Phase 1
3. Continue loop until:
   - Zero non-obsolete warnings
   - All tests passing
   - Clean build

## Success Criteria

✅ **Complete Success When**:
- Build shows 0 errors
- Build shows 0 non-obsolete warnings (CS0618/CS0619 warnings are acceptable)
- All tests pass
- Code follows quality standards
- Clear report of:
  - Number and types of warnings fixed
  - Number of obsolete warnings (not fixed, just reported)
  - Any test adjustments made
  - Final build/test status

## Execution Strategy

```
LOOP:
  1. Run csharp-build-warnings-fixer
     - Fix all non-obsolete warnings
     - Report obsolete warning count
  
  2. Run dotnet test
     - If PASS → Check for new warnings
       - If no new warnings → SUCCESS, exit
       - If new warnings → Go to step 1
     - If FAIL → Go to step 3
  
  3. Run csharp-build-test-fixer
     - Fix failing tests
     - Ensure quality standards
  
  4. Go to step 1 (re-check for warnings)
  
MAX_ITERATIONS: 3 (to prevent infinite loops)
```

## Report Format

After completion, provide a summary:

```
=== Build Warnings Fix Report ===

Initial State:
- Build Errors: X
- Build Warnings: Y (including Z obsolete)
- Test Failures: A

Actions Taken:
- Fixed X nullable reference warnings
- Fixed Y unused variable warnings
- Fixed Z async/await warnings
- Reported but did not fix: N obsolete warnings
- Updated M tests to match improved patterns

Final State:
- Build Errors: 0
- Build Warnings: N (all obsolete, part of ongoing refactoring)
- Test Failures: 0

✅ Build is clean (excluding obsolete warnings)
✅ All tests passing
```

## Implementation Notes

- **Never fix obsolete warnings** - these indicate ongoing refactoring
- **Always verify tests** after fixing warnings
- **Follow quality standards** strictly - no shortcuts
- **Report everything** - transparency is key
- **Maximum 3 iterations** to prevent infinite fixing loops