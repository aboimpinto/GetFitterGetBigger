using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for managing WorkoutTemplateExercise entities with phase/round-based organization
/// </summary>
public interface IWorkoutTemplateExerciseRepository : IRepository
{
    // CRUD Operations
    /// <summary>
    /// Gets a WorkoutTemplateExercise by ID
    /// </summary>
    /// <param name="id">The WorkoutTemplateExercise ID</param>
    /// <returns>WorkoutTemplateExercise or WorkoutTemplateExercise.Empty if not found</returns>
    Task<WorkoutTemplateExercise> GetByIdAsync(WorkoutTemplateExerciseId id);
    
    /// <summary>
    /// Gets all exercises for a specific workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <returns>List of exercises ordered by phase, round, and order within round</returns>
    Task<List<WorkoutTemplateExercise>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId);
    
    /// <summary>
    /// Gets exercises for a specific phase within a workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="phase">The phase (Warmup, Workout, Cooldown)</param>
    /// <returns>List of exercises in the specified phase ordered by round and order</returns>
    Task<List<WorkoutTemplateExercise>> GetByTemplateAndPhaseAsync(WorkoutTemplateId workoutTemplateId, string phase);
    
    /// <summary>
    /// Gets exercises for a specific phase and round within a workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="phase">The phase (Warmup, Workout, Cooldown)</param>
    /// <param name="roundNumber">The round number</param>
    /// <returns>List of exercises in the specified phase and round ordered by order in round</returns>
    Task<List<WorkoutTemplateExercise>> GetByTemplatePhaseAndRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber);
    
    // Auto-linking support queries
    /// <summary>
    /// Gets all workout exercises (phase = "Workout") for auto-linking functionality
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <returns>List of workout exercises for auto-linking warmup/cooldown exercises</returns>
    Task<List<WorkoutTemplateExercise>> GetWorkoutExercisesAsync(WorkoutTemplateId workoutTemplateId);
    
    /// <summary>
    /// Checks if an exercise exists in the template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="exerciseId">The exercise ID to check</param>
    /// <returns>True if the exercise exists in any phase of the template</returns>
    Task<bool> ExistsInTemplateAsync(WorkoutTemplateId workoutTemplateId, ExerciseId exerciseId);
    
    /// <summary>
    /// Checks if an exercise exists in a specific phase of the template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="phase">The phase to check</param>
    /// <param name="exerciseId">The exercise ID to check</param>
    /// <returns>True if the exercise exists in the specified phase</returns>
    Task<bool> ExistsInPhaseAsync(WorkoutTemplateId workoutTemplateId, string phase, ExerciseId exerciseId);
    
    // Order management
    /// <summary>
    /// Gets the maximum order in round for a specific phase and round
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="phase">The phase</param>
    /// <param name="roundNumber">The round number</param>
    /// <returns>The highest order in round, or 0 if no exercises exist</returns>
    Task<int> GetMaxOrderInRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber);
    
    /// <summary>
    /// Reorders exercises within a specific round
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="phase">The phase</param>
    /// <param name="roundNumber">The round number</param>
    /// <param name="newOrders">Dictionary mapping exercise IDs to their new order in round</param>
    /// <returns>Task representing the async operation</returns>
    Task ReorderExercisesInRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber, Dictionary<WorkoutTemplateExerciseId, int> newOrders);
    
    // Round management
    /// <summary>
    /// Gets all exercises in a specific round
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="phase">The phase</param>
    /// <param name="roundNumber">The round number</param>
    /// <returns>List of exercises in the round ordered by order in round</returns>
    Task<List<WorkoutTemplateExercise>> GetRoundExercisesAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber);
    
    /// <summary>
    /// Gets the maximum round number for a specific phase
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="phase">The phase</param>
    /// <returns>The highest round number in the phase, or 0 if no exercises exist</returns>
    Task<int> GetMaxRoundNumberAsync(WorkoutTemplateId workoutTemplateId, string phase);
    
    // Modification operations
    /// <summary>
    /// Adds a new exercise to a workout template
    /// </summary>
    /// <param name="exercise">The exercise to add</param>
    /// <returns>Task representing the async operation</returns>
    Task AddAsync(WorkoutTemplateExercise exercise);
    
    /// <summary>
    /// Adds multiple exercises to a workout template in bulk
    /// </summary>
    /// <param name="exercises">The exercises to add</param>
    /// <returns>Task representing the async operation</returns>
    Task AddRangeAsync(List<WorkoutTemplateExercise> exercises);
    
    /// <summary>
    /// Updates an existing workout template exercise
    /// </summary>
    /// <param name="exercise">The exercise to update</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdateAsync(WorkoutTemplateExercise exercise);
    
    /// <summary>
    /// Removes an exercise from a workout template
    /// </summary>
    /// <param name="id">The exercise ID to remove</param>
    /// <returns>Task representing the async operation</returns>
    Task DeleteAsync(WorkoutTemplateExerciseId id);
    
    /// <summary>
    /// Removes multiple exercises from a workout template in bulk
    /// </summary>
    /// <param name="ids">The exercise IDs to remove</param>
    /// <returns>Task representing the async operation</returns>
    Task DeleteRangeAsync(List<WorkoutTemplateExerciseId> ids);

    // LEGACY METHODS - For backward compatibility until service layer is updated
    /// <summary>
    /// Gets a WorkoutTemplateExercise by ID with all related entities (legacy method)
    /// </summary>
    /// <param name="id">The WorkoutTemplateExercise ID</param>
    /// <returns>WorkoutTemplateExercise with Exercise and related data loaded</returns>
    Task<WorkoutTemplateExercise> GetByIdWithDetailsAsync(WorkoutTemplateExerciseId id);

    /// <summary>
    /// Gets the maximum sequence order for a zone in a workout template (legacy method)
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="zone">The zone to check</param>
    /// <returns>The highest sequence order in the zone</returns>
    Task<int> GetMaxSequenceOrderAsync(WorkoutTemplateId workoutTemplateId, WorkoutZone zone);

    /// <summary>
    /// Reorders exercises within a zone (legacy method)
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <param name="zone">The zone to reorder</param>
    /// <param name="exerciseOrders">Dictionary of exercise IDs to their new sequence orders</param>
    /// <returns>True if successful</returns>
    Task<bool> ReorderExercisesAsync(WorkoutTemplateId workoutTemplateId, WorkoutZone zone, Dictionary<WorkoutTemplateExerciseId, int> exerciseOrders);
}