using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Interfaces;

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
    /// Gets a paginated list of workout templates for a specific creator
    /// </summary>
    /// <param name="creatorId">The ID of the creator/trainer</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="categoryFilter">Optional category filter</param>
    /// <param name="difficultyFilter">Optional difficulty filter</param>
    /// <returns>Paginated response containing workout templates</returns>
    Task<PagedResponse<WorkoutTemplateDto>> GetPagedByCreatorAsync(
        UserId creatorId, 
        int pageNumber = 1, 
        int pageSize = 20,
        WorkoutCategoryId? categoryFilter = null,
        DifficultyLevelId? difficultyFilter = null);

    /// <summary>
    /// Gets all active workout templates for a specific creator
    /// </summary>
    /// <param name="creatorId">The ID of the creator/trainer</param>
    /// <returns>Service result containing collection of workout templates</returns>
    Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetAllActiveByCreatorAsync(UserId creatorId);

    /// <summary>
    /// Searches workout templates by name pattern
    /// </summary>
    /// <param name="namePattern">The pattern to search for in template names</param>
    /// <param name="creatorFilter">Optional creator filter</param>
    /// <returns>Service result containing matching workout templates</returns>
    Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetByNamePatternAsync(
        string namePattern, 
        UserId? creatorFilter = null);

    /// <summary>
    /// Gets workout templates by workout category
    /// </summary>
    /// <param name="categoryId">The workout category ID</param>
    /// <param name="creatorFilter">Optional creator filter</param>
    /// <param name="includeInactive">Whether to include archived templates</param>
    /// <returns>Service result containing workout templates in the specified category</returns>
    Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetByCategoryAsync(
        WorkoutCategoryId categoryId, 
        UserId? creatorFilter = null,
        bool includeInactive = false);

    /// <summary>
    /// Gets workout templates by workout objective
    /// </summary>
    /// <param name="objectiveId">The workout objective ID</param>
    /// <param name="creatorFilter">Optional creator filter</param>
    /// <param name="includeInactive">Whether to include archived templates</param>
    /// <returns>Service result containing workout templates with the specified objective</returns>
    Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetByObjectiveAsync(
        WorkoutObjectiveId objectiveId,
        UserId? creatorFilter = null,
        bool includeInactive = false);

    /// <summary>
    /// Gets workout templates by difficulty level
    /// </summary>
    /// <param name="difficultyId">The difficulty level ID</param>
    /// <param name="creatorFilter">Optional creator filter</param>
    /// <param name="includeInactive">Whether to include archived templates</param>
    /// <returns>Service result containing workout templates with the specified difficulty</returns>
    Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetByDifficultyAsync(
        DifficultyLevelId difficultyId,
        UserId? creatorFilter = null,
        bool includeInactive = false);

    /// <summary>
    /// Gets workout templates that contain a specific exercise
    /// </summary>
    /// <param name="exerciseId">The exercise ID</param>
    /// <param name="creatorFilter">Optional creator filter</param>
    /// <param name="includeInactive">Whether to include archived templates</param>
    /// <returns>Service result containing workout templates containing the specified exercise</returns>
    Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetByExerciseAsync(
        ExerciseId exerciseId,
        UserId? creatorFilter = null,
        bool includeInactive = false);

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
    /// <param name="creatorId">The ID of the creator for the duplicated template</param>
    /// <returns>Service result containing the duplicated workout template</returns>
    Task<ServiceResult<WorkoutTemplateDto>> DuplicateAsync(WorkoutTemplateId id, string newName, UserId creatorId);

    /// <summary>
    /// Soft deletes a workout template (sets state to ARCHIVED)
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult<bool>> SoftDeleteAsync(WorkoutTemplateId id);

    /// <summary>
    /// Permanently deletes a workout template (only allowed if no execution logs exist)
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>Service result indicating success or failure</returns>
    Task<ServiceResult<bool>> DeleteAsync(WorkoutTemplateId id);

    /// <summary>
    /// Checks if a workout template exists by ID
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>True if the template exists, false otherwise</returns>
    Task<bool> ExistsAsync(WorkoutTemplateId id);

    /// <summary>
    /// Checks if a workout template exists by ID (string overload)
    /// </summary>
    /// <param name="id">The workout template ID as string</param>
    /// <returns>True if the template exists, false otherwise</returns>
    Task<bool> ExistsAsync(string id);

    /// <summary>
    /// Checks if a workout template with the given name exists for a creator
    /// </summary>
    /// <param name="name">The template name</param>
    /// <param name="creatorId">The creator ID</param>
    /// <returns>True if a template with the name exists for the creator</returns>
    Task<bool> ExistsByNameAsync(string name, UserId creatorId);

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