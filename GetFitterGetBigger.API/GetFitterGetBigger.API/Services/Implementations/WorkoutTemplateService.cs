using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Implementations.Extensions;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;
using GetFitterGetBigger.API.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for WorkoutTemplate operations
/// </summary>
public class WorkoutTemplateService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IWorkoutStateService workoutStateService,
    IExerciseService exerciseService,
    IWorkoutTemplateExerciseService workoutTemplateExerciseService,
    ILogger<WorkoutTemplateService> logger) : IWorkoutTemplateService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IWorkoutStateService _workoutStateService = workoutStateService;
    private readonly IExerciseService _exerciseService = exerciseService;
    private readonly IWorkoutTemplateExerciseService _workoutTemplateExerciseService = workoutTemplateExerciseService;
    private readonly ILogger<WorkoutTemplateService> _logger = logger;

    public async Task<ServiceResult<WorkoutTemplateDto>> GetByIdAsync(WorkoutTemplateId id)
    {
        var result = id.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.InvalidFormat("WorkoutTemplateId", "GUID format")),
            false => await LoadWorkoutTemplateByIdAsync(id)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> LoadWorkoutTemplateByIdAsync(WorkoutTemplateId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var workoutTemplate = await repository.GetByIdWithDetailsAsync(id);
        
        if (workoutTemplate == null || workoutTemplate.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.NotFound("Workout template"));
        }
        
        return ServiceResult<WorkoutTemplateDto>.Success(workoutTemplate.ToDto());
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
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        // Build query with filters and sorting (but NOT paging yet)
        var workoutTemplatesQuery = repository
            .GetWorkoutTemplatesQueryable()
            .ApplyNamePatternFilter(namePattern)
            .ApplyCategoryFilter(categoryId)
            .ApplyDifficultyFilter(difficultyId)
            .ApplyObjectiveFilter(objectiveId);
        
        // Apply state filter or exclude archived by default
        workoutTemplatesQuery = stateId.IsEmpty 
            ? workoutTemplatesQuery.ExcludeArchived()
            : workoutTemplatesQuery.ApplyStateFilter(stateId);
            
        workoutTemplatesQuery = workoutTemplatesQuery.ApplySorting(sortBy, sortOrder);
        
        // Log for SQL verification (will help verify EF Core behavior)
        _logger.LogDebug("Executing count query for workout templates search");
        var totalCount = await workoutTemplatesQuery.CountAsync();
        _logger.LogDebug("Count query completed. Total: {TotalCount}", totalCount);
        
        // Now apply paging and execute
        _logger.LogDebug("Executing paged query for workout templates");
        var items = await workoutTemplatesQuery
            .ApplyPaging(page, pageSize)
            .ToListAsync();
        _logger.LogDebug("Paged query completed. Retrieved: {ItemCount} items", items.Count);
        
        PagedResponse<WorkoutTemplateDto> response = new()
        {
            Items = items.Select(t => t.ToDto()).ToList(),
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
        
        return ServiceResult<PagedResponse<WorkoutTemplateDto>>.Success(response);
    }



    public async Task<ServiceResult<WorkoutTemplateDto>> CreateAsync(CreateWorkoutTemplateCommand command)
    {
        // Handle null command for test compatibility
        if (command == null)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed("Create command cannot be null"));
        }

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
            // Dependent validations only run if above pass
            .WhenValid()
            .EnsureNameIsUniqueWhenValidAsync(
                async () => await IsNameUniqueAsync(command.Name),
                "WorkoutTemplate",
                command.Name!)
            .MatchAsync(
                whenValid: async () => await ProcessCreateWorkoutTemplateAsync(command)
            );
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> ProcessCreateWorkoutTemplateAsync(CreateWorkoutTemplateCommand command)
    {
        return await CreateWorkoutTemplateEntityAsync(command);
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> CreateWorkoutTemplateEntityAsync(CreateWorkoutTemplateCommand command)
    {
        // Use service layer to get Draft state (follows proper UnitOfWork pattern)
        var draftStateResult = await _workoutStateService.GetByValueAsync("Draft");
        
        if (!draftStateResult.IsSuccess)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.InternalError("Draft workout state not found in database"));
        }
        
        var defaultWorkoutStateId = WorkoutStateId.ParseOrEmpty(draftStateResult.Data.Id);
        
        var entityResult = WorkoutTemplate.Handler.CreateNew(
            command.Name,
            command.Description,
            command.CategoryId,
            command.DifficultyId,
            command.EstimatedDurationMinutes,
            command.Tags,
            command.IsPublic,
            defaultWorkoutStateId);

        var result = entityResult.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors))),
            true => await PersistWorkoutTemplateAsync(entityResult.Value)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> PersistWorkoutTemplateAsync(WorkoutTemplate workoutTemplate)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var createdTemplate = await repository.AddAsync(workoutTemplate);
        await unitOfWork.CommitAsync();

        _logger.LogInformation("Created workout template {TemplateId}", 
            createdTemplate.Id);

        return ServiceResult<WorkoutTemplateDto>.Success(createdTemplate.ToDto());
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> UpdateAsync(WorkoutTemplateId id, UpdateWorkoutTemplateCommand command)
    {
        // Handle null command for test compatibility
        if (command == null)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed("Update command cannot be null"));
        }

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
            // Dependent validations only run if above pass
            .WhenValid()
            .EnsureAsyncWhenValid(
                async () => await WorkoutTemplateExistsAsync(id),
                ServiceError.NotFound("WorkoutTemplate", id.ToString()))
            .EnsureNameIsUniqueWhenValidAsync(
                async () => await IsNameUniqueForUpdateAsync(command.Name, id),
                "WorkoutTemplate",
                command.Name!)
            .MatchAsync(
                whenValid: async () => await ProcessUpdateWorkoutTemplateAsync(id, command)
            );
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> ProcessUpdateWorkoutTemplateAsync(WorkoutTemplateId id, UpdateWorkoutTemplateCommand command)
    {
        return await UpdateWorkoutTemplateEntityAsync(id, command);
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> UpdateWorkoutTemplateEntityAsync(WorkoutTemplateId id, UpdateWorkoutTemplateCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var existingTemplate = await repository.GetByIdAsync(id);
        if (existingTemplate == null || existingTemplate.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.NotFound("Workout template"));
        }
        
        var result = await PerformWorkoutTemplateUpdateAsync(repository, existingTemplate, command, unitOfWork, id);
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> PerformWorkoutTemplateUpdateAsync(
        IWorkoutTemplateRepository repository,
        WorkoutTemplate existingTemplate,
        UpdateWorkoutTemplateCommand command,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId id)
    {
        var entityResult = WorkoutTemplate.Handler.Update(
            existingTemplate,
            command.Name,
            command.Description,
            command.CategoryId,
            command.DifficultyId,
            command.EstimatedDurationMinutes,
            command.Tags,
            command.IsPublic);

        var result = entityResult.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors))),
            true => await SaveUpdatedWorkoutTemplateAsync(repository, entityResult.Value, unitOfWork, id)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> SaveUpdatedWorkoutTemplateAsync(
        IWorkoutTemplateRepository repository,
        WorkoutTemplate workoutTemplate,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId id)
    {
        var updatedTemplate = await repository.UpdateAsync(workoutTemplate);
        await unitOfWork.CommitAsync();

        _logger.LogInformation("Updated workout template {TemplateId}", id);

        return ServiceResult<WorkoutTemplateDto>.Success(updatedTemplate.ToDto());
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> ChangeStateAsync(WorkoutTemplateId id, WorkoutStateId newStateId)
    {
        var existingResult = await GetByIdAsync(id);
        var result = existingResult.IsSuccess switch
        {
            false => existingResult,
            true => await ProcessChangeStateAsync(id, newStateId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> ProcessChangeStateAsync(WorkoutTemplateId id, WorkoutStateId newStateId)
    {
        var result = newStateId.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.InvalidFormat("WorkoutStateId", "Expected format: 'workoutstate-{guid}' (e.g., 'workoutstate-02000001-0000-0000-0000-000000000002' for Production)")),
            false => await ChangeWorkoutTemplateStateAsync(id, newStateId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> ChangeWorkoutTemplateStateAsync(WorkoutTemplateId id, WorkoutStateId newStateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var existingTemplate = await repository.GetByIdAsync(id);
        if (existingTemplate == null || existingTemplate.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.NotFound("Workout template"));
        }
        
        var result = await PerformStateChangeAsync(repository, existingTemplate, newStateId, unitOfWork, id);
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> PerformStateChangeAsync(
        IWorkoutTemplateRepository repository,
        WorkoutTemplate existingTemplate,
        WorkoutStateId newStateId,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId id)
    {
        var entityResult = WorkoutTemplate.Handler.ChangeState(existingTemplate, newStateId);
        var result = entityResult.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors))),
            true => await SaveStateChangeAsync(repository, entityResult.Value, unitOfWork, id, newStateId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> SaveStateChangeAsync(
        IWorkoutTemplateRepository repository,
        WorkoutTemplate workoutTemplate,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId id,
        WorkoutStateId newStateId)
    {
        var updatedTemplate = await repository.UpdateAsync(workoutTemplate);
        await unitOfWork.CommitAsync();

        _logger.LogInformation("Changed state of workout template {TemplateId} to {NewState}", id, newStateId);

        return ServiceResult<WorkoutTemplateDto>.Success(updatedTemplate.ToDto());
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> DuplicateAsync(WorkoutTemplateId id, string newName)
    {
        var originalResult = await GetByIdAsync(id);
        var result = originalResult.IsSuccess switch
        {
            false => originalResult,
            true => await ProcessDuplicateAsync(id, newName)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> ProcessDuplicateAsync(WorkoutTemplateId id, string newName)
    {
        var validationResult = ValidateDuplicateParameters(newName);
        var result = validationResult.IsSuccess switch
        {
            false => validationResult,
            true => await CheckNameAndDuplicateAsync(id, newName)
        };
        
        return result;
    }
    
    private ServiceResult<WorkoutTemplateDto> ValidateDuplicateParameters(string newName)
    {
        var result = string.IsNullOrWhiteSpace(newName) switch
        {
            true => ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed("Template name is required")),
            false => ServiceResult<WorkoutTemplateDto>.Success(WorkoutTemplateDto.Empty) // Dummy success to continue
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> CheckNameAndDuplicateAsync(WorkoutTemplateId id, string newName)
    {
        var nameExistsResult = await ExistsByNameAsync(newName);
        var result = nameExistsResult switch
        {
            true => ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.AlreadyExists("WorkoutTemplate", newName)),
            false => await PerformDuplicateAsync(id, newName)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> PerformDuplicateAsync(WorkoutTemplateId id, string newName)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var originalTemplate = await repository.GetByIdWithDetailsAsync(id);
        if (originalTemplate == null || originalTemplate.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.NotFound("Original workout template"));
        }
        
        var result = await CreateDuplicateAsync(repository, originalTemplate, newName, unitOfWork, id);
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> CreateDuplicateAsync(
        IWorkoutTemplateRepository repository,
        WorkoutTemplate originalTemplate,
        string newName,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId originalId)
    {
        var entityResult = WorkoutTemplate.Handler.Duplicate(originalTemplate, newName);
        var result = entityResult.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors))),
            true => await SaveDuplicateAsync(repository, entityResult.Value, unitOfWork, originalId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> SaveDuplicateAsync(
        IWorkoutTemplateRepository repository,
        WorkoutTemplate duplicateTemplate,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId originalId)
    {
        var duplicatedTemplate = await repository.AddAsync(duplicateTemplate);
        await unitOfWork.CommitAsync();

        _logger.LogInformation("Duplicated workout template {OriginalId} as {NewId}", 
            originalId, duplicatedTemplate.Id);

        return ServiceResult<WorkoutTemplateDto>.Success(duplicatedTemplate.ToDto());
    }

    public async Task<ServiceResult<bool>> SoftDeleteAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .EnsureAsync(
                async () => (await GetByIdAsync(id)).IsSuccess,
                ServiceError.NotFound("WorkoutTemplate", id.ToString()))
            .MatchAsync(
                whenValid: async () => await PerformSoftDeleteAsync(id),
                whenInvalid: errors => ServiceResult<bool>.Failure(false, errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }
    
    private async Task<ServiceResult<bool>> PerformSoftDeleteAsync(WorkoutTemplateId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var deleteResult = await repository.SoftDeleteAsync(id);
        
        var result = deleteResult switch
        {
            true => await CommitAndLogAsync(unitOfWork, id, "Soft deleted workout template {TemplateId}"),
            false => ServiceResult<bool>.Success(false)
        };
        
        return result;
    }

    public async Task<ServiceResult<bool>> DeleteAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .EnsureAsync(
                async () => (await GetByIdAsync(id)).IsSuccess,
                ServiceError.NotFound("WorkoutTemplate", id.ToString()))
            .MatchAsync(
                whenValid: async () => await PerformDeleteAsync(id),
                whenInvalid: errors => ServiceResult<bool>.Failure(false, errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }
    
    private async Task<ServiceResult<bool>> PerformDeleteAsync(WorkoutTemplateId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var deleteResult = await repository.DeleteAsync(id);
        
        var result = deleteResult switch
        {
            true => await CommitAndLogAsync(unitOfWork, id, "Permanently deleted workout template {TemplateId}"),
            false => ServiceResult<bool>.Success(false)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<bool>> CommitAndLogAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, WorkoutTemplateId id, string logMessage)
    {
        await unitOfWork.CommitAsync();
        _logger.LogInformation(logMessage, id);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> ExistsAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
            });
    }


    public async Task<bool> ExistsByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
            
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        return await repository.ExistsByNameAsync(name);
    }
    
    private async Task<bool> IsNameUniqueAsync(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
            
        // Return true if name is unique (doesn't exist)
        return !await ExistsByNameAsync(name);
    }
    
    private async Task<bool> IsNameUniqueForUpdateAsync(string? name, WorkoutTemplateId excludeId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
            
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        // First check if the name exists
        var nameExists = await repository.ExistsByNameAsync(name);
        if (!nameExists)
            return true; // Name is unique
            
        // If name exists, check if it's the same template we're updating
        // Get all templates to find the one with this name
        var templates = repository.GetWorkoutTemplatesQueryable()
            .Where(t => t.Name == name)
            .Select(t => t.Id)
            .FirstOrDefault();
            
        // If the template with this name is the one we're updating, it's OK
        return !templates.IsEmpty && templates.Equals(excludeId);
    }
    
    private async Task<bool> WorkoutTemplateExistsAsync(WorkoutTemplateId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        return await repository.ExistsAsync(id);
    }

    public async Task<ServiceResult<IEnumerable<ExerciseDto>>> GetSuggestedExercisesAsync(
        WorkoutCategoryId categoryId,
        IEnumerable<ExerciseId> existingExerciseIds,
        int maxSuggestions = 10)
    {
        return await ServiceValidate.Build<IEnumerable<ExerciseDto>>()
            .EnsureNotEmpty(categoryId, "CategoryId is required")
            .Ensure(() => maxSuggestions > 0, ServiceError.ValidationFailed("MaxSuggestions must be greater than 0"))
            .MatchAsync(
                whenValid: async () => await LoadSuggestedExercisesAsync(categoryId, existingExerciseIds, maxSuggestions),
                whenInvalid: errors => ServiceResult<IEnumerable<ExerciseDto>>.Failure(
                    [], 
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }
    
    private async Task<ServiceResult<IEnumerable<ExerciseDto>>> LoadSuggestedExercisesAsync(
        WorkoutCategoryId categoryId,
        IEnumerable<ExerciseId> existingExerciseIds,
        int maxSuggestions)
    {
        // Use ExerciseService instead of direct repository access
        var command = new GetExercisesCommand(
            Page: 1,
            PageSize: 100,
            Name: string.Empty,
            SearchTerm: string.Empty,
            DifficultyLevelId: DifficultyLevelId.Empty,
            MuscleGroupIds: [],
            EquipmentIds: [],
            MovementPatternIds: [],
            BodyPartIds: [],
            IncludeInactive: false,
            IsActive: true);
            
        var exercisesResult = await _exerciseService.GetPagedAsync(command);
        // PagedResponse doesn't have error handling, so we assume it always succeeds
        
        var existingIdSet = existingExerciseIds.ToHashSet();
        
        // Filter out existing exercises and take the requested amount
        // Note: This is a basic implementation. A more sophisticated algorithm would:
        // - Consider muscle group distribution
        // - Match exercises to workout category characteristics
        // - Consider difficulty levels and progression
        var suggestedExercises = exercisesResult.Data.Items
            .Where(exercise => !existingIdSet.Contains(ExerciseId.ParseOrEmpty(exercise.Id)))
            .Take(maxSuggestions)
            .ToList();
        
        return ServiceResult<IEnumerable<ExerciseDto>>.Success(suggestedExercises);
    }
    
    public async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetRequiredEquipmentAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.Build<IEnumerable<EquipmentDto>>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .EnsureExistsAsync(
                async () => await WorkoutTemplateExistsAsync(id),
                "WorkoutTemplate")
            .MatchAsync(
                whenValid: async () => await LoadRequiredEquipmentAsync(id),
                whenInvalid: errors => ServiceResult<IEnumerable<EquipmentDto>>.Failure(
                    [], 
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }
    
    private async Task<ServiceResult<IEnumerable<EquipmentDto>>> LoadRequiredEquipmentAsync(WorkoutTemplateId id)
    {
        // Get all template exercises using WorkoutTemplateExerciseService
        var templateExercisesResult = await _workoutTemplateExerciseService.GetByWorkoutTemplateAsync(id);
        if (!templateExercisesResult.IsSuccess)
        {
            return ServiceResult<IEnumerable<EquipmentDto>>.Failure(
                [],
                templateExercisesResult.Errors);
        }
        
        var templateExercises = templateExercisesResult.Data.WarmupExercises
            .Concat(templateExercisesResult.Data.MainExercises)
            .Concat(templateExercisesResult.Data.CooldownExercises);
            
        if (!templateExercises.Any())
        {
            return ServiceResult<IEnumerable<EquipmentDto>>.Success([]);
        }
        
        return await ExtractEquipmentFromExercisesAsync(templateExercises);
    }
    
    private async Task<ServiceResult<IEnumerable<EquipmentDto>>> ExtractEquipmentFromExercisesAsync(
        IEnumerable<WorkoutTemplateExerciseDto> templateExercises)
    {
        // Get all unique exercise IDs
        var exerciseIds = templateExercises
            .Select(te => ExerciseId.ParseOrEmpty(te.Exercise.Id))
            .Where(id => !id.IsEmpty)
            .Distinct()
            .ToList();
        
        // Get all exercises with their equipment using ExerciseService
        var equipmentSet = new HashSet<EquipmentDto>(new EquipmentDtoComparer());
        
        foreach (var exerciseId in exerciseIds)
        {
            var exerciseResult = await _exerciseService.GetByIdAsync(exerciseId);
            if (!exerciseResult.IsSuccess)
            {
                // Log warning but continue - one exercise failing shouldn't fail the whole operation
                _logger.LogWarning("Failed to get exercise {ExerciseId} for equipment extraction", exerciseId);
                continue;
            }
            
            var exerciseDto = exerciseResult.Data;
            if (!exerciseDto.IsEmpty && exerciseDto.Equipment?.Any() == true)
            {
                foreach (var equipmentRef in exerciseDto.Equipment)
                {
                    var equipmentDto = MapReferenceDataToEquipmentDto(equipmentRef);
                    equipmentSet.Add(equipmentDto);
                }
            }
        }
        
        return ServiceResult<IEnumerable<EquipmentDto>>.Success(equipmentSet.OrderBy(e => e.Name));
    }
    
    private static EquipmentDto MapReferenceDataToEquipmentDto(ReferenceDataDto equipmentRef)
    {
        return new()
        {
            Id = equipmentRef.Id,
            Name = equipmentRef.Value,
            IsActive = true, // Assume active since it's in the exercise
            CreatedAt = DateTime.UtcNow
        };
    }
    
    
    // Custom comparer to avoid duplicate equipment based on ID
    private class EquipmentDtoComparer : IEqualityComparer<EquipmentDto>
    {
        public bool Equals(EquipmentDto? x, EquipmentDto? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Id == y.Id;
        }
        
        public int GetHashCode(EquipmentDto obj)
        {
            return obj.Id?.GetHashCode() ?? 0;
        }
    }


}

