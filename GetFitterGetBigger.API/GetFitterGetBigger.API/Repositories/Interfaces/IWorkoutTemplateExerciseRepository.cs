using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for managing WorkoutTemplateExercise entities
/// </summary>
public interface IWorkoutTemplateExerciseRepository : IRepository
{
    /// <summary>
    /// Gets a WorkoutTemplateExercise by ID with all related entities
    /// </summary>
    /// <param name="id">The WorkoutTemplateExercise ID</param>
    /// <returns>WorkoutTemplateExercise with Exercise and SetConfigurations loaded, or WorkoutTemplateExercise.Empty if not found</returns>
    Task<WorkoutTemplateExercise> GetByIdWithDetailsAsync(WorkoutTemplateExerciseId id);
    
    /// <summary>
    /// Gets all exercises for a specific workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <returns>List of exercises ordered by zone and sequence</returns>
    Task<IEnumerable<WorkoutTemplateExercise>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId);
    
    /// <summary>
    /// Gets exercises for a specific zone within a workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="zone">The zone (Warmup, Main, Cooldown)</param>
    /// <returns>List of exercises in the specified zone ordered by sequence</returns>
    Task<IEnumerable<WorkoutTemplateExercise>> GetByZoneAsync(WorkoutTemplateId workoutTemplateId, WorkoutZone zone);
    
    /// <summary>
    /// Checks if a specific exercise is used in any workout template
    /// </summary>
    /// <param name="exerciseId">The exercise ID to check</param>
    /// <returns>True if the exercise is used in any template</returns>
    Task<bool> IsExerciseInUseAsync(ExerciseId exerciseId);
    
    /// <summary>
    /// Gets the count of templates using a specific exercise
    /// </summary>
    /// <param name="exerciseId">The exercise ID</param>
    /// <returns>Number of templates using this exercise</returns>
    Task<int> GetTemplateCountByExerciseAsync(ExerciseId exerciseId);
    
    /// <summary>
    /// Gets the maximum sequence order for a zone in a workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="zone">The zone to check</param>
    /// <returns>The highest sequence order in the zone, or 0 if no exercises</returns>
    Task<int> GetMaxSequenceOrderAsync(WorkoutTemplateId workoutTemplateId, WorkoutZone zone);
    
    /// <summary>
    /// Reorders exercises within a zone
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="zone">The zone to reorder</param>
    /// <param name="exerciseOrders">Dictionary of exercise IDs to their new sequence orders</param>
    /// <returns>True if successful</returns>
    Task<bool> ReorderExercisesAsync(WorkoutTemplateId workoutTemplateId, WorkoutZone zone, Dictionary<WorkoutTemplateExerciseId, int> exerciseOrders);
    
    /// <summary>
    /// Adds a new exercise to a workout template
    /// </summary>
    /// <param name="workoutTemplateExercise">The exercise to add</param>
    /// <returns>The added exercise</returns>
    Task<WorkoutTemplateExercise> AddAsync(WorkoutTemplateExercise workoutTemplateExercise);
    
    /// <summary>
    /// Adds multiple exercises to a workout template in bulk
    /// </summary>
    /// <param name="workoutTemplateExercises">The exercises to add</param>
    /// <returns>The added exercises</returns>
    Task<IEnumerable<WorkoutTemplateExercise>> AddRangeAsync(IEnumerable<WorkoutTemplateExercise> workoutTemplateExercises);
    
    /// <summary>
    /// Updates an existing workout template exercise
    /// </summary>
    /// <param name="workoutTemplateExercise">The exercise to update</param>
    /// <returns>The updated exercise</returns>
    Task<WorkoutTemplateExercise> UpdateAsync(WorkoutTemplateExercise workoutTemplateExercise);
    
    /// <summary>
    /// Removes an exercise from a workout template
    /// </summary>
    /// <param name="id">The exercise ID to remove</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteAsync(WorkoutTemplateExerciseId id);
    
    /// <summary>
    /// Removes all exercises from a workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <returns>Number of exercises removed</returns>
    Task<int> DeleteAllByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId);
}