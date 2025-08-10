# Analyze Code Quality Command

Analyzes C# files against GetFitterGetBigger API code quality standards and automatically refactors violations.

## Usage

```
/analyze-code-quality <file-path>
```

## Examples

```
/analyze-code-quality Services/Implementations/WorkoutTemplateService.cs
/analyze-code-quality Controllers/EquipmentController.cs
/analyze-code-quality DTOs/ExerciseDto.cs
```

## Description

This command triggers the Code Quality Analyzer agent to:

1. **Analyze** the specified file against API code quality standards
2. **Report** all violations found with severity levels
3. **Create** a refactoring plan to fix violations
4. **Execute** the refactoring automatically
5. **Update** associated tests to match refactored code
6. **Verify** all changes compile and tests pass

## Process Flow

### 1. Initial Analysis
- Loads the target file
- Identifies the class type (Service, Controller, DTO, Repository, Entity)
- Checks against both universal and API-specific standards

### 2. Violation Detection
Checks for critical violations including:
- **Try-catch anti-pattern** - Blanket exception handling
- **Single Repository Rule** - Services accessing wrong repositories
- **Missing ServiceResult** - Methods not returning ServiceResult<T>
- **Multiple exit points** - Not using pattern matching
- **Missing ServiceValidate** - Manual validation instead of fluent API
- **Empty pattern violations** - Returning null instead of Empty
- **UnitOfWork misuse** - WritableUnitOfWork for queries
- **Missing IEmptyDto** - DTOs without Empty pattern
- **Controller violations** - Business logic in controllers
- **Modern C# patterns** - Not using primary constructors or []

### 3. Automated Refactoring
The agent will:
- Fix violations in priority order (Critical → High → Medium → Low)
- Update one file at a time with verification
- Maintain functionality while improving code quality
- Track progress using the TodoWrite tool

### 4. Test Adaptation
When tests need updating:
- Updates mock setups for new service dependencies
- Replaces error message assertions with ServiceErrorCode checks
- Adapts to Empty pattern usage
- Never compromises production code to make tests pass
- Documents unfixable tests and continues

### 5. Verification
- Runs `dotnet build` after each change
- Runs `dotnet test` to ensure tests pass
- Reports final status with summary

## Violation Severity Levels

### Critical (Must Fix)
- Try-catch anti-pattern
- Single Repository Rule violations
- Missing ServiceResult<T> returns
- Null returns instead of Empty pattern
- WritableUnitOfWork used for queries

### High (Should Fix)
- Not using ServiceValidate fluent API
- Multiple exit points in methods
- Business logic in controllers
- Missing IEmptyDto<T> implementation

### Medium (Recommended)
- Not using primary constructors
- Verbose collection initialization
- Not using CacheLoad pattern
- Missing authorization attributes

### Low (Nice to Have)
- Code organization issues
- Naming convention violations
- Missing XML documentation

## Output

The command provides:
1. **Analysis Report** - All violations found with line numbers
2. **Refactoring Plan** - Step-by-step fixes to apply
3. **Progress Updates** - Real-time status during refactoring
4. **Test Update Log** - Changes made to test files
5. **Final Summary** - Success/failure status with statistics

## Limitations

The agent cannot automatically fix:
- Architectural redesigns requiring major restructuring
- Breaking API changes that affect consumers
- Complex business logic extraction from controllers
- Database schema changes
- Issues requiring human judgment

These will be documented for manual resolution.

## Standards Applied

The agent enforces standards from:
- `/memory-bank/CODE_QUALITY_STANDARDS.md` - API quality standards and patterns
- Guidelines in `/memory-bank/CodeQualityGuidelines/` folder

Key patterns enforced:
- ServiceResult<T> for all service methods
- ServiceValidate fluent validation API
- Empty pattern with IEmptyDto<T>
- Single Repository Rule
- Pattern matching for single exit points
- Modern C# 12+ features
- Proper UnitOfWork usage
- Controller pass-through pattern

## Best Practices

1. **Backup First**: Consider committing current state before running
2. **Single File**: Start with one file to understand changes
3. **Review Changes**: Check the refactored code meets expectations
4. **Test Thoroughly**: Run full test suite after refactoring
5. **Incremental**: Refactor services incrementally, not all at once

## Related Commands

- `/fix-failing-tests` - Fix test failures after refactoring
- `/fix-build-warnings` - Clean up build warnings
- `/fix-build` - Fix build errors

## Notes

- The agent uses the Task tool with "general-purpose" subagent type
- References `.claude/agents/code-quality-analyzer.md` for detailed logic
- Follows guidelines in memory-bank standards documents
- Tracks all changes for audit and rollback if needed