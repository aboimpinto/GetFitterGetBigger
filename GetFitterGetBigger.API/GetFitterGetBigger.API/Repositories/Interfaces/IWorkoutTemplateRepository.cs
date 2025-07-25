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
    /// Gets an IQueryable of workout templates with necessary includes for querying and filtering
    /// </summary>
    /// <returns>IQueryable of workout templates with includes applied</returns>
    IQueryable<WorkoutTemplate> GetWorkoutTemplatesQueryable();

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
    /// Checks if a workout template exists by ID
    /// </summary>
    /// <param name="id">The workout template ID</param>
    /// <returns>True if the template exists, false otherwise</returns>
    Task<bool> ExistsAsync(WorkoutTemplateId id);

    /// <summary>
    /// Checks if a workout template with the given name exists
    /// </summary>
    /// <param name="name">The template name</param>
    /// <param name="excludeTemplateId">Optional template ID to exclude from the check</param>
    /// <returns>True if a template with the name exists, false otherwise</returns>
    Task<bool> ExistsByNameAsync(
        string name, 
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