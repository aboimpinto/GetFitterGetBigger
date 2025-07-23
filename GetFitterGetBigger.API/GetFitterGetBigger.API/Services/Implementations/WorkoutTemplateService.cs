using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using ValidationResult = GetFitterGetBigger.API.Services.Results.ValidationResult;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for WorkoutTemplate operations
/// </summary>
public class WorkoutTemplateService : IWorkoutTemplateService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IWorkoutStateService _workoutStateService;
    private readonly IExerciseService _exerciseService;
    private readonly IWorkoutTemplateExerciseService _workoutTemplateExerciseService;
    private readonly ILogger<WorkoutTemplateService> _logger;

    public WorkoutTemplateService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IWorkoutStateService workoutStateService,
        IExerciseService exerciseService,
        IWorkoutTemplateExerciseService workoutTemplateExerciseService,
        ILogger<WorkoutTemplateService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _workoutStateService = workoutStateService;
        _exerciseService = exerciseService;
        _workoutTemplateExerciseService = workoutTemplateExerciseService;
        _logger = logger;
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> GetByIdAsync(WorkoutTemplateId id)
    {
        var result = id.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
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
        
        var result = workoutTemplate.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.NotFound("Workout template not found")),
            false => ServiceResult<WorkoutTemplateDto>.Success(MapToDto(workoutTemplate))
        };
        
        return result;
    }

    public async Task<PagedResponse<WorkoutTemplateDto>> GetPagedAsync(
        int pageNumber = 1, 
        int pageSize = 20,
        WorkoutCategoryId? categoryFilter = null,
        DifficultyLevelId? difficultyFilter = null)
    {
        var result = await LoadPagedWorkoutTemplatesAsync(pageNumber, pageSize);
        
        return result;
    }
    
    private PagedResponse<WorkoutTemplateDto> CreateEmptyPagedResponse(int pageNumber, int pageSize) =>
        new()
        {
            Items = new List<WorkoutTemplateDto>(),
            TotalCount = 0,
            CurrentPage = pageNumber,
            PageSize = pageSize
        };
    
    private async Task<PagedResponse<WorkoutTemplateDto>> LoadPagedWorkoutTemplatesAsync(
        int pageNumber, 
        int pageSize)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var (workoutTemplates, totalCount) = await repository.GetPagedAsync(
            pageNumber, pageSize);

        var result = new PagedResponse<WorkoutTemplateDto>
        {
            Items = workoutTemplates.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            CurrentPage = pageNumber,
            PageSize = pageSize
        };
        
        return result;
    }

    public async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetAllActiveByCreatorAsync(UserId creatorId)
    {
        var result = creatorId.IsEmpty switch
        {
            true => ServiceResult<IEnumerable<WorkoutTemplateDto>>.Success(new List<WorkoutTemplateDto>()),
            false => await LoadAllActiveByCreatorAsync(creatorId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> LoadAllActiveByCreatorAsync(UserId creatorId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var workoutTemplates = await repository.GetAllActiveByCreatorAsync(creatorId);
        
        var result = ServiceResult<IEnumerable<WorkoutTemplateDto>>.Success(
            workoutTemplates.Select(MapToDto));
            
        return result;
    }

    public async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetByNamePatternAsync(
        string namePattern, 
        UserId? creatorFilter = null)
    {
        var result = string.IsNullOrWhiteSpace(namePattern) switch
        {
            true => ServiceResult<IEnumerable<WorkoutTemplateDto>>.Success(new List<WorkoutTemplateDto>()),
            false => await LoadByNamePatternAsync(namePattern, creatorFilter)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> LoadByNamePatternAsync(
        string namePattern, 
        UserId? creatorFilter)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var workoutTemplates = await repository.GetByNamePatternAsync(namePattern, creatorFilter ?? UserId.Empty);
        
        var result = ServiceResult<IEnumerable<WorkoutTemplateDto>>.Success(
            workoutTemplates.Select(MapToDto));
            
        return result;
    }

    public async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetByCategoryAsync(
        WorkoutCategoryId categoryId, 
        UserId? creatorFilter = null,
        bool includeInactive = false)
    {
        var result = categoryId.IsEmpty switch
        {
            true => ServiceResult<IEnumerable<WorkoutTemplateDto>>.Failure(
                new List<WorkoutTemplateDto>(),
                ServiceError.InvalidFormat("CategoryId", "GUID format")),
            false => await LoadByCategoryAsync(categoryId, creatorFilter, includeInactive)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> LoadByCategoryAsync(
        WorkoutCategoryId categoryId,
        UserId? creatorFilter,
        bool includeInactive)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var workoutTemplates = await repository.GetByCategoryAsync(categoryId, creatorFilter ?? UserId.Empty, includeInactive);
        
        var result = ServiceResult<IEnumerable<WorkoutTemplateDto>>.Success(
            workoutTemplates.Select(MapToDto));
            
        return result;
    }

    public async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetByObjectiveAsync(
        WorkoutObjectiveId objectiveId,
        UserId? creatorFilter = null,
        bool includeInactive = false)
    {
        var result = objectiveId.IsEmpty switch
        {
            true => ServiceResult<IEnumerable<WorkoutTemplateDto>>.Failure(
                new List<WorkoutTemplateDto>(),
                ServiceError.InvalidFormat("ObjectiveId", "GUID format")),
            false => await LoadByObjectiveAsync(objectiveId, creatorFilter, includeInactive)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> LoadByObjectiveAsync(
        WorkoutObjectiveId objectiveId,
        UserId? creatorFilter,
        bool includeInactive)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var workoutTemplates = await repository.GetByObjectiveAsync(objectiveId, creatorFilter ?? UserId.Empty, includeInactive);
        
        var result = ServiceResult<IEnumerable<WorkoutTemplateDto>>.Success(
            workoutTemplates.Select(MapToDto));
            
        return result;
    }

    public async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetByDifficultyAsync(
        DifficultyLevelId difficultyId,
        UserId? creatorFilter = null,
        bool includeInactive = false)
    {
        var result = difficultyId.IsEmpty switch
        {
            true => ServiceResult<IEnumerable<WorkoutTemplateDto>>.Failure(
                new List<WorkoutTemplateDto>(),
                ServiceError.InvalidFormat("DifficultyId", "GUID format")),
            false => await LoadByDifficultyAsync(difficultyId, creatorFilter, includeInactive)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> LoadByDifficultyAsync(
        DifficultyLevelId difficultyId,
        UserId? creatorFilter,
        bool includeInactive)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var workoutTemplates = await repository.GetByDifficultyAsync(difficultyId, creatorFilter ?? UserId.Empty, includeInactive);
        
        var result = ServiceResult<IEnumerable<WorkoutTemplateDto>>.Success(
            workoutTemplates.Select(MapToDto));
            
        return result;
    }

    public async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> GetByExerciseAsync(
        ExerciseId exerciseId,
        UserId? creatorFilter = null,
        bool includeInactive = false)
    {
        var result = exerciseId.IsEmpty switch
        {
            true => ServiceResult<IEnumerable<WorkoutTemplateDto>>.Failure(
                new List<WorkoutTemplateDto>(),
                ServiceError.InvalidFormat("ExerciseId", "GUID format")),
            false => await LoadByExerciseAsync(exerciseId, creatorFilter, includeInactive)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<WorkoutTemplateDto>>> LoadByExerciseAsync(
        ExerciseId exerciseId,
        UserId? creatorFilter,
        bool includeInactive)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var workoutTemplates = await repository.GetByExerciseAsync(exerciseId, creatorFilter ?? UserId.Empty, includeInactive);
        
        var result = ServiceResult<IEnumerable<WorkoutTemplateDto>>.Success(
            workoutTemplates.Select(MapToDto));
            
        return result;
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> CreateAsync(CreateWorkoutTemplateCommand command)
    {
        var validationResult = await ValidateCreateCommandAsync(command);
        var result = validationResult.IsValid switch
        {
            false => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.ValidationFailed(string.Join(", ", validationResult.Errors))),
            true => await ProcessCreateWorkoutTemplateAsync(command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> ProcessCreateWorkoutTemplateAsync(CreateWorkoutTemplateCommand command)
    {
        var nameExistsResult = await ExistsByNameAsync(command.Name, command.CreatedBy);
        var result = nameExistsResult switch
        {
            true => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.AlreadyExists("WorkoutTemplate", command.Name)),
            false => await CreateWorkoutTemplateEntityAsync(command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> CreateWorkoutTemplateEntityAsync(CreateWorkoutTemplateCommand command)
    {
        // Use service layer to get Draft state (follows proper UnitOfWork pattern)
        var draftStateResult = await _workoutStateService.GetByValueAsync("Draft");
        
        if (!draftStateResult.IsSuccess)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
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
            command.CreatedBy,
            defaultWorkoutStateId);

        var result = entityResult.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors))),
            true => await PersistWorkoutTemplateAsync(entityResult.Value, command.CreatedBy)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> PersistWorkoutTemplateAsync(WorkoutTemplate workoutTemplate, UserId creatorId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var createdTemplate = await repository.AddAsync(workoutTemplate);
        await unitOfWork.CommitAsync();

        _logger.LogInformation("Created workout template {TemplateId} for creator {CreatorId}", 
            createdTemplate.Id, creatorId);

        return ServiceResult<WorkoutTemplateDto>.Success(MapToDto(createdTemplate));
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> UpdateAsync(WorkoutTemplateId id, UpdateWorkoutTemplateCommand command)
    {
        var existingResult = await GetByIdAsync(id);
        var result = existingResult.IsSuccess switch
        {
            false => existingResult,
            true => await ProcessUpdateWorkoutTemplateAsync(id, command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> ProcessUpdateWorkoutTemplateAsync(WorkoutTemplateId id, UpdateWorkoutTemplateCommand command)
    {
        var validationResult = await ValidateUpdateCommandAsync(id, command);
        var result = validationResult.IsValid switch
        {
            false => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.ValidationFailed(string.Join(", ", validationResult.Errors))),
            true => await UpdateWorkoutTemplateEntityAsync(id, command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> UpdateWorkoutTemplateEntityAsync(WorkoutTemplateId id, UpdateWorkoutTemplateCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var existingTemplate = await repository.GetByIdAsync(id);
        var result = existingTemplate.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.NotFound("Workout template not found")),
            false => await PerformWorkoutTemplateUpdateAsync(repository, existingTemplate, command, unitOfWork, id)
        };
        
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
                CreateEmptyDto(),
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

        return ServiceResult<WorkoutTemplateDto>.Success(MapToDto(updatedTemplate));
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
                CreateEmptyDto(),
                ServiceError.InvalidFormat("WorkoutStateId", "GUID format")),
            false => await ChangeWorkoutTemplateStateAsync(id, newStateId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> ChangeWorkoutTemplateStateAsync(WorkoutTemplateId id, WorkoutStateId newStateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var existingTemplate = await repository.GetByIdAsync(id);
        var result = existingTemplate.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.NotFound("Workout template not found")),
            false => await PerformStateChangeAsync(repository, existingTemplate, newStateId, unitOfWork, id)
        };
        
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
                CreateEmptyDto(),
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

        return ServiceResult<WorkoutTemplateDto>.Success(MapToDto(updatedTemplate));
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> DuplicateAsync(WorkoutTemplateId id, string newName, UserId creatorId)
    {
        var originalResult = await GetByIdAsync(id);
        var result = originalResult.IsSuccess switch
        {
            false => originalResult,
            true => await ProcessDuplicateAsync(id, newName, creatorId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> ProcessDuplicateAsync(WorkoutTemplateId id, string newName, UserId creatorId)
    {
        var validationResult = ValidateDuplicateParameters(newName, creatorId);
        var result = validationResult.IsSuccess switch
        {
            false => validationResult,
            true => await CheckNameAndDuplicateAsync(id, newName, creatorId)
        };
        
        return result;
    }
    
    private ServiceResult<WorkoutTemplateDto> ValidateDuplicateParameters(string newName, UserId creatorId)
    {
        var result = (string.IsNullOrWhiteSpace(newName), creatorId.IsEmpty) switch
        {
            (true, _) => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.ValidationFailed("Template name is required")),
            (false, true) => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.ValidationFailed("Creator ID is required")),
            _ => ServiceResult<WorkoutTemplateDto>.Success(CreateEmptyDto()) // Dummy success to continue
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> CheckNameAndDuplicateAsync(WorkoutTemplateId id, string newName, UserId creatorId)
    {
        var nameExistsResult = await ExistsByNameAsync(newName, creatorId);
        var result = nameExistsResult switch
        {
            true => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.AlreadyExists("WorkoutTemplate", newName)),
            false => await PerformDuplicateAsync(id, newName, creatorId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> PerformDuplicateAsync(WorkoutTemplateId id, string newName, UserId creatorId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var originalTemplate = await repository.GetByIdWithDetailsAsync(id);
        var result = originalTemplate.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.NotFound("Original workout template not found")),
            false => await CreateDuplicateAsync(repository, originalTemplate, newName, creatorId, unitOfWork, id)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> CreateDuplicateAsync(
        IWorkoutTemplateRepository repository,
        WorkoutTemplate originalTemplate,
        string newName,
        UserId creatorId,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId originalId)
    {
        var entityResult = WorkoutTemplate.Handler.Duplicate(originalTemplate, newName, creatorId);
        var result = entityResult.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors))),
            true => await SaveDuplicateAsync(repository, entityResult.Value, unitOfWork, originalId, creatorId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> SaveDuplicateAsync(
        IWorkoutTemplateRepository repository,
        WorkoutTemplate duplicateTemplate,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId originalId,
        UserId creatorId)
    {
        var duplicatedTemplate = await repository.AddAsync(duplicateTemplate);
        await unitOfWork.CommitAsync();

        _logger.LogInformation("Duplicated workout template {OriginalId} as {NewId} for creator {CreatorId}", 
            originalId, duplicatedTemplate.Id, creatorId);

        return ServiceResult<WorkoutTemplateDto>.Success(MapToDto(duplicatedTemplate));
    }

    public async Task<ServiceResult<bool>> SoftDeleteAsync(WorkoutTemplateId id)
    {
        var existingResult = await GetByIdAsync(id);
        
        var result = existingResult.IsSuccess switch
        {
            false => ServiceResult<bool>.Failure(false, existingResult.Errors),
            true => await PerformSoftDeleteAsync(id)
        };
        
        return result;
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
        var existingResult = await GetByIdAsync(id);
        
        var result = existingResult.IsSuccess switch
        {
            false => ServiceResult<bool>.Failure(false, existingResult.Errors),
            true => await PerformDeleteAsync(id)
        };
        
        return result;
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

    public async Task<bool> ExistsAsync(WorkoutTemplateId id)
    {
        var result = id.IsEmpty switch
        {
            true => false,
            false => await CheckExistsInRepositoryAsync(id)
        };
        
        return result;
    }
    
    private async Task<bool> CheckExistsInRepositoryAsync(WorkoutTemplateId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        return await repository.ExistsAsync(id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var workoutTemplateId = WorkoutTemplateId.ParseOrEmpty(id);
        return await ExistsAsync(workoutTemplateId);
    }

    public async Task<bool> ExistsByNameAsync(string name, UserId creatorId)
    {
        var result = (string.IsNullOrWhiteSpace(name), creatorId.IsEmpty) switch
        {
            (true, _) or (_, true) => false,
            _ => await CheckExistsByNameInRepositoryAsync(name, creatorId)
        };
        
        return result;
    }
    
    private async Task<bool> CheckExistsByNameInRepositoryAsync(string name, UserId creatorId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        return await repository.ExistsByNameAsync(name, creatorId);
    }

    public async Task<ServiceResult<IEnumerable<ExerciseDto>>> GetSuggestedExercisesAsync(
        WorkoutCategoryId categoryId,
        IEnumerable<ExerciseId> existingExerciseIds,
        int maxSuggestions = 10)
    {
        var result = (categoryId.IsEmpty, maxSuggestions <= 0) switch
        {
            (true, _) => ServiceResult<IEnumerable<ExerciseDto>>.Failure(
                new List<ExerciseDto>(),
                ServiceError.InvalidFormat("CategoryId", "GUID format")),
            (_, true) => ServiceResult<IEnumerable<ExerciseDto>>.Failure(
                new List<ExerciseDto>(),
                ServiceError.ValidationFailed("MaxSuggestions must be greater than 0")),
            _ => await LoadSuggestedExercisesAsync(categoryId, existingExerciseIds, maxSuggestions)
        };
        
        return result;
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
            MuscleGroupIds: new List<MuscleGroupId>(),
            EquipmentIds: new List<EquipmentId>(),
            MovementPatternIds: new List<MovementPatternId>(),
            BodyPartIds: new List<BodyPartId>(),
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
        var suggestedExercises = exercisesResult.Items
            .Where(exercise => !existingIdSet.Contains(ExerciseId.ParseOrEmpty(exercise.Id)))
            .Take(maxSuggestions)
            .ToList();
        
        return ServiceResult<IEnumerable<ExerciseDto>>.Success(suggestedExercises);
    }
    
    private ExerciseDto MapExerciseToDto(Exercise exercise) => new()
    {
        Id = exercise.Id.ToString(),
        Name = exercise.Name,
        Description = exercise.Description,
        // Map CoachNotes if available
        CoachNotes = exercise.CoachNotes?.Select(cn => new CoachNoteDto
        {
            Id = cn.Id.ToString(),
            Text = cn.Text,
            Order = cn.Order
        }).ToList() ?? new List<CoachNoteDto>()
    };

    public async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetRequiredEquipmentAsync(WorkoutTemplateId id)
    {
        var templateResult = await GetByIdAsync(id);
        if (!templateResult.IsSuccess)
        {
            return ServiceResult<IEnumerable<EquipmentDto>>.Failure(
                new List<EquipmentDto>(),
                templateResult.Errors);
        }

        // Get all template exercises using WorkoutTemplateExerciseService
        var templateExercisesResult = await _workoutTemplateExerciseService.GetByWorkoutTemplateAsync(id);
        if (!templateExercisesResult.IsSuccess)
        {
            return ServiceResult<IEnumerable<EquipmentDto>>.Failure(
                new List<EquipmentDto>(),
                templateExercisesResult.Errors);
        }
        
        var templateExercises = templateExercisesResult.Data.WarmupExercises
            .Concat(templateExercisesResult.Data.MainExercises)
            .Concat(templateExercisesResult.Data.CooldownExercises);
        if (!templateExercises.Any())
        {
            return ServiceResult<IEnumerable<EquipmentDto>>.Success(new List<EquipmentDto>());
        }
        
        // Get all unique exercise IDs
        var exerciseIds = templateExercises.Select(te => ExerciseId.ParseOrEmpty(te.Exercise.Id)).Distinct().ToList();
        
        // Get all exercises with their equipment using ExerciseService
        var equipmentSet = new HashSet<EquipmentDto>(new EquipmentDtoComparer());
        
        foreach (var exerciseId in exerciseIds)
        {
            var exerciseDto = await _exerciseService.GetByIdAsync(exerciseId);
            if (!exerciseDto.IsEmpty && exerciseDto.Equipment?.Any() == true)
            {
                foreach (var equipmentRef in exerciseDto.Equipment)
                {
                    var equipmentDto = new EquipmentDto
                    {
                        Id = equipmentRef.Id,
                        Name = equipmentRef.Value,
                        IsActive = true, // Assume active since it's in the exercise
                        CreatedAt = DateTime.UtcNow
                    };
                    equipmentSet.Add(equipmentDto);
                }
            }
        }
        
        return ServiceResult<IEnumerable<EquipmentDto>>.Success(equipmentSet.OrderBy(e => e.Name));
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

    #region Private Methods

    private static WorkoutTemplateDto CreateEmptyDto() => WorkoutTemplateDto.Empty;

    private static WorkoutTemplateDto MapToDto(WorkoutTemplate workoutTemplate) =>
        new()
        {
            Id = workoutTemplate.Id.ToString(),
            Name = workoutTemplate.Name,
            Description = workoutTemplate.Description,
            Category = MapToReferenceDataDto(workoutTemplate.Category),
            Difficulty = MapToReferenceDataDto(workoutTemplate.Difficulty),
            EstimatedDurationMinutes = workoutTemplate.EstimatedDurationMinutes,
            Tags = workoutTemplate.Tags.ToList(),
            IsPublic = workoutTemplate.IsPublic,
            CreatedBy = workoutTemplate.CreatedBy.ToString(),
            WorkoutState = MapToReferenceDataDto(workoutTemplate.WorkoutState),
            Objectives = workoutTemplate.Objectives?.Select(o => MapToReferenceDataDto(o.WorkoutObjective)).ToList() ?? new(),
            Exercises = workoutTemplate.Exercises?.Select(MapToWorkoutTemplateExerciseDto).ToList() ?? new(),
            CreatedAt = workoutTemplate.CreatedAt,
            UpdatedAt = workoutTemplate.UpdatedAt
        };

    private static ReferenceDataDto MapToReferenceDataDto(dynamic? entity)
    {
        if (entity == null)
            return ReferenceDataDto.Empty;
            
        try
        {
            bool isEmpty = entity.IsEmpty;
            if (isEmpty)
                return ReferenceDataDto.Empty;
        }
        catch
        {
            // If IsEmpty property doesn't exist, continue with mapping
        }
            
        return new ReferenceDataDto
        {
            Id = entity.Id?.ToString() ?? string.Empty,
            Value = (string?)(entity.Value ?? entity.Name) ?? string.Empty,
            Description = (string?)entity.Description
        };
    }

    private static WorkoutTemplateExerciseDto MapToWorkoutTemplateExerciseDto(WorkoutTemplateExercise exercise) =>
        new()
        {
            Id = exercise.Id.ToString(),
            Exercise = exercise.Exercise != null ? new ExerciseDto 
            { 
                Id = exercise.Exercise.Id.ToString(),
                Name = exercise.Exercise.Name,
                Description = exercise.Exercise.Description
            } : ExerciseDto.Empty,
            Zone = exercise.Zone.ToString(),
            SequenceOrder = exercise.SequenceOrder,
            Notes = exercise.Notes,
            SetConfigurations = exercise.Configurations?.Select(MapToSetConfigurationDto).ToList() ?? new(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static SetConfigurationDto MapToSetConfigurationDto(SetConfiguration config) =>
        new()
        {
            Id = config.Id.ToString(),
            SetNumber = config.SetNumber,
            TargetReps = config.TargetReps,
            TargetWeight = config.TargetWeight,
            TargetTime = config.TargetTimeSeconds,
            RestSeconds = config.RestSeconds,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private Task<ValidationResult> ValidateCreateCommandAsync(CreateWorkoutTemplateCommand command)
    {
        var result = command switch
        {
            null => Task.FromResult(ValidationResult.Failure(new[] { "Create command cannot be null" })),
            _ => Task.FromResult(ValidateCreateCommandDetails(command))
        };
        
        return result;
    }
    
    private ValidationResult ValidateCreateCommandDetails(CreateWorkoutTemplateCommand command)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(command.Name))
            errors.Add("Name is required");
        else if (command.Name.Length < 3 || command.Name.Length > 100)
            errors.Add("Name must be between 3 and 100 characters");

        if (!string.IsNullOrEmpty(command.Description) && command.Description.Length > 1000)
            errors.Add("Description cannot exceed 1000 characters");

        if (command.CategoryId.IsEmpty)
            errors.Add("Category is required");

        if (command.DifficultyId.IsEmpty)
            errors.Add("Difficulty level is required");

        if (command.EstimatedDurationMinutes < 5 || command.EstimatedDurationMinutes > 300)
            errors.Add("Estimated duration must be between 5 and 300 minutes");

        if (command.Tags != null && command.Tags.Count > 10)
            errors.Add("Maximum 10 tags allowed");

        if (command.CreatedBy.IsEmpty)
            errors.Add("Creator is required");

        // TODO: Validate that CategoryId, DifficultyId, and ObjectiveIds exist in the database

        var result = errors.Any() switch
        {
            true => ValidationResult.Failure(errors.ToArray()),
            false => ValidationResult.Success()
        };
        
        return result;
    }

    private Task<ValidationResult> ValidateUpdateCommandAsync(WorkoutTemplateId id, UpdateWorkoutTemplateCommand command)
    {
        var result = command switch
        {
            null => Task.FromResult(ValidationResult.Failure(new[] { "Update command cannot be null" })),
            _ => Task.FromResult(ValidateUpdateCommandDetails(command))
        };
        
        return result;
    }
    
    private ValidationResult ValidateUpdateCommandDetails(UpdateWorkoutTemplateCommand command)
    {
        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(command.Name))
        {
            if (command.Name.Length < 3 || command.Name.Length > 100)
                errors.Add("Name must be between 3 and 100 characters");
        }

        if (!string.IsNullOrEmpty(command.Description) && command.Description.Length > 1000)
            errors.Add("Description cannot exceed 1000 characters");

        if (command.CategoryId != null && command.CategoryId.Value.IsEmpty)
            errors.Add("Category ID cannot be empty");

        if (command.DifficultyId != null && command.DifficultyId.Value.IsEmpty)
            errors.Add("Difficulty level ID cannot be empty");

        if (command.EstimatedDurationMinutes.HasValue && 
            (command.EstimatedDurationMinutes < 5 || command.EstimatedDurationMinutes > 300))
            errors.Add("Estimated duration must be between 5 and 300 minutes");

        if (command.Tags != null && command.Tags.Count > 10)
            errors.Add("Maximum 10 tags allowed");

        // TODO: Validate that CategoryId, DifficultyId, and ObjectiveIds exist in the database

        var result = errors.Any() switch
        {
            true => ValidationResult.Failure(errors.ToArray()),
            false => ValidationResult.Success()
        };
        
        return result;
    }

    #endregion
}

