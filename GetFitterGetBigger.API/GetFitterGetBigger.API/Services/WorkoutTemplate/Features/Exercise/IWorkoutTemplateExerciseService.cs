using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;

/// <summary>
/// Service for managing exercises within workout templates
/// </summary>
public interface IWorkoutTemplateExerciseService
{
    /// <summary>
    /// Gets all exercises for a workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <returns>List of exercises grouped by zone</returns>
    Task<ServiceResult<WorkoutTemplateExerciseListDto>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId);

    /// <summary>
    /// Gets a specific exercise configuration by ID
    /// </summary>
    /// <param name="exerciseId">The workout template exercise ID</param>
    /// <returns>The exercise configuration with set configurations</returns>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> GetByIdAsync(WorkoutTemplateExerciseId exerciseId);

    /// <summary>
    /// Adds an exercise to a workout template
    /// </summary>
    /// <param name="command">Command containing exercise details</param>
    /// <returns>The created exercise configuration</returns>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> AddExerciseAsync(AddExerciseToTemplateCommand command);

    /// <summary>
    /// Updates an exercise in a workout template
    /// </summary>
    /// <param name="command">Command containing updated exercise details</param>
    /// <returns>The updated exercise configuration</returns>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> UpdateExerciseAsync(UpdateTemplateExerciseCommand command);

    /// <summary>
    /// Removes an exercise from a workout template
    /// </summary>
    /// <param name="workoutTemplateExerciseId">The exercise to remove</param>
    /// <returns>Success or error result</returns>
    Task<ServiceResult<BooleanResultDto>> RemoveExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId);

    /// <summary>
    /// Reorders exercises within a zone
    /// </summary>
    /// <param name="command">Command containing reordering details</param>
    /// <returns>Success or error result</returns>
    Task<ServiceResult<BooleanResultDto>> ReorderExercisesAsync(ReorderTemplateExercisesCommand command);

    /// <summary>
    /// Changes the zone of an exercise
    /// </summary>
    /// <param name="command">Command containing zone change details</param>
    /// <returns>The updated exercise configuration</returns>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> ChangeExerciseZoneAsync(ChangeExerciseZoneCommand command);

    /// <summary>
    /// Duplicates exercises from one template to another
    /// </summary>
    /// <param name="command">Command containing duplication details</param>
    /// <returns>Number of exercises duplicated</returns>
    Task<ServiceResult<int>> DuplicateExercisesAsync(DuplicateTemplateExercisesCommand command);

    /// <summary>
    /// Gets suggested exercises based on template objectives and existing exercises
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="zone">The zone to get suggestions for</param>
    /// <param name="maxSuggestions">Maximum number of suggestions</param>
    /// <returns>List of suggested exercises</returns>
    Task<ServiceResult<List<ExerciseDto>>> GetExerciseSuggestionsAsync(
        WorkoutTemplateId workoutTemplateId, 
        string zone, 
        int maxSuggestions = 5);

    /// <summary>
    /// Validates if exercises can be added to a template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="exerciseIds">List of exercise IDs to validate</param>
    /// <returns>Validation result with any issues found</returns>
    Task<ServiceResult<BooleanResultDto>> ValidateExercisesAsync(
        WorkoutTemplateId workoutTemplateId, 
        List<ExerciseId> exerciseIds);
}