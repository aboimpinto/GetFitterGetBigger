using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;

/// <summary>
/// Handles duplication of workout templates with business logic
/// </summary>
public class DuplicationHandler(
    IWorkoutTemplateQueryDataService queryDataService,
    IWorkoutTemplateCommandDataService commandDataService,
    ILogger<DuplicationHandler> logger)
{
    private readonly IWorkoutTemplateQueryDataService _queryDataService = queryDataService;
    private readonly IWorkoutTemplateCommandDataService _commandDataService = commandDataService;
    private readonly ILogger<DuplicationHandler> _logger = logger;

    /// <summary>
    /// Duplicates a workout template with a new name
    /// </summary>
    public async Task<ServiceResult<WorkoutTemplateDto>> DuplicateAsync(
        WorkoutTemplateId originalTemplateId,
        string newName)
    {
        // All validations in a single chain, no nested validations
        return await ServiceValidate.Build<WorkoutTemplateDto>()
            .EnsureNotEmpty(originalTemplateId, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .EnsureNotWhiteSpace(newName, WorkoutTemplateErrorMessages.NameRequired)
            .EnsureMaxLength(newName, 100, WorkoutTemplateErrorMessages.NameLengthInvalid)
            .EnsureAsync(
                async () => await IsNameUniqueAsync(newName),
                WorkoutTemplateErrorMessages.NameAlreadyExists)
            .EnsureExistsAsync(
                async () => (await _queryDataService.ExistsAsync(originalTemplateId)).Data.Value,
                "WorkoutTemplate")
            .MatchAsync(
                whenValid: async () => 
                {
                    // Use the DataService's built-in duplication method
                    // This handles all entity operations internally
                    var duplicateResult = await _commandDataService.DuplicateAsync(
                        originalTemplateId, 
                        newName, 
                        UserId.Empty); // TODO: Get actual user ID from context when available
                    
                    if (duplicateResult.IsSuccess)
                    {
                        _logger.LogInformation(
                            "Duplicated WorkoutTemplate {OriginalId} to {NewId} with name '{NewName}'",
                            originalTemplateId, duplicateResult.Data.Id, newName);
                    }
                    
                    return duplicateResult;
                },
                whenInvalid: (errors) => ServiceResult<WorkoutTemplateDto>.Failure(
                    WorkoutTemplateDto.Empty,
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error")));
    }

    private async Task<bool> IsNameUniqueAsync(string name)
    {
        var existsResult = await _queryDataService.ExistsByNameAsync(name);
        return existsResult?.IsSuccess == true && !existsResult.Data.Value;
    }
}