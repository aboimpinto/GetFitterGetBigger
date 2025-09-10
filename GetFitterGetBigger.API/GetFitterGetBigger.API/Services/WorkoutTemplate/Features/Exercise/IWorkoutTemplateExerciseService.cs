using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;

/// <summary>
/// Enhanced service for managing exercises within workout templates with phase/round organization
/// </summary>
public interface IWorkoutTemplateExerciseService
{
    /// <summary>
    /// Adds an exercise to a workout template with auto-linking support
    /// </summary>
    /// <param name="templateId">The workout template ID</param>
    /// <param name="dto">DTO containing exercise details</param>
    /// <returns>Result containing all added exercises (main + auto-linked)</returns>
    Task<ServiceResult<AddExerciseResultDto>> AddExerciseAsync(WorkoutTemplateId templateId, AddExerciseDto dto);

    /// <summary>
    /// Removes an exercise from a workout template with orphan cleanup
    /// </summary>
    /// <param name="templateId">The workout template ID</param>
    /// <param name="exerciseId">The exercise ID to remove</param>
    /// <returns>Result containing all removed exercises (main + orphaned)</returns>
    Task<ServiceResult<RemoveExerciseResultDto>> RemoveExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId);

    /// <summary>
    /// Updates exercise metadata for ExecutionProtocol-specific configuration
    /// </summary>
    /// <param name="templateId">The workout template ID</param>
    /// <param name="exerciseId">The exercise ID to update</param>
    /// <param name="metadata">JSON metadata string</param>
    /// <returns>The updated exercise configuration</returns>
    Task<ServiceResult<UpdateMetadataResultDto>> UpdateExerciseMetadataAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, string metadata);

    /// <summary>
    /// Reorders an exercise within a round
    /// </summary>
    /// <param name="templateId">The workout template ID</param>
    /// <param name="exerciseId">The exercise ID to reorder</param>
    /// <param name="newOrderInRound">The new order position within the round</param>
    /// <returns>Result containing all reordered exercises</returns>
    Task<ServiceResult<ReorderResultDto>> ReorderExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, int newOrderInRound);

    /// <summary>
    /// Copies a complete round to another location with new GUIDs
    /// </summary>
    /// <param name="templateId">The workout template ID</param>
    /// <param name="dto">DTO containing copy details</param>
    /// <returns>Result containing all copied exercises with new GUIDs</returns>
    Task<ServiceResult<CopyRoundResultDto>> CopyRoundAsync(WorkoutTemplateId templateId, CopyRoundDto dto);

    /// <summary>
    /// Gets all exercises for a workout template organized by phases and rounds
    /// </summary>
    /// <param name="templateId">The workout template ID</param>
    /// <returns>Exercises organized by Warmup, Workout, Cooldown phases with rounds</returns>
    Task<ServiceResult<WorkoutTemplateExercisesDto>> GetTemplateExercisesAsync(WorkoutTemplateId templateId);

    /// <summary>
    /// Gets a specific exercise configuration by ID
    /// </summary>
    /// <param name="exerciseId">The workout template exercise ID</param>
    /// <returns>The exercise configuration with metadata</returns>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> GetExerciseByIdAsync(WorkoutTemplateExerciseId exerciseId);

    /// <summary>
    /// Validates exercise metadata against ExecutionProtocol requirements
    /// </summary>
    /// <param name="exerciseId">The exercise ID</param>
    /// <param name="protocolId">The execution protocol ID</param>
    /// <param name="metadata">JSON metadata to validate</param>
    /// <returns>Validation result</returns>
    Task<ServiceResult<BooleanResultDto>> ValidateExerciseMetadataAsync(ExerciseId exerciseId, ExecutionProtocolId protocolId, string metadata);

    /// <summary>
    /// Gets exercise suggestions for a specific workout template and zone
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="zone">The zone to get suggestions for</param>
    /// <param name="maxSuggestions">Maximum number of suggestions to return</param>
    /// <returns>List of suggested exercises</returns>
    Task<ServiceResult<ExerciseListDto>> GetExerciseSuggestionsAsync(WorkoutTemplateId workoutTemplateId, string zone, int maxSuggestions = 5);

    /// <summary>
    /// Validates a list of exercises for a workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="exerciseIds">List of exercise IDs to validate</param>
    /// <returns>Validation result</returns>
    Task<ServiceResult<BooleanResultDto>> ValidateExercisesAsync(WorkoutTemplateId workoutTemplateId, List<ExerciseId> exerciseIds);

}