using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Handlers;

/// <summary>
/// Interface for handling bidirectional link creation and deletion for exercise links
/// </summary>
public interface IBidirectionalLinkHandler
{
    /// <summary>
    /// Creates bidirectional links between exercises
    /// </summary>
    Task<ServiceResult<ExerciseLinkDto>> CreateBidirectionalLinkAsync(
        ExerciseId sourceId,
        ExerciseId targetId,
        ExerciseLinkType linkType);
    
    /// <summary>
    /// Deletes a link and optionally its reverse link
    /// </summary>
    Task<ServiceResult<BooleanResultDto>> DeleteBidirectionalLinkAsync(
        ExerciseLinkId linkId, 
        bool deleteReverse);
}