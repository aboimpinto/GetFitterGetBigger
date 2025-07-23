using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for managing exercises within workout templates
/// </summary>
public interface IWorkoutTemplateExerciseService
{
    /// <summary>
    /// Gets a workout template exercise by ID with full details
    /// </summary>
    /// <param name="id">The workout template exercise ID</param>
    /// <returns>ServiceResult containing the exercise details or error</returns>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> GetByIdAsync(WorkoutTemplateExerciseId id);
    
    /// <summary>
    /// Gets all exercises for a workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <returns>ServiceResult containing list of exercises ordered by zone and sequence</returns>
    Task<ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>> GetByTemplateAsync(WorkoutTemplateId workoutTemplateId);
    
    /// <summary>
    /// Gets exercises for a specific zone within a template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="zone">The zone to filter by</param>
    /// <returns>ServiceResult containing list of exercises in the zone</returns>
    Task<ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>> GetByZoneAsync(
        WorkoutTemplateId workoutTemplateId, 
        WorkoutZone zone);
    
    /// <summary>
    /// Adds an exercise to a workout template
    /// </summary>
    /// <param name="command">Command containing exercise details</param>
    /// <returns>ServiceResult containing the created exercise or error</returns>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> AddExerciseAsync(AddExerciseToTemplateCommand command);
    
    /// <summary>
    /// Adds multiple exercises to a workout template in bulk
    /// </summary>
    /// <param name="command">Command containing multiple exercises</param>
    /// <returns>ServiceResult containing the created exercises or error</returns>
    Task<ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>> AddExercisesAsync(
        AddExercisesToTemplateCommand command);
    
    /// <summary>
    /// Updates an exercise's notes
    /// </summary>
    /// <param name="id">The exercise ID</param>
    /// <param name="notes">The new notes</param>
    /// <returns>ServiceResult containing the updated exercise or error</returns>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> UpdateNotesAsync(
        WorkoutTemplateExerciseId id, 
        string? notes);
    
    /// <summary>
    /// Changes an exercise's zone and updates sequence order
    /// </summary>
    /// <param name="id">The exercise ID</param>
    /// <param name="newZone">The new zone</param>
    /// <param name="sequenceOrder">The sequence order in the new zone</param>
    /// <returns>ServiceResult containing the updated exercise or error</returns>
    Task<ServiceResult<WorkoutTemplateExerciseDto>> ChangeZoneAsync(
        WorkoutTemplateExerciseId id, 
        WorkoutZone newZone, 
        int sequenceOrder);
    
    /// <summary>
    /// Reorders exercises within a zone
    /// </summary>
    /// <param name="command">Command containing the new order</param>
    /// <returns>ServiceResult indicating success or error</returns>
    Task<ServiceResult<bool>> ReorderExercisesAsync(ReorderExercisesCommand command);
    
    /// <summary>
    /// Removes an exercise from a workout template
    /// </summary>
    /// <param name="id">The exercise ID to remove</param>
    /// <returns>ServiceResult indicating success or error</returns>
    Task<ServiceResult<bool>> RemoveExerciseAsync(WorkoutTemplateExerciseId id);
    
    /// <summary>
    /// Removes all exercises from a workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <returns>ServiceResult containing the number of exercises removed</returns>
    Task<ServiceResult<int>> RemoveAllExercisesAsync(WorkoutTemplateId workoutTemplateId);
    
    /// <summary>
    /// Checks if an exercise is used in any workout template
    /// </summary>
    /// <param name="exerciseId">The exercise ID to check</param>
    /// <returns>True if the exercise is in use</returns>
    Task<bool> IsExerciseInUseAsync(ExerciseId exerciseId);
    
    /// <summary>
    /// Gets the count of templates using a specific exercise
    /// </summary>
    /// <param name="exerciseId">The exercise ID</param>
    /// <returns>Number of templates using this exercise</returns>
    Task<int> GetTemplateCountByExerciseAsync(ExerciseId exerciseId);
}