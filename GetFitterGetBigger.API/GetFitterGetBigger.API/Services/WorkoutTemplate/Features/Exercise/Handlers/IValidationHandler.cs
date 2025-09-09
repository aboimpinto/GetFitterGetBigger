using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;

public interface IValidationHandler
{
    Task<bool> IsTemplateInDraftStateAsync(WorkoutTemplateId templateId);
    Task<bool> IsExerciseActiveAsync(ExerciseId exerciseId);
    Task<bool> DoesTemplateExistAsync(WorkoutTemplateId templateId);
    Task<bool> DoesExerciseExistInTemplateAsync(
        WorkoutTemplateExerciseId exerciseId,
        WorkoutTemplateId templateId);
    Task<bool> DoesRoundExistAsync(
        WorkoutTemplateId templateId,
        string phase,
        int roundNumber);
    Task<bool> TemplateHasExercisesAsync(WorkoutTemplateId templateId);
}