using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Services;

/// <summary>
/// Service interface for managing exercise links (warmup, cooldown, and alternative relationships)
/// </summary>
public interface IExerciseLinkService
{
    /// <summary>
    /// Creates a new link between a source exercise and a target exercise
    /// For Alternative links, also creates the reverse bidirectional relationship automatically
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="createLinkDto">The link creation data</param>
    /// <returns>The created exercise link</returns>
    Task<ExerciseLinkDto> CreateLinkAsync(string exerciseId, CreateExerciseLinkDto createLinkDto);

    /// <summary>
    /// Creates a bidirectional link between exercises (used for Alternative links)
    /// This method ensures both directions of the relationship are created
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="createLinkDto">The link creation data</param>
    /// <returns>The created exercise link</returns>
    Task<ExerciseLinkDto> CreateBidirectionalLinkAsync(string exerciseId, CreateExerciseLinkDto createLinkDto);

    /// <summary>
    /// Gets all links for a specific exercise, optionally filtered by link type
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="linkType">Optional filter by link type ("Warmup", "Cooldown", or "Alternative")</param>
    /// <param name="includeExerciseDetails">Whether to include full exercise data for targets</param>
    /// <param name="includeReverse">Whether to include reverse relationships (for Alternative links)</param>
    /// <returns>Response containing the exercise links</returns>
    Task<ExerciseLinksResponseDto> GetLinksAsync(string exerciseId, string? linkType = null, bool includeExerciseDetails = false, bool includeReverse = false);

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

    /// <summary>
    /// Deletes a bidirectional link between exercises (used for Alternative links)
    /// This method ensures both directions of the relationship are removed
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="linkId">The link ID to delete</param>
    /// <param name="deleteReverse">Whether to delete the reverse relationship as well (default: true for Alternative links)</param>
    Task DeleteBidirectionalLinkAsync(string exerciseId, string linkId, bool deleteReverse = true);
}