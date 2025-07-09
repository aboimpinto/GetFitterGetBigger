using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for ExerciseLink data operations
/// </summary>
public interface IExerciseLinkRepository : IRepository
{
    /// <summary>
    /// Gets all links for a source exercise, optionally filtered by link type
    /// </summary>
    /// <param name="sourceExerciseId">The ID of the source exercise</param>
    /// <param name="linkType">Optional filter by link type (Warmup or Cooldown)</param>
    /// <returns>A collection of exercise links</returns>
    Task<IEnumerable<ExerciseLink>> GetBySourceExerciseAsync(ExerciseId sourceExerciseId, string? linkType = null);
    
    /// <summary>
    /// Gets all links where the specified exercise is the target
    /// </summary>
    /// <param name="targetExerciseId">The ID of the target exercise</param>
    /// <returns>A collection of exercise links</returns>
    Task<IEnumerable<ExerciseLink>> GetByTargetExerciseAsync(ExerciseId targetExerciseId);
    
    /// <summary>
    /// Checks if a link already exists between two exercises with a specific type
    /// </summary>
    /// <param name="sourceExerciseId">The source exercise ID</param>
    /// <param name="targetExerciseId">The target exercise ID</param>
    /// <param name="linkType">The type of link (Warmup or Cooldown)</param>
    /// <returns>True if the link exists, false otherwise</returns>
    Task<bool> ExistsAsync(ExerciseId sourceExerciseId, ExerciseId targetExerciseId, string linkType);
    
    /// <summary>
    /// Gets the most commonly used links across all exercises
    /// </summary>
    /// <param name="count">The number of links to retrieve</param>
    /// <returns>A collection of the most used exercise links with usage count</returns>
    Task<IEnumerable<(ExerciseLink link, int usageCount)>> GetMostUsedLinksAsync(int count);
    
    /// <summary>
    /// Gets a specific exercise link by ID
    /// </summary>
    /// <param name="id">The ID of the exercise link</param>
    /// <returns>The exercise link if found, null otherwise</returns>
    Task<ExerciseLink?> GetByIdAsync(ExerciseLinkId id);
    
    /// <summary>
    /// Adds a new exercise link
    /// </summary>
    /// <param name="exerciseLink">The exercise link to add</param>
    /// <returns>The added exercise link</returns>
    Task<ExerciseLink> AddAsync(ExerciseLink exerciseLink);
    
    /// <summary>
    /// Updates an existing exercise link
    /// </summary>
    /// <param name="exerciseLink">The exercise link with updated data</param>
    /// <returns>The updated exercise link</returns>
    Task<ExerciseLink> UpdateAsync(ExerciseLink exerciseLink);
    
    /// <summary>
    /// Deletes an exercise link (soft delete)
    /// </summary>
    /// <param name="id">The ID of the exercise link to delete</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteAsync(ExerciseLinkId id);
}