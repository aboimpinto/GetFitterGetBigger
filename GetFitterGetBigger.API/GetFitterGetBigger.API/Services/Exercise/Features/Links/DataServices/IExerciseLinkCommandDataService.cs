using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;

/// <summary>
/// Data service interface for ExerciseLink write operations.
/// Encapsulates all database modifications and entity-to-DTO mapping.
/// </summary>
public interface IExerciseLinkCommandDataService
{
    /// <summary>
    /// Creates a new exercise link
    /// </summary>
    /// <param name="linkDto">The DTO containing the link data to create</param>
    Task<ServiceResult<ExerciseLinkDto>> CreateAsync(ExerciseLinkDto linkDto);
    
    /// <summary>
    /// Updates an existing exercise link using a transformation function
    /// </summary>
    /// <param name="linkId">The ID of the link to update</param>
    /// <param name="updateAction">Function to apply updates to the entity</param>
    Task<ServiceResult<ExerciseLinkDto>> UpdateAsync(
        ExerciseLinkId linkId,
        Func<ExerciseLink, ExerciseLink> updateAction);
    
    /// <summary>
    /// Deletes an exercise link (soft delete)
    /// </summary>
    Task<ServiceResult<BooleanResultDto>> DeleteAsync(ExerciseLinkId id);
    
    /// <summary>
    /// Creates bidirectional links in a single transaction
    /// </summary>
    /// <param name="primaryLinkDto">The primary link DTO to create</param>
    /// <param name="reverseLinkDto">Optional reverse link DTO to create atomically</param>
    /// <returns>The created primary link DTO</returns>
    Task<ServiceResult<ExerciseLinkDto>> CreateBidirectionalAsync(
        ExerciseLinkDto primaryLinkDto, 
        ExerciseLinkDto? reverseLinkDto = null);
}