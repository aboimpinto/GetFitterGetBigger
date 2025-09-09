using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;

public interface IReorderExerciseHandler
{
    Task<ServiceResult<ReorderResultDto>> ReorderExerciseAsync(
        WorkoutTemplateId templateId,
        WorkoutTemplateExerciseId exerciseId,
        int newOrderInRound);

    Task ReorderAfterRemovalAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateId templateId,
        List<WorkoutTemplateExercise> removedExercises);
}