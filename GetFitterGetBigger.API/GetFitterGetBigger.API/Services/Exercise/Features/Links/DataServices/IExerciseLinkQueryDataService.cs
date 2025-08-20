using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;

/// <summary>
/// Data service interface for ExerciseLink read operations.
/// Encapsulates all database queries and entity-to-DTO mapping.
/// </summary>
public interface IExerciseLinkQueryDataService
{
    /// <summary>
    /// Gets all links for a specific exercise
    /// </summary>
    Task<ServiceResult<ExerciseLinksResponseDto>> GetLinksAsync(GetExerciseLinksCommand command);
    
    /// <summary>
    /// Gets a specific exercise link by ID
    /// </summary>
    Task<ServiceResult<ExerciseLinkDto>> GetByIdAsync(ExerciseLinkId id);
    
    /// <summary>
    /// Gets a specific exercise link entity by ID for updates
    /// </summary>
    Task<ServiceResult<Models.Entities.ExerciseLink>> GetEntityByIdAsync(ExerciseLinkId id);
    
    /// <summary>
    /// Checks if a link already exists between two exercises with a specific type
    /// </summary>
    Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseId sourceId, ExerciseId targetId, string linkType);
    
    /// <summary>
    /// Gets links by source exercise for circular reference checking
    /// </summary>
    Task<ServiceResult<List<ExerciseLinkDto>>> GetBySourceExerciseAsync(ExerciseId sourceId, string? linkType = null);
    
    /// <summary>
    /// Gets the count of links for a specific exercise and type
    /// </summary>
    Task<ServiceResult<int>> GetLinkCountAsync(ExerciseId sourceId, string linkType);
    
    /// <summary>
    /// Gets suggested links based on common usage patterns
    /// </summary>
    Task<ServiceResult<List<ExerciseLinkDto>>> GetSuggestedLinksAsync(string exerciseId, int count);
}