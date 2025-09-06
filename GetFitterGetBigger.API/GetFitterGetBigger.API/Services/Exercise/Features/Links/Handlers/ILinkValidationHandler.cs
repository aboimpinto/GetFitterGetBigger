using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Handlers;

/// <summary>
/// Interface for handling link-specific validation logic for exercise links
/// </summary>
public interface ILinkValidationHandler
{
    /// <summary>
    /// Checks if a unique link already exists
    /// </summary>
    Task<bool> IsLinkUniqueAsync(ExerciseId sourceId, ExerciseId targetId, string linkType);
    
    /// <summary>
    /// Checks if a bidirectional link between two exercises is unique (no forward or reverse link exists)
    /// </summary>
    Task<bool> IsBidirectionalLinkUniqueAsync(ExerciseId source, ExerciseId target, ExerciseLinkType linkType);
    
    /// <summary>
    /// Checks if the maximum number of links has been reached
    /// </summary>
    Task<bool> IsUnderMaximumLinksAsync(ExerciseId sourceId, string linkType);
    
    /// <summary>
    /// Validates if a link exists in the database
    /// </summary>
    Task<bool> DoesLinkExistAsync(ExerciseLinkId id);
    
    /// <summary>
    /// Validates if a link belongs to a specific exercise
    /// </summary>
    Task<bool> DoesLinkBelongToExerciseAsync(ExerciseId exerciseId, ExerciseLinkId linkId);
}