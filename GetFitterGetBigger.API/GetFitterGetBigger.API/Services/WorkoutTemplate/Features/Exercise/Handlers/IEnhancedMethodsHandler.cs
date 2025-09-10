using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;

/// <summary>
/// Handler for enhanced workout template exercise operations with phase/round support
/// </summary>
public interface IEnhancedMethodsHandler
{
    /// <summary>
    /// Adds an exercise to a workout template with auto-linking support
    /// </summary>
    Task<ServiceResult<AddExerciseResultDto>> AddExerciseAsync(WorkoutTemplateId templateId, AddExerciseDto dto);

    /// <summary>
    /// Removes an exercise from a workout template with orphan cleanup
    /// </summary>
    Task<ServiceResult<RemoveExerciseResultDto>> RemoveExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId);

    /// <summary>
    /// Handles enhanced exercise metadata updates
    /// </summary>
    Task<ServiceResult<UpdateMetadataResultDto>> UpdateExerciseMetadataAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, string metadata);

    /// <summary>
    /// Reorders an exercise within its round
    /// </summary>
    Task<ServiceResult<ReorderResultDto>> ReorderExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, int newOrderInRound);

    /// <summary>
    /// Copies all exercises from one round to another
    /// </summary>
    Task<ServiceResult<CopyRoundResultDto>> CopyRoundAsync(WorkoutTemplateId templateId, CopyRoundDto dto);

    /// <summary>
    /// Handles template exercise retrieval organized by phases and rounds
    /// </summary>
    Task<ServiceResult<WorkoutTemplateExercisesDto>> GetTemplateExercisesAsync(WorkoutTemplateId templateId);

    /// <summary>
    /// Handles exercise metadata validation
    /// </summary>
    Task<ServiceResult<BooleanResultDto>> ValidateExerciseMetadataAsync(ExerciseId exerciseId, ExecutionProtocolId protocolId, string metadata);
}