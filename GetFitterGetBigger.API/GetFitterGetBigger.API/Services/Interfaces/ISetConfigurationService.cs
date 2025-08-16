using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.SetConfigurations;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for managing set configurations
/// </summary>
public interface ISetConfigurationService
{
    /// <summary>
    /// Gets a set configuration by ID
    /// </summary>
    /// <param name="id">The set configuration ID</param>
    /// <returns>ServiceResult with SetConfigurationDto or failure</returns>
    Task<ServiceResult<SetConfigurationDto>> GetByIdAsync(SetConfigurationId id);
    
    /// <summary>
    /// Gets all set configurations for a workout template exercise
    /// </summary>
    /// <param name="workoutTemplateExerciseId">The workout template exercise ID</param>
    /// <returns>ServiceResult with list of set configurations ordered by set number</returns>
    Task<ServiceResult<IEnumerable<SetConfigurationDto>>> GetByWorkoutTemplateExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId);
    
    /// <summary>
    /// Gets all set configurations for a workout template
    /// </summary>
    /// <param name="workoutTemplateId">The workout template ID</param>
    /// <returns>ServiceResult with all set configurations for the template</returns>
    Task<ServiceResult<IEnumerable<SetConfigurationDto>>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId);
    
    /// <summary>
    /// Creates a new set configuration
    /// </summary>
    /// <param name="command">The create set configuration command</param>
    /// <returns>ServiceResult with created SetConfigurationDto or failure</returns>
    Task<ServiceResult<SetConfigurationDto>> CreateAsync(CreateSetConfigurationCommand command);
    
    /// <summary>
    /// Creates multiple set configurations in bulk
    /// </summary>
    /// <param name="command">The bulk create command</param>
    /// <returns>ServiceResult with list of created set configurations</returns>
    Task<ServiceResult<IEnumerable<SetConfigurationDto>>> CreateBulkAsync(CreateBulkSetConfigurationsCommand command);
    
    /// <summary>
    /// Updates an existing set configuration
    /// </summary>
    /// <param name="command">The update set configuration command</param>
    /// <returns>ServiceResult with updated SetConfigurationDto or failure</returns>
    Task<ServiceResult<SetConfigurationDto>> UpdateAsync(UpdateSetConfigurationCommand command);
    
    /// <summary>
    /// Updates multiple set configurations in bulk
    /// </summary>
    /// <param name="command">The bulk update command</param>
    /// <returns>ServiceResult with count of updated configurations</returns>
    Task<ServiceResult<int>> UpdateBulkAsync(UpdateBulkSetConfigurationsCommand command);
    
    /// <summary>
    /// Reorders set numbers for a workout template exercise
    /// </summary>
    /// <param name="command">The reorder command with new set number assignments</param>
    /// <returns>ServiceResult with success or failure</returns>
    Task<ServiceResult<BooleanResultDto>> ReorderSetsAsync(ReorderSetConfigurationsCommand command);
    
    /// <summary>
    /// Deletes a set configuration
    /// </summary>
    /// <param name="id">The set configuration ID</param>
    /// <returns>ServiceResult with success or failure</returns>
    Task<ServiceResult<BooleanResultDto>> DeleteAsync(SetConfigurationId id);
    
    /// <summary>
    /// Deletes all set configurations for a workout template exercise
    /// </summary>
    /// <param name="workoutTemplateExerciseId">The workout template exercise ID</param>
    /// <returns>ServiceResult with count of deleted configurations</returns>
    Task<ServiceResult<int>> DeleteByWorkoutTemplateExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId);
    
    /// <summary>
    /// Checks if a set configuration exists
    /// </summary>
    /// <param name="id">The set configuration ID</param>
    /// <returns>A service result containing true if the set configuration exists</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(SetConfigurationId id);
    
    /// <summary>
    /// Checks if a set configuration exists for a specific exercise and set number
    /// </summary>
    /// <param name="workoutTemplateExerciseId">The workout template exercise ID</param>
    /// <param name="setNumber">The set number</param>
    /// <returns>A service result containing true if the set configuration exists</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId, int setNumber);
}