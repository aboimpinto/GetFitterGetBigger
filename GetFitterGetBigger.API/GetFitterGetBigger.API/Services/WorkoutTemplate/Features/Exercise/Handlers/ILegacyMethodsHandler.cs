using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise.Handlers;

/// <summary>
/// Handler for legacy workout template exercise operations to maintain backward compatibility
/// </summary>
public interface ILegacyMethodsHandler
{
    /// <summary>
    /// Handles legacy add exercise command processing
    /// </summary>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> AddExerciseAsync(AddExerciseToTemplateCommand command);

    /// <summary>
    /// Handles legacy update exercise command processing
    /// </summary>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> UpdateExerciseAsync(UpdateTemplateExerciseCommand command);

    /// <summary>
    /// Handles legacy remove exercise processing
    /// </summary>
    Task<ServiceResult<BooleanResultDto>> RemoveExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId);

    /// <summary>
    /// Handles legacy exercise reordering
    /// </summary>
    Task<ServiceResult<BooleanResultDto>> ReorderExercisesAsync(ReorderTemplateExercisesCommand command);

    /// <summary>
    /// Handles legacy exercise zone changes
    /// </summary>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> ChangeExerciseZoneAsync(ChangeExerciseZoneCommand command);

    /// <summary>
    /// Handles legacy exercise duplication
    /// </summary>
    Task<ServiceResult<int>> DuplicateExercisesAsync(DuplicateTemplateExercisesCommand command);
}