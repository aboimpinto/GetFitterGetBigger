# Fix Code Quality Command

Automatically fixes code quality violations identified by the analyze-code-quality command.

## Usage

```
/fix-code-quality <file-path>
```

## Examples

```
/fix-code-quality Services/Implementations/DifficultyLevelService.cs
/fix-code-quality Controllers/EquipmentController.cs
/fix-code-quality DTOs/ExerciseDto.cs
```

## Description

This command triggers the Code Quality Fixer agent to:

1. **Load** the specified file and identify violations
2. **Fix** violations in priority order (Critical → High → Medium → Low)
3. **Apply** refactoring patterns from CODE_QUALITY_STANDARDS.md
4. **Update** associated tests to match refactored code
5. **Verify** all changes compile and tests pass
6. **Report** summary of changes made

## Process Flow

### 1. Violation Identification
- Loads the target file
- Identifies violations or uses previous analysis report
- Creates a prioritized fix list

### 2. Systematic Refactoring
The agent fixes violations in this order:

#### Critical Fixes (Must Do)
- **Try-catch removal** - Eliminates blanket exception handling
- **Repository boundaries** - Fixes Single Repository Rule violations
- **ServiceResult enforcement** - Ensures all methods return ServiceResult<T>
- **Empty pattern** - Replaces null returns with Empty
- **UnitOfWork correction** - Uses ReadOnly for queries

#### High Priority Fixes
- **ServiceValidate adoption** - Applies fluent validation API
- **Pattern matching** - Enforces single exit points
- **DTO patterns** - Implements IEmptyDto<T>
- **Controller cleanup** - Removes business logic

#### Medium Priority Fixes
- **Primary constructors** - Modernizes dependency injection
- **Collection expressions** - Uses [] syntax
- **CacheLoad pattern** - Standardizes cache operations

### 3. Test Adaptation
When code is refactored:
- Updates mock setups for new dependencies
- Replaces error message checks with ErrorCode assertions
- Adapts to new return types (e.g., BooleanResultDto)
- Documents tests that cannot be automatically fixed

### 4. Verification
- Runs `dotnet build` after each file change
- Runs `dotnet test` after all changes
- Reports final status

## Violation Types Fixed

### Service Class Violations
- Try-catch anti-patterns → ServiceValidate patterns
- Direct repository access → Service dependencies
- Missing ServiceResult → All methods return ServiceResult<T>
- Multiple returns → Pattern matching
- Old validation → ServiceValidate fluent API
- Null returns → Empty pattern
- Traditional constructors → Primary constructors
- new List<T>() → []

### Controller Violations
- Business logic → Pass-through only
- Complex methods → Single expressions
- Error interpretation → Direct pass-through
- Missing auth → Authorization attributes

### DTO Violations
- Missing Empty pattern → IEmptyDto<T> implementation
- No Empty property → Static Empty added
- No IsEmpty check → IsEmpty property added

## Example Transformations

### ServiceValidate Pattern
```csharp
// BEFORE
public async Task<ServiceResult<IEnumerable<T>>> GetAllAsync()
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var entities = await repository.GetAllAsync();
    return ServiceResult<IEnumerable<T>>.Success(entities);
}

// AFTER
public async Task<ServiceResult<IEnumerable<T>>> GetAllAsync()
{
    return await ServiceValidate.Build<IEnumerable<T>>()
        .WhenValidAsync(async () => await LoadAllActiveFromDatabaseAsync());
}

private async Task<ServiceResult<IEnumerable<T>>> LoadAllActiveFromDatabaseAsync()
{
    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
    var entities = await repository.GetAllAsync();
    return ServiceResult<IEnumerable<T>>.Success(entities);
}
```

### BooleanResultDto Pattern
```csharp
// BEFORE
public async Task<ServiceResult<bool>> ExistsAsync(Id id)
{
    // ... validation and check
    return ServiceResult<bool>.Success(exists);
}

// AFTER
public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(Id id)
{
    return await ServiceValidate.For<BooleanResultDto>()
        .EnsureNotEmpty(id, "Invalid ID")
        .MatchAsync(whenValid: async () =>
        {
            // ... check logic
            return ServiceResult<BooleanResultDto>.Success(
                BooleanResultDto.Create(exists));
        });
}
```

### Collection Expressions
```csharp
// BEFORE
var dtos = entities.Select(MapToDto).ToList();

// AFTER
var dtos = [.. entities.Select(MapToDto)];
```

## Output

The command provides:
1. **Initial Report** - Violations found and fix plan
2. **Progress Updates** - Real-time status of fixes
3. **Test Updates** - Changes made to test files
4. **Final Summary** - Statistics and outcomes

Example output:
```
STARTING CODE QUALITY FIX
Target: Services/Implementations/DifficultyLevelService.cs
Violations to fix: 3
Priority breakdown:
- High: 2 (ServiceValidate pattern, ExistsAsync return type)
- Medium: 1 (Collection expression)

✅ Fixed: Added ServiceValidate pattern to GetAllActiveAsync
✅ Fixed: Updated ExistsAsync to return BooleanResultDto
✅ Fixed: Modernized collection expression
✅ Updated 2 test files

CODE QUALITY FIX COMPLETE
Files modified: 1
Violations fixed: 3/3
Tests updated: 2
Build status: ✅ Success
Test status: ✅ All pass
```

## Limitations

The agent cannot automatically fix:
- Major architectural changes requiring redesign
- Complex business logic extraction from controllers
- Database schema modifications
- Breaking changes without impact analysis

These will be documented with recommendations for manual resolution.

## Standards Applied

The agent strictly follows:
- `/memory-bank/CODE_QUALITY_STANDARDS.md` - API quality standards and patterns

Key patterns enforced:
- ServiceResult<T> for all service methods
- ServiceValidate fluent validation API
- Empty pattern with IEmptyDto<T>
- Single Repository Rule
- Pattern matching for single exit
- Modern C# 12+ features
- Proper UnitOfWork usage

## Best Practices

1. **Commit first**: Save current state before running
2. **One file at a time**: Start with single files to understand changes
3. **Review changes**: Check the refactored code meets expectations
4. **Run tests**: Verify full test suite after refactoring
5. **Incremental approach**: Fix services one by one

## Prerequisites

Before running this command:
1. Ensure the project builds successfully
2. All tests should be passing (or known failures documented)
3. Consider running `/analyze-code-quality` first to understand violations
4. Have `/memory-bank/CODE_QUALITY_STANDARDS.md` as reference

## Related Commands

- `/analyze-code-quality` - Analyze files for violations (run first)
- `/fix-failing-tests` - Fix test failures after refactoring
- `/fix-build-warnings` - Clean up build warnings
- `/fix-build` - Fix build errors

## Notes

- The agent uses the Task tool with "code-quality-fixer" subagent type
- References `.claude/agents/code-quality-fixer.md` for detailed logic
- Follows guidelines in memory-bank standards documents
- Uses TodoWrite to track progress through fixes
- Verifies each change with `dotnet build`
- All changes are auditable and can be reviewed in git diff