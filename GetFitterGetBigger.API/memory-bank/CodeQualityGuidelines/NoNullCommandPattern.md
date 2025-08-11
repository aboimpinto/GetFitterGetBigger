# NO NULL Command Pattern - CRITICAL RULE

**🔴 ABSOLUTE RULE**: Commands and DTOs passed to service methods are NEVER NULL. This is a fundamental architectural decision that MUST be understood and followed.

## Core Principle

```
┌─────────────────────────────────────────────────────────────────┐
│ 🚨 COMMANDS ARE NEVER NULL - STOP CHECKING FOR NULL COMMANDS!  │
├─────────────────────────────────────────────────────────────────┤
│ Controllers create commands using ToCommand() methods          │
│ Commands use Empty pattern when data is missing                │
│ Service methods trust commands are valid objects               │
│ NO NULL CHECKS on command parameters - EVER!                   │
└─────────────────────────────────────────────────────────────────┘
```

## Why Commands Are Never Null

### 1. Controller Responsibility
Controllers ALWAYS create valid command objects:

```csharp
// Controller ALWAYS creates a valid command
public async Task<IActionResult> GetExercises([FromQuery] ExerciseFilterParams filterParams)
{
    // filterParams.ToCommand() ALWAYS returns a valid command, never null
    var result = await _exerciseService.GetPagedAsync(filterParams.ToCommand());
    // ...
}
```

### 2. ToCommand() Pattern
Every Request DTO has a ToCommand() method that guarantees a valid command:

```csharp
public GetExercisesCommand ToCommand()
{
    // Even if 'this' is null or has null properties, 
    // ToCommand() returns a valid command with Empty values
    return new GetExercisesCommand
    {
        Page = this?.Page ?? 1,
        PageSize = this?.PageSize ?? 10,
        Name = this?.Name ?? string.Empty,
        DifficultyLevelId = DifficultyLevelId.ParseOrEmpty(this?.DifficultyId),
        // ... all fields mapped to valid non-null values
    };
}
```

### 3. Null Object Pattern Implementation
We use the Null Object Pattern (Empty pattern) throughout:
- Strings: `string.Empty` instead of `null`
- IDs: `ExerciseId.Empty` instead of `null`
- Lists: `[]` (empty collection) instead of `null`
- DTOs: `ExerciseDto.Empty` instead of `null`

## ❌ ANTI-PATTERN - Never Do This

```csharp
// ❌ WRONG - Unnecessary null check that clutters code
public async Task<ServiceResult<PagedResponse<ExerciseDto>>> GetPagedAsync(GetExercisesCommand filterParams)
{
    if (filterParams == null)  // ❌ NEVER DO THIS!
    {
        return ServiceResult<PagedResponse<ExerciseDto>>.Failure(
            new PagedResponse<ExerciseDto> { Items = [], TotalCount = 0 },
            ServiceError.ValidationFailed("Filter parameters cannot be null"));
    }
    // ...
}

// ❌ WRONG - Checking command properties for null
public async Task<ServiceResult<ExerciseDto>> CreateAsync(CreateExerciseCommand command)
{
    return await ServiceValidate.For<ExerciseDto>()
        .EnsureNotNull(command, ServiceError.ValidationFailed("Command cannot be null"))  // ❌ NEVER!
        .EnsureNotNull(command?.Name, ...)  // ❌ Use command.Name directly!
        // ...
}
```

## ✅ CORRECT Pattern

```csharp
// ✅ CORRECT - Trust that command is valid
public async Task<ServiceResult<PagedResponse<ExerciseDto>>> GetPagedAsync(GetExercisesCommand filterParams)
{
    // No null check - just use the command directly
    using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
    var repository = readOnlyUow.GetRepository<IExerciseRepository>();
    
    var (exercises, totalCount) = await repository.GetPagedAsync(
        filterParams.Page,         // Direct access - no null checks
        filterParams.PageSize,      // These are guaranteed valid
        filterParams.Name,          // Will be string.Empty if not provided
        filterParams.DifficultyLevelId,  // Will be Empty ID if not provided
        // ...
    );
    // ...
}

// ✅ CORRECT - Validate business rules, not null
public async Task<ServiceResult<ExerciseDto>> CreateAsync(CreateExerciseCommand command)
{
    return await ServiceValidate.For<ExerciseDto>()
        // Check business rules, not null
        .EnsureNotWhiteSpace(command.Name, ServiceError.ValidationFailed("Name is required"))
        .Ensure(() => !command.DifficultyId.IsEmpty, ServiceError.ValidationFailed("Difficulty is required"))
        // ...
}
```

## Command Validation Rules

### What TO Validate:
- **Business rules**: "Name cannot be empty", "Email must be valid format"
- **Domain constraints**: "Exercise must have at least one type"
- **Cross-field rules**: "REST exercises cannot have equipment"

### What NOT TO Validate:
- **Null commands**: Commands are NEVER null
- **Null properties**: Properties use Empty pattern
- **Collection nulls**: Collections are empty [], never null

## Benefits of This Pattern

1. **Cleaner Code**: No null checks cluttering business logic
2. **Better Readability**: Focus on business rules, not defensive programming
3. **Consistent Behavior**: Predictable Empty values instead of nulls
4. **Reduced Complexity**: Fewer branches in code paths
5. **Type Safety**: Compiler helps ensure non-null guarantees

## Key Takeaways

1. **Commands created by controllers are ALWAYS valid objects**
2. **Never check if command parameter is null**
3. **Use Empty pattern for missing/invalid data**
4. **Trust the architecture - it's designed to prevent nulls**
5. **Validate business rules, not null references**

## Related Documentation
- [Null Object Pattern](./NullObjectPattern.md)
- [Clean Validation Pattern](./CleanValidationPattern.md)
- [ServiceValidate Pattern](./ServiceValidatePattern.md)
- [Request-Command Separation](./ArchitecturalPatterns/RequestCommandSeparation.md)

## Remember

> "If you're checking for null commands in service methods, you're doing it wrong. The architecture guarantees commands are never null. Trust the pattern and focus on business logic."

---

*This is a CRITICAL pattern. Violating this rule leads to unnecessary code complexity and reduces readability. When in doubt: Commands are NEVER null.*