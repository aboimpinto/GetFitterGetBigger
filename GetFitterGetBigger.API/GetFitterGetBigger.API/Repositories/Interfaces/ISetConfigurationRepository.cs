using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for managing SetConfiguration entities
/// </summary>
public interface ISetConfigurationRepository : IRepository
{
    /// <summary>
    /// Gets a SetConfiguration by ID
    /// </summary>
    /// <param name="id">The SetConfiguration ID</param>
    /// <returns>SetConfiguration or SetConfiguration.Empty if not found</returns>
    Task<SetConfiguration> GetByIdAsync(SetConfigurationId id);
    
    /// <summary>
    /// Gets all set configurations for a specific workout template exercise
    /// </summary>
    /// <param name="workoutTemplateExerciseId">The workout template exercise ID</param>
    /// <returns>List of set configurations ordered by set number</returns>
    Task<IEnumerable<SetConfiguration>> GetByWorkoutTemplateExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId);
    
    /// <summary>
    /// Gets set configurations for multiple workout template exercises
    /// </summary>
    /// <param name="workoutTemplateExerciseIds">The workout template exercise IDs</param>
    /// <returns>Dictionary mapping exercise IDs to their set configurations</returns>
    Task<Dictionary<WorkoutTemplateExerciseId, IEnumerable<SetConfiguration>>> GetByWorkoutTemplateExercisesAsync(IEnumerable<WorkoutTemplateExerciseId> workoutTemplateExerciseIds);
    
    /// <summary>
    /// Gets all set configurations for a workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <returns>Set configurations grouped by exercise</returns>
    Task<IEnumerable<SetConfiguration>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId);
    
    /// <summary>
    /// Gets the maximum set number for a workout template exercise
    /// </summary>
    /// <param name="workoutTemplateExerciseId">The workout template exercise ID</param>
    /// <returns>The highest set number, or 0 if no sets exist</returns>
    Task<int> GetMaxSetNumberAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId);
    
    /// <summary>
    /// Checks if a set configuration exists for a specific exercise and set number
    /// </summary>
    /// <param name="workoutTemplateExerciseId">The workout template exercise ID</param>
    /// <param name="setNumber">The set number</param>
    /// <returns>True if the set configuration exists</returns>
    Task<bool> ExistsAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId, int setNumber);
    
    /// <summary>
    /// Adds a new set configuration
    /// </summary>
    /// <param name="setConfiguration">The set configuration to add</param>
    /// <returns>The added set configuration</returns>
    Task<SetConfiguration> AddAsync(SetConfiguration setConfiguration);
    
    /// <summary>
    /// Adds multiple set configurations in bulk
    /// </summary>
    /// <param name="setConfigurations">The set configurations to add</param>
    /// <returns>The added set configurations</returns>
    Task<IEnumerable<SetConfiguration>> AddRangeAsync(IEnumerable<SetConfiguration> setConfigurations);
    
    /// <summary>
    /// Updates an existing set configuration
    /// </summary>
    /// <param name="setConfiguration">The set configuration to update</param>
    /// <returns>The updated set configuration</returns>
    Task<SetConfiguration> UpdateAsync(SetConfiguration setConfiguration);
    
    /// <summary>
    /// Updates multiple set configurations in bulk
    /// </summary>
    /// <param name="setConfigurations">The set configurations to update</param>
    /// <returns>Number of updated set configurations</returns>
    Task<int> UpdateRangeAsync(IEnumerable<SetConfiguration> setConfigurations);
    
    /// <summary>
    /// Deletes a set configuration
    /// </summary>
    /// <param name="id">The set configuration ID to delete</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteAsync(SetConfigurationId id);
    
    /// <summary>
    /// Deletes all set configurations for a workout template exercise
    /// </summary>
    /// <param name="workoutTemplateExerciseId">The workout template exercise ID</param>
    /// <returns>Number of deleted set configurations</returns>
    Task<int> DeleteByWorkoutTemplateExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId);
    
    /// <summary>
    /// Deletes all set configurations for a workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <returns>Number of deleted set configurations</returns>
    Task<int> DeleteByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId);
    
    /// <summary>
    /// Reorders set numbers for a workout template exercise
    /// </summary>
    /// <param name="workoutTemplateExerciseId">The workout template exercise ID</param>
    /// <param name="setReorders">Dictionary mapping set configuration IDs to their new set numbers</param>
    /// <returns>True if successful</returns>
    Task<bool> ReorderSetsAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId, Dictionary<SetConfigurationId, int> setReorders);
}