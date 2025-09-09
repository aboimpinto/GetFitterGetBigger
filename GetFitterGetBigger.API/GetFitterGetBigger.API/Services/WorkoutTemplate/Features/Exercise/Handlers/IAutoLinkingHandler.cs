using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;

public interface IAutoLinkingHandler
{
    Task<List<WorkoutTemplateExercise>> AddAutoLinkedExercisesAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateId templateId,
        ExerciseId workoutExerciseId);

    Task<List<WorkoutTemplateExercise>> FindOrphanedExercisesAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateId templateId,
        ExerciseId removedWorkoutExerciseId);
}