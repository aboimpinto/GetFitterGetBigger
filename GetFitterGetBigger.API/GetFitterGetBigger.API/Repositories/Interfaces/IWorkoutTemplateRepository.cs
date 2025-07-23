using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for WorkoutTemplate entity operations
/// </summary>
public interface IWorkoutTemplateRepository : IRepository
{
    /// <summary>
    /// Gets a workout template by ID
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>The workout template, or WorkoutTemplate.Empty if not found</returns>
    Task<WorkoutTemplate> GetByIdAsync(WorkoutTemplateId id);

    /// <summary>
    /// Gets a workout template by ID with all related data
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>The workout template with all navigation properties loaded, or WorkoutTemplate.Empty if not found</returns>
    Task<WorkoutTemplate> GetByIdWithDetailsAsync(WorkoutTemplateId id);

    /// <summary>
    /// Gets a paginated list of workout templates for a specific creator
    /// </summary>
    /// <param name="creatorId">The ID of the creator</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="includeInactive">Whether to include inactive templates</param>
    /// <returns>Tuple containing the templates and total count</returns>
    Task<(IEnumerable<WorkoutTemplate> templates, int totalCount)> GetPagedByCreatorAsync(
        UserId creatorId, 
        int pageNumber, 
        int pageSize, 
        bool includeInactive = false);

    /// <summary>
    /// Gets all active workout templates for a specific creator
    /// </summary>
    /// <param name="creatorId">The ID of the creator</param>
    /// <returns>Collection of active workout templates</returns>
    Task<IEnumerable<WorkoutTemplate>> GetAllActiveByCreatorAsync(UserId creatorId);

    /// <summary>
    /// Gets workout templates by name pattern
    /// </summary>
    /// <param name="namePattern">The name pattern to search for</param>
    /// <param name="creatorId">Optional creator ID to filter by</param>
    /// <param name="includeInactive">Whether to include inactive templates</param>
    /// <returns>Collection of matching workout templates</returns>
    Task<IEnumerable<WorkoutTemplate>> GetByNamePatternAsync(
        string namePattern, 
        UserId creatorId = default, 
        bool includeInactive = false);

    /// <summary>
    /// Gets workout templates by category
    /// </summary>
    /// <param name="categoryId">The workout category ID</param>
    /// <param name="creatorId">Optional creator ID to filter by</param>
    /// <param name="includeInactive">Whether to include inactive templates</param>
    /// <returns>Collection of workout templates in the specified category</returns>
    Task<IEnumerable<WorkoutTemplate>> GetByCategoryAsync(
        WorkoutCategoryId categoryId, 
        UserId creatorId = default, 
        bool includeInactive = false);

    /// <summary>
    /// Gets workout templates by objective
    /// </summary>
    /// <param name="objectiveId">The workout objective ID</param>
    /// <param name="creatorId">Optional creator ID to filter by</param>
    /// <param name="includeInactive">Whether to include inactive templates</param>
    /// <returns>Collection of workout templates with the specified objective</returns>
    Task<IEnumerable<WorkoutTemplate>> GetByObjectiveAsync(
        WorkoutObjectiveId objectiveId, 
        UserId creatorId = default, 
        bool includeInactive = false);

    /// <summary>
    /// Gets workout templates by difficulty level
    /// </summary>
    /// <param name="difficultyLevelId">The difficulty level ID</param>
    /// <param name="creatorId">Optional creator ID to filter by</param>
    /// <param name="includeInactive">Whether to include inactive templates</param>
    /// <returns>Collection of workout templates with the specified difficulty</returns>
    Task<IEnumerable<WorkoutTemplate>> GetByDifficultyAsync(
        DifficultyLevelId difficultyLevelId, 
        UserId creatorId = default, 
        bool includeInactive = false);

    /// <summary>
    /// Gets workout templates that contain a specific exercise
    /// </summary>
    /// <param name="exerciseId">The exercise ID</param>
    /// <param name="creatorId">Optional creator ID to filter by</param>
    /// <param name="includeInactive">Whether to include inactive templates</param>
    /// <returns>Collection of workout templates containing the exercise</returns>
    Task<IEnumerable<WorkoutTemplate>> GetByExerciseAsync(
        ExerciseId exerciseId, 
        UserId creatorId = default, 
        bool includeInactive = false);

    /// <summary>
    /// Checks if a workout template exists by ID
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>True if the template exists, false otherwise</returns>
    Task<bool> ExistsAsync(WorkoutTemplateId id);

    /// <summary>
    /// Checks if a workout template with the given name exists for a creator
    /// </summary>
    /// <param name="name">The template name</param>
    /// <param name="creatorId">The creator ID</param>
    /// <param name="excludeTemplateId">Optional template ID to exclude from the check</param>
    /// <returns>True if a template with the name exists, false otherwise</returns>
    Task<bool> ExistsByNameAsync(
        string name, 
        UserId creatorId, 
        WorkoutTemplateId excludeTemplateId = default);

    /// <summary>
    /// Adds a new workout template
    /// </summary>
    /// <param name="workoutTemplate">The workout template to add</param>
    /// <returns>The added workout template</returns>
    Task<WorkoutTemplate> AddAsync(WorkoutTemplate workoutTemplate);

    /// <summary>
    /// Updates an existing workout template
    /// </summary>
    /// <param name="workoutTemplate">The workout template to update</param>
    /// <returns>The updated workout template</returns>
    Task<WorkoutTemplate> UpdateAsync(WorkoutTemplate workoutTemplate);

    /// <summary>
    /// Soft deletes a workout template (marks as inactive)
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>True if the template was deleted, false if not found</returns>
    Task<bool> SoftDeleteAsync(WorkoutTemplateId id);

    /// <summary>
    /// Permanently deletes a workout template
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>True if the template was deleted, false if not found</returns>
    Task<bool> DeleteAsync(WorkoutTemplateId id);
}