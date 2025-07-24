Start implementing a feature by intelligently analyzing the context and providing relevant guidelines.

## Process Flow

1. **Feature Selection**:
   - Check `/memory-bank/features/1-READY_TO_DEVELOP/` for available features
   - If multiple features exist, ask user which to implement
   - Move selected feature to `2-IN_PROGRESS` folder
   - Update feature-status.md

2. **Baseline Health Check Verification** (MANDATORY):
   - Read feature-tasks.md and check for "Baseline Health Check Report" section
   - If NO baseline health check exists:
     - **STOP IMMEDIATELY** and inform user
     - Run baseline health check using format below
     - Add results to top of feature-tasks.md
     - Cannot proceed without APPROVED health check
   - If baseline health check exists but shows failures:
     - **STOP IMMEDIATELY** and inform user
     - Fix all issues before proceeding
     - Re-run health check and update report
   - Only proceed if "Approval to Proceed: ✅" is present

3. **Initial Setup** (After Approved Health Check):
   - Create feature branch as specified in tasks file
   - Verify branch creation and clean working directory
   - Load implementation tracking document if exists

4. **Task Analysis**:
   - Identify the first uncompleted task in feature-tasks.md
   - Detect task type based on patterns
   - Load relevant guidelines from implementation-guidelines-map.md

5. **Context-Aware Guidance**:
   Based on detected task type, provide:
   - Specific documentation references
   - Code examples from high-coverage components
   - Testing requirements and patterns
   - Common pitfalls to avoid

## Task Type Detection & Guidelines

### For API Service Tasks:
- Review @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md#api-service-testing-xunit
- Check existing service patterns (e.g., ExerciseService)
- Set up HTTP mocking for tests
- Plan for error handling and retry logic

### For Component Tasks:
- Review @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md#blazor-component-testing-bunit
- Add data-testid attributes from the start
- Make methods internal for testability
- Plan both UI interaction and logic tests

### For State Management Tasks:
- Review @memory-bank/COMPREHENSIVE-TESTING-GUIDE.md#state-service-testing
- Study ExerciseWeightTypeStateService pattern
- Plan optimistic updates with rollback
- Ensure error message persistence

### For Testing Tasks:
- Review specific testing section based on what's being tested
- Aim for 80%+ coverage on new code
- Use established patterns from well-tested components
- Follow Boy Scout Rule for existing code

### For Checkpoint Tasks:
- Stop implementation
- Run full build and test suite
- Create code review following @memory-bank/CODE_REVIEW_PROCESS.md
- Cannot proceed until review status is APPROVED

## Implementation Steps

0. **VERIFY BASELINE HEALTH CHECK** - Must have "Approval to Proceed: ✅"
1. **Update task status** to [InProgress: Started: YYYY-MM-DD HH:MM]
2. **Implement the feature** following loaded guidelines
3. **Write tests immediately** (if applicable to task)
4. **Verify build and tests** pass
5. **Commit with hash** and update task
6. **Stop at checkpoints** for review

## Key Principles

- Clean build between every task
- Tests immediately follow implementation
- No broken builds committed
- Follow existing patterns
- Ask for clarification on ambiguous requirements

## Baseline Health Check Format

### Required Format for feature-tasks.md:
```markdown
## Baseline Health Check Report
**Date/Time**: YYYY-MM-DD HH:MM:SS
**Branch**: feature/[branch-name]

### Build Status
- **Build Result**: [Succeeded/Failed]
- **Warning Count**: [number]
- **Warning Details**: [list warnings or "None"]

### Test Status
- **Total Tests**: [number]
- **Passed**: [number]
- **Failed**: [number]
- **Skipped/Ignored**: [number]
- **Test Execution Time**: [time]

### Code Analysis Status
- **Errors**: [number]
- **Warnings**: [number]

### Decision to Proceed
- [ ] All tests passing
- [ ] Build successful
- [ ] No code analysis errors
- [ ] Warnings documented and approved

**Approval to Proceed**: ✅ Ready to proceed with implementation
```

### Health Check Commands:
1. **Run build**: `dotnet build`
2. **Run tests**: `dotnet test`
3. **Check for warnings**: Review build output
4. **Update report**: Add results to feature-tasks.md

## Special Handling

### When NO Baseline Health Check Exists:
- **STOP** - Do not proceed with any implementation
- Inform user: "⚠️ BASELINE HEALTH CHECK REQUIRED"
- Run health check commands
- Add report to TOP of feature-tasks.md
- Get approval before proceeding

### When Health Check Shows Failures:
- **STOP** - Do not proceed with implementation
- List all failures clearly
- Suggest fixes based on error types
- Re-run health check after fixes
- Update report with new results

### When Guidelines Not Found:
- Note: "⚠️ No existing pattern found for [concept]"
- Ask user for guidance
- Document new pattern for future use

Always reference @.claude/commands/implementation-guidelines-map.md for detailed task-type mappings.