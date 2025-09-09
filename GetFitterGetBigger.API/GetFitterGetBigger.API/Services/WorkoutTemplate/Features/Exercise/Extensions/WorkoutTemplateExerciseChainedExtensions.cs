using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Extensions;

/// <summary>
/// Chained extension methods for async operations in WorkoutTemplateExerciseService
/// </summary>
internal static class WorkoutTemplateExerciseChainedExtensions
{
    /// <summary>
    /// Ensures a condition is met asynchronously and continues the chain
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> ThenEnsureAsyncChained<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        Func<DynamicChainContext, Task<bool>> predicate,
        ServiceError error)
    {
        var builder = await builderTask;
        return await builder.ThenEnsureAsync(predicate, error);
    }
    
    /// <summary>
    /// Ensures a condition is met asynchronously and continues the chain (string overload)
    /// Automatically wraps the error message in ServiceError.ValidationFailed
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> ThenEnsureAsyncChained<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        Func<DynamicChainContext, Task<bool>> predicate,
        string errorMessage)
    {
        var builder = await builderTask;
        return await builder.ThenEnsureAsync(predicate, ServiceError.ValidationFailed(errorMessage));
    }
    
    /// <summary>
    /// Performs an async action and continues the chain
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> ThenPerformAsyncChained<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        Func<DynamicChainContext, Task> action)
    {
        var builder = await builderTask;
        return await builder.ThenPerformAsync(action);
    }
    
    /// <summary>
    /// Performs a conditional action based on zone type
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> ThenPerformIfZone<TResult>(
        this TransactionalServiceValidationBuilder<FitnessDbContext, TResult> builder,
        WorkoutZone targetZone,
        Func<IWorkoutTemplateExerciseRepository, WorkoutTemplateExercise, Task> action,
        string description)
    {
        return await builder.ThenPerformIfAsync(
            context =>
            {
                var entity = context.Get<WorkoutTemplateExercise>("LoadedEntity");
                return entity != null && entity.Zone == targetZone;
            },
            async context =>
            {
                var repository = context.GetRepository<IWorkoutTemplateExerciseRepository>(isReadOnly: false);
                var entity = context.Get<WorkoutTemplateExercise>("LoadedEntity");
                await action(repository, entity);
            });
    }
    
}