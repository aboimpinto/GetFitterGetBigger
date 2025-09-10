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
    /// Handles enhanced exercise metadata updates
    /// </summary>
    Task<ServiceResult<UpdateMetadataResultDto>> UpdateExerciseMetadataAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, string metadata);

    /// <summary>
    /// Handles exercise metadata validation
    /// </summary>
    Task<ServiceResult<BooleanResultDto>> ValidateExerciseMetadataAsync(ExerciseId exerciseId, ExecutionProtocolId protocolId, string metadata);

    /// <summary>
    /// Handles template exercise retrieval organized by phases and rounds
    /// </summary>
    Task<ServiceResult<WorkoutTemplateExercisesDto>> GetTemplateExercisesAsync(WorkoutTemplateId templateId);
}