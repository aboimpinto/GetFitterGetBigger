using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Equipment;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;
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
    DuplicationHandler duplicationHandler) : IWorkoutTemplateService
{
    private readonly IWorkoutTemplateQueryDataService _queryDataService = queryDataService;
    private readonly IWorkoutTemplateCommandDataService _commandDataService = commandDataService;
    private readonly IWorkoutStateService _workoutStateService = workoutStateService;
    private readonly IEquipmentRequirementsService _equipmentRequirementsService = equipmentRequirementsService;
    private readonly SuggestionHandler _suggestionHandler = suggestionHandler;
    private readonly DuplicationHandler _duplicationHandler = duplicationHandler;

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
            .EnsureNumberBetween(page, 1, int.MaxValue, WorkoutTemplateErrorMessages.PageNumberInvalid)
            .EnsureNumberBetween(pageSize, 1, 100, WorkoutTemplateErrorMessages.PageSizeInvalid)
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
        return stateId.IsEmpty 
            ? await SearchExcludingArchivedAsync(page, pageSize, namePattern, categoryId, objectiveId, difficultyId, sortBy, sortOrder)
            : await _queryDataService.SearchAsync(page, pageSize, namePattern, categoryId, objectiveId, difficultyId, stateId, sortBy, sortOrder);
    }
    
    private async Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> SearchExcludingArchivedAsync(
        int page,
        int pageSize, 
        string namePattern,
        WorkoutCategoryId categoryId,
        WorkoutObjectiveId objectiveId,
        DifficultyLevelId difficultyId,
        string sortBy,
        string sortOrder)
    {
        // Get ARCHIVED state ID to exclude it from results
        var archivedStateResult = await _workoutStateService.GetByValueAsync("ARCHIVED");
        
        return archivedStateResult.IsSuccess 
            ? await FilterArchivedTemplatesAsync(page, pageSize, namePattern, categoryId, objectiveId, difficultyId, sortBy, sortOrder, archivedStateResult.Data.Id)
            : await _queryDataService.SearchAsync(page, pageSize, namePattern, categoryId, objectiveId, difficultyId, WorkoutStateId.Empty, sortBy, sortOrder);
    }
    
    private async Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> FilterArchivedTemplatesAsync(
        int page,
        int pageSize,
        string namePattern,
        WorkoutCategoryId categoryId,
        WorkoutObjectiveId objectiveId,
        DifficultyLevelId difficultyId,
        string sortBy,
        string sortOrder,
        string archivedStateIdString)
    {
        var archivedStateId = WorkoutStateId.ParseOrEmpty(archivedStateIdString);
        
        // Get all templates for current page
        var allResults = await _queryDataService.SearchAsync(
            page, pageSize, namePattern, categoryId, objectiveId, 
            difficultyId, WorkoutStateId.Empty, sortBy, sortOrder);

        return allResults.IsSuccess 
            ? await BuildFilteredResponseAsync(page, pageSize, namePattern, categoryId, objectiveId, difficultyId, sortBy, sortOrder, allResults, archivedStateId)
            : allResults;
    }
    
    private async Task<ServiceResult<PagedResponse<WorkoutTemplateDto>>> BuildFilteredResponseAsync(
        int page,
        int pageSize,
        string namePattern,
        WorkoutCategoryId categoryId,
        WorkoutObjectiveId objectiveId,
        DifficultyLevelId difficultyId,
        string sortBy,
        string sortOrder,
        ServiceResult<PagedResponse<WorkoutTemplateDto>> allResults,
        WorkoutStateId archivedStateId)
    {
        // Filter out ARCHIVED templates from the items
        var filteredItems = allResults.Data.Items
            .Where(item => item.WorkoutState?.Id != archivedStateId.ToString())
            .ToList();

        // Use GetCountAsync to get the actual count without pagination limits
        var totalCountResult = await _queryDataService.GetCountAsync(
            namePattern, categoryId, objectiveId, difficultyId, WorkoutStateId.Empty);
        
        // Now we need to subtract the count of archived templates
        var archivedCountResult = await _queryDataService.GetCountAsync(
            namePattern, categoryId, objectiveId, difficultyId, archivedStateId);
        
        var totalNonArchivedCount = totalCountResult.IsSuccess && archivedCountResult.IsSuccess
            ? totalCountResult.Data - archivedCountResult.Data
            : allResults.Data.TotalCount; // Fallback to original count

        var filteredResponse = new PagedResponse<WorkoutTemplateDto>
        {
            Items = filteredItems,
            TotalCount = totalNonArchivedCount,
            CurrentPage = page,
            PageSize = pageSize
        };

        return ServiceResult<PagedResponse<WorkoutTemplateDto>>.Success(filteredResponse);
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
            // Business validations - ALL in the chain!
            .EnsureNameIsUniqueAsync(
                async () => await IsNameUniqueAsync(command.Name),
                "WorkoutTemplate", command.Name)
            .EnsureAsync(
                async () => await IsDraftStateAvailableAsync(),
                ServiceError.InternalError(WorkoutTemplateErrorMessages.DraftStateNotFound))
            // Single operation when ALL validations pass
            .WhenValidAsync(async () => await CreateWorkoutTemplateEntityAsync(command));
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> CreateWorkoutTemplateEntityAsync(CreateWorkoutTemplateCommand command)
    {
        // Get Draft state (validation already done in ServiceValidate chain)
        var draftStateResult = await _workoutStateService.GetByValueAsync(WorkoutStateConstants.Draft);
        var defaultWorkoutStateId = WorkoutStateId.ParseOrEmpty(draftStateResult.Data.Id);
        
        // Use default REPS_AND_SETS ExecutionProtocol (ID from feature tasks)
        var defaultExecutionProtocolId = ExecutionProtocolId.From(Guid.Parse("30000003-3000-4000-8000-300000000001"));
        
        // Create entity (validation happens at entity level)
        var entityResult = WorkoutTemplateEntity.Handler.CreateNew(
            command.Name,
            command.Description,
            command.CategoryId,
            command.DifficultyId,
            command.EstimatedDurationMinutes,
            command.Tags,
            command.IsPublic,
            defaultWorkoutStateId,
            defaultExecutionProtocolId);

        return entityResult.IsSuccess switch
        {
            true => await _commandDataService.CreateAsync(entityResult.Value),
            false => ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors)))
        };
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
            .WhenValidAsync(async () => await UpdateWorkoutTemplateEntityAsync(id, command));
    }
    
    private async Task<bool> IsNameUniqueAsync(string name)
    {
        return await _queryDataService.IsWorkoutTemplateNameUniqueAsync(name);
    }
    
    private async Task<bool> IsNameUniqueForUpdateAsync(string name, WorkoutTemplateId excludeId)
    {
        return await _queryDataService.IsWorkoutTemplateNameUniqueAsync(name, excludeId);
    }
    
    private async Task<bool> IsWorkoutTemplateDeletableAsync(WorkoutTemplateId id)
    {
        return await _queryDataService.IsWorkoutTemplateDeletableAsync(id);
    }
    
    private async Task<bool> IsDraftStateAvailableAsync()
    {
        var draftStateResult = await _workoutStateService.GetByValueAsync(WorkoutStateConstants.Draft);
        return draftStateResult.IsSuccess;
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
            .EnsureNotEmpty(newStateId, WorkoutTemplateErrorMessages.InvalidStateIdFormat)
            .EnsureExistsAsync(
                async () => (await _queryDataService.ExistsAsync(id)).Data.Value,
                "WorkoutTemplate")
            .WhenValidAsync(async () => await _commandDataService.ChangeStateAsync(id, newStateId));
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
            .WhenValidAsync(async () => await _commandDataService.SoftDeleteAsync(id));
    }

    public async Task<ServiceResult<BooleanResultDto>> DeleteAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.Build<BooleanResultDto>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .EnsureExistsAsync(
                async () => (await _queryDataService.ExistsAsync(id)).Data.Value,
                "WorkoutTemplate")
            .EnsureAsync(
                async () => await IsWorkoutTemplateDeletableAsync(id),
                ServiceError.ValidationFailed(WorkoutTemplateErrorMessages.CannotDeleteWithExecutionLogs))
            .WhenValidAsync(async () => await _commandDataService.DeleteAsync(id));
    }

    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.Build<BooleanResultDto>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () => await _queryDataService.ExistsAsync(id));
    }

    public async Task<ServiceResult<BooleanResultDto>> ExistsByNameAsync(string name)
    {
        return await ServiceValidate.Build<BooleanResultDto>()
            .EnsureNotWhiteSpace(name, WorkoutTemplateErrorMessages.NameCannotBeEmpty)
            .WhenValidAsync(async () => await _queryDataService.ExistsByNameAsync(name));
    }

    public async Task<ServiceResult<IEnumerable<ExerciseDto>>> GetSuggestedExercisesAsync(
        WorkoutCategoryId categoryId,
        IEnumerable<ExerciseId> existingExerciseIds,
        int maxSuggestions = 10)
    {
        return await ServiceValidate.Build<IEnumerable<ExerciseDto>>()
            .EnsureNotEmpty(categoryId, WorkoutTemplateErrorMessages.CategoryIdRequired)
            .EnsureNumberBetween(maxSuggestions, 1, 50, WorkoutTemplateErrorMessages.MaxSuggestionsRange)
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