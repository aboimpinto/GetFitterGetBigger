using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Extensions;

/// <summary>
/// Extensions for building dynamic repository chains using the centralized DynamicChainContext
/// </summary>
internal static class WorkoutTemplateExerciseDynamicChainExtensions
{
    /// <summary>
    /// Converts a builder to a Task for chaining
    /// </summary>
    public static Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> AsTask<TResult>(
        this TransactionalServiceValidationBuilder<FitnessDbContext, TResult> builder)
    {
        return Task.FromResult(builder);
    }
    /// <summary>
    /// Ensures a writable repository is available and stores it in the context
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> EnsureChainWithWritableRepository<TRepo, TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        DynamicChainContext context)
        where TRepo : class, IRepository
    {
        var builder = await builderTask;
        
        return await builder.EnsureAsync(
            () =>
            {
                // Check if unit of work is stored in context
                if (!context.TryGet<IWritableUnitOfWork<FitnessDbContext>>("UnitOfWork", out var unitOfWork) || unitOfWork == null)
                {
                    // This should not happen as BuildTransactional creates it
                    return Task.FromResult(false);
                }
                
                // Try to get the repository - it should be stored with the automatic naming
                return Task.FromResult(context.TryGetRepository<TRepo>(isReadOnly: false, out _));
            },
            ServiceError.InternalError("Failed to create repository"));
    }
    
    /// <summary>
    /// Ensures a writable repository is available (non-async version for chain start)
    /// </summary>
    public static TransactionalServiceValidationBuilder<FitnessDbContext, TResult> EnsureChainWithWritableRepository<TRepo, TResult>(
        this TransactionalServiceValidationBuilder<FitnessDbContext, TResult> builder,
        DynamicChainContext context,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
        where TRepo : class, IRepository
    {
        // Store the unit of work in context
        context.Store("UnitOfWork", unitOfWork);
        
        // Get repository from unit of work and store it
        var repo = unitOfWork.GetRepository<TRepo>();
        context.StoreRepository(repo, isReadOnly: false);
        
        return builder.Ensure(
            () => repo != null,
            ServiceError.InternalError("Failed to create repository"));
    }
    
    /// <summary>
    /// Loads source exercises and stores them in context
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> ThenLoadSourceExercises<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        DynamicChainContext context,
        Func<IWorkoutTemplateExerciseRepository, Task<IEnumerable<WorkoutTemplateExercise>>> loadFunc)
    {
        var builder = await builderTask;
        
        // Get repository using the new pattern
        if (!context.TryGetRepository<IWorkoutTemplateExerciseRepository>(isReadOnly: false, out var repo) || repo == null)
        {
            return builder.Ensure(() => false, ServiceError.InternalError("Repository not available"));
        }
        
        var exercises = (await loadFunc(repo)).ToList();
        context.Store("SourceExercises", exercises);
        
        return builder;
    }
    
    /// <summary>
    /// Duplicates exercises using loaded data from context
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> ThenDuplicateExercisesAsync<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        DynamicChainContext context,
        Func<List<WorkoutTemplateExercise>, IWorkoutTemplateExerciseRepository, Task<int>> duplicateFunc)
    {
        var builder = await builderTask;
        
        // Get stored exercises and repository
        if (!context.TryGet<List<WorkoutTemplateExercise>>("SourceExercises", out var exercises) || exercises == null ||
            !context.TryGetRepository<IWorkoutTemplateExerciseRepository>(isReadOnly: false, out var repo) || repo == null)
        {
            return builder.Ensure(() => false, ServiceError.InternalError("Required data not available"));
        }
        
        var count = await duplicateFunc(exercises, repo);
        context.Store("DuplicatedCount", count);
        
        return builder;
    }
    
    /// <summary>
    /// Conditionally performs an action based on a condition
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> ThenPerformIf<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        DynamicChainContext context,
        Func<DynamicChainContext, bool> condition,
        Func<DynamicChainContext, Task> action)
    {
        var builder = await builderTask;
        
        if (condition(context))
        {
            await action(context);
        }
        
        return builder;
    }
    
    /// <summary>
    /// Duplicates set configurations if conditions are met
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> ThenDuplicateSettingsAsync<TResult>(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, TResult>> builderTask,
        DynamicChainContext context,
        Func<List<WorkoutTemplateExercise>, ISetConfigurationRepository, Task> duplicateFunc)
    {
        var builder = await builderTask;
        
        // Try to get stored exercises and repository
        if (!context.TryGet<List<WorkoutTemplateExercise>>("SourceExercises", out var exercises) || exercises == null ||
            !context.TryGetRepository<ISetConfigurationRepository>(isReadOnly: false, out var repo) || repo == null)
        {
            // This is not an error - it's optional
            return builder;
        }
        
        await duplicateFunc(exercises, repo);
        
        return builder;
    }
    
    /// <summary>
    /// Extension method for ISetConfigurationRepository to duplicate configurations with exercises
    /// </summary>
    internal static async Task DuplicateConfigurationsForExercisesAsync(
        this ISetConfigurationRepository repository,
        List<WorkoutTemplateExercise> sourceExercises,
        WorkoutTemplateId targetTemplateId,
        IWorkoutTemplateExerciseRepository exerciseRepo)
    {
        // For now, we'll use the existing pattern from the codebase
        // This needs to be implemented based on the actual SetConfiguration structure
        // The actual implementation would duplicate the set configurations from source to target
        
        // Convert to exercise IDs and call an existing method if available
        var sourceIds = sourceExercises.Select(e => e.Id).ToList();
        
        // TODO: Implement the actual duplication logic based on repository capabilities
        // This is a placeholder showing the pattern
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Gets the final result from the context
    /// </summary>
    public static async Task<TransactionalServiceValidationBuilder<FitnessDbContext, int>> ThenReturnDuplicatedCount(
        this Task<TransactionalServiceValidationBuilder<FitnessDbContext, int>> builderTask,
        DynamicChainContext context)
    {
        var builder = await builderTask;
        
        // Get the duplicated count from context
        if (context.TryGet<int>("DuplicatedCount", out var count))
        {
            // The value is already in the builder's result
        }
        
        return builder;
    }
}