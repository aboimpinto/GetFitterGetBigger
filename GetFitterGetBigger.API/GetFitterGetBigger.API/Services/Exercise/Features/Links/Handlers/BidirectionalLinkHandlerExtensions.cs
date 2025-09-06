using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Handlers;

/// <summary>
/// Extension methods for BidirectionalLinkHandler to promote clean code patterns
/// and eliminate nested if statements following ServiceValidator pattern principles
/// </summary>
public static class BidirectionalLinkHandlerExtensions
{
    /// <summary>
    /// Attempts to find a reverse link of a specific type that points back to the original source
    /// </summary>
    public static async Task<ExerciseLinkDto?> TryFindReverseLinkAsync(
        this IExerciseLinkQueryDataService queryDataService,
        ExerciseId targetExerciseId,
        ExerciseLinkType linkType,
        string originalSourceId,
        ILogger logger)
    {
        var linksResult = await queryDataService.GetBySourceExerciseWithEnumAsync(
            targetExerciseId, 
            linkType);
        
        // Use pattern matching for clean result handling
        var reverseLink = linksResult switch
        {
            { IsSuccess: true, Data: not null } => 
                linksResult.Data.FirstOrDefault(link => link.TargetExerciseId == originalSourceId),
            _ => null
        };
        
        // Push logging down to where the operation occurs
        if (reverseLink != null)
        {
            logger.LogInformation(
                "Found reverse {LinkType} link {LinkId} for WORKOUT link deletion",
                linkType, reverseLink.Id);
        }
        
        return reverseLink;
    }
    
    /// <summary>
    /// Finds the first matching reverse link from a collection of possible link types
    /// </summary>
    public static async Task<ExerciseLinkDto> FindFirstMatchingReverseLinkAsync(
        this IExerciseLinkQueryDataService queryDataService,
        ExerciseId targetExerciseId,
        IEnumerable<ExerciseLinkType> possibleLinkTypes,
        string originalSourceId,
        ILogger logger)
    {
        // Try each link type sequentially until we find a match
        foreach (var linkType in possibleLinkTypes)
        {
            var reverseLink = await queryDataService.TryFindReverseLinkAsync(
                targetExerciseId,
                linkType,
                originalSourceId,
                logger);
            
            if (reverseLink != null)
            {
                return reverseLink;
            }
        }
        
        // No reverse link found - log at the operation level
        logger.LogWarning(
            "No reverse link found for WORKOUT link from {SourceId} to {TargetId}. " +
            "This may indicate data inconsistency.",
            originalSourceId, targetExerciseId);
        
        return ExerciseLinkDto.Empty;
    }
    
    /// <summary>
    /// Creates a ServiceResult from a found link or returns Empty success
    /// </summary>
    public static ServiceResult<ExerciseLinkDto> ToServiceResult(this ExerciseLinkDto? link)
    {
        return ServiceResult<ExerciseLinkDto>.Success(link ?? ExerciseLinkDto.Empty);
    }
    
    /// <summary>
    /// Gets the possible reverse link types for a WORKOUT link
    /// </summary>
    public static IEnumerable<ExerciseLinkType> GetPossibleReverseTypes(this ExerciseLinkType linkType)
    {
        return linkType switch
        {
            ExerciseLinkType.WORKOUT => new[] { ExerciseLinkType.WARMUP, ExerciseLinkType.COOLDOWN },
            _ => Array.Empty<ExerciseLinkType>()
        };
    }
    
    /// <summary>
    /// Deletes a link if it exists and logs the operation
    /// </summary>
    public static async Task<ServiceResult<BooleanResultDto>> DeleteIfExistsAsync(
        this IExerciseLinkCommandDataService commandDataService,
        ServiceResult<ExerciseLinkDto> linkResult,
        ILogger logger)
    {
        // Pattern matching for clean deletion logic
        return linkResult switch
        {
            { IsSuccess: true, Data: { IsEmpty: false } } => 
                await DeleteAndLogAsync(commandDataService, linkResult.Data, logger),
            _ => ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true))
        };
    }
    
    private static async Task<ServiceResult<BooleanResultDto>> DeleteAndLogAsync(
        IExerciseLinkCommandDataService commandDataService,
        ExerciseLinkDto link,
        ILogger logger)
    {
        var linkId = ExerciseLinkId.ParseOrEmpty(link.Id);
        var result = await commandDataService.DeleteAsync(linkId);
        
        // Log at the operation level
        if (result.IsSuccess)
        {
            logger.LogInformation("Deleted reverse link: {ReverseId}", link.Id);
        }
        
        return result;
    }
}