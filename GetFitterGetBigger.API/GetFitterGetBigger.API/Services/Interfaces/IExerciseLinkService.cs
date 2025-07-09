using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Interfaces;

/// <summary>
/// Service interface for managing exercise links
/// </summary>
public interface IExerciseLinkService
{
    /// <summary>
    /// Creates a new link between exercises
    /// </summary>
    /// <param name="sourceExerciseId">The source exercise ID (must be a Workout type)</param>
    /// <param name="dto">The link creation data</param>
    /// <returns>The created exercise link</returns>
    Task<ExerciseLinkDto> CreateLinkAsync(string sourceExerciseId, CreateExerciseLinkDto dto);
    
    /// <summary>
    /// Gets all links for a specific exercise
    /// </summary>
    /// <param name="exerciseId">The exercise ID</param>
    /// <param name="linkType">Optional filter by link type</param>
    /// <param name="includeExerciseDetails">Whether to include full exercise details</param>
    /// <returns>The exercise links response</returns>
    Task<ExerciseLinksResponseDto> GetLinksAsync(string exerciseId, string? linkType = null, bool includeExerciseDetails = false);
    
    /// <summary>
    /// Updates an existing exercise link
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="linkId">The link ID to update</param>
    /// <param name="dto">The update data</param>
    /// <returns>The updated exercise link</returns>
    Task<ExerciseLinkDto> UpdateLinkAsync(string exerciseId, string linkId, UpdateExerciseLinkDto dto);
    
    /// <summary>
    /// Deletes an exercise link
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="linkId">The link ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteLinkAsync(string exerciseId, string linkId);
    
    /// <summary>
    /// Gets suggested links based on common usage patterns
    /// </summary>
    /// <param name="exerciseId">The exercise ID</param>
    /// <param name="count">Number of suggestions to return</param>
    /// <returns>List of suggested exercise links</returns>
    Task<List<ExerciseLinkDto>> GetSuggestedLinksAsync(string exerciseId, int count = 5);
    
    /// <summary>
    /// Validates that no circular reference would be created
    /// </summary>
    /// <param name="sourceId">The source exercise ID</param>
    /// <param name="targetId">The target exercise ID</param>
    /// <returns>True if no circular reference exists</returns>
    Task<bool> ValidateNoCircularReferenceAsync(ExerciseId sourceId, ExerciseId targetId);
}