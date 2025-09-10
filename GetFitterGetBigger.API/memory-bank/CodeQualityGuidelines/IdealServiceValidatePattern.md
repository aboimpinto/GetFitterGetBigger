# The Ideal ServiceValidate Pattern - Context-Driven Chain

## Current Problem

The current ServiceValidate pattern has fundamental clarity issues:

1. **Hidden Context**: Methods like `ThenEnsureNotEmptyAsync` don't show WHAT they're checking
2. **Mystery Parameters**: Parameters like `(repo, exercise)` appear without clear origin
3. **No Context Flow**: Each step doesn't have access to a shared context
4. **Implicit State**: The chain carries hidden state that's not visible in the code

## The Ideal Pattern

### What We Want to Write

```csharp
public async Task<ServiceResult<RemoveExerciseResultDto>> RemoveExerciseAsync(
    WorkoutTemplateId templateId, 
    WorkoutTemplateExerciseId exerciseId)
{
    return await ServiceValidate.BuildTransactional<RemovalContext, RemoveExerciseResultDto>(_unitOfWorkProvider)
        // Initialize context
        .WithContext(new RemovalContext 
        { 
            TemplateId = templateId,
            ExerciseId = exerciseId 
        })
        
        // LOAD: Explicit about what we're loading and where it goes
        .ThenLoad(
            context => context.ExerciseRepository.GetByIdAsync(context.ExerciseId),
            (context, exercise) => context.PrimaryExercise = exercise)
        
        // VALIDATE: Explicit about what we're checking
        .ThenEnsureNotEmpty(
            context => context.PrimaryExercise,
            WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound)
        
        // EXECUTE: Build removal list
        .ThenExecute(context => 
        {
            context.ExercisesToRemove.Add(context.PrimaryExercise);
        })
        
        // EXECUTE: Conditionally find orphans
        .ThenExecuteIf(
            context => context.PrimaryExercise.Zone == WorkoutZone.Main,
            async context => 
            {
                context.OrphanedExercises = await _autoLinkingHandler.FindOrphanedExercisesAsync(
                    context.ExerciseRepository, 
                    context.TemplateId, 
                    context.PrimaryExercise.ExerciseId);
                context.ExercisesToRemove.AddRange(context.OrphanedExercises);
            })
        
        // EXECUTE: Delete all exercises
        .ThenExecute(async context => 
        {
            foreach (var exercise in context.ExercisesToRemove)
            {
                await context.ExerciseRepository.DeleteAsync(exercise.Id);
            }
        })
        
        // EXECUTE: Reorder remaining
        .ThenExecute(async context => 
        {
            await _reorderExerciseHandler.ReorderAfterRemovalAsync(
                context.ExerciseRepository, 
                context.TemplateId, 
                context.ExercisesToRemove);
        })
        
        // COMMIT: Create result
        .ThenCommit(context => new RemoveExerciseResultDto(
            context.ExercisesToRemove.Select(e => e.ToDto()).ToList(),
            context.BuildMessage()));
}
```

## Key Principles

### 1. Explicit Context
Every operation receives and modifies an explicit context object:
```csharp
.ThenExecute(context => { /* context is visible and typed */ })
```

### 2. Clear Parameter Origins
No mystery parameters - everything comes from context:
```csharp
// ❌ BAD - Where does 'repo' come from?
.ThenPerform(async (repo, exercise) => { })

// ✅ GOOD - Clear origin
.ThenExecute(async context => {
    var repo = context.ExerciseRepository;
    var exercise = context.PrimaryExercise;
})
```

### 3. Explicit Validation Targets
Validation shows WHAT is being validated:
```csharp
// ❌ BAD - What's not empty?
.ThenEnsureNotEmptyAsync(error)

// ✅ GOOD - Clear target
.ThenEnsureNotEmpty(context => context.PrimaryExercise, error)
```

### 4. Single Responsibility per Step
Each step does ONE thing with a clear name:
```csharp
.ThenExecute(context => context.ExercisesToRemove.Add(context.PrimaryExercise))
.ThenExecute(async context => await DeleteAllExercises(context))
.ThenExecute(async context => await ReorderRemaining(context))
```

## Context Design

### The Context Object
```csharp
public class RemovalContext : IChainContext
{
    // Input parameters
    public WorkoutTemplateId TemplateId { get; init; }
    public WorkoutTemplateExerciseId ExerciseId { get; init; }
    
    // Repositories (injected by framework)
    public IWorkoutTemplateExerciseRepository ExerciseRepository { get; set; }
    
    // Working state
    public WorkoutTemplateExercise? PrimaryExercise { get; set; }
    public List<WorkoutTemplateExercise> OrphanedExercises { get; set; } = new();
    public List<WorkoutTemplateExercise> ExercisesToRemove { get; set; } = new();
    
    // Helper methods
    public string BuildMessage() => OrphanedExercises.Any()
        ? $"Removed {ExercisesToRemove.Count} exercise(s) including {OrphanedExercises.Count} orphaned"
        : $"Removed {ExercisesToRemove.Count} exercise(s)";
}
```

## Benefits of This Pattern

### 1. Self-Documenting Code
Reading the chain tells you exactly what happens:
- Load primary exercise into context
- Ensure primary exercise is not empty
- Add primary exercise to removal list
- If main zone, find and add orphans
- Delete all exercises
- Reorder remaining
- Commit and return result

### 2. Testability
Each step can be tested independently with a mock context.

### 3. Debuggability
You can inspect the context at any point to see the full state.

### 4. No Hidden Magic
Everything is explicit - no hidden state, no mystery parameters.

## Migration Path

To achieve this ideal pattern, we need:

1. **New Base Class**: `ContextualServiceValidationBuilder<TContext, TResult>`
2. **Context-Aware Methods**: All methods receive context as first parameter
3. **Repository Injection**: Framework injects repositories into context
4. **Explicit State Management**: Context carries all state explicitly

## Example Comparison

### Current (Confusing)
```csharp
.ThenEnsureNotEmptyAsync(error)  // What's not empty?
.ThenPerformIfZone(               // Where does this zone come from?
    WorkoutZone.Main,
    async (repo, exercise) =>      // What repo? What exercise?
    {
        removedExercises.Add(exercise);  // External variable capture
    })
```

### Ideal (Clear)
```csharp
.ThenEnsureNotEmpty(
    context => context.PrimaryExercise,  // Explicit: checking primary exercise
    error)
.ThenExecuteIf(
    context => context.PrimaryExercise.Zone == WorkoutZone.Main,  // Clear condition
    async context =>  // Single, typed context parameter
    {
        context.ExercisesToRemove.Add(context.PrimaryExercise);  // Explicit state
    })
```

## Conclusion

The ideal pattern makes everything explicit:
- **What** we're validating
- **Where** data comes from
- **How** state flows through the chain
- **Why** each step exists

This eliminates the guesswork and makes the code self-documenting. Every line answers its own "what" and "why" questions.