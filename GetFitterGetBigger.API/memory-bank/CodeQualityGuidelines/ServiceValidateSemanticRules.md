# ServiceValidate Chain Semantic Rules

## Overview
The ServiceValidate pattern provides a fluent interface for building validation chains. However, each method in the chain has a **specific semantic purpose** that must be respected to maintain code clarity and maintainability.

## Core Principle: Semantic Clarity
Each step in a ServiceValidate chain must have ONE clear, explicit purpose that matches its method name.

## The Mental Model

### Method Purpose Matrix

| Method Pattern | Purpose | Returns | Side Effects | When to Use |
|---------------|---------|---------|--------------|-------------|
| `Ensure*` | Validation check | `bool` | ❌ None | Checking conditions |
| `Load*` | Fetch from database | Entity | ❌ None | Loading data |
| `Transform*` | Modify/prepare data | Modified data | ❌ None | Data transformation |
| `Execute*` | Perform business logic | Result | ✅ Allowed | Business operations |
| `Match*` | Final operation | ServiceResult | ✅ Allowed | Chain conclusion |
| `When*` | Conditional branching | Chain | ❌ None | Conditional logic |

## The Four Golden Rules

### Rule 1: Single Purpose per Step
Each step in a ServiceValidate chain must have ONE clear purpose.

❌ **BAD** - Multiple purposes hidden in validation:
```csharp
.EnsureAsync(
    async () => await LoadAndValidateAndAddToContext(), // Does 3 things!
    error)
```

✅ **GOOD** - Clear separation:
```csharp
.LoadAsync(repo => repo.GetByIdAsync(id))        // Purpose: Load
.EnsureNotEmpty(error)                           // Purpose: Validate
.ExecuteAsync(entity => context.Add(entity))     // Purpose: Execute
```

### Rule 2: Semantic Method Selection
Choose the correct method based on intent, not convenience.

#### Decision Tree:
```
What am I doing?
├── Checking a condition? → Use Ensure*
├── Loading from database? → Use Load*
├── Transforming data? → Use Transform*
├── Performing business logic? → Use Execute*
└── Making final decision? → Use Match*
```

### Rule 3: Validation vs Execution
`Ensure*` methods are for VALIDATION ONLY.

#### Ensure* Method Contract:
- ✅ Must return boolean
- ✅ Must have no side effects
- ✅ Must not modify state
- ✅ Must not perform business logic

❌ **BAD** - Business logic in validation:
```csharp
.EnsureAsync(async () => {
    var entity = await LoadEntity();
    ProcessEntity(entity);  // ❌ Business logic!
    context.Add(entity);    // ❌ State modification!
    return entity != null;
})
```

✅ **GOOD** - Pure validation:
```csharp
.LoadAsync(repo => repo.GetByIdAsync(id))
.EnsureNotEmpty(error)
.ExecuteAsync(entity => {
    ProcessEntity(entity);  // ✅ Business logic in Execute
    context.Add(entity);    // ✅ State modification in Execute
})
```

### Rule 4: Visible Business Logic
Business logic must be explicit in the chain, not hidden in helper methods.

❌ **BAD** - Hidden logic:
```csharp
private async Task<bool> FindOrphanedExercisesIfNeeded(context)
{
    if (context.Exercise?.Zone != WorkoutZone.Main)
        return true;  // Looks like validation
    
    // Hidden business logic!
    var orphans = await FindOrphans();
    context.RemovedExercises.AddRange(orphans);
    return true;
}

// In chain:
.EnsureAsync(
    async () => await FindOrphanedExercisesIfNeeded(context), // Logic hidden!
    error)
```

✅ **GOOD** - Explicit logic:
```csharp
.When(
    entity => entity.Zone == WorkoutZone.Main,
    chain => chain.ExecuteAsync(async context => {
        var orphans = await _handler.FindOrphans(context);
        context.RemovedExercises.AddRange(orphans);
    }))
```

## Practical Examples

### Example 1: Loading and Validating
```csharp
// ❌ WRONG - Loading inside Ensure
.EnsureAsync(
    async () => {
        var entity = await repo.GetByIdAsync(id);
        return entity != null;
    },
    error)

// ✅ CORRECT - Separate Load and Ensure
.LoadAsync(repo => repo.GetByIdAsync(id))
.EnsureNotEmpty(error)
```

### Example 2: Complex Business Logic
```csharp
// ❌ WRONG - Business logic in validation
.EnsureAsync(
    async () => await ProcessRemovalAndReturnSuccess(context),
    error)

// ✅ CORRECT - Explicit execution steps
.LoadAsync(repo => repo.GetByIdAsync(context.ExerciseId))
.EnsureNotEmpty(WorkoutTemplateExerciseErrorMessages.NotFound)
.ExecuteAsync(exercise => {
    context.ExerciseToRemove = exercise;
    context.RemovedExercises.Add(exercise);
})
.When(
    ctx => ctx.ExerciseToRemove.Zone == WorkoutZone.Main,
    chain => chain.ExecuteAsync(async ctx => {
        var orphans = await FindOrphanedExercises(ctx);
        ctx.RemovedExercises.AddRange(orphans);
    }))
.MatchAsync(
    whenValid: async () => await CommitRemoval(context),
    whenInvalid: errors => ServiceResult.Failure(errors))
```

## Method Naming Conventions

To ensure correct usage, follow these naming patterns:

| Method Prefix | Must be used with | Example |
|---------------|-------------------|---------|
| `Is*`, `Has*`, `Does*` | `Ensure*` | `IsValidAsync()` → `.EnsureAsync()` |
| `Load*`, `Get*`, `Fetch*` | `Load*` | `LoadEntityAsync()` → `.LoadAsync()` |
| `Process*`, `Handle*`, `Execute*` | `Execute*` | `ProcessRemovalAsync()` → `.ExecuteAsync()` |
| `Transform*`, `Convert*`, `Map*` | `Transform*` | `TransformToDto()` → `.TransformAsync()` |

## Code Review Checklist

When reviewing ServiceValidate chains, ask:

1. **Purpose Check**: Does each step have a single, clear purpose?
2. **Semantic Check**: Is each operation using the semantically correct method?
3. **Validation Purity**: Do all `Ensure*` methods only return bool without side effects?
4. **Visibility Check**: Is all business logic visible in the chain itself?
5. **Naming Check**: Do helper method names match their usage pattern?

## Red Flags to Watch For

🚩 **Warning Signs**:
- `EnsureAsync` with methods that don't start with `Is`, `Has`, or `Does`
- Methods that do more than their name suggests
- Context modifications inside `Ensure*` methods
- Business logic hidden in validation methods
- Loading data inside validation checks
- Multiple database calls in a single `Ensure*`

## The Correct Mental Model

Think of ServiceValidate chains as a **recipe**:

1. **Gather ingredients** (Load)
2. **Check they're fresh** (Ensure)
3. **Prepare them** (Transform)
4. **Cook the dish** (Execute)
5. **Serve it** (Match)

Each step should be obvious from reading the chain - no surprises hidden in helper methods!

## Summary

The ServiceValidate pattern is powerful, but its power comes from **semantic clarity**. Each method in the chain has a specific purpose, and mixing purposes leads to confusing, unmaintainable code. When in doubt:

- **Make it explicit** rather than clever
- **Show the logic** rather than hide it
- **Separate concerns** rather than combine them
- **Name accurately** rather than generically

Remember: The person reading your ServiceValidate chain should understand the entire flow without looking at any helper methods!