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
    Task<ServiceResult<ExerciseLinkDto>> CreateAsync(ExerciseLink exerciseLink);
    
    /// <summary>
    /// Updates an existing exercise link
    /// </summary>
    Task<ServiceResult<ExerciseLinkDto>> UpdateAsync(ExerciseLink exerciseLink);
    
    /// <summary>
    /// Deletes an exercise link (soft delete)
    /// </summary>
    Task<ServiceResult<BooleanResultDto>> DeleteAsync(ExerciseLinkId id);
    
    /// <summary>
    /// Creates bidirectional links in a single transaction
    /// </summary>
    /// <param name="primaryLink">The primary link to create</param>
    /// <param name="reverseLink">Optional reverse link to create atomically</param>
    /// <returns>The created primary link DTO</returns>
    Task<ServiceResult<ExerciseLinkDto>> CreateBidirectionalAsync(
        ExerciseLink primaryLink, 
        ExerciseLink? reverseLink = null);
}