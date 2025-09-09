using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Extensions;

/// <summary>
/// Extension methods for WorkoutTemplateExercise validation chains
/// </summary>
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
    /// Extension to chain EnsureAsync after an async operation (auto-wraps with ValidationFailed)
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> EnsureAsyncChained<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        Func<Task<bool>> predicate,
        string errorMessage)
    {
        var builder = await builderTask;
        return await builder.EnsureAsync(predicate, ServiceError.ValidationFailed(errorMessage));
    }
    
    /// <summary>
    /// Extension to chain EnsureAsync after an async operation (with ServiceError)
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> EnsureAsyncChained<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        Func<Task<bool>> predicate,
        ServiceError error)
    {
        var builder = await builderTask;
        return await builder.EnsureAsync(predicate, error);
    }
    
    /// <summary>
    /// Extension to chain EnsureAsync with NotFound error
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> EnsureExistsAsyncChained<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        Func<Task<bool>> predicate,
        string notFoundMessage)
    {
        var builder = await builderTask;
        return await builder.EnsureAsync(predicate, ServiceError.NotFound(notFoundMessage));
    }
    
    /// <summary>
    /// Chains ThenCreateWritableRepository after an async operation
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> ThenCreateWritableRepositoryChained<TResult, TRepo>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        string? customKey = null)
        where TRepo : class, IRepository
    {
        var builder = await builderTask;
        return builder.ThenCreateWritableRepository<FitnessDbContext, TResult, TRepo>(customKey);
    }
    
    /// <summary>
    /// Chains ThenLoadAsync after an async operation
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> ThenLoadAsyncChained<TResult, TData>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        string storeAs,
        Func<DynamicChainContext, Task<TData>> loadFunc)
    {
        var builder = await builderTask;
        return await builder.ThenLoadAsync(storeAs, loadFunc);
    }
    
    /// <summary>
    /// Chains ThenExecuteIf after an async operation
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> ThenExecuteIfChained<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        Func<bool> condition,
        Func<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>, TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> action)
    {
        var builder = await builderTask;
        return builder.ThenExecuteIf(condition, action);
    }
    
    /// <summary>
    /// Chains ThenPerformIfAsync after an async operation
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> ThenPerformIfAsyncChained<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        Func<DynamicChainContext, bool> condition,
        Func<DynamicChainContext, Task> action)
    {
        var builder = await builderTask;
        return await builder.ThenPerformIfAsync(condition, action);
    }
    
    /// <summary>
    /// Chains ThenExecuteAsync after an async operation (terminal operation)
    /// </summary>
    public static async Task<ServiceResult<TResult>> ThenExecuteAsyncChained<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        Func<DynamicChainContext, Task<TResult>> executeFunc)
    {
        var builder = await builderTask;
        return await builder.ThenExecuteAsync(executeFunc);
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
    /// Ensures that a zone string can be parsed to WorkoutZone enum
    /// </summary>
    public static TransactionalServiceValidationBuilder<FitnessDbContext, TResult> EnsureValidZone<TResult>(
        this TransactionalServiceValidationBuilder<FitnessDbContext, TResult> builder,
        string zone,
        string errorMessageFormat)
    {
        return builder.Ensure(
            () => Enum.TryParse<WorkoutZone>(zone, out _),
            ServiceError.ValidationFailed(string.Format(errorMessageFormat, zone ?? "null")));
    }
    
    /// <summary>
    /// Extension to chain ThenEnsureNotEmpty after an async operation
    /// </summary>
    public static async Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> ThenEnsureNotEmptyAsync<TEntity, TResult>(
        this Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> chainTask,
        ServiceError error)
        where TEntity : class
    {
        var chain = await chainTask;
        return chain.ThenEnsureNotEmpty(error);
    }
    
    /// <summary>
    /// Extension to chain ThenEnsureAsync after an async operation
    /// </summary>
    public static async Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> ThenEnsureAsyncChained<TEntity, TResult>(
        this Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> chainTask,
        Func<TEntity, Task<bool>> predicate,
        ServiceError error)
        where TEntity : class
    {
        var chain = await chainTask;
        return await chain.ThenEnsureAsync(predicate, error);
    }
    
    /// <summary>
    /// Extension to chain ThenEnsureAsync after an async operation (string overload)
    /// Automatically wraps the error message in ServiceError.ValidationFailed
    /// </summary>
    public static async Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> ThenEnsureAsyncChained<TEntity, TResult>(
        this Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> chainTask,
        Func<TEntity, Task<bool>> predicate,
        string errorMessage)
        where TEntity : class
    {
        var chain = await chainTask;
        return await chain.ThenEnsureAsync(predicate, ServiceError.ValidationFailed(errorMessage));
    }
    
    /// <summary>
    /// Extension to chain ThenPerformAsync after an async operation
    /// </summary>
    public static async Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> ThenPerformAsyncChained<TEntity, TResult, TRepo>(
        this Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> chainTask,
        Func<TRepo, TEntity, Task> operation,
        string operationDescription)
        where TEntity : class
        where TRepo : class, IRepository
    {
        var chain = await chainTask;
        return await chain.ThenPerformAsync<TRepo>(operation, operationDescription);
    }
    
    
    /// <summary>
    /// Extension to chain ThenReloadAsync after an async operation
    /// </summary>
    public static async Task<TransactionalEntityChain<FitnessDbContext, TNewEntity, TResult>> ThenReloadAsyncChained<TEntity, TResult, TRepo, TNewEntity>(
        this Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> chainTask,
        Func<TRepo, TEntity, Task<TNewEntity>> reloadFunc)
        where TEntity : class
        where TNewEntity : class
        where TRepo : class, IRepository
    {
        var chain = await chainTask;
        return await chain.ThenReloadAsync<TRepo, TNewEntity>(reloadFunc);
    }
    
    /// <summary>
    /// Extension to chain ThenCommitAsync after an async operation
    /// </summary>
    public static async Task<ServiceResult<TResult>> ThenCommitAsyncChained<TEntity, TResult>(
        this Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> chainTask,
        Func<TEntity, TResult> mapToResult)
        where TEntity : class
    {
        var chain = await chainTask;
        return await chain.ThenCommitAsync(mapToResult);
    }
    
    /// <summary>
    /// Extension to chain ThenTransform after an async operation
    /// </summary>
    public static async Task<TransactionalEntityChain<FitnessDbContext, TNewEntity, TResult>> ThenTransformChained<TEntity, TNewEntity, TResult>(
        this Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> chainTask,
        Func<TEntity, EntityResult<TNewEntity>> transformFunc,
        string operationDescription)
        where TEntity : class
        where TNewEntity : class, IEmptyEntity<TNewEntity>
    {
        var chain = await chainTask;
        return chain.ThenTransform(transformFunc, operationDescription);
    }
    
    /// <summary>
    /// Extension to transform WorkoutTemplateExercise with repository access and handle EntityResult
    /// </summary>
    public static async Task<TransactionalEntityChain<FitnessDbContext, WorkoutTemplateExercise, TResult>> ThenTransformWorkoutTemplateExerciseAsync<TResult>(
        this Task<TransactionalEntityChain<FitnessDbContext, WorkoutTemplateExercise, TResult>> chainTask,
        Func<IWorkoutTemplateExerciseRepository, WorkoutTemplateExercise, Task<EntityResult<WorkoutTemplateExercise>>> transformFunc,
        string operationDescription)
    {
        var chain = await chainTask;
        
        // Use the new ThenTransformAndUpdateAsync method that properly handles EntityResult
        return await chain.ThenTransformAndUpdateAsync<IWorkoutTemplateExerciseRepository, WorkoutTemplateExercise>(
            transformFunc,
            async (repo, entity) => await repo.UpdateAsync(entity),
            operationDescription);
    }
    
    /// <summary>
    /// Conditionally performs an operation when the entity's zone matches the specified zone
    /// </summary>
    public static async Task<TransactionalEntityChain<FitnessDbContext, WorkoutTemplateExercise, TResult>> ThenPerformIfZone<TResult>(
        this Task<TransactionalEntityChain<FitnessDbContext, WorkoutTemplateExercise, TResult>> chainTask,
        WorkoutZone targetZone,
        Func<IWorkoutTemplateExerciseRepository, WorkoutTemplateExercise, Task> operation,
        string operationDescription)
    {
        var chain = await chainTask;
        
        // Only perform the operation if the zone matches
        return await chain.ThenPerformAsync<IWorkoutTemplateExerciseRepository>(
            async (repo, entity) =>
            {
                if (entity.Zone == targetZone)
                {
                    await operation(repo, entity);
                }
            },
            operationDescription);
    }
    
    /// <summary>
    /// Conditionally performs an operation based on a predicate
    /// </summary>
    public static async Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> ThenPerformIf<TEntity, TResult>(
        this Task<TransactionalEntityChain<FitnessDbContext, TEntity, TResult>> chainTask,
        Func<TEntity, bool> predicate,
        Func<IRepository, TEntity, Task> operation,
        string operationDescription)
        where TEntity : class
    {
        var chain = await chainTask;
        
        return await chain.ThenPerformAsync<IRepository>(
            async (repo, entity) =>
            {
                if (predicate(entity))
                {
                    await operation(repo, entity);
                }
            },
            operationDescription);
    }
    
    /// <summary>
    /// Extension to chain ThenExecuteWithUnitOfWorkAsync
    /// </summary>
    public static async Task<ServiceResult<TResult>> ThenExecuteWithUnitOfWorkAsyncChained<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        Func<IWritableUnitOfWork<FitnessDbContext>, Task<TResult>> executeFunc)
    {
        var builder = await builderTask;
        return await builder.ThenExecuteWithUnitOfWorkAsync(executeFunc);
    }
    
    /// <summary>
    /// Performs exercise duplication with proper separation of concerns
    /// </summary>
    public static async Task<int> DuplicateExercisesWithValidationAsync(
        this IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateId sourceTemplateId,
        WorkoutTemplateId targetTemplateId,
        bool includeSetConfigurations,
        ISetConfigurationRepository setConfigRepo)
    {
        // Load source exercises
        var sourceExercises = (await repository.GetByWorkoutTemplateAsync(sourceTemplateId)).ToList();
        
        // Early return if no source exercises
        if (!sourceExercises.Any())
            return 0;
        
        // Duplicate exercises
        var duplicatedCount = await repository.DuplicateExercisesOnlyAsync(
            sourceExercises,
            targetTemplateId);
        
        // Duplicate configurations if requested and exercises were duplicated
        if (includeSetConfigurations && duplicatedCount > 0)
        {
            var duplicatedExercises = (await repository.GetByWorkoutTemplateAsync(targetTemplateId))
                .OrderBy(e => e.SequenceOrder)
                .Take(duplicatedCount)
                .ToList();
                
            await DuplicateSetConfigurationsAsync(
                sourceExercises,
                duplicatedExercises,
                setConfigRepo);
        }
        
        return duplicatedCount;
    }
    
    /// <summary>
    /// Duplicates only the exercises (single responsibility)
    /// </summary>
    public static async Task<int> DuplicateExercisesOnlyAsync(
        this IWorkoutTemplateExerciseRepository repository,
        List<WorkoutTemplateExercise> sourceExercises,
        WorkoutTemplateId targetTemplateId)
    {
        var duplicatedExercises = sourceExercises
            .Select(e => WorkoutTemplateExercise.Handler.CreateNew(
                targetTemplateId, 
                e.ExerciseId, 
                e.Zone, 
                e.SequenceOrder, 
                e.Notes))
            .Where(r => r.IsSuccess)
            .Select(r => r.Value)
            .ToList();
        
        if (duplicatedExercises.Any())
        {
            await repository.AddRangeAsync(duplicatedExercises);
            return duplicatedExercises.Count;
        }
        
        return 0;
    }
    
    /// <summary>
    /// Validates that source template has exercises
    /// </summary>
    public static async Task<bool> HasExercisesAsync(
        this IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateId templateId)
    {
        var exercises = await repository.GetByWorkoutTemplateAsync(templateId);
        return exercises.Any();
    }
    
    /// <summary>
    /// Duplicates set configurations from source to target exercises
    /// </summary>
    private static async Task DuplicateSetConfigurationsAsync(
        List<WorkoutTemplateExercise> sourceExercises,
        List<WorkoutTemplateExercise> targetExercises,
        ISetConfigurationRepository setConfigRepo)
    {
        for (int i = 0; i < Math.Min(sourceExercises.Count, targetExercises.Count); i++)
        {
            var sourceExercise = sourceExercises[i];
            var targetExercise = targetExercises[i];
            
            if (sourceExercise.Configurations?.Any() == true)
            {
                var duplicatedConfigs = sourceExercise.Configurations
                    .Select(c => GetFitterGetBigger.API.Models.Entities.SetConfiguration.Handler.CreateNew(
                        targetExercise.Id, 
                        c.SetNumber, 
                        c.TargetReps, 
                        c.TargetWeight, 
                        c.TargetTimeSeconds, 
                        c.RestSeconds))
                    .Where(r => r.IsSuccess)
                    .Select(r => r.Value)
                    .ToList();
                
                if (duplicatedConfigs.Any())
                    await setConfigRepo.AddRangeAsync(duplicatedConfigs);
            }
        }
    }
    
    /// <summary>
    /// Validates that all exercises in the list exist in the database
    /// </summary>
    public static async Task<bool> AreAllExercisesValidAsync(
        this IExerciseRepository repository,
        List<ExerciseId> exerciseIds)
    {
        foreach (var exerciseId in exerciseIds)
        {
            var exercise = await repository.GetByIdAsync(exerciseId);
            if (exercise.IsEmpty)
                return false;
        }
        return true;
    }
    
}