using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate;

/// <summary>
/// Service interface for WorkoutTemplate operations
/// </summary>
public interface IWorkoutTemplateService
{
    /// <summary>
    /// Gets a workout template by its ID with full details including exercises and configurations
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>Service result containing the workout template with all related data</returns>
    Task<ServiceResult<WorkoutTemplateDto>> GetByIdAsync(WorkoutTemplateId id);

    /// <summary>
    /// Searches workout templates with multiple filter criteria
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="namePattern">Name pattern to search (empty string for no filter)</param>
    /// <param name="categoryId">Category filter (Empty for no filter)</param>
    /// <param name="objectiveId">Objective filter (Empty for no filter)</param>
    /// <param name="difficultyId">Difficulty filter (Empty for no filter)</param>
    /// <param name="stateId">State filter (Empty for no filter)</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortOrder">Sort order (asc/desc)</param>
    /// <returns>Service result containing paginated workout templates</returns>
    Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> SearchAsync(
        int page,
        int pageSize,
        string namePattern,
        WorkoutCategoryId categoryId,
        WorkoutObjectiveId objectiveId,
        DifficultyLevelId difficultyId,
        WorkoutStateId stateId,
        string sortBy,
        string sortOrder);


    /// <summary>
    /// Creates a new workout template
    /// </summary>
    /// <param name="command">The create workout template command</param>
    /// <returns>Service result containing the created workout template</returns>
    Task<ServiceResult<WorkoutTemplateDto>> CreateAsync(CreateWorkoutTemplateCommand command);

    /// <summary>
    /// Updates an existing workout template
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <param name="command">The update workout template command</param>
    /// <returns>Service result containing the updated workout template</returns>
    Task<ServiceResult<WorkoutTemplateDto>> UpdateAsync(WorkoutTemplateId id, UpdateWorkoutTemplateCommand command);

    /// <summary>
    /// Changes the state of a workout template (DRAFT → PRODUCTION → ARCHIVED)
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <param name="newStateId">The new workout state ID</param>
    /// <returns>Service result containing the updated workout template</returns>
    Task<ServiceResult<WorkoutTemplateDto>> ChangeStateAsync(WorkoutTemplateId id, WorkoutStateId newStateId);

    /// <summary>
    /// Duplicates an existing workout template
    /// </summary>
    /// <param name="id">The workout template ID to duplicate</param>
    /// <param name="newName">The name for the new template</param>
    /// <returns>Service result containing the duplicated workout template</returns>
    Task<ServiceResult<WorkoutTemplateDto>> DuplicateAsync(WorkoutTemplateId id, string newName);

    /// <summary>
    /// Soft deletes a workout template (sets state to ARCHIVED)
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult<BooleanResultDto>> SoftDeleteAsync(WorkoutTemplateId id);

    /// <summary>
    /// Permanently deletes a workout template (only allowed if no execution logs exist)
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult<BooleanResultDto>> DeleteAsync(WorkoutTemplateId id);

    /// <summary>
    /// Checks if a workout template exists by ID
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>A service result containing true if the template exists, false otherwise</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutTemplateId id);


    /// <summary>
    /// Checks if a workout template with the given name exists
    /// </summary>
    /// <param name="name">The template name</param>
    /// <returns>True if a template with the name exists</returns>
    Task<ServiceResult<BooleanResultDto>> ExistsByNameAsync(string name);

    /// <summary>
    /// Gets suggested exercises for a workout template based on category and existing exercises
    /// </summary>
    /// <param name="categoryId">The workout category ID</param>
    /// <param name="existingExerciseIds">Already included exercise IDs</param>
    /// <param name="maxSuggestions">Maximum number of suggestions to return</param>
    /// <returns>Service result containing suggested exercises</returns>
    Task<ServiceResult<IEnumerable<ExerciseDto>>> GetSuggestedExercisesAsync(
        WorkoutCategoryId categoryId,
        IEnumerable<ExerciseId> existingExerciseIds,
        int maxSuggestions = 10);

    /// <summary>
    /// Gets the aggregated equipment required for a workout template
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>Service result containing required equipment</returns>
    Task<ServiceResult<IEnumerable<EquipmentDto>>> GetRequiredEquipmentAsync(WorkoutTemplateId id);
}