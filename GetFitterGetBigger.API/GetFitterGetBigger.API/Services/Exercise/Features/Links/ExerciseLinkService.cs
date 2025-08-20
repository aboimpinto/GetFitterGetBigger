using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
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
    /// Creates a new link between exercises
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
    /// Deletes an exercise link
    /// </summary>
    public async Task<ServiceResult<BooleanResultDto>> DeleteLinkAsync(string exerciseId, string linkId)
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
                whenValid: async () => await commandDataService.DeleteAsync(ExerciseLinkId.ParseOrEmpty(linkId))
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
    
    // Private helper methods for validation
    
    private static bool AreSameExercise(string sourceId, string targetId)
    {
        var source = ExerciseId.ParseOrEmpty(sourceId);
        var target = ExerciseId.ParseOrEmpty(targetId);
        return source == target;
    }
    
    private static bool IsValidLinkType(string linkType)
    {
        return linkType == "Warmup" || linkType == "Cooldown";
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