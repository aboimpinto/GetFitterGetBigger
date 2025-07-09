using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services;

/// <summary>
/// Service interface for managing exercise links (warmup and cooldown relationships)
/// </summary>
public interface IExerciseLinkService
{
    /// <summary>
    /// Creates a new link between a source exercise and a target exercise
    /// </summary>
    /// <param name="exerciseId">The source exercise ID (must be a Workout type)</param>
    /// <param name="createLinkDto">The link creation data</param>
    /// <returns>The created exercise link</returns>
    Task<ExerciseLinkDto> CreateLinkAsync(string exerciseId, CreateExerciseLinkDto createLinkDto);

    /// <summary>
    /// Gets all links for a specific exercise, optionally filtered by link type
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="linkType">Optional filter by link type ("Warmup" or "Cooldown")</param>
    /// <param name="includeExerciseDetails">Whether to include full exercise data for targets</param>
    /// <returns>Response containing the exercise links</returns>
    Task<ExerciseLinksResponseDto> GetLinksAsync(string exerciseId, string? linkType = null, bool includeExerciseDetails = false);

    /// <summary>
    /// Gets suggested exercises that could be linked based on common usage patterns
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="count">Number of suggestions to return (default: 5, max: 20)</param>
    /// <returns>List of suggested exercise links</returns>
    Task<List<ExerciseLinkDto>> GetSuggestedLinksAsync(string exerciseId, int count = 5);

    /// <summary>
    /// Updates an existing exercise link's display order and active status
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="linkId">The link ID to update</param>
    /// <param name="updateLinkDto">The link update data</param>
    /// <returns>The updated exercise link</returns>
    Task<ExerciseLinkDto> UpdateLinkAsync(string exerciseId, string linkId, UpdateExerciseLinkDto updateLinkDto);

    /// <summary>
    /// Soft deletes an exercise link (marks it as inactive)
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="linkId">The link ID to delete</param>
    Task DeleteLinkAsync(string exerciseId, string linkId);
}