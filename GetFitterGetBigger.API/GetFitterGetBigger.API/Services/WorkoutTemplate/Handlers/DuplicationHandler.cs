using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Validation;
using GetFitterGetBigger.API.Constants.ErrorMessages;
using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;

/// <summary>
/// Handles duplication of workout templates with business logic
/// </summary>
public class DuplicationHandler(
    IWorkoutTemplateQueryDataService queryDataService,
    IWorkoutTemplateCommandDataService commandDataService,
    IWorkoutTemplateExerciseCommandDataService exerciseCommandDataService,
    ILogger<DuplicationHandler> logger)
{
    private readonly IWorkoutTemplateQueryDataService _queryDataService = queryDataService;
    private readonly IWorkoutTemplateCommandDataService _commandDataService = commandDataService;
    private readonly IWorkoutTemplateExerciseCommandDataService _exerciseCommandDataService = exerciseCommandDataService;
    private readonly ILogger<DuplicationHandler> _logger = logger;

    /// <summary>
    /// Duplicates a workout template with a new name
    /// </summary>
    public async Task<ServiceResult<WorkoutTemplateDto>> DuplicateAsync(
        WorkoutTemplateId originalTemplateId,
        string newName)
    {
        // Use the builder's WhenValidAsync to execute async validations properly
        return await ServiceValidate.Build<WorkoutTemplateDto>()
            .EnsureNotEmpty(originalTemplateId, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .EnsureNotWhiteSpace(newName, WorkoutTemplateErrorMessages.NameRequired)
            .EnsureMaxLength(newName, 100, WorkoutTemplateErrorMessages.NameLengthInvalid)
            .EnsureAsync(
                async () => await IsNameUniqueAsync(newName),
                WorkoutTemplateErrorMessages.NameAlreadyExists)
            .WhenValidAsync(async () => 
            {
                // Now load template and continue with duplication
                return await ServiceValidate.Build<WorkoutTemplateDto>()
                    .Validation
                    .AsAsync()
                    .EnsureWorkoutTemplateExists(
                        _queryDataService, 
                        originalTemplateId, 
                        WorkoutTemplateErrorMessages.OriginalNotFound)
                    .ThenWithWorkoutTemplate(async template => 
                        await ProcessDuplicationAsync(template, newName));
            });
    }

    private async Task<ServiceResult<WorkoutTemplateDto>> ProcessDuplicationAsync(
        WorkoutTemplateDto originalTemplate,
        string newName)
    {
        // Convert DTO to entity for duplication
        var originalEntity = await ConvertToEntityAsync(originalTemplate);
        
        // Create duplicate template using entity handler
        var duplicateResult = WorkoutTemplateEntity.Handler.Duplicate(originalEntity, newName);
        
        return duplicateResult switch
        {
            { IsSuccess: false } => ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed(string.Join(", ", duplicateResult.Errors))),
            { IsSuccess: true } => await ExecuteDuplicationAsync(duplicateResult.Value, originalTemplate)
        };
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> ExecuteDuplicationAsync(
        WorkoutTemplateEntity duplicateTemplate,
        WorkoutTemplateDto originalTemplate)
    {
        // Create the duplicate template
        var createResult = await _commandDataService.CreateAsync(duplicateTemplate);
        
        return createResult switch
        {
            { IsSuccess: false } => createResult,
            { IsSuccess: true } => await CompleteTemplateCreationAsync(createResult, originalTemplate)
        };
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> CompleteTemplateCreationAsync(
        ServiceResult<WorkoutTemplateDto> createResult,
        WorkoutTemplateDto originalTemplate)
    {
        // Duplicate exercises if any exist
        await (originalTemplate.Exercises?.Any() == true
            ? DuplicateExercisesAsync(
                originalTemplate.Exercises, 
                WorkoutTemplateId.ParseOrEmpty(createResult.Data.Id))
            : Task.CompletedTask);

        _logger.LogInformation(
            "Duplicated WorkoutTemplate {OriginalId} to {NewId} with name '{NewName}'",
            originalTemplate.Id, createResult.Data.Id, createResult.Data.Name);

        return createResult;
    }

    private async Task DuplicateExercisesAsync(
        IEnumerable<WorkoutTemplateExerciseDto> originalExercises,
        WorkoutTemplateId newTemplateId)
    {
        foreach (var originalExercise in originalExercises.OrderBy(e => e.SequenceOrder))
        {
            // Create new exercise with entity handler
            var exerciseId = ExerciseId.ParseOrEmpty(originalExercise.Exercise.Id);
            var zone = Enum.TryParse<WorkoutZone>(originalExercise.Zone, out var parsedZone) 
                ? parsedZone 
                : WorkoutZone.Main;
            
            var duplicateResult = WorkoutTemplateExercise.Handler.CreateNew(
                newTemplateId,
                exerciseId,
                zone,
                originalExercise.SequenceOrder,
                originalExercise.Notes);

            await (duplicateResult switch
            {
                { IsSuccess: true } => _exerciseCommandDataService.CreateAsync(duplicateResult.Value),
                _ => Task.CompletedTask
            });
        }
    }

    private async Task<bool> IsNameUniqueAsync(string name)
    {
        var existsResult = await _queryDataService.ExistsByNameAsync(name);
        return existsResult?.IsSuccess == true && !existsResult.Data.Value;
    }
    
    private async Task<WorkoutTemplateEntity> ConvertToEntityAsync(WorkoutTemplateDto dto)
    {
        // Convert DTO to entity for duplication
        // The actual entity will be fetched from the database via QueryDataService
        // This is a fallback conversion that shouldn't normally be needed
        // since we should get the entity directly from the database
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(dto.Id);
        var categoryId = WorkoutCategoryId.ParseOrEmpty(dto.Category.Id);
        var difficultyId = DifficultyLevelId.ParseOrEmpty(dto.Difficulty.Id);
        var stateId = WorkoutStateId.ParseOrEmpty(dto.WorkoutState.Id);
        
        // Use the Handler to create a properly validated entity
        var createResult = WorkoutTemplateEntity.Handler.Create(
            templateId,
            dto.Name,
            dto.Description,
            categoryId,
            difficultyId,
            dto.EstimatedDurationMinutes,
            dto.Tags,
            dto.IsPublic,
            stateId,
            dto.CreatedAt,
            dto.UpdatedAt
        );
        
        return await Task.FromResult(createResult switch
        {
            { IsSuccess: true } => createResult.Value,
            { IsSuccess: false } => throw new InvalidOperationException(
                $"Failed to convert DTO to entity: {string.Join(", ", createResult.Errors)}")
        });
    }
}