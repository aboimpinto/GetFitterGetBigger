using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Equipment;
using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate;

/// <summary>
/// WorkoutTemplate service that uses DataServices instead of direct UnitOfWork access.
/// This service focuses purely on business logic without data access concerns.
/// </summary>
public class WorkoutTemplateService(
    IWorkoutTemplateQueryDataService queryDataService,
    IWorkoutTemplateCommandDataService commandDataService,
    IWorkoutStateService workoutStateService,
    IEquipmentRequirementsService equipmentRequirementsService,
    SuggestionHandler suggestionHandler,
    DuplicationHandler duplicationHandler,
    ILogger<WorkoutTemplateService> logger) : IWorkoutTemplateService
{
    private readonly IWorkoutTemplateQueryDataService _queryDataService = queryDataService;
    private readonly IWorkoutTemplateCommandDataService _commandDataService = commandDataService;
    private readonly IWorkoutStateService _workoutStateService = workoutStateService;
    private readonly IEquipmentRequirementsService _equipmentRequirementsService = equipmentRequirementsService;
    private readonly SuggestionHandler _suggestionHandler = suggestionHandler;
    private readonly DuplicationHandler _duplicationHandler = duplicationHandler;
    private readonly ILogger<WorkoutTemplateService> _logger = logger;

    public async Task<ServiceResult<WorkoutTemplateDto>> GetByIdAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.For<WorkoutTemplateDto>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .MatchAsync(async () => await _queryDataService.GetByIdWithDetailsAsync(id));
    }

    public async Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> SearchAsync(
        int page,
        int pageSize,
        string namePattern,
        WorkoutCategoryId categoryId,
        WorkoutObjectiveId objectiveId,
        DifficultyLevelId difficultyId,
        WorkoutStateId stateId,
        string sortBy,
        string sortOrder)
    {
        // Validate pagination parameters
        return await ServiceValidate.Build<PagedResponse<WorkoutTemplateDto>>()
            .EnsureNumberBetween(page, 1, int.MaxValue, "Page number must be at least 1")
            .EnsureNumberBetween(pageSize, 1, 100, "Page size must be between 1 and 100")
            .WhenValidAsync(async () => await SearchWithBusinessLogicAsync(
                page, pageSize, namePattern, categoryId, objectiveId, 
                difficultyId, stateId, sortBy, sortOrder));
    }

    private async Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> SearchWithBusinessLogicAsync(
        int page,
        int pageSize,
        string namePattern,
        WorkoutCategoryId categoryId,
        WorkoutObjectiveId objectiveId,
        DifficultyLevelId difficultyId,
        WorkoutStateId stateId,
        string sortBy,
        string sortOrder)
    {
        // Business Logic: When no state filter is provided, exclude ARCHIVED templates
        // This ensures "active template lists" only show DRAFT and PRODUCTION templates
        if (stateId.IsEmpty)
        {
            // Get ARCHIVED state ID to exclude it from results
            var archivedStateResult = await _workoutStateService.GetByValueAsync("ARCHIVED");
            if (archivedStateResult.IsSuccess)
            {
                var archivedStateId = WorkoutStateId.ParseOrEmpty(archivedStateResult.Data.Id);
                
                // Use the data service's search method but we'll need to filter out archived results
                // For now, pass empty state and filter the results post-query
                // TODO: Consider adding a SearchExcludingStateAsync method to the data service for better performance
                var allResults = await _queryDataService.SearchAsync(
                    page, pageSize, namePattern, categoryId, objectiveId, 
                    difficultyId, WorkoutStateId.Empty, sortBy, sortOrder);

                if (allResults.IsSuccess)
                {
                    // Filter out ARCHIVED templates from the results
                    var filteredItems = allResults.Data.Items
                        .Where(item => item.WorkoutState?.Id != archivedStateId.ToString())
                        .ToList();

                    var filteredResponse = new PagedResponse<WorkoutTemplateDto>
                    {
                        Items = filteredItems,
                        TotalCount = filteredItems.Count, // Note: This may not be accurate for pagination
                        CurrentPage = page,
                        PageSize = pageSize
                    };

                    return ServiceResult<PagedResponse<WorkoutTemplateDto>>.Success(filteredResponse);
                }
                
                return allResults;
            }
        }

        // If a specific state is requested, or if we can't get ARCHIVED state, use normal search
        return await _queryDataService.SearchAsync(
            page, pageSize, namePattern, categoryId, objectiveId, 
            difficultyId, stateId, sortBy, sortOrder);
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> CreateAsync(CreateWorkoutTemplateCommand command)
    {
        return await ServiceValidate.Build<WorkoutTemplateDto>()
            // Basic validations first (stateless, fast)
            .EnsureNotWhiteSpace(command.Name, WorkoutTemplateErrorMessages.NameRequired)
            .EnsureMinLength(command.Name, 3, WorkoutTemplateErrorMessages.NameLengthInvalid)
            .EnsureMaxLength(command.Name, 100, WorkoutTemplateErrorMessages.NameLengthInvalid)
            .EnsureMaxLength(command.Description, 1000, WorkoutTemplateErrorMessages.DescriptionTooLong)
            .EnsureNotEmpty(command.CategoryId, WorkoutTemplateErrorMessages.CategoryRequired)
            .EnsureNotEmpty(command.DifficultyId, WorkoutTemplateErrorMessages.DifficultyRequired)
            .EnsureNumberBetween(command.EstimatedDurationMinutes, 5, 300, WorkoutTemplateErrorMessages.DurationInvalid)
            .EnsureMaxCount(command.Tags, 10, WorkoutTemplateErrorMessages.TooManyTags)
            // Dependent validations
            .EnsureNameIsUniqueAsync(
                async () => await IsNameUniqueAsync(command.Name),
                "WorkoutTemplate", command.Name)
            .MatchAsync(async () => await CreateWorkoutTemplateEntityAsync(command)
            );
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> CreateWorkoutTemplateEntityAsync(CreateWorkoutTemplateCommand command)
    {
        // Use service layer to get Draft state
        var draftStateResult = await _workoutStateService.GetByValueAsync("Draft");
        
        if (!draftStateResult.IsSuccess)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.InternalError("Draft workout state not found in database"));
        }
        
        var defaultWorkoutStateId = WorkoutStateId.ParseOrEmpty(draftStateResult.Data.Id);
        
        var entityResult = WorkoutTemplateEntity.Handler.CreateNew(
            command.Name,
            command.Description,
            command.CategoryId,
            command.DifficultyId,
            command.EstimatedDurationMinutes,
            command.Tags,
            command.IsPublic,
            defaultWorkoutStateId);

        if (!entityResult.IsSuccess)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors)));
        }
        
        // Use DataService for persistence
        var result = await _commandDataService.CreateAsync(entityResult.Value);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Created workout template {TemplateId}", result.Data.Id);
        }
        
        return result;
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> UpdateAsync(WorkoutTemplateId id, UpdateWorkoutTemplateCommand command)
    {
        return await ServiceValidate.Build<WorkoutTemplateDto>()
            // ID validation first
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            // Basic field validations (stateless)
            .EnsureNotWhiteSpace(command.Name, WorkoutTemplateErrorMessages.NameRequired)
            .EnsureMinLength(command.Name, 3, WorkoutTemplateErrorMessages.NameLengthInvalid)
            .EnsureMaxLength(command.Name, 100, WorkoutTemplateErrorMessages.NameLengthInvalid)
            .EnsureMaxLength(command.Description, 1000, WorkoutTemplateErrorMessages.DescriptionTooLong)
            .EnsureNotEmpty(command.CategoryId, WorkoutTemplateErrorMessages.CategoryIdEmpty)
            .EnsureNotEmpty(command.DifficultyId, WorkoutTemplateErrorMessages.DifficultyIdEmpty)
            .EnsureNumberBetween(command.EstimatedDurationMinutes, 5, 300, WorkoutTemplateErrorMessages.DurationInvalid)
            .EnsureMaxCount(command.Tags, 10, WorkoutTemplateErrorMessages.TooManyTags)
            // Dependent validations
            .EnsureExistsAsync(
                async () => (await _queryDataService.ExistsAsync(id)).Data.Value,
                "WorkoutTemplate")
            .EnsureNameIsUniqueAsync(
                async () => await IsNameUniqueForUpdateAsync(command.Name!, id),
                "WorkoutTemplate", command.Name!)
            .MatchAsync(async () => await UpdateWorkoutTemplateEntityAsync(id, command)
            );
    }
    
    private async Task<bool> IsNameUniqueAsync(string name)
    {
        var existsResult = await _queryDataService.ExistsByNameAsync(name);
        return !existsResult.Data.Value; // Return true when name IS unique
    }
    
    private async Task<bool> IsNameUniqueForUpdateAsync(string name, WorkoutTemplateId excludeId)
    {
        if (_queryDataService == null)
        {
            throw new InvalidOperationException("Query data service is not initialized");
        }
        
        // Check if a template with this name exists
        var existsResult = await _queryDataService.ExistsByNameAsync(name);
        
        if (existsResult?.Data == null || !existsResult.Data.Value)
        {
            return true; // Name doesn't exist, so it's unique
        }
        
        // If it exists, we need to check if it's the same template we're updating
        // TODO: Implement proper check to see if the existing template with this name has the excludeId
        // For now, assuming it's valid for update scenarios (simplified)
        return true;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> UpdateWorkoutTemplateEntityAsync(
        WorkoutTemplateId id, 
        UpdateWorkoutTemplateCommand command)
    {
        // Use DataService to update with an update action
        return await _commandDataService.UpdateAsync(id, existingTemplate =>
        {
            var entityResult = WorkoutTemplateEntity.Handler.Update(
                existingTemplate,
                command.Name,
                command.Description,
                command.CategoryId,
                command.DifficultyId,
                command.EstimatedDurationMinutes,
                command.Tags,
                command.IsPublic);
            
            // Let the caller handle validation failures
            return entityResult.IsSuccess 
                ? entityResult.Value 
                : existingTemplate; // Return original if validation fails
        });
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> ChangeStateAsync(WorkoutTemplateId id, WorkoutStateId newStateId)
    {
        return await ServiceValidate.Build<WorkoutTemplateDto>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .EnsureNotEmpty(newStateId, "New state ID is required")
            .EnsureExistsAsync(
                async () => (await _queryDataService.ExistsAsync(id)).Data.Value,
                "WorkoutTemplate")
            .MatchAsync(async () => 
            {
                // For now, just change the state directly
                // In production, you would validate the transition rules here
                return await _commandDataService.ChangeStateAsync(id, newStateId);
            });
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> DuplicateAsync(WorkoutTemplateId id, string newName)
    {
        // Use the DuplicationHandler for business logic
        return await _duplicationHandler.DuplicateAsync(id, newName);
    }

    public async Task<ServiceResult<BooleanResultDto>> SoftDeleteAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.Build<BooleanResultDto>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .EnsureExistsAsync(
                async () => (await _queryDataService.ExistsAsync(id)).Data.Value,
                "WorkoutTemplate")
            .MatchAsync(async () => await _commandDataService.SoftDeleteAsync(id));
    }

    public async Task<ServiceResult<BooleanResultDto>> DeleteAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.Build<BooleanResultDto>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .EnsureExistsAsync(
                async () => (await _queryDataService.ExistsAsync(id)).Data.Value,
                "WorkoutTemplate")
            .EnsureAsync(
                async () => !(await _queryDataService.HasExecutionLogsAsync(id)).Data.Value,
                ServiceError.ValidationFailed("Cannot delete workout template with execution logs"))
            .MatchAsync(async () => await _commandDataService.DeleteAsync(id));
    }

    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.Build<BooleanResultDto>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .MatchAsync(async () => await _queryDataService.ExistsAsync(id));
    }

    public async Task<ServiceResult<BooleanResultDto>> ExistsByNameAsync(string name)
    {
        return await ServiceValidate.Build<BooleanResultDto>()
            .EnsureNotWhiteSpace(name, "Name cannot be empty")
            .MatchAsync(async () => await _queryDataService.ExistsByNameAsync(name));
    }

    public async Task<ServiceResult<IEnumerable<ExerciseDto>>> GetSuggestedExercisesAsync(
        WorkoutCategoryId categoryId,
        IEnumerable<ExerciseId> existingExerciseIds,
        int maxSuggestions = 10)
    {
        return await ServiceValidate.Build<IEnumerable<ExerciseDto>>()
            .EnsureNotEmpty(categoryId, "Category ID is required for suggestions")
            .EnsureNumberBetween(maxSuggestions, 1, 50, "Max suggestions must be between 1 and 50")
            .WhenValidAsync(async () => 
            {
                // Delegate to suggestion handler
                return await _suggestionHandler.GetSuggestedExercisesAsync(
                    categoryId, existingExerciseIds, maxSuggestions);
            });
    }

    public async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetRequiredEquipmentAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.Build<IEnumerable<EquipmentDto>>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .EnsureExistsAsync(
                async () => (await _queryDataService.ExistsAsync(id)).Data.Value,
                "WorkoutTemplate")
            .WhenValidAsync(async () => 
            {
                // Delegate to equipment requirements service
                return await _equipmentRequirementsService.GetRequiredEquipmentAsync(id);
            });
    }
}