using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Infrastructure;
using GetFitterGetBigger.API.Services.Results;
using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;

/// <summary>
/// Data service interface for WorkoutTemplate write operations.
/// Handles all database modifications and entity-to-DTO mapping.
/// </summary>
public interface IWorkoutTemplateCommandDataService
{
    /// <summary>
    /// Creates a new workout template
    /// </summary>
    /// <param name="entity">The workout template entity to create</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result containing the created workout template DTO</returns>
    Task<ServiceResult<WorkoutTemplateDto>> CreateAsync(
        WorkoutTemplateEntity entity,
        ITransactionScope? scope = null);
    
    /// <summary>
    /// Updates an existing workout template
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <param name="updateAction">Function to apply updates to the entity</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result containing the updated workout template DTO</returns>
    Task<ServiceResult<WorkoutTemplateDto>> UpdateAsync(
        WorkoutTemplateId id,
        Func<WorkoutTemplateEntity, WorkoutTemplateEntity> updateAction,
        ITransactionScope? scope = null);
    
    /// <summary>
    /// Changes the state of a workout template
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <param name="newStateId">The new state ID</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result containing the updated workout template DTO</returns>
    Task<ServiceResult<WorkoutTemplateDto>> ChangeStateAsync(
        WorkoutTemplateId id,
        WorkoutStateId newStateId,
        ITransactionScope? scope = null);
    
    /// <summary>
    /// Duplicates an existing workout template
    /// </summary>
    /// <param name="sourceId">The source workout template ID</param>
    /// <param name="newName">The name for the duplicated template</param>
    /// <param name="createdById">The user creating the duplicate</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result containing the duplicated workout template DTO</returns>
    Task<ServiceResult<WorkoutTemplateDto>> DuplicateAsync(
        WorkoutTemplateId sourceId,
        string newName,
        UserId createdById,
        ITransactionScope? scope = null);
    
    /// <summary>
    /// Soft deletes a workout template (sets to ARCHIVED state)
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult<BooleanResultDto>> SoftDeleteAsync(
        WorkoutTemplateId id,
        ITransactionScope? scope = null);
    
    /// <summary>
    /// Permanently deletes a workout template from the database
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult<BooleanResultDto>> DeleteAsync(
        WorkoutTemplateId id,
        ITransactionScope? scope = null);
    
    /// <summary>
    /// Adds an exercise to a workout template
    /// </summary>
    /// <param name="templateId">The workout template ID</param>
    /// <param name="exerciseId">The exercise ID to add</param>
    /// <param name="orderInWorkout">The order of the exercise in the workout</param>
    /// <param name="sets">Number of sets</param>
    /// <param name="reps">Number of reps</param>
    /// <param name="restSeconds">Rest time in seconds</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result containing the updated workout template DTO</returns>
    Task<ServiceResult<WorkoutTemplateDto>> AddExerciseAsync(
        WorkoutTemplateId templateId,
        ExerciseId exerciseId,
        int orderInWorkout,
        int sets,
        int reps,
        int restSeconds,
        ITransactionScope? scope = null);
    
    /// <summary>
    /// Removes an exercise from a workout template
    /// </summary>
    /// <param name="templateId">The workout template ID</param>
    /// <param name="exerciseId">The exercise ID to remove</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result containing the updated workout template DTO</returns>
    Task<ServiceResult<WorkoutTemplateDto>> RemoveExerciseAsync(
        WorkoutTemplateId templateId,
        ExerciseId exerciseId,
        ITransactionScope? scope = null);
    
    /// <summary>
    /// Updates the exercise configuration in a workout template
    /// </summary>
    /// <param name="templateId">The workout template ID</param>
    /// <param name="exerciseId">The exercise ID</param>
    /// <param name="sets">Updated number of sets</param>
    /// <param name="reps">Updated number of reps</param>
    /// <param name="restSeconds">Updated rest time in seconds</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result containing the updated workout template DTO</returns>
    Task<ServiceResult<WorkoutTemplateDto>> UpdateExerciseConfigurationAsync(
        WorkoutTemplateId templateId,
        ExerciseId exerciseId,
        int sets,
        int reps,
        int restSeconds,
        ITransactionScope? scope = null);
    
    /// <summary>
    /// Reorders exercises in a workout template
    /// </summary>
    /// <param name="templateId">The workout template ID</param>
    /// <param name="exerciseOrders">Dictionary of exercise IDs to their new orders</param>
    /// <param name="scope">Optional transaction scope for participating in larger transactions</param>
    /// <returns>Service result containing the updated workout template DTO</returns>
    Task<ServiceResult<WorkoutTemplateDto>> ReorderExercisesAsync(
        WorkoutTemplateId templateId,
        Dictionary<ExerciseId, int> exerciseOrders,
        ITransactionScope? scope = null);
}