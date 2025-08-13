# Pattern Matching Refactoring Guide

## Quick Reference for Common Refactorings

### 1. IsSuccess Pattern
```csharp
// ❌ BEFORE
if (!result.IsSuccess)
{
    return ServiceResult<T>.Failure(T.Empty, result.Errors.First());
}
return await ProcessAsync(result.Data);

// ✅ AFTER
return result switch
{
    { IsSuccess: false } => ServiceResult<T>.Failure(T.Empty, result.Errors.First()),
    { IsSuccess: true } => await ProcessAsync(result.Data)
};
```

### 2. Null Check Pattern
```csharp
// ❌ BEFORE
if (entity == null)
{
    return ServiceResult<T>.Failure(T.Empty, ServiceError.NotFound());
}
return ServiceResult<T>.Success(entity.ToDto());

// ✅ AFTER
return entity switch
{
    null => ServiceResult<T>.Failure(T.Empty, ServiceError.NotFound()),
    _ => ServiceResult<T>.Success(entity.ToDto())
};
```

### 3. Conditional Execution Pattern
```csharp
// ❌ BEFORE
if (collection?.Any() == true)
{
    await ProcessCollectionAsync(collection);
}

// ✅ AFTER
await (collection?.Any() == true 
    ? ProcessCollectionAsync(collection) 
    : Task.CompletedTask);
```

### 4. Multiple Conditions Pattern
```csharp
// ❌ BEFORE
if (entity == null || entity.IsEmpty)
{
    return NotFound();
}
if (!entity.IsActive)
{
    return Forbidden();
}
return Ok(entity);

// ✅ AFTER
return entity switch
{
    null or { IsEmpty: true } => NotFound(),
    { IsActive: false } => Forbidden(),
    _ => Ok(entity)
};
```

## Step-by-Step Refactoring Process

### Step 1: Identify the Pattern
Look for these code smells:
- Early returns with `if (!condition) return`
- Multiple if-else chains
- Nested if statements
- Null/empty checks followed by processing

### Step 2: Determine the Return Type
- Is it async? (use `await` in the appropriate branch)
- What's the return type? (ensure all branches return the same type)
- Are there side effects? (may need to extract to separate method)

### Step 3: Apply the Pattern
1. Replace if-else with switch expression
2. Ensure single exit point
3. Use property patterns for object checks
4. Use logical patterns (`and`, `or`, `not`) for complex conditions

### Step 4: Extract Complex Logic
If the switch expression becomes too complex:
```csharp
// Extract to separate method
private async Task<ServiceResult<T>> CompleteOperationAsync(...)
{
    // Complex logic here
}

// Use in switch
return result switch
{
    { IsSuccess: false } => result,
    { IsSuccess: true } => await CompleteOperationAsync(...)
};
```

## Common Pitfalls to Avoid

### 1. Async/Await Confusion
```csharp
// ❌ WRONG - Can't await the switch itself
return await result switch { ... };

// ✅ CORRECT - Await inside the branch
return result switch
{
    { IsSuccess: true } => await ProcessAsync(),
    _ => Task.FromResult(defaultValue)
};
```

### 2. Type Mismatches
```csharp
// ❌ WRONG - Different return types
return result switch
{
    { IsSuccess: true } => entity,        // Returns Entity
    { IsSuccess: false } => null          // Returns null
};

// ✅ CORRECT - Same return type
return result switch
{
    { IsSuccess: true } => entity,
    { IsSuccess: false } => Entity.Empty  // Both return Entity
};
```

### 3. Side Effects in Switch
```csharp
// ❌ WRONG - Side effects in switch expression
return result switch
{
    { IsSuccess: true } => { 
        _logger.LogInfo("Success");  // Can't have statements
        return value;
    }
};

// ✅ CORRECT - Extract to method
return result switch
{
    { IsSuccess: true } => LogAndReturn(value),
    _ => defaultValue
};
```

## Real-World Examples from Codebase

### Example 1: SetConfigurationService
```csharp
// BEFORE
if (!entityResult.IsSuccess)
{
    return ServiceResult<SetConfigurationDto>.Failure(
        SetConfigurationDto.Empty,
        ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors)));
}

var entity = entityResult.Value;
// ... continue processing

// AFTER
return entityResult switch
{
    { IsSuccess: false } => ServiceResult<SetConfigurationDto>.Failure(
        SetConfigurationDto.Empty,
        ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors))),
    { IsSuccess: true } => await ProcessEntityAsync(entityResult.Value)
};
```

### Example 2: EquipmentRequirementsService
```csharp
// BEFORE
if (x == null && y == null) return true;
if (x == null || y == null) return false;
return x.Id == y.Id;

// AFTER
return (x, y) switch
{
    (null, null) => true,
    (null, _) or (_, null) => false,
    _ => x.Id == y.Id
};
```

## Validation Checklist
- [ ] All branches return the same type
- [ ] Async operations are properly awaited
- [ ] No side effects in switch expressions
- [ ] Single exit point achieved
- [ ] Code is more readable than before
- [ ] All tests still pass

## Tools and Commands

### Find Candidates
```bash
# Run from memory-bank/temp
./find-if-statement-candidates.sh

# Or manually search
grep -r "if.*!.*\.IsSuccess" --include="*.cs" | grep -v Test
```

### Verify After Refactoring
```bash
# Build and test
dotnet build --no-restore
dotnet test --no-build

# Check specific service tests
dotnet test --filter "FullyQualifiedName~ServiceName"
```

## Related Documents
- [PatternMatchingOverIfStatements.md](../CodeQualityGuidelines/PatternMatchingOverIfStatements.md) - Complete guideline
- [ServiceValidatePattern.md](../CodeQualityGuidelines/ServiceValidatePattern.md) - Validation patterns
- [CODE_QUALITY_STANDARDS.md](../CODE_QUALITY_STANDARDS.md) - Overall standards