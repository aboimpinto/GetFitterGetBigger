using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Enums;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Commands;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Handlers;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Validation;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;

namespace GetFitterGetBigger.API.Services.Exercise.Features.Links;

/// <summary>
/// Service implementation for managing exercise links
/// </summary>
public class ExerciseLinkService(
    IExerciseLinkQueryDataService queryDataService,
    IExerciseLinkCommandDataService commandDataService,
    IBidirectionalLinkHandler bidirectionalLinkHandler,
    ILinkValidationHandler linkValidationHandler) : IExerciseLinkService
{
    private readonly IExerciseLinkQueryDataService _queryDataService = queryDataService;
    private readonly IExerciseLinkCommandDataService _commandDataService = commandDataService;
    private readonly IBidirectionalLinkHandler _bidirectionalLinkHandler = bidirectionalLinkHandler;
    private readonly ILinkValidationHandler _linkValidationHandler = linkValidationHandler;
    /// <summary>
    /// Creates a new link between exercises using enum LinkType
    /// DisplayOrder is calculated server-side based on existing links
    /// </summary>
    public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(
        ExerciseId sourceExerciseId,
        ExerciseId targetExerciseId,
        ExerciseLinkType linkType)
    {
        return await ServiceValidate.Build<ExerciseLinkDto>()
            .EnsureNotEmpty(sourceExerciseId, ExerciseLinkErrorMessages.InvalidSourceExerciseId)
            .EnsureNotEmpty(targetExerciseId, ExerciseLinkErrorMessages.InvalidTargetExerciseId)
            .EnsureExercisesAreDifferent(sourceExerciseId, targetExerciseId, ExerciseLinkErrorMessages.CannotLinkToSelf)
            .Ensure(
                () => LinkValidationHandler.IsValidLinkType(linkType),
                ExerciseLinkErrorMessages.InvalidLinkTypeEnum)
            .EnsureLinkTypeIsNotWorkout(linkType, ExerciseLinkErrorMessages.WorkoutLinksAutoCreated)
            // REMOVED: Bidirectional uniqueness validation moved to transaction-aware layer
            // The validation is now performed within CreateBidirectionalAsync to address 
            // transaction isolation issues where validation couldn't see uncommitted changes
            .AsExerciseLinkValidation()
            .EnsureSourceExerciseExists(
                this._queryDataService,
                sourceExerciseId,
                ExerciseLinkErrorMessages.SourceExerciseNotFound)
            .EnsureSourceExerciseIsNotRest(
                ExerciseLinkErrorMessages.RestExercisesCannotHaveLinks)
            .EnsureSourceExerciseCanCreateLinkType(
                linkType,
                ExerciseLinkErrorMessages.OnlyWorkoutExercisesCanCreateLinks)
            .EnsureTargetExerciseExists(
                this._queryDataService,
                targetExerciseId,
                ExerciseLinkErrorMessages.TargetExerciseNotFound)
            .EnsureTargetExerciseIsNotRest(
                ExerciseLinkErrorMessages.RestExercisesCannotBeLinked)
            .EnsureExercisesAreCompatibleForLinkType(
                linkType,
                GetLinkTypeCompatibilityError(linkType))
            .EnsureUnderMaximumLinks(
                this._linkValidationHandler,
                sourceExerciseId,
                linkType,
                ExerciseLinkErrorMessages.MaximumLinksReached)
            .MatchAsyncWithExercises(
                whenValid: async () => await this._bidirectionalLinkHandler.CreateBidirectionalLinkAsync(sourceExerciseId, targetExerciseId, linkType));
    }
    
    /// <summary>
    /// Gets all links for a specific exercise
    /// </summary>
    public async Task<ServiceResult<ExerciseLinksResponseDto>> GetLinksAsync(GetExerciseLinksCommand command)
    {
        return await ServiceValidate.Build<ExerciseLinksResponseDto>()
            .EnsureNotEmpty(command.ExerciseId, ExerciseLinkErrorMessages.InvalidSourceExerciseId)
            .MatchAsync(
                whenValid: async () => await this._queryDataService.GetLinksAsync(command),
                whenInvalid: errors => ServiceResult<ExerciseLinksResponseDto>.Failure(
                    ExerciseLinksResponseDto.Empty,
                    errors.FirstOrDefault() ?? ExerciseLinkErrorMessages.ValidationFailed)
            );
    }
    
    /// <summary>
    /// Updates an existing exercise link
    /// </summary>
    public async Task<ServiceResult<ExerciseLinkDto>> UpdateLinkAsync(UpdateExerciseLinkCommand command)
    {   
        return await ServiceValidate.Build<ExerciseLinkDto>()
            .EnsureNotEmpty(command.ExerciseId, ExerciseLinkErrorMessages.InvalidSourceExerciseId)
            .EnsureNotEmpty(command.LinkId, ExerciseLinkErrorMessages.InvalidLinkId)
            .EnsureDisplayOrderIsNotNegative(command.DisplayOrder, ExerciseLinkErrorMessages.DisplayOrderMustBeNonNegative)
            .EnsureAsync(
                async () => await this._linkValidationHandler.DoesLinkExistAsync(command.LinkId),
                ServiceError.NotFound("ExerciseLink", command.LinkId.ToString()))
            .EnsureAsync(
                async () => await this._linkValidationHandler.DoesLinkBelongToExerciseAsync(command.ExerciseId, command.LinkId),
                ExerciseLinkErrorMessages.LinkDoesNotBelongToExercise)
            .MatchAsync(
                whenValid: async () => await UpdateLinkInternalAsync(command),
                whenInvalid: errors => ServiceResult<ExerciseLinkDto>.Failure(
                    ExerciseLinkDto.Empty,
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed(ExerciseLinkErrorMessages.ValidationFailed))
            );
    }
    
    /// <summary>
    /// Deletes an exercise link with bidirectional deletion support
    /// </summary>
    public async Task<ServiceResult<BooleanResultDto>> DeleteLinkAsync(ExerciseId exerciseId, ExerciseLinkId linkId, bool deleteReverse = true)
    {   
        return await ServiceValidate.Build<BooleanResultDto>()
            .EnsureNotEmpty(exerciseId, ExerciseLinkErrorMessages.InvalidSourceExerciseId)
            .EnsureNotEmpty(linkId, ExerciseLinkErrorMessages.InvalidLinkId)
            .EnsureAsync(
                async () => await this._linkValidationHandler.DoesLinkExistAsync(linkId),
                ServiceError.NotFound("ExerciseLink", linkId.ToString()))
            .EnsureAsync(
                async () => await this._linkValidationHandler.DoesLinkBelongToExerciseAsync(exerciseId, linkId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.LinkDoesNotBelongToExercise))
            .MatchAsync(
                whenValid: async () => await this._bidirectionalLinkHandler.DeleteBidirectionalLinkAsync(linkId, deleteReverse),
                whenInvalid: errors => ServiceResult<BooleanResultDto>.Failure(
                    BooleanResultDto.Create(false),
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed(ExerciseLinkErrorMessages.ValidationFailed))
            );
    }
    
    /// <summary>
    /// Gets suggested links based on common usage patterns
    /// </summary>
    public async Task<ServiceResult<List<ExerciseLinkDto>>> GetSuggestedLinksAsync(ExerciseId exerciseId, int count = 5)
    {
        return await ServiceValidate.Build<List<ExerciseLinkDto>>()
            .EnsureNotEmpty(exerciseId, ExerciseLinkErrorMessages.InvalidSourceExerciseId)
            .EnsureCountIsInRange(count, 1, 20, ExerciseLinkErrorMessages.CountMustBeBetween1And20)
            .MatchAsync(
                whenValid: async () => await this._queryDataService.GetSuggestedLinksAsync(exerciseId, count),
                whenInvalid: errors => ServiceResult<List<ExerciseLinkDto>>.Failure(
                    [],
                    errors.FirstOrDefault() ?? ExerciseLinkErrorMessages.InvalidSourceExerciseId)
            );
    }
    
    // Private helper methods for validation
    
    // REMOVED: The following helper methods were removed as they are no longer needed.
    // The new validation pattern loads exercises once and carries them through the chain,
    // eliminating redundant database calls. These methods were making 6+ DB calls:
    // - IsSourceExerciseValidAsync
    // - IsTargetExerciseValidAsync 
    // - IsSourceExerciseWorkoutTypeAsync
    // - IsTargetExerciseMatchingTypeAsync
    // - IsNotRestExerciseAsync
    // - IsLinkTypeCompatibleAsync
    // Now we make only 2 DB calls total using the ExerciseLinkValidationExtensions.
    
    private static string GetLinkTypeCompatibilityError(ExerciseLinkType linkType) =>
        linkType switch
        {
            ExerciseLinkType.WARMUP => ExerciseLinkErrorMessages.WarmupMustLinkToWorkout,
            ExerciseLinkType.COOLDOWN => ExerciseLinkErrorMessages.CooldownMustLinkToWorkout,
            ExerciseLinkType.ALTERNATIVE => ExerciseLinkErrorMessages.AlternativeCannotLinkToRest,
            ExerciseLinkType.WORKOUT => ExerciseLinkErrorMessages.WorkoutLinksAutoCreated,
            _ => ExerciseLinkErrorMessages.InvalidLinkTypeEnum
        };
    
    private async Task<ServiceResult<ExerciseLinkDto>> UpdateLinkInternalAsync(UpdateExerciseLinkCommand command)
    {
        // TRUST THE INFRASTRUCTURE! The link existence has already been validated in UpdateLinkAsync
        // Pass a transformation function to update only the fields we need - SINGLE DATABASE CALL
        return await this._commandDataService.UpdateAsync(
            command.LinkId,
            existingLink => ExerciseLink.Handler.Create(
                existingLink.Id,
                existingLink.SourceExerciseId,  // Keep original
                existingLink.TargetExerciseId,  // Keep original  
                existingLink.LinkType,           // Keep original
                command.DisplayOrder,            // Update this
                command.IsActive,                // Update this
                existingLink.CreatedAt,          // Keep original
                DateTime.UtcNow                  // Update timestamp
            )
        );
    }
    
    // REMOVED: IsBidirectionalLinkUniqueWithFallbackAsync method
    // The bidirectional link validation has been moved to the transaction-aware layer 
    // in CreateBidirectionalAsync to resolve transaction isolation issues.
}