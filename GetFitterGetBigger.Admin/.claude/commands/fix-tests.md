---
description: Analyzes and fixes failing tests, determining whether failures are due to production changes or actual bugs
argument-hint: [optional: specific test class or additional context]
---

Triggers the test-fixer agent to analyze and fix failing tests in the GetFitterGetBigger Admin Blazor project.

## What this command does:

The test-fixer agent will:
1. **Analyze** all failing tests in the project
2. **Determine** if failures are due to:
   - Production code changes requiring test updates
   - Actual bugs/regressions in production code
   - Test infrastructure issues
3. **Ask for clarification** when the root cause is unclear (never assumes!)
4. **Fix** the issues appropriately based on analysis
5. **Verify** that fixes don't break other tests

## Usage:

```bash
# Fix all failing tests
/fix-tests

# Focus on specific test class or area
/fix-tests FourWayExerciseLinkManagerTests

# Provide additional context
/fix-tests "Focus on the alternative exercise validation tests"

# Reference specific files
/fix-tests Check changes in @ExerciseLinkStateService.cs
```

## Agent Behavior:

### When it WILL ask for clarification:
- Business logic changes that could be intentional or bugs
- API contract changes where intent is unclear
- Component behavior changes that affect user experience
- Any situation where the "correct" behavior is ambiguous

### When it WON'T ask (clear cases):
- Obvious test setup issues (missing mocks, wrong parameters)
- Clear production code bugs (null reference, logic errors)
- Test infrastructure problems (missing usings, obsolete methods)
- Simple synchronization issues between test and production code

## Example Scenarios:

### Scenario 1: Clear Test Update Needed
If production code was intentionally changed (e.g., a DTO property renamed), the agent will automatically update tests to match.

### Scenario 2: Clear Production Bug
If tests reveal an actual bug (e.g., null reference exception), the agent will fix the production code.

### Scenario 3: Unclear - Asks for Guidance
If a business rule seems to have changed (e.g., discount percentage different), the agent will present both options and ask which is correct.

## Integration with Project Standards:

The agent follows:
- `COMPREHENSIVE-TESTING-GUIDE.md` for testing patterns
- `CODE_QUALITY_STANDARDS.md` for code quality
- `ADMIN-CODE_QUALITY_STANDARDS.md` for Blazor-specific standards
- Zero warnings policy from `DEVELOPMENT_PROCESS.md`

## Notes:
- The agent will never make assumptions about business logic
- All fixes maintain the project's code quality standards
- Test coverage is preserved or improved, never reduced
- The agent provides clear summaries of what was fixed and why

---

## Agent Invocation:

@test-fixer $ARGUMENTS