using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links.Handlers;

/// <summary>
/// Handles link-specific validation logic for exercise links
/// </summary>
public class LinkValidationHandler(
    IExerciseLinkQueryDataService queryDataService,
    ILogger<LinkValidationHandler> logger) : ILinkValidationHandler
{
    private const int MaximumLinksPerType = 10;
    
    /// <summary>
    /// Validates if a link type is valid (enum or legacy string)
    /// </summary>
    public static bool IsValidLinkType(string linkType)
    {
        return linkType == "Warmup" || linkType == "Cooldown" || 
               Enum.TryParse<ExerciseLinkType>(linkType, out _);
    }
    
    /// <summary>
    /// Validates if a link type enum is valid
    /// </summary>
    public static bool IsValidLinkType(ExerciseLinkType linkType)
    {
        return Enum.IsDefined(typeof(ExerciseLinkType), linkType);
    }
    
    /// <summary>
    /// Checks if a link between two exercises is unique
    /// </summary>
    public async Task<bool> IsLinkUniqueAsync(ExerciseId source, ExerciseId target, string linkType)
    {
        var result = await queryDataService.ExistsAsync(source, target, linkType);
        return IsLinkUniqueInternal(result);
    }
    
    /// <summary>
    /// Checks if a bidirectional link between two exercises is unique (no forward or reverse link exists)
    /// </summary>
    public async Task<bool> IsBidirectionalLinkUniqueAsync(ExerciseId source, ExerciseId target, ExerciseLinkType linkType)
    {
        // Use the dedicated bidirectional exists method which properly handles all bidirectional logic
        var existsResult = await queryDataService.ExistsBidirectionalAsync(source, target, linkType);
        
        // Return true if unique (doesn't exist)
        return IsLinkUniqueInternal(existsResult);
    }
    
    /// <summary>
    /// Checks if a source exercise hasn't reached the maximum number of links for a type
    /// </summary>
    public async Task<bool> IsUnderMaximumLinksAsync(ExerciseId source, string linkType)
    {
        var countResult = await queryDataService.GetLinkCountAsync(source, linkType);
        
        var isUnderLimit = countResult.IsSuccess && countResult.Data < MaximumLinksPerType;
        
        if (!isUnderLimit && countResult.IsSuccess)
        {
            logger.LogWarning(
                "Exercise {ExerciseId} has reached maximum {LinkType} links: {Count}/{Max}",
                source, linkType, countResult.Data, MaximumLinksPerType);
        }
        
        return isUnderLimit;
    }
    
    /// <summary>
    /// Validates if a link exists in the database
    /// </summary>
    public async Task<bool> DoesLinkExistAsync(ExerciseLinkId id)
    {
        var result = await queryDataService.GetByIdAsync(id);
        
        return result.IsSuccess && !result.Data.IsEmpty;
    }
    
    /// <summary>
    /// Validates if a link belongs to a specific exercise
    /// </summary>
    public async Task<bool> DoesLinkBelongToExerciseAsync(ExerciseId exerciseId, ExerciseLinkId linkId)
    {
        var linkResult = await queryDataService.GetByIdAsync(linkId);
        
        return linkResult.IsSuccess 
            ? linkResult.Data.SourceExerciseId == exerciseId.ToString()
            : false;
    }
    
    private static bool IsLinkUniqueInternal(ServiceResult<BooleanResultDto> existsResult)
    {
        return existsResult.IsSuccess ? !existsResult.Data.Value : false;
    }
}