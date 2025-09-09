# BuildTransactional Pattern - Complete Guide

**🎯 PURPOSE**: Eliminate boilerplate transaction management code while maintaining a fluent, readable API for database operations with automatic rollback on failure. The pattern uses `DynamicChainContext` as a first-class citizen to flow data and repositories through the entire validation chain.

## 🚨 CRITICAL PRINCIPLE

> **"Don't be afraid of creating extension methods - readability is KING!"**
> 
> If you see a pattern repeated twice, extract it. If you see an if-statement, create a conditional extension. If logic is complex, wrap it in a semantic extension method.

## Core Pattern Overview

The `ServiceValidate.BuildTransactional` pattern provides a fluent API for database transactions with:
- **DynamicChainContext**: First-class context flowing through the entire chain
- Automatic UnitOfWork management with rollback on failure
- Repository creation and storage in context
- Data loading and storage between chain steps
- Entity loading and transformation
- Conditional operations based on context state
- Clean, readable chains without intermediate variables

## Understanding DynamicChainContext

The `DynamicChainContext` is the heart of BuildTransactional - it flows through every step of your chain, carrying:
- **Repositories**: Created once, reused throughout
- **Loaded Data**: Entities, counts, intermediate results
- **State**: Flags, conditions, computation results
- **UnitOfWork**: Managed automatically

### Basic Structure with Context

```csharp
public async Task<ServiceResult<int>> DuplicateExercisesAsync(Command command)
{
    return await ServiceValidate.BuildTransactional<int>(_unitOfWorkProvider)
        // Step 1: Create repository - stored in context
        .ThenCreateWritableRepository<IWorkoutTemplateExerciseRepository>()
        
        // Step 2: Load data into context with named key
        .ThenLoadAsync("SourceExercises", async context => {
            var repository = context.GetRepository<IWorkoutTemplateExerciseRepository>();
            return await repository.GetByWorkoutTemplateAsync(command.SourceId);
        })
        
        // Step 3: Use previously loaded data
        .ThenLoadAsync("DuplicatedCount", async context => {
            var sourceExercises = context.Get<List<WorkoutTemplateExercise>>("SourceExercises");
            var repository = context.GetRepository<IWorkoutTemplateExerciseRepository>();
            return await repository.DuplicateExercisesOnlyAsync(sourceExercises, command.TargetId);
        })
        
        // Step 4: Conditional operation based on command
        .ThenExecuteIf(
            () => command.IncludeSetConfigurations,
            builder => builder.ThenCreateWritableRepository<ISetConfigurationRepository>())
        
        // Step 5: Return value from context
        .ThenExecuteAsync(context => 
            Task.FromResult(context.Get<int>("DuplicatedCount")));
}
```

### Context Methods

```csharp
// Store data with a key
context.Store("key", value);

// Retrieve data by key
var value = context.Get<T>("key");

// Check if key exists
if (context.Contains("key")) { }

// Get repository (automatically typed)
var repository = context.GetRepository<IRepository>();

// Store repository (done automatically by ThenCreateRepository)
context.StoreRepository(repository, isReadOnly: false);
```

## ❌ BEFORE: The Old Way (DON'T DO THIS!)

```csharp
private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ExecuteUpdateExerciseAsync(UpdateTemplateExerciseCommand command)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var exerciseTemplateRepo = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
    
    var exerciseTemplate = await exerciseTemplateRepo.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId);
    
    return await ServiceValidate.Build<WorkoutTemplateExerciseDto>()
        .Ensure(
            () => !exerciseTemplate.IsEmpty,
            ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound))
        .EnsureAsync(
            async () => await _validationHandler.IsTemplateInDraftStateAsync(exerciseTemplate.WorkoutTemplateId),
            WorkoutTemplateExerciseErrorMessages.CanOnlyUpdateExercisesInDraftTemplates)
        .MatchAsync(
            whenValid: async () =>
            {
                var updated = WorkoutTemplateExercise.Handler.UpdateNotes(exerciseTemplate, command.Notes);
                if (!updated.IsSuccess)  // REDUNDANT CHECK!
                {
                    return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                        WorkoutTemplateExerciseDto.Empty,
                        ServiceError.ValidationFailed(string.Join(", ", updated.Errors)));
                }

                await exerciseTemplateRepo.UpdateAsync(updated.Value);
                await unitOfWork.CommitAsync();

                var reloaded = await exerciseTemplateRepo.GetByIdWithDetailsAsync(updated.Value.Id);
                return ServiceResult<WorkoutTemplateExerciseDto>.Success(reloaded.ToDto());
            },
            whenInvalid: (IReadOnlyList<ServiceError> errors) => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                WorkoutTemplateExerciseDto.Empty,
                errors.First())
        );
}
```

### Problems with the Old Way:
- Manual UnitOfWork management
- Nested code in MatchAsync
- Manual EntityResult checking
- Verbose error handling
- Hard to read flow
- Multiple exit points

## ✅ AFTER: The BuildTransactional Way

```csharp
private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ExecuteUpdateExerciseAsync(UpdateTemplateExerciseCommand command)
{
    return await ServiceValidate.BuildTransactional<WorkoutTemplateExerciseDto>(_unitOfWorkProvider)
        .ThenLoadAsync<WorkoutTemplateExercise, IWorkoutTemplateExerciseRepository>(
            async repo => await repo.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId))
        .ThenEnsureNotEmptyAsync(
            ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound))
        .ThenEnsureAsyncChained(
            async entity => await _validationHandler.IsTemplateInDraftStateAsync(entity.WorkoutTemplateId),
            ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CanOnlyUpdateExercisesInDraftTemplates))
        .ThenTransform(
            entity => WorkoutTemplateExercise.Handler.UpdateNotes(entity, command.Notes),
            "Update exercise notes")
        .ThenPerformAsync<IWorkoutTemplateExerciseRepository>(
            async (repo, entity) => await repo.UpdateAsync(entity),
            "Save updated exercise")
        .ThenReloadAsync<IWorkoutTemplateExerciseRepository, WorkoutTemplateExercise>(
            async (repo, entity) => await repo.GetByIdWithDetailsAsync(entity.Id))
        .ThenCommitAsync(entity => entity.ToDto());
}
```

### Benefits:
- Automatic transaction management
- Linear, readable flow
- Automatic EntityResult handling
- No nested logic
- Single exit point
- Clear operation sequence

## Available Chain Methods

### 1. Loading Entities

```csharp
.ThenLoadAsync<TEntity, TRepo>(
    async repo => await repo.GetByIdAsync(id))
```

### 2. Validation

```csharp
// Check entity not empty
.ThenEnsureNotEmptyAsync(
    ServiceError.NotFound("Not found"))

// Custom async validation
.ThenEnsureAsyncChained(
    async entity => await ValidateAsync(entity),
    ServiceError.ValidationFailed("Invalid"))

// Sync validation
.ThenEnsure(
    entity => entity.IsActive,
    ServiceError.ValidationFailed("Not active"))
```

### 3. Transformation

```csharp
// Transform with EntityResult handling
.ThenTransform(
    entity => Entity.Handler.Update(entity, command),
    "Update operation")

// Async transformation with repository
.ThenTransformAndUpdateAsync<TRepo, TEntity>(
    async (repo, entity) => await TransformAsync(entity),
    async (repo, transformed) => await repo.UpdateAsync(transformed),
    "Transform and update")
```

### 4. Operations

```csharp
// Perform repository operation
.ThenPerformAsync<TRepo>(
    async (repo, entity) => await repo.UpdateAsync(entity),
    "Update entity")

// Delete operation
.ThenPerformAsync<TRepo>(
    async (repo, entity) => await repo.DeleteAsync(entity.Id),
    "Delete entity")
```

### 5. Conditional Operations

```csharp
// Zone-specific operation
.ThenPerformIfZone(
    WorkoutZone.Main,
    async (repo, entity) => await HandleMainZoneAsync(repo, entity),
    "Handle main zone logic")

// Generic conditional
.ThenPerformIf(
    entity => entity.NeedsProcessing,
    async (repo, entity) => await ProcessAsync(repo, entity),
    "Process if needed")
```

### 6. Reloading

```csharp
// Reload entity with full details
.ThenReloadAsync<TRepo, TEntity>(
    async (repo, entity) => await repo.GetByIdWithDetailsAsync(entity.Id))
```

### 7. Committing

```csharp
// Commit and map to result
.ThenCommitAsync(entity => entity.ToDto())

// Commit with custom mapping
.ThenCommitAsync(entity => new CustomDto { 
    Id = entity.Id,
    Name = entity.Name 
})
```

## Creating Custom Extension Methods

### Philosophy: Readability Over Everything

**NEVER hesitate to create an extension method if it improves readability!**

### Example 1: Eliminating If-Statements

```csharp
// ❌ BEFORE - If-statement in the chain
.ThenPerformAsync<IRepository>(
    async (repo, entity) =>
    {
        if (entity.Zone == WorkoutZone.Main)
        {
            // Complex logic here
            var orphaned = await FindOrphaned(repo, entity);
            foreach (var o in orphaned)
                await repo.DeleteAsync(o.Id);
        }
    },
    "Handle orphaned")

// ✅ AFTER - Clean conditional extension
.ThenPerformIfZone(
    WorkoutZone.Main,
    async (repo, entity) => await HandleOrphanedExercisesAsync(repo, entity),
    "Handle orphaned exercises for main zone")
```

### Example 2: Complex Validation

```csharp
// ❌ BEFORE - Complex inline validation
.ThenEnsureAsync(
    async entity => 
    {
        var exists = await _dataService.ExistsByNameAsync(entity.Name);
        return !exists.Data.Value;
    },
    ServiceError.DuplicateName("Name exists"))

// ✅ AFTER - Semantic extension method
.ThenEnsureNameIsUniqueAsync(
    async entity => await IsNameUniqueAsync(entity.Name),
    "Exercise", entity.Name)
```

### Example 3: Repeated Patterns

```csharp
// If you see this pattern twice:
.ThenLoadAsync<WorkoutTemplateExercise, IWorkoutTemplateExerciseRepository>(
    async repo => await repo.GetByIdWithDetailsAsync(id))
.ThenEnsureNotEmptyAsync(
    ServiceError.NotFound("Not found"))

// Create an extension:
.ThenLoadWorkoutTemplateExerciseAsync(id)
```

## Extension Method Guidelines

### 1. When to Create Extensions

Create an extension method when:
- You see the same pattern twice
- You have an if-statement in a chain
- Logic is complex (>3 lines)
- The operation has semantic meaning
- It would improve readability

### 2. Naming Conventions

```csharp
// Conditional operations
ThenPerformIf[Condition]     // ThenPerformIfZone, ThenPerformIfActive
ThenTransformIf[Condition]   // ThenTransformIfDraft
ThenEnsureIf[Condition]      // ThenEnsureIfRequired

// Semantic operations
ThenEnsure[What]IsUnique     // ThenEnsureNameIsUnique
ThenEnsure[What]Exists       // ThenEnsureExerciseExists
ThenLoad[EntityName]         // ThenLoadWorkoutTemplate
ThenValidate[What]           // ThenValidatePermissions
```

### 3. Extension Method Structure

```csharp
public static async Task<TransactionalEntityChain<TContext, TEntity, TResult>> 
    ThenYourExtensionAsync<TEntity, TResult>(
        this Task<TransactionalEntityChain<TContext, TEntity, TResult>> chainTask,
        // Your parameters
        string operationDescription)
    where TEntity : class
{
    var chain = await chainTask;
    
    return await chain.ThenPerformAsync<IRepository>(
        async (repo, entity) =>
        {
            // Your logic here
        },
        operationDescription);
}
```

## Real-World Example: Complete Service Method

```csharp
public async Task<ServiceResult<BooleanResultDto>> RemoveExerciseAsync(
    WorkoutTemplateExerciseId workoutTemplateExerciseId)
{
    return await ServiceValidate.BuildTransactional<BooleanResultDto>(_unitOfWorkProvider)
        // Validation
        .EnsureNotEmpty(workoutTemplateExerciseId, ErrorMessages.InvalidExerciseId)
        
        // Load entity
        .ThenLoadAsync<WorkoutTemplateExercise, IWorkoutTemplateExerciseRepository>(
            async repo => await repo.GetByIdWithDetailsAsync(workoutTemplateExerciseId))
        
        // Ensure exists
        .ThenEnsureNotEmptyAsync(
            ServiceError.NotFound(ErrorMessages.TemplateExerciseNotFound))
        
        // Validate business rule
        .ThenEnsureAsyncChained(
            async entity => await _validationHandler.IsTemplateInDraftStateAsync(entity.WorkoutTemplateId),
            ServiceError.ValidationFailed(ErrorMessages.CanOnlyRemoveFromDraft))
        
        // Conditional operation - NO IF-STATEMENT!
        .ThenPerformIfZone(
            WorkoutZone.Main,
            async (repo, entity) => await HandleOrphanedExercisesAsync(repo, entity),
            "Handle orphaned exercises for main zone")
        
        // Delete
        .ThenPerformAsyncChained<WorkoutTemplateExercise, BooleanResultDto, IWorkoutTemplateExerciseRepository>(
            async (repo, _) => await repo.DeleteAsync(workoutTemplateExerciseId),
            "Remove exercise")
        
        // Commit
        .ThenCommitAsyncChained(_ => BooleanResultDto.Create(true));
}
```

## Common Patterns to Extract

### 1. Load and Validate Existence

```csharp
// Create extension for this common pattern:
public static async Task<TransactionalEntityChain<TContext, TEntity, TResult>> 
    ThenLoadAndEnsureExistsAsync<TEntity, TResult>(
        this TransactionalServiceValidationBuilder<TContext, TResult> builder,
        Func<IRepository, Task<TEntity>> loadFunc,
        string entityName)
    where TEntity : class, IEmptyEntity<TEntity>
{
    return await builder
        .ThenLoadAsync<TEntity, IRepository>(loadFunc)
        .ThenEnsureNotEmptyAsync(
            ServiceError.NotFound($"{entityName} not found"));
}
```

### 2. Transform and Save

```csharp
// Create extension for transform-update pattern:
public static async Task<TransactionalEntityChain<TContext, TEntity, TResult>> 
    ThenTransformAndSaveAsync<TEntity, TResult>(
        this Task<TransactionalEntityChain<TContext, TEntity, TResult>> chainTask,
        Func<TEntity, EntityResult<TEntity>> transformFunc,
        string operationDescription)
    where TEntity : class, IEmptyEntity<TEntity>
{
    var chain = await chainTask;
    
    return await chain
        .ThenTransform(transformFunc, operationDescription)
        .ThenPerformAsync<IRepository>(
            async (repo, entity) => await repo.UpdateAsync(entity),
            $"Save {operationDescription}");
}
```

### 3. Conditional Delete

```csharp
// Create extension for conditional cascading deletes:
public static async Task<TransactionalEntityChain<TContext, TEntity, TResult>> 
    ThenDeleteIfConditionAsync<TEntity, TResult>(
        this Task<TransactionalEntityChain<TContext, TEntity, TResult>> chainTask,
        Func<TEntity, bool> condition,
        Func<IRepository, TEntity, Task<IEnumerable<IEntity>>> findRelatedFunc,
        string operationDescription)
    where TEntity : class
{
    var chain = await chainTask;
    
    return await chain.ThenPerformIf(
        condition,
        async (repo, entity) =>
        {
            var related = await findRelatedFunc(repo, entity);
            foreach (var item in related)
                await repo.DeleteAsync(item.Id);
        },
        operationDescription);
}
```

## When to Use BuildTransactional vs Build vs For

### Use `ServiceValidate.For<T>()` when:
- **ALL validations are synchronous**
- No database operations needed
- Simple validation of input parameters
- No entity loading or transformation

```csharp
return ServiceValidate.For<Result>()
    .EnsureNotNull(command, "Command required")
    .EnsureNotWhiteSpace(command.Name, "Name required")
    .Ensure(() => command.Age > 0, "Invalid age")
    .Match(
        whenValid: () => ServiceResult<Result>.Success(new Result()),
        whenInvalid: errors => ServiceResult<Result>.Failure(Result.Empty, errors.First()));
```

### Use `ServiceValidate.Build<T>()` when:
- **ANY validation is asynchronous**
- Need to check database state (exists, unique, etc.)
- No transaction management needed
- Read-only operations

```csharp
return await ServiceValidate.Build<ExerciseDto>()
    .EnsureNotEmpty(exerciseId, "Invalid ID")
    .EnsureAsync(
        async () => await ExerciseExistsAsync(exerciseId),
        ServiceError.NotFound("Exercise not found"))
    .MatchAsync(
        whenValid: async () => await LoadExerciseAsync(exerciseId));
```

### Use `ServiceValidate.BuildTransactional<T>()` when:
- **Database modifications are involved**
- Multiple repository operations
- Need transaction with rollback
- Complex multi-step operations
- Loading entities for transformation
- Conditional database operations

```csharp
return await ServiceValidate.BuildTransactional<Result>(_unitOfWorkProvider)
    .EnsureNotEmpty(id, "Invalid ID")
    .ThenLoadAsync<Entity, IRepository>(
        async repo => await repo.GetByIdAsync(id))
    .ThenTransform(entity => Entity.Handler.Update(entity, command))
    .ThenPerformAsync<IRepository>(
        async (repo, entity) => await repo.UpdateAsync(entity))
    .ThenCommitAsync(entity => entity.ToDto());
```

### Decision Tree
```
Need validation?
├─ All synchronous? → Use For<T>()
├─ Any async validation?
│  ├─ No DB changes? → Use Build<T>()
│  └─ DB changes? → Use BuildTransactional<T>()
└─ Complex DB operation? → Use BuildTransactional<T>()
```

## Migration Guide: From Build to BuildTransactional

### Step 1: Identify Candidates

Look for methods with:
- Manual `using var unitOfWork = _unitOfWorkProvider.CreateWritable()`
- Multiple repository operations
- Entity transformations with EntityResult
- Manual commit calls

### Step 2: Refactor Structure

1. Replace `ServiceValidate.Build<T>()` with `ServiceValidate.BuildTransactional<T>(_unitOfWorkProvider)`
2. Move entity loading into `ThenLoadAsync`
3. Replace validation inside MatchAsync with ThenEnsure methods
4. Extract transformations to `ThenTransform`
5. Move repository operations to `ThenPerformAsync`
6. Replace manual commit with `ThenCommitAsync`

### Step 3: Extract Extensions

After refactoring, look for:
- Repeated patterns → Create shared extensions
- If-statements → Create conditional extensions
- Complex logic → Create semantic extensions

## Testing BuildTransactional Chains

```csharp
[Fact]
public async Task UpdateExercise_WhenTemplateNotInDraft_ShouldReturnValidationError()
{
    // Arrange
    var command = new UpdateTemplateExerciseCommand { /* ... */ };
    
    _validationHandlerMock
        .Setup(x => x.IsTemplateInDraftStateAsync(It.IsAny<WorkoutTemplateId>()))
        .ReturnsAsync(false);
    
    // Act
    var result = await _service.UpdateExerciseAsync(command);
    
    // Assert
    result.Should().BeFailure();
    result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
}
```

## Key Principles

1. **Readability is King**: If an extension makes code more readable, create it
2. **No If-Statements**: Use conditional extensions instead
3. **Semantic Names**: Methods should describe WHAT, not HOW
4. **Extract Early**: Don't wait for three repetitions - two is enough
5. **Trust the Chain**: The framework handles errors - you don't need to
6. **Single Responsibility**: Each chain method does ONE thing
7. **Composition Over Complexity**: Many simple extensions > few complex ones

## Dealing with Async Chaining Problems

### The Challenge: Async Breaks the Chain

When an async method returns `Task<TransactionalServiceValidationBuilder>`, you can't continue chaining directly:

```csharp
// ❌ PROBLEM - Can't chain after async operation
return await ServiceValidate.BuildTransactional<int>(_unitOfWorkProvider)
    .EnsureNotEmpty(sourceId, "Invalid")
    .ThenLoadAsync(...) // Returns Task<Builder>
    .ThenLoadAsync(...) // ERROR: Task has no ThenLoadAsync method!
```

### The Solution: Chained Extension Methods

Create "Chained" versions that bridge the async gap:

```csharp
// Extension that accepts Task<Builder> and continues the chain
public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> 
    ThenLoadAsyncChained<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        string storeAs,
        Func<DynamicChainContext, Task<object>> loadFunc)
{
    var builder = await builderTask; // Await the Task
    return await builder.ThenLoadAsync(storeAs, loadFunc); // Continue chain
}
```

### Step-by-Step Chaining Process

When building a complex chain with async operations:

1. **Start with synchronous validations** - These chain normally
2. **First async operation breaks the chain** - Returns Task<Builder>
3. **Use "Chained" extension for next step** - Bridges the gap
4. **Continue with "Chained" methods** - All subsequent operations
5. **Terminal operation returns ServiceResult** - Final step

```csharp
return await ServiceValidate.BuildTransactional<int>(_unitOfWorkProvider)
    // Step 1: Sync validations - chain normally
    .EnsureNotEmpty(sourceId, "Invalid source")
    .EnsureNotEmpty(targetId, "Invalid target")
    
    // Step 2: First async - use Chained version
    .ThenCreateWritableRepositoryChained<int, IWorkoutTemplateExerciseRepository>()
    
    // Step 3-6: All subsequent operations use Chained
    .ThenLoadAsyncChained("SourceExercises", async context => {...})
    .ThenLoadAsyncChained("DuplicatedCount", async context => {...})
    .ThenExecuteIfChained(condition, action)
    .ThenPerformIfAsyncChained(condition, asyncAction)
    .ThenExecuteAsyncChained(context => Task.FromResult(...));
```

### Creating Your Own Chained Extensions

Follow this template:

```csharp
public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> 
    Then[YourMethod]Chained<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        /* your parameters */)
{
    var builder = await builderTask; // ALWAYS await first
    return builder.Then[YourMethod](/* forward parameters */); // Or await if async
}
```

### Best Practices for Chained Extensions

1. **Keep them local** - Put in service-specific extension files
2. **Name consistently** - Always append "Chained"
3. **Document the chain** - Comment each step's purpose
4. **Test the full chain** - Ensure all steps execute correctly
5. **Don't nest** - Keep the chain flat and readable

## Remember

> "The best code is not the shortest code, it's the most readable code. Create as many extension methods as needed to make your intentions crystal clear."

> "When async breaks your chain, don't fight it - bridge it with Chained extensions!"

---

*This pattern is mandatory for all new transactional service methods. Existing methods should be migrated when touched.*