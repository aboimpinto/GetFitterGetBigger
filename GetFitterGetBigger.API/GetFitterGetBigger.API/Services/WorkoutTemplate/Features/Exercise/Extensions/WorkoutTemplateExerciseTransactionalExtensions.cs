using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Results;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Extensions;

/// <summary>
/// Advanced transactional extensions for handling EntityResult transformations
/// </summary>
internal static class WorkoutTemplateExerciseTransactionalExtensions
{
    /// <summary>
    /// Special extension for WorkoutTemplateExercise zone change operations
    /// </summary>
    public static async Task<ServiceResult<TResult>> ThenChangeZoneAndCommitAsync<TResult>(
        this Task<TransactionalEntityChain<FitnessDbContext, WorkoutTemplateExercise, TResult>> chainTask,
        WorkoutZone newZone,
        int newSequenceOrder,
        Func<WorkoutTemplateExercise, TResult> mapToResult)
    {
        var chain = await chainTask;
        
        // We need to handle this specially because we can't break the chain with EntityResult
        // So we'll use the existing ThenPerformAsync but handle the EntityResult inside
        var updatedChain = await chain.ThenPerformAsync<IWorkoutTemplateExerciseRepository>(
            async (repo, entity) =>
            {
                var updated = WorkoutTemplateExercise.Handler.ChangeZone(entity, newZone, newSequenceOrder);
                if (updated.IsSuccess)
                {
                    await repo.UpdateAsync(updated.Value);
                }
                // If not successful, we don't update, but we also don't throw
                // The entity remains unchanged, which will be handled by the final validation
            },
            "Change exercise zone");
        
        // Reload the entity to get the updated version
        var reloadedChain = await updatedChain.ThenReloadAsync<IWorkoutTemplateExerciseRepository, WorkoutTemplateExercise>(
            async (repo, entity) => await repo.GetByIdWithDetailsAsync(entity.Id));
        
        return await reloadedChain.ThenCommitAsync(mapToResult);
    }
}