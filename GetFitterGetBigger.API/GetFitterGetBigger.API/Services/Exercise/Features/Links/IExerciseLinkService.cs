using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links;

/// <summary>
/// Service interface for managing exercise links
/// </summary>
public interface IExerciseLinkService
{
    /// <summary>
    /// Creates a new link between exercises
    /// </summary>
    /// <param name="command">The link creation command</param>
    /// <returns>The created exercise link</returns>
    Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(CreateExerciseLinkCommand command);
    
    /// <summary>
    /// Gets all links for a specific exercise
    /// </summary>
    /// <param name="command">The query command</param>
    /// <returns>The exercise links response</returns>
    Task<ServiceResult<ExerciseLinksResponseDto>> GetLinksAsync(GetExerciseLinksCommand command);
    
    /// <summary>
    /// Updates an existing exercise link
    /// </summary>
    /// <param name="command">The update command</param>
    /// <returns>The updated exercise link</returns>
    Task<ServiceResult<ExerciseLinkDto>> UpdateLinkAsync(UpdateExerciseLinkCommand command);
    
    /// <summary>
    /// Deletes an exercise link
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="linkId">The link ID to delete</param>
    /// <returns>True if deleted successfully</returns>
    Task<ServiceResult<BooleanResultDto>> DeleteLinkAsync(string exerciseId, string linkId);
    
    /// <summary>
    /// Gets suggested links based on common usage patterns
    /// </summary>
    /// <param name="exerciseId">The exercise ID</param>
    /// <param name="count">Number of suggestions to return</param>
    /// <returns>List of suggested exercise links</returns>
    Task<ServiceResult<List<ExerciseLinkDto>>> GetSuggestedLinksAsync(string exerciseId, int count = 5);
}