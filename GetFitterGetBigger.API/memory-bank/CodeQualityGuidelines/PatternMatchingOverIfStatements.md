# Pattern Matching Over If Statements

## Overview
Modern C# provides pattern matching as a more functional and declarative alternative to traditional if statements. This guideline documents when and how to replace if statements with pattern matching for cleaner, more maintainable code.

## Problem: Traditional If Statements
Traditional if statements, especially when checking success/failure states or handling multiple conditions, create:
- Multiple exit points
- Nested code blocks
- Early returns that break flow
- Imperative rather than declarative code

## Solution: Pattern Matching
Use pattern matching to create single-exit-point methods with clear, declarative logic.

## Identification Patterns

### 1. Success/Failure Checks
**Look for:**
```csharp
// ❌ BAD: Traditional if statement
if (!result.IsSuccess)
{
    return ServiceResult<T>.Failure(...);
}
// Continue with success case
return await ProcessAsync(result.Value);
```

**Replace with:**
```csharp
// ✅ GOOD: Pattern matching
return result switch
{
    { IsSuccess: false } => ServiceResult<T>.Failure(...),
    { IsSuccess: true } => await ProcessAsync(result.Value)
};
```

### 2. Early Returns
**Look for:**
```csharp
// ❌ BAD: Early return pattern
private async Task<ServiceResult<T>> ProcessAsync(Entity entity)
{
    var result = await ValidateAsync(entity);
    
    if (!result.IsSuccess)
    {
        return result;  // Early return
    }
    
    // Continue processing
    return await ExecuteAsync(result.Value);
}
```

**Replace with:**
```csharp
// ✅ GOOD: Single return with pattern matching
private async Task<ServiceResult<T>> ProcessAsync(Entity entity)
{
    var result = await ValidateAsync(entity);
    
    return result switch
    {
        { IsSuccess: false } => result,
        { IsSuccess: true } => await ExecuteAsync(result.Value)
    };
}
```

### 3. Conditional Execution
**Look for:**
```csharp
// ❌ BAD: If statement for conditional execution
if (collection?.Any() == true)
{
    await ProcessCollectionAsync(collection);
}
```

**Replace with:**
```csharp
// ✅ GOOD: Ternary operator
await (collection?.Any() == true
    ? ProcessCollectionAsync(collection)
    : Task.CompletedTask);
```

### 4. Null/Empty Checks
**Look for:**
```csharp
// ❌ BAD: Multiple if statements
if (entity == null || entity.IsEmpty)
{
    return ServiceResult<T>.Failure(T.Empty, ServiceError.NotFound());
}
return ServiceResult<T>.Success(entity.ToDto());
```

**Replace with:**
```csharp
// ✅ GOOD: Pattern matching with property patterns
return entity switch
{
    null => ServiceResult<T>.Failure(T.Empty, ServiceError.NotFound()),
    { IsEmpty: true } => ServiceResult<T>.Failure(T.Empty, ServiceError.NotFound()),
    _ => ServiceResult<T>.Success(entity.ToDto())
};
```

### 5. Validation with Throw
**Look for:**
```csharp
// ❌ BAD: If statement with throw
if (!createResult.IsSuccess)
{
    throw new InvalidOperationException($"Failed: {string.Join(", ", createResult.Errors)}");
}
return createResult.Value;
```

**Replace with:**
```csharp
// ✅ GOOD: Pattern matching with throw expression
return createResult switch
{
    { IsSuccess: true } => createResult.Value,
    { IsSuccess: false } => throw new InvalidOperationException(
        $"Failed: {string.Join(", ", createResult.Errors)}")
};
```

## Complete Example: Before and After

### Before: Multiple If Statements
```csharp
private async Task<ServiceResult<WorkoutTemplateDto>> ExecuteDuplicationAsync(
    WorkoutTemplateEntity duplicateTemplate,
    WorkoutTemplateDto originalTemplate)
{
    // Create the duplicate template
    var createResult = await _commandDataService.CreateAsync(duplicateTemplate);
    
    if (!createResult.IsSuccess)
    {
        return createResult;  // Early return
    }
    
    // If original had exercises, duplicate them too
    if (originalTemplate.Exercises?.Any() == true)
    {
        var newTemplateId = WorkoutTemplateId.ParseOrEmpty(createResult.Data.Id);
        await DuplicateExercisesAsync(originalTemplate.Exercises, newTemplateId);
    }

    _logger.LogInformation("Duplicated template");
    return createResult;
}
```

### After: Pattern Matching
```csharp
private async Task<ServiceResult<WorkoutTemplateDto>> ExecuteDuplicationAsync(
    WorkoutTemplateEntity duplicateTemplate,
    WorkoutTemplateDto originalTemplate)
{
    var createResult = await _commandDataService.CreateAsync(duplicateTemplate);
    
    return createResult switch
    {
        { IsSuccess: false } => createResult,
        { IsSuccess: true } => await CompleteTemplateCreationAsync(createResult, originalTemplate)
    };
}

private async Task<ServiceResult<WorkoutTemplateDto>> CompleteTemplateCreationAsync(
    ServiceResult<WorkoutTemplateDto> createResult,
    WorkoutTemplateDto originalTemplate)
{
    // Duplicate exercises if any exist
    await (originalTemplate.Exercises?.Any() == true
        ? DuplicateExercisesAsync(
            originalTemplate.Exercises, 
            WorkoutTemplateId.ParseOrEmpty(createResult.Data.Id))
        : Task.CompletedTask);

    _logger.LogInformation("Duplicated template");
    return createResult;
}
```

## When to Apply

### Always Use Pattern Matching For:
1. **Result checking** (IsSuccess/IsFailure patterns)
2. **Null/Empty checks** on entities
3. **Enum-based branching**
4. **Type checking and casting**
5. **Property-based conditions**

### Consider Traditional If When:
1. **Complex conditions** that don't fit pattern syntax
2. **Side effects only** (no return value)
3. **Guard clauses** at method entry (parameter validation)
4. **Single condition** with no else branch and no return

## Benefits
1. **Single Exit Point**: Each method has one return statement
2. **Functional Style**: More declarative, less imperative
3. **Reduced Nesting**: Flatter code structure
4. **Type Safety**: Compiler ensures all cases handled
5. **Readability**: Intent is clearer
6. **Immutability**: Encourages immutable patterns

## Migration Strategy

### Step 1: Identify Candidates
Search for patterns:
```bash
# Find early returns
grep -r "if.*!.*IsSuccess" --include="*.cs" | grep -v "Test"

# Find null checks with returns
grep -r "if.*== null.*return" --include="*.cs" | grep -v "Test"

# Find IsEmpty checks
grep -r "if.*IsEmpty.*return" --include="*.cs" | grep -v "Test"
```

### Step 2: Categorize by Pattern
Group findings by pattern type (success checks, null checks, etc.)

### Step 3: Refactor Systematically
1. Start with simple success/failure checks
2. Move to null/empty patterns
3. Handle complex conditions last
4. Test after each refactoring

### Step 4: Verify
Ensure all tests pass after refactoring each method

## Code Review Checklist
- [ ] No early returns (except guard clauses)
- [ ] Single exit point per method
- [ ] Pattern matching for success/failure checks
- [ ] Ternary operators for simple conditionals
- [ ] Switch expressions for multiple conditions
- [ ] No nested if statements for result checking

## Related Guidelines
- [ServiceValidatePattern.md](ServiceValidatePattern.md) - Fluent validation patterns
- [SingleExitPoint.md](../ServicePatterns/SingleExitPoint.md) - Single exit point principle
- [EmptyPattern.md](EmptyPattern.md) - Empty object pattern
- [CODE_QUALITY_STANDARDS.md](../CODE_QUALITY_STANDARDS.md) - Overall quality standards