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
public class ExerciseLinkService : IExerciseLinkService
{
    private readonly IExerciseLinkQueryDataService queryDataService;
    private readonly IExerciseLinkCommandDataService commandDataService;
    private readonly IExerciseService exerciseService;
    private readonly ILogger<ExerciseLinkService> logger;
    private readonly BidirectionalLinkHandler _bidirectionalLinkHandler;
    private readonly CircularReferenceValidationHandler _circularReferenceHandler;
    private readonly LinkValidationHandler _linkValidationHandler;
    
    public ExerciseLinkService(
        IExerciseLinkQueryDataService queryDataService,
        IExerciseLinkCommandDataService commandDataService,
        IExerciseService exerciseService,
        ILoggerFactory loggerFactory)
    {
        this.queryDataService = queryDataService;
        this.commandDataService = commandDataService;
        this.exerciseService = exerciseService;
        this.logger = loggerFactory.CreateLogger<ExerciseLinkService>();
        
        _bidirectionalLinkHandler = new BidirectionalLinkHandler(
            queryDataService, 
            commandDataService, 
            loggerFactory.CreateLogger<BidirectionalLinkHandler>());
        
        _circularReferenceHandler = new CircularReferenceValidationHandler(
            queryDataService,
            loggerFactory.CreateLogger<CircularReferenceValidationHandler>());
        
        _linkValidationHandler = new LinkValidationHandler(
            queryDataService,
            loggerFactory.CreateLogger<LinkValidationHandler>());
    }
    /// <summary>
    /// Creates a new link between exercises using enum LinkType (enhanced functionality)
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
            .AsExerciseLinkValidation()
            .EnsureSourceExerciseExists(
                exerciseService,
                sourceExerciseId,
                ExerciseLinkErrorMessages.SourceExerciseNotFound)
            .EnsureSourceExerciseIsNotRest(
                ExerciseLinkErrorMessages.RestExercisesCannotHaveLinks)
            .EnsureTargetExerciseExists(
                exerciseService,
                targetExerciseId,
                ExerciseLinkErrorMessages.TargetExerciseNotFound)
            .EnsureTargetExerciseIsNotRest(
                ExerciseLinkErrorMessages.RestExercisesCannotBeLinked)
            .EnsureExercisesAreCompatibleForLinkType(
                linkType,
                GetLinkTypeCompatibilityError(linkType))
            .MatchAsyncWithExercises(
                whenValid: async () => await _bidirectionalLinkHandler.CreateBidirectionalLinkAsync(sourceExerciseId, targetExerciseId, linkType));
    }

    /// <summary>
    /// Creates a new link between exercises using traditional command (backward compatibility)
    /// </summary>
    public async Task<ServiceResult<ExerciseLinkDto>> CreateLinkAsync(CreateExerciseLinkCommand command)
    {
        return await ServiceValidate.Build<ExerciseLinkDto>()
            .EnsureNotEmpty(command.SourceExerciseId, ExerciseLinkErrorMessages.InvalidSourceExerciseId)
            .EnsureNotEmpty(command.TargetExerciseId, ExerciseLinkErrorMessages.InvalidTargetExerciseId)
            .EnsureExercisesAreDifferent(command.SourceExerciseId, command.TargetExerciseId, ExerciseLinkErrorMessages.CannotLinkToSelf)
            .EnsureNotWhiteSpace(command.LinkType, ExerciseLinkErrorMessages.LinkTypeRequired)
            .Ensure(
                () => LinkValidationHandler.IsValidLinkType(command.LinkType),
                ExerciseLinkErrorMessages.InvalidLinkType)
            .EnsureDisplayOrderIsNotNegative(command.DisplayOrder, ExerciseLinkErrorMessages.DisplayOrderMustBeNonNegative)
            .AsExerciseLinkValidation()
            .EnsureSourceExerciseExists(
                exerciseService,
                command.SourceExerciseId,
                ExerciseLinkErrorMessages.SourceExerciseNotFound)
            .EnsureSourceExerciseIsWorkoutType(
                ExerciseLinkErrorMessages.SourceMustBeWorkout)
            .EnsureSourceExerciseIsNotRest(
                ExerciseLinkErrorMessages.RestExercisesCannotHaveLinks)
            .EnsureTargetExerciseExists(
                exerciseService,
                command.TargetExerciseId,
                ExerciseLinkErrorMessages.TargetExerciseNotFound)
            .EnsureTargetExerciseMatchesLinkType(
                command.LinkType,
                ExerciseLinkErrorMessages.TargetMustMatchLinkType)
            .EnsureTargetExerciseIsNotRest(
                ExerciseLinkErrorMessages.RestExercisesCannotBeLinked)
            .EnsureAsync(
                async () => await _linkValidationHandler.IsLinkUniqueAsync(command.SourceExerciseId, command.TargetExerciseId, command.LinkType),
                ExerciseLinkErrorMessages.LinkAlreadyExists)
            .EnsureAsync(
                async () => await _circularReferenceHandler.IsNoCircularReferenceAsync(command.SourceExerciseId, command.TargetExerciseId),
                ExerciseLinkErrorMessages.CircularReferenceDetected)
            .EnsureAsync(
                async () => await _linkValidationHandler.IsUnderMaximumLinksAsync(command.SourceExerciseId, command.LinkType),
                ExerciseLinkErrorMessages.MaximumLinksReached)
            .MatchAsyncWithExercises(
                whenValid: async () => await CreateLinkInternalAsync(command));
    }
    
    /// <summary>
    /// Gets all links for a specific exercise
    /// </summary>
    public async Task<ServiceResult<ExerciseLinksResponseDto>> GetLinksAsync(GetExerciseLinksCommand command)
    {
        return await ServiceValidate.Build<ExerciseLinksResponseDto>()
            .EnsureNotEmpty(command.ExerciseId, ExerciseLinkErrorMessages.InvalidSourceExerciseId)
            .MatchAsync(
                whenValid: async () => await queryDataService.GetLinksAsync(command),
                whenInvalid: errors => ServiceResult<ExerciseLinksResponseDto>.Failure(
                    ExerciseLinksResponseDto.Empty,
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed(ExerciseLinkErrorMessages.ValidationFailed))
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
                async () => await _linkValidationHandler.DoesLinkExistAsync(command.LinkId),
                ServiceError.NotFound("ExerciseLink", command.LinkId.ToString()))
            .EnsureAsync(
                async () => await _linkValidationHandler.DoesLinkBelongToExerciseAsync(command.ExerciseId, command.LinkId),
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
                async () => await _linkValidationHandler.DoesLinkExistAsync(linkId),
                ServiceError.NotFound("ExerciseLink", linkId.ToString()))
            .EnsureAsync(
                async () => await _linkValidationHandler.DoesLinkBelongToExerciseAsync(exerciseId, linkId),
                ServiceError.ValidationFailed(ExerciseLinkErrorMessages.LinkDoesNotBelongToExercise))
            .MatchAsync(
                whenValid: async () => await _bidirectionalLinkHandler.DeleteBidirectionalLinkAsync(linkId, deleteReverse),
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
                whenValid: async () => await queryDataService.GetSuggestedLinksAsync(exerciseId, count),
                whenInvalid: errors => ServiceResult<List<ExerciseLinkDto>>.Failure(
                    [],
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed(ExerciseLinkErrorMessages.InvalidSourceExerciseId))
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
    
    private async Task<ServiceResult<ExerciseLinkDto>> CreateLinkInternalAsync(CreateExerciseLinkCommand command)
    {
        // Create a DTO from the command
        var linkDto = new ExerciseLinkDto
        {
            Id = string.Empty, // Will be generated by the database
            SourceExerciseId = command.SourceExerciseId.ToString(),
            TargetExerciseId = command.TargetExerciseId.ToString(),
            LinkType = command.LinkType,
            DisplayOrder = command.DisplayOrder,
            IsActive = true
        };
        
        return await commandDataService.CreateAsync(linkDto);
    }
    
    private async Task<ServiceResult<ExerciseLinkDto>> UpdateLinkInternalAsync(UpdateExerciseLinkCommand command)
    {
        // TRUST THE INFRASTRUCTURE! The link existence has already been validated in UpdateLinkAsync
        // Pass a transformation function to update only the fields we need - SINGLE DATABASE CALL
        return await commandDataService.UpdateAsync(
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
}