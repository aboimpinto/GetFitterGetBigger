# Transactional Validation Pattern

## Overview

The Transactional Validation Pattern provides a fluent, chainable approach to managing database transactions with built-in validation, error handling, and automatic commit/rollback functionality. This pattern ensures that all operations within a service method are executed as a single atomic transaction.

## Core Components

### 1. TransactionalServiceValidationBuilder
The entry point for creating transactional validation chains. It manages the UnitOfWork lifecycle and provides validation methods.

### 2. TransactionalEntityChain
Represents a chain of operations on an entity within a transaction. Provides methods for transforming, validating, and persisting entities.

### 3. Extension Methods
Local extension methods that maintain chain continuity while providing domain-specific validation and transformation logic.

## Key Benefits

- **Single Transaction**: All operations execute within a single database transaction
- **Automatic Rollback**: Failed validations or errors automatically trigger rollback
- **Fluent Interface**: Chainable methods improve readability and maintainability
- **No Exception Throwing**: Errors are handled through the ServiceResult pattern
- **Resource Management**: Automatic disposal of database connections and resources

## Implementation Pattern

### Before: Traditional Approach with Scattered UnitOfWork Management

```csharp
public async Task<ServiceResult<WorkoutTemplateExerciseDto>> ChangeExerciseZoneAsync(
    ChangeExerciseZoneCommand command)
{
    // Multiple validation checks with early returns
    if (command == null)
        return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
            WorkoutTemplateExerciseDto.Empty,
            ServiceError.ValidationFailed("Command cannot be null"));
    
    if (command.WorkoutTemplateExerciseId.IsEmpty)
        return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
            WorkoutTemplateExerciseDto.Empty,
            ServiceError.ValidationFailed("Invalid exercise ID"));
    
    if (string.IsNullOrWhiteSpace(command.NewZone))
        return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
            WorkoutTemplateExerciseDto.Empty,
            ServiceError.ValidationFailed("Zone cannot be empty"));
    
    if (!Enum.TryParse<WorkoutZone>(command.NewZone, out var newZone))
        return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
            WorkoutTemplateExerciseDto.Empty,
            ServiceError.ValidationFailed($"Invalid zone: {command.NewZone}"));
    
    // UnitOfWork created in the middle of validation logic
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
    
    var entity = await repository.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId);
    
    if (entity.IsEmpty)
        return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
            WorkoutTemplateExerciseDto.Empty,
            ServiceError.NotFound("Exercise not found"));
    
    // More validation...
    if (!await _validationHandler.IsTemplateInDraftStateAsync(entity.WorkoutTemplateId))
        return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
            WorkoutTemplateExerciseDto.Empty,
            ServiceError.ValidationFailed("Can only change zones in draft templates"));
    
    // Business logic with error handling
    var newSequenceOrder = command.NewSequenceOrder ?? 
        await repository.GetMaxSequenceOrderAsync(entity.WorkoutTemplateId, newZone) + 1;
    
    var result = WorkoutTemplateExercise.Handler.ChangeZone(entity, newZone, newSequenceOrder);
    
    if (!result.IsSuccess)
        return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
            WorkoutTemplateExerciseDto.Empty,
            ServiceError.ValidationFailed(result.FirstError));
    
    await repository.UpdateAsync(result.Value);
    await unitOfWork.CommitAsync();
    
    // Reload to get updated entity
    var updated = await repository.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId);
    
    return ServiceResult<WorkoutTemplateExerciseDto>.Success(updated.ToDto());
}
```

### After: Transactional Chain Pattern

```csharp
public async Task<ServiceResult<WorkoutTemplateExerciseDto>> ChangeExerciseZoneAsync(
    ChangeExerciseZoneCommand command)
{
    return await ServiceValidate.BuildTransactional<WorkoutTemplateExerciseDto>(_unitOfWorkProvider)
        // Input validation using extension methods for readability
        .EnsureNotEmpty(command.WorkoutTemplateExerciseId, 
            WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)
        .EnsureNotEmpty(command.UserId, 
            WorkoutTemplateExerciseErrorMessages.InvalidUserId)
        .EnsureNotWhiteSpace(command.NewZone, 
            WorkoutTemplateExerciseErrorMessages.InvalidZone)
        .EnsureValidZone(command.NewZone, 
            WorkoutTemplateExerciseErrorMessages.InvalidZone)
        
        // Load entity and start entity chain
        .ThenLoadAsync<WorkoutTemplateExercise, IWorkoutTemplateExerciseRepository>(
            async repo => await repo.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId))
        
        // Entity validation
        .ThenEnsureNotEmptyAsync(
            ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound))
        
        // Business rule validation
        .ThenEnsureAsyncChained(
            async entity => await _validationHandler.IsTemplateInDraftStateAsync(entity.WorkoutTemplateId),
            ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CanOnlyChangeZonesInDraftTemplates))
        
        // Transform and update with EntityResult handling
        .ThenTransformWorkoutTemplateExerciseAsync(
            async (repo, entity) =>
            {
                var newZone = Enum.Parse<WorkoutZone>(command.NewZone);
                var newSequenceOrder = command.NewSequenceOrder ?? 
                    await repo.GetMaxSequenceOrderAsync(entity.WorkoutTemplateId, newZone) + 1;
                
                return WorkoutTemplateExercise.Handler.ChangeZone(entity, newZone, newSequenceOrder);
            },
            "Change exercise zone")
        
        // Reload and commit
        .ThenReloadAsyncChained<WorkoutTemplateExercise, WorkoutTemplateExerciseDto, 
            IWorkoutTemplateExerciseRepository, WorkoutTemplateExercise>(
            async (repo, entity) => await repo.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId))
        
        .ThenCommitAsyncChained(entity => entity.ToDto());
}
```

## Creating Local Extension Methods

To maintain chain continuity and improve readability, create local extension methods in a dedicated file:

```csharp
// WorkoutTemplateExerciseValidationExtensions.cs
internal static class WorkoutTemplateExerciseValidationExtensions
{
    /// <summary>
    /// Ensures that a specialized ID is not empty
    /// </summary>
    public static TransactionalServiceValidationBuilder<FitnessDbContext, TResult> EnsureNotEmpty<TResult, TId>(
        this TransactionalServiceValidationBuilder<FitnessDbContext, TResult> builder,
        TId id,
        string errorMessage)
        where TId : struct, ISpecializedId<TId>
    {
        return builder.Ensure(
            () => !id.IsEmpty,
            ServiceError.ValidationFailed(errorMessage));
    }
    
    /// <summary>
    /// Ensures that a string is not null or whitespace
    /// </summary>
    public static TransactionalServiceValidationBuilder<FitnessDbContext, TResult> EnsureNotWhiteSpace<TResult>(
        this TransactionalServiceValidationBuilder<FitnessDbContext, TResult> builder,
        string value,
        string errorMessage)
    {
        return builder.Ensure(
            () => !string.IsNullOrWhiteSpace(value),
            ServiceError.ValidationFailed(errorMessage));
    }
    
    /// <summary>
    /// Extension to chain async operations without breaking the chain
    /// </summary>
    public static async Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> 
        ThenEnsureAsyncChained<TEntity, TResult>(
            this Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> chainTask,
            Func<TEntity, Task<bool>> predicate,
            ServiceError error)
        where TEntity : class
    {
        var chain = await chainTask;
        return await chain.ThenEnsureAsync(predicate, error);
    }
    
    /// <summary>
    /// Extension to handle EntityResult transformations without throwing exceptions
    /// </summary>
    public static async Task<TransactionalEntityChain<FitnessDbContext, WorkoutTemplateExercise, TResult>> 
        ThenTransformWorkoutTemplateExerciseAsync<TResult>(
            this Task<TransactionalEntityChain<FitnessDbContext, WorkoutTemplateExercise, TResult>> chainTask,
            Func<IWorkoutTemplateExerciseRepository, WorkoutTemplateExercise, 
                Task<EntityResult<WorkoutTemplateExercise>>> transformFunc,
            string operationDescription)
    {
        var chain = await chainTask;
        
        // Use ThenTransformAndUpdateAsync to handle EntityResult properly
        return await chain.ThenTransformAndUpdateAsync<IWorkoutTemplateExerciseRepository, WorkoutTemplateExercise>(
            transformFunc,
            async (repo, entity) => await repo.UpdateAsync(entity),
            operationDescription);
    }
}
```

## Key Principles

### 1. Single Chain Philosophy
Maintain a single, unbroken chain from validation to commit. This ensures:
- Clear execution flow
- Automatic transaction management
- Consistent error handling

### 2. No Exception Throwing
All errors are handled through the ServiceResult pattern:
- EntityResult failures are converted to ServiceErrors
- Validation failures add errors to the chain
- Final result includes all accumulated errors

### 3. Automatic Transaction Management
- **Success Path**: Automatic commit when all validations pass
- **Failure Path**: Automatic rollback on any validation failure
- **Exception Path**: Automatic rollback and error capture

### 4. Extension Methods for Domain Logic
Create local extension methods to:
- Encapsulate domain-specific validation
- Maintain chain continuity with async operations
- Improve code readability
- Avoid breaking the chain with intermediate variables

## Benefits Over Traditional Approach

1. **Reduced Boilerplate**: No manual UnitOfWork management
2. **Improved Readability**: Linear flow of operations
3. **Consistent Error Handling**: All errors follow the same pattern
4. **Automatic Resource Management**: No forgotten disposals or commits
5. **Transaction Safety**: Guaranteed rollback on failure
6. **Testability**: Each step in the chain can be tested independently

## Usage Guidelines

1. **Start with BuildTransactional**: Always begin with `ServiceValidate.BuildTransactional<TResult>(_unitOfWorkProvider)`

2. **Use Extension Methods**: Create local extensions for common validations to maintain readability

3. **Keep the Chain**: Avoid breaking the chain with intermediate variables. Use extension methods that return the chain.

4. **Handle EntityResult**: Use `ThenTransformAndUpdateAsync` for operations that return EntityResult

5. **End with ThenCommitAsync**: Always end the chain with `ThenCommitAsync` to ensure proper transaction completion

## Migration Strategy

When migrating existing methods:
1. Identify all validation points
2. Create extension methods for domain-specific validations
3. Build the chain from validation through commit
4. Remove manual UnitOfWork management
5. Test thoroughly to ensure behavior is preserved