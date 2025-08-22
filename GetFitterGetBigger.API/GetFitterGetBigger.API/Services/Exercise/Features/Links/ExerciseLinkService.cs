using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links;

/// <summary>
/// Service implementation for managing exercise links
/// </summary>
public class ExerciseLinkService(
    IExerciseLinkQueryDataService queryDataService,
    IExerciseLinkCommandDataService commandDataService,
    IExerciseService exerciseService) : IExerciseLinkService
{
    /// <summary>
    /// Creates a new link between exercises using enum LinkType (enhanced functionality)
    /// DisplayOrder is calculated server-side based on existing links
    /// </summary>
    public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(
        string sourceExerciseId,
        string targetExerciseId,
        ExerciseLinkType linkType)
    {
        // Validate parameters
        var sourceId = ExerciseId.ParseOrEmpty(sourceExerciseId);
        var targetId = ExerciseId.ParseOrEmpty(targetExerciseId);
        
        return await ServiceValidate.Build<ExerciseLinkDto>()
            .EnsureNotEmpty(
                sourceId,
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidSourceExerciseId))
            .EnsureNotEmpty(
                targetId,
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidTargetExerciseId))
            .Ensure(
                () => !AreSameExercise(sourceExerciseId, targetExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.CannotLinkToSelf))
            .Ensure(
                () => IsValidLinkType(linkType),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidLinkTypeEnum))
            .Ensure(
                () => linkType != ExerciseLinkType.WORKOUT,
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.WorkoutLinksAutoCreated))
            .EnsureAsync(
                async () => await IsSourceExerciseValidAsync(sourceExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.SourceExerciseNotFound))
            .EnsureAsync(
                async () => await IsTargetExerciseValidAsync(targetExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.TargetExerciseNotFound))
            .EnsureAsync(
                async () => await IsNotRestExerciseAsync(sourceExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.RestExercisesCannotHaveLinks))
            .EnsureAsync(
                async () => await IsNotRestExerciseAsync(targetExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.RestExercisesCannotBeLinked))
            .EnsureAsync(
                async () => await IsLinkTypeCompatibleAsync(sourceId, targetId, linkType),
                ServiceError.ValidationFailed(GetLinkTypeCompatibilityError(linkType)))
            .MatchAsync(
                whenValid: async () => await CreateBidirectionalLinkAsync(sourceExerciseId, targetExerciseId, linkType)
            );
    }

    /// <summary>
    /// Creates a new link between exercises using traditional command (backward compatibility)
    /// </summary>
    public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(CreateExerciseLinkCommand command)
    {
        return await ServiceValidate.Build<ExerciseLinkDto>()
            .EnsureNotEmpty(
                ExerciseId.ParseOrEmpty(command.SourceExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidSourceExerciseId))
            .EnsureNotEmpty(
                ExerciseId.ParseOrEmpty(command.TargetExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidTargetExerciseId))
            .Ensure(
                () => !AreSameExercise(command.SourceExerciseId, command.TargetExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.CannotLinkToSelf))
            .EnsureNotWhiteSpace(
                command.LinkType,
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.LinkTypeRequired))
            .Ensure(
                () => IsValidLinkType(command.LinkType),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidLinkType))
            .Ensure(
                () => command.DisplayOrder >= 0,
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.DisplayOrderMustBeNonNegative))
            .EnsureAsync(
                async () => await IsSourceExerciseValidAsync(command.SourceExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.SourceExerciseNotFound))
            .EnsureAsync(
                async () => await IsSourceExerciseWorkoutTypeAsync(command.SourceExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.SourceMustBeWorkout))
            .EnsureAsync(
                async () => await IsNotRestExerciseAsync(command.SourceExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.RestExercisesCannotHaveLinks))
            .EnsureAsync(
                async () => await IsTargetExerciseValidAsync(command.TargetExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.TargetExerciseNotFound))
            .EnsureAsync(
                async () => await IsTargetExerciseMatchingTypeAsync(command.TargetExerciseId, command.LinkType),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.TargetMustMatchLinkType))
            .EnsureAsync(
                async () => await IsNotRestExerciseAsync(command.TargetExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.RestExercisesCannotBeLinked))
            .EnsureAsync(
                async () => await IsLinkUniqueAsync(command.SourceExerciseId, command.TargetExerciseId, command.LinkType),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.LinkAlreadyExists))
            .EnsureAsync(
                async () => await IsNoCircularReferenceAsync(command.SourceExerciseId, command.TargetExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.CircularReferenceDetected))
            .EnsureAsync(
                async () => await IsUnderMaximumLinksAsync(command.SourceExerciseId, command.LinkType),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.MaximumLinksReached))
            .MatchAsync(
                whenValid: async () => await CreateLinkInternalAsync(command)
            );
    }
    
    /// <summary>
    /// Gets all links for a specific exercise
    /// </summary>
    public async Task<ServiceResult<ExerciseLinksResponseDto>> GetLinksAsync(GetExerciseLinksCommand command)
    {
        return await ServiceValidate.Build<ExerciseLinksResponseDto>()
            .EnsureNotEmpty(
                ExerciseId.ParseOrEmpty(command.ExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidSourceExerciseId))
            .MatchAsync(
                whenValid: async () => await queryDataService.GetLinksAsync(command)
            );
    }
    
    /// <summary>
    /// Updates an existing exercise link
    /// </summary>
    public async Task<ServiceResult<ExerciseLinkDto>> UpdateLinkAsync(UpdateExerciseLinkCommand command)
    {
        return await ServiceValidate.Build<ExerciseLinkDto>()
            .EnsureNotEmpty(
                ExerciseId.ParseOrEmpty(command.ExerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidSourceExerciseId))
            .EnsureNotEmpty(
                ExerciseLinkId.ParseOrEmpty(command.LinkId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidLinkId))
            .Ensure(
                () => command.DisplayOrder >= 0,
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.DisplayOrderMustBeNonNegative))
            .EnsureAsync(
                async () => await DoesLinkExistAsync(command.LinkId),
                ServiceError.NotFound("ExerciseLink", command.LinkId))
            .EnsureAsync(
                async () => await DoesLinkBelongToExerciseAsync(command.ExerciseId, command.LinkId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.LinkDoesNotBelongToExercise))
            .MatchAsync(
                whenValid: async () => await UpdateLinkInternalAsync(command)
            );
    }
    
    /// <summary>
    /// Deletes an exercise link with bidirectional deletion support
    /// </summary>
    public async Task<ServiceResult<BooleanResultDto>> DeleteLinkAsync(string exerciseId, string linkId, bool deleteReverse = true)
    {
        return await ServiceValidate.Build<BooleanResultDto>()
            .EnsureNotEmpty(
                ExerciseId.ParseOrEmpty(exerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidSourceExerciseId))
            .EnsureNotEmpty(
                ExerciseLinkId.ParseOrEmpty(linkId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidLinkId))
            .EnsureAsync(
                async () => await DoesLinkExistAsync(linkId),
                ServiceError.NotFound("ExerciseLink", linkId))
            .EnsureAsync(
                async () => await DoesLinkBelongToExerciseAsync(exerciseId, linkId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.LinkDoesNotBelongToExercise))
            .MatchAsync(
                whenValid: async () => await DeleteBidirectionalLinkAsync(exerciseId, linkId, deleteReverse)
            );
    }
    
    /// <summary>
    /// Gets suggested links based on common usage patterns
    /// </summary>
    public async Task<ServiceResult<List<ExerciseLinkDto>>> GetSuggestedLinksAsync(string exerciseId, int count = 5)
    {
        return await ServiceValidate.Build<List<ExerciseLinkDto>>()
            .EnsureNotEmpty(
                ExerciseId.ParseOrEmpty(exerciseId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidSourceExerciseId))
            .Ensure(
                () => count > 0 && count <= 20,
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.CountMustBeBetween1And20))
            .MatchAsync(
                whenValid: async () => await queryDataService.GetSuggestedLinksAsync(exerciseId, count),
                whenInvalid: errors => ServiceResult<List<ExerciseLinkDto>>.Failure(
                    new List<ExerciseLinkDto>(), 
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Validation failed"))
            );
    }
    private async Task<ServiceResult<BooleanResultDto>> DeleteBidirectionalLinkAsync(
        string exerciseId, 
        string linkId, 
        bool deleteReverse)
    {
        var primaryLinkId = ExerciseLinkId.ParseOrEmpty(linkId);
        
        // Get the primary link first to determine reverse link details
        var primaryLinkResult = await queryDataService.GetByIdAsync(primaryLinkId);
        if (!primaryLinkResult.IsSuccess || primaryLinkResult.Data.IsEmpty)
        {
            return ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Create(false),
                ServiceError.NotFound("ExerciseLink", linkId));
        }
        
        var primaryLink = primaryLinkResult.Data;
        
        // Delete the primary link first
        var deleteResult = await commandDataService.DeleteAsync(primaryLinkId);
        if (!deleteResult.IsSuccess)
        {
            return deleteResult;
        }
        
        // If bidirectional deletion is requested, find and delete the reverse link
        if (deleteReverse)
        {
            var reverseLinkResult = await FindReverseLinkAsync(primaryLink);
            if (reverseLinkResult.IsSuccess && !reverseLinkResult.Data.IsEmpty)
            {
                // Delete the reverse link
                var reverseLinkId = ExerciseLinkId.ParseOrEmpty(reverseLinkResult.Data.Id);
                await commandDataService.DeleteAsync(reverseLinkId);
            }
        }
        
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true));
    }
    
    private async Task<ServiceResult<ExerciseLinkDto>> FindReverseLinkAsync(ExerciseLinkDto originalLink)
    {
        // Determine what the reverse link type should be based on the original link
        var originalLinkType = DetermineActualLinkType(originalLink);
        var reverseLinkType = GetReverseExerciseLinkType(originalLinkType);
        
        if (!reverseLinkType.HasValue)
        {
            // No reverse link expected for this link type
            return ServiceResult<ExerciseLinkDto>.Success(ExerciseLinkDto.Empty);
        }
        
        // Look for the reverse link: where original target becomes source and original source becomes target
        var targetId = ExerciseId.ParseOrEmpty(originalLink.TargetExerciseId);
        var reverseLinksResult = await queryDataService.GetBySourceExerciseWithEnumAsync(targetId, reverseLinkType.Value);
        
        if (reverseLinksResult == null || !reverseLinksResult.IsSuccess)
        {
            return ServiceResult<ExerciseLinkDto>.Success(ExerciseLinkDto.Empty);
        }
        
        // Find the specific reverse link that points back to the original source
        var reverseLink = reverseLinksResult.Data != null
            ? reverseLinksResult.Data.FirstOrDefault(link => link.TargetExerciseId == originalLink.SourceExerciseId)
            : null;
            
        return reverseLink != null
            ? ServiceResult<ExerciseLinkDto>.Success(reverseLink)
            : ServiceResult<ExerciseLinkDto>.Success(ExerciseLinkDto.Empty);
    }
    
    private static ExerciseLinkType DetermineActualLinkType(ExerciseLinkDto link)
    {
        // Use the ActualLinkType logic from the entity
        if (Enum.TryParse<ExerciseLinkType>(link.LinkType, out var enumValue))
        {
            return enumValue;
        }
        
        // Fallback for string-based links
        return link.LinkType switch
        {
            "Warmup" => ExerciseLinkType.WARMUP,
            "Cooldown" => ExerciseLinkType.COOLDOWN,
            _ => ExerciseLinkType.WARMUP // Default fallback
        };
    }
    
    // Private helper methods for validation
    
    private static bool AreSameExercise(string sourceId, string targetId)
    {
        var source = ExerciseId.ParseOrEmpty(sourceId);
        var target = ExerciseId.ParseOrEmpty(targetId);
        return source == target;
    }
    
    private static bool IsValidLinkType(string linkType)
    {
        return linkType == "Warmup" || linkType == "Cooldown" || 
               Enum.TryParse<ExerciseLinkType>(linkType, out _);
    }
    
    private static bool IsValidLinkType(ExerciseLinkType linkType)
    {
        return Enum.IsDefined(typeof(ExerciseLinkType), linkType);
    }
    
    private async Task<bool> IsSourceExerciseValidAsync(string exerciseId)
    {
        var result = await exerciseService.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId));
        return result.IsSuccess && result.Data.IsActive;
    }
    
    private async Task<bool> IsTargetExerciseValidAsync(string exerciseId)
    {
        var result = await exerciseService.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId));
        return result.IsSuccess && result.Data.IsActive;
    }
    
    private async Task<bool> IsSourceExerciseWorkoutTypeAsync(string exerciseId)
    {
        var result = await exerciseService.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId));
        return result.IsSuccess 
            ? result.Data.ExerciseTypes.Any(et => et.Value == "Workout")
            : false;
    }
    
    private async Task<bool> IsTargetExerciseMatchingTypeAsync(string exerciseId, string linkType)
    {
        var result = await exerciseService.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId));
        return result.IsSuccess 
            ? result.Data.ExerciseTypes.Any(et => et.Value == linkType)
            : false;
    }
    
    private async Task<bool> IsNotRestExerciseAsync(string exerciseId)
    {
        var result = await exerciseService.GetByIdAsync(ExerciseId.ParseOrEmpty(exerciseId));
        return result.IsSuccess 
            ? !result.Data.ExerciseTypes.Any(et => et.Value == "Rest")
            : false;
    }
    
    private async Task<bool> IsLinkTypeCompatibleAsync(
        ExerciseId sourceId, 
        ExerciseId targetId, 
        ExerciseLinkType linkType)
    {
        // Get source exercise information
        var sourceResult = await exerciseService.GetByIdAsync(sourceId);
        if (!sourceResult.IsSuccess) return false;
        
        // Get target exercise information
        var targetResult = await exerciseService.GetByIdAsync(targetId);
        if (!targetResult.IsSuccess) return false;
        
        var sourceExercise = sourceResult.Data;
        var targetExercise = targetResult.Data;
        
        // REST exercises cannot have any links
        if (sourceExercise.ExerciseTypes.Any(et => et.Value == "Rest") ||
            targetExercise.ExerciseTypes.Any(et => et.Value == "Rest"))
        {
            return false;
        }
        
        // Implement compatibility matrix from feature requirements
        return linkType switch
        {
            // WARMUP can only link to WORKOUT exercises
            ExerciseLinkType.WARMUP => targetExercise.ExerciseTypes.Any(et => et.Value == "Workout"),
            
            // COOLDOWN can only link to WORKOUT exercises
            ExerciseLinkType.COOLDOWN => targetExercise.ExerciseTypes.Any(et => et.Value == "Workout"),
            
            // ALTERNATIVE can link to any non-REST exercise (already checked above)
            ExerciseLinkType.ALTERNATIVE => true,
            
            // WORKOUT links are only created automatically as reverse links
            ExerciseLinkType.WORKOUT => false,
            
            _ => false
        };
    }
    
    private static string GetLinkTypeCompatibilityError(ExerciseLinkType linkType) =>
        linkType switch
        {
            ExerciseLinkType.WARMUP => ExerciseLinkErrorMessages.WarmupMustLinkToWorkout,
            ExerciseLinkType.COOLDOWN => ExerciseLinkErrorMessages.CooldownMustLinkToWorkout,
            ExerciseLinkType.ALTERNATIVE => ExerciseLinkErrorMessages.AlternativeCannotLinkToRest,
            ExerciseLinkType.WORKOUT => ExerciseLinkErrorMessages.WorkoutLinksAutoCreated,
            _ => ExerciseLinkErrorMessages.InvalidLinkTypeEnum
        };
    
    private async Task<ServiceResult<ExerciseLinkDto>> CreateBidirectionalLinkAsync(
        string sourceExerciseId,
        string targetExerciseId,
        ExerciseLinkType linkType)
    {
        // Parse IDs
        var sourceId = ExerciseId.ParseOrEmpty(sourceExerciseId);
        var targetId = ExerciseId.ParseOrEmpty(targetExerciseId);
        
        // Get the reverse link type based on the mapping rules
        var reverseLinkType = GetReverseExerciseLinkType(linkType);
        
        // Calculate display orders for both links
        var primaryDisplayOrder = await CalculateDisplayOrderAsync(sourceExerciseId, linkType);
        
        // Create the primary link entity
        var primaryLink = ExerciseLink.Handler.CreateNew(
            sourceId,
            targetId,
            linkType,
            primaryDisplayOrder
        );
        
        // Create the reverse link entity if needed
        ExerciseLink? reverseLink = null;
        if (reverseLinkType.HasValue)
        {
            var reverseDisplayOrder = await CalculateDisplayOrderAsync(targetExerciseId, reverseLinkType.Value);
            reverseLink = ExerciseLink.Handler.CreateNew(
                targetId,
                sourceId,
                reverseLinkType.Value,
                reverseDisplayOrder
            );
        }
        
        // Create both links atomically using the new transaction-aware method
        return await commandDataService.CreateBidirectionalAsync(primaryLink, reverseLink);
    }
    
    private static ExerciseLinkType? GetReverseExerciseLinkType(ExerciseLinkType linkType)
    {
        // Based on the feature requirements, determine the reverse link type
        return linkType switch
        {
            ExerciseLinkType.WARMUP => ExerciseLinkType.WORKOUT,
            ExerciseLinkType.COOLDOWN => ExerciseLinkType.WORKOUT,
            ExerciseLinkType.ALTERNATIVE => ExerciseLinkType.ALTERNATIVE, // ALTERNATIVE is bidirectional with itself
            ExerciseLinkType.WORKOUT => null, // WORKOUT links are only created as reverse, never as primary
            _ => null
        };
    }
    
    private async Task<int> CalculateDisplayOrderAsync(
        string sourceExerciseId, 
        ExerciseLinkType linkType)
    {
        var source = ExerciseId.ParseOrEmpty(sourceExerciseId);
        
        // Get existing links of same type for this source exercise
        var countResult = await queryDataService.GetLinkCountAsync(source, linkType.ToString());
        
        // Return next available display order (count + 1)
        return countResult.IsSuccess ? countResult.Data + 1 : 1;
    }
    
    private async Task<bool> IsLinkUniqueAsync(string sourceId, string targetId, string linkType)
    {
        var source = ExerciseId.ParseOrEmpty(sourceId);
        var target = ExerciseId.ParseOrEmpty(targetId);
        
        var result = await queryDataService.ExistsAsync(source, target, linkType);
        return !result.Data.Value;
    }
    
    private async Task<bool> IsNoCircularReferenceAsync(string sourceId, string targetId)
    {
        var source = ExerciseId.ParseOrEmpty(sourceId);
        var target = ExerciseId.ParseOrEmpty(targetId);
        
        return await ValidateNoCircularReferenceRecursiveAsync(target, source, new HashSet<ExerciseId>());
    }
    
    private async Task<bool> ValidateNoCircularReferenceRecursiveAsync(
        ExerciseId currentId,
        ExerciseId originalSourceId,
        HashSet<ExerciseId> visited)
    {
        if (visited.Contains(currentId))
        {
            return true; // Already checked this node
        }
        
        visited.Add(currentId);
        
        var linksResult = await queryDataService.GetBySourceExerciseAsync(currentId);
        if (!linksResult.IsSuccess)
            return true;
        
        foreach (var link in linksResult.Data)
        {
            var targetId = ExerciseId.ParseOrEmpty(link.TargetExerciseId);
            if (targetId == originalSourceId)
            {
                return false; // Found circular reference
            }
            
            var isValid = await ValidateNoCircularReferenceRecursiveAsync(
                targetId,
                originalSourceId,
                visited);
                
            if (!isValid)
            {
                return false;
            }
        }
        
        return true;
    }
    
    private async Task<bool> IsUnderMaximumLinksAsync(string sourceId, string linkType)
    {
        var source = ExerciseId.ParseOrEmpty(sourceId);
        var countResult = await queryDataService.GetLinkCountAsync(source, linkType);
        
        return countResult.IsSuccess && countResult.Data < 10;
    }
    
    private async Task<bool> DoesLinkExistAsync(string linkId)
    {
        var id = ExerciseLinkId.ParseOrEmpty(linkId);
        var result = await queryDataService.GetByIdAsync(id);
        
        return result.IsSuccess && !result.Data.IsEmpty;
    }
    
    private async Task<bool> DoesLinkBelongToExerciseAsync(string exerciseId, string linkId)
    {
        var linkIdParsed = ExerciseLinkId.ParseOrEmpty(linkId);
        var linkResult = await queryDataService.GetByIdAsync(linkIdParsed);
        
        return linkResult.IsSuccess 
            ? linkResult.Data.SourceExerciseId == exerciseId
            : false;
    }
    
    private async Task<ServiceResult<ExerciseLinkDto>> CreateLinkInternalAsync(CreateExerciseLinkCommand command)
    {
        var sourceId = ExerciseId.ParseOrEmpty(command.SourceExerciseId);
        var targetId = ExerciseId.ParseOrEmpty(command.TargetExerciseId);
        
        var exerciseLink = ExerciseLink.Handler.CreateNew(
            sourceId,
            targetId,
            command.LinkType,
            command.DisplayOrder
        );
        
        return await commandDataService.CreateAsync(exerciseLink);
    }
    
    private async Task<ServiceResult<ExerciseLinkDto>> UpdateLinkInternalAsync(UpdateExerciseLinkCommand command)
    {
        var linkId = ExerciseLinkId.ParseOrEmpty(command.LinkId);
        var entityResult = await queryDataService.GetEntityByIdAsync(linkId);
        
        if (!entityResult.IsSuccess || entityResult.Data.IsEmpty)
        {
            return ServiceResult<ExerciseLinkDto>.Failure(
                ExerciseLinkDto.Empty,
                ServiceError.NotFound("ExerciseLink", command.LinkId));
        }
        
        var existingLink = entityResult.Data;
        
        var updatedLink = ExerciseLink.Handler.Create(
            existingLink.Id,
            existingLink.SourceExerciseId,
            existingLink.TargetExerciseId,
            existingLink.LinkType,
            command.DisplayOrder,
            command.IsActive,
            existingLink.CreatedAt,
            DateTime.UtcNow
        );
        
        return await commandDataService.UpdateAsync(updatedLink);
    }
}