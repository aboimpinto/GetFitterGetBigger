# Validation Anti-Patterns and Solutions

## Overview
This document identifies common validation anti-patterns in service implementations and provides clean solutions using established patterns.

## Anti-Pattern 1: Execution-Validation-Execution

### The Problem
```csharp
// ❌ BAD - Mixed concerns with validation in the middle
private async Task<ServiceResult<T>> UpdateAsync(Id id, Command command)
{
    var entity = await LoadEntityAsync(id);        // Execution (DB read)
    if (entity.IsEmpty)                            // Validation
        return Failure(...);                       
    
    using var unitOfWork = CreateWritable();       // Execution continues
    // ... perform update
}
```

This pattern has multiple issues:
- **Multiple exit points** without pattern matching
- **Mixed concerns** - validation interrupts execution flow
- **Hard to test** - can't test validation separately from execution
- **Poor readability** - logic flow is interrupted

### The Solution
```csharp
// ✅ GOOD - Clear separation with pattern matching
private async Task<ServiceResult<T>> UpdateAsync(Id id, Command command)
{
    var entity = await LoadEntityAsync(id);
    
    // Single decision point with pattern matching
    return entity.IsEmpty switch
    {
        true => ServiceResult<T>.Failure(T.Empty, ServiceError.NotFound("Entity")),
        false => await PerformUpdateAsync(entity, command)
    };
}

private async Task<ServiceResult<T>> PerformUpdateAsync(Entity entity, Command command)
{
    using var unitOfWork = CreateWritable();
    // ... perform update
}
```

Benefits:
- **Single exit point** via pattern matching
- **Clear separation** of concerns
- **Testable** - each method has single responsibility
- **Readable** - linear flow without interruptions

## Anti-Pattern 2: Defensive Null-Checking Proliferation

### The Problem
```csharp
// ❌ BAD - Excessive defensive programming
.Ensure(() => command != null, "Command is null")
.EnsureNotWhiteSpace(command?.Name, "Name is empty")
.EnsureAsync(
    async () => command == null || !await ExistsAsync(command?.Id ?? 0),
    "Invalid")
```

Issues:
- **Noise** - `?`, `??`, `!` operators everywhere
- **Distrust** - Not trusting layer boundaries
- **Complex logic** - Unnecessary null checks in predicates
- **Readability** - Hard to understand actual business logic

### The Solution
```csharp
// ✅ GOOD - Trust boundaries, minimal defensive checks
public async Task<ServiceResult<T>> CreateAsync(Command command)
{
    // Single defensive check at entry if absolutely necessary
    if (command == null)
        return ServiceResult<T>.Failure(T.Empty, ServiceError.ValidationFailed("..."));
    
    // Trust command is valid from here
    return await ServiceValidate.Build<T>()
        .EnsureNotWhiteSpace(command.Name, "Name is empty")
        .EnsureIsUniqueAsync(
            async () => await IsNameUniqueAsync(command.Name),
            "Entity", 
            command.Name)
        .MatchAsync(...);
}
```

## Anti-Pattern 3: Negative Validation Logic

### The Problem
```csharp
// ❌ BAD - Double negatives and complex boolean logic
.EnsureAsync(
    async () => !await CheckDuplicateNameAsync(name),
    "Name exists")
.Ensure(() => !id.IsEmpty, "Invalid ID")
.EnsureAsync(
    async () => !await IsInUseAsync(id),
    "In use")
```

Issues:
- **Mental gymnastics** - Double negatives are hard to parse
- **Error-prone** - Easy to get boolean logic wrong
- **Poor naming** - Methods don't express positive intent

### The Solution
```csharp
// ✅ GOOD - Positive assertions with clear intent
.EnsureValidId(id, "Invalid ID")
.EnsureIsUniqueAsync(
    async () => await IsNameUniqueAsync(name),
    "Entity", name)
.EnsureAsync(
    async () => await CanDeleteAsync(id),
    ServiceError.DependencyExists("Entity", "in use"))

// Positive helper methods
private async Task<bool> IsNameUniqueAsync(string name) { ... }
private async Task<bool> CanDeleteAsync(Id id) { ... }
```

## Anti-Pattern 4: Verbose ServiceError Creation

### The Problem
```csharp
// ❌ BAD - Repetitive ServiceError creation
.EnsureExistsAsync(
    async () => await ExistsAsync(id),
    ServiceError.NotFound("Equipment"))
.EnsureIsUniqueAsync(
    async () => await IsUniqueAsync(name),
    ServiceError.AlreadyExists("Equipment", name))
```

### The Solution
```csharp
// ✅ GOOD - Smart overloads reduce boilerplate
.EnsureExistsAsync(
    async () => await ExistsAsync(id),
    "Equipment")  // Auto-creates ServiceError.NotFound
.EnsureIsUniqueAsync(
    async () => await IsUniqueAsync(name),
    "Equipment",  // Auto-creates ServiceError.AlreadyExists
    name)
```

## How to Identify These Anti-Patterns

### Code Smells to Look For

1. **Multiple Returns Without Pattern Matching**
   - Look for: Multiple `return` statements in a method
   - Especially: Returns after validation checks
   - Solution: Use switch expressions or pattern matching

2. **Null-Check Cascades**
   - Look for: `?.`, `??`, `!` operators in validation
   - Especially: `command?.Property ?? default`
   - Solution: Single null check at entry, trust thereafter

3. **Negation in Validation**
   - Look for: `!await`, `!IsValid`, `!Exists`
   - Especially: Double negatives like `!IsNotValid`
   - Solution: Create positive helper methods

4. **Mixed Execution and Validation**
   - Look for: Database operations followed by validation followed by more database operations
   - Solution: Separate into Load → Validate → Execute phases

### Quick Audit Checklist

When reviewing a service method, ask:

- [ ] Does it have a single exit point using pattern matching?
- [ ] Are validations using positive assertions?
- [ ] Is there minimal defensive null checking?
- [ ] Are concerns clearly separated (validation vs execution)?
- [ ] Do helper methods have positive names?
- [ ] Are we using smart overloads to reduce boilerplate?

## Refactoring Strategy

### Step 1: Identify the Pattern
Look for the code smells above in existing services.

### Step 2: Extract Helper Methods
Create positive-named helper methods for validation logic.

### Step 3: Implement Pattern Matching
Replace multiple returns with switch expressions.

### Step 4: Remove Defensive Noise
Trust layer boundaries, remove unnecessary null checks.

### Step 5: Use Smart Overloads
Replace verbose ServiceError creation with simplified overloads.

### Step 6: Test
Ensure all tests still pass after refactoring.

## Related Documentation

- [CleanValidationPattern.md](CleanValidationPattern.md) - The clean validation approach
- [SingleExitPointPattern.md](SingleExitPointPattern.md) - Pattern matching for flow control
- [ServiceValidatePattern.md](ServiceValidatePattern.md) - Fluent validation pattern

## Summary

These anti-patterns often appear together and compound each other's problems. By systematically applying the solutions:

1. **Code becomes more readable** - Intent is clear
2. **Testing improves** - Single responsibility per method
3. **Maintenance is easier** - Less cognitive load
4. **Bugs decrease** - Simpler logic has fewer edge cases

The key insight: **Trust your boundaries, express positive intent, and use pattern matching for clean control flow.**