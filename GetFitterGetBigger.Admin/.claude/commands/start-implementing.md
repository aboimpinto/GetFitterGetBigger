Start implementing a feature by intelligently analyzing the context and providing relevant guidelines.

## Process Flow

1. **Feature Selection**:
   - Check `/memory-bank/features/1-READY_TO_DEVELOP/` for available features
   - If multiple features exist, ask user which to implement
   - Move selected feature to `2-IN_PROGRESS` folder
   - Update feature-status.md

2. **Initial Setup**:
   - Read feature-description.md and feature-tasks.md
   - Create feature branch as specified in tasks file
   - Run baseline health check and document results
   - Only proceed if build is green and tests pass

3. **Task Analysis**:
   - Identify the first uncompleted task in feature-tasks.md
   - Detect task type based on patterns
   - Load relevant guidelines from implementation-guidelines-map.md

4. **Context-Aware Guidance**:
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

## Special Handling

### When Starting Fresh:
- Run baseline health check first
- Fix any existing issues before proceeding
- Document baseline state in feature-tasks.md

### When Guidelines Not Found:
- Note: "⚠️ No existing pattern found for [concept]"
- Ask user for guidance
- Document new pattern for future use

Always reference @.claude/commands/implementation-guidelines-map.md for detailed task-type mappings.