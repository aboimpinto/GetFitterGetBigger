using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using ValidationResult = GetFitterGetBigger.API.Services.Results.ValidationResult;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;
using GetFitterGetBigger.API.Extensions;
using Microsoft.EntityFrameworkCore;

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
        
        if (workoutTemplate == null || workoutTemplate.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
                ServiceError.NotFound("Workout template"));
        }
        
        var result = ServiceResult<WorkoutTemplateDto>.Success(MapToDto(workoutTemplate));
        
        return result;
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
        
        var response = new PagedResponse<WorkoutTemplateDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
        
        return ServiceResult<PagedResponse<WorkoutTemplateDto>>.Success(response);
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
        var nameExistsResult = await ExistsByNameAsync(command.Name);
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
            defaultWorkoutStateId);

        var result = entityResult.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
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
        if (existingTemplate == null || existingTemplate.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
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
                CreateEmptyDto(),
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
                CreateEmptyDto(),
                ServiceError.ValidationFailed("Template name is required")),
            false => ServiceResult<WorkoutTemplateDto>.Success(CreateEmptyDto()) // Dummy success to continue
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> CheckNameAndDuplicateAsync(WorkoutTemplateId id, string newName)
    {
        var nameExistsResult = await ExistsByNameAsync(newName);
        var result = nameExistsResult switch
        {
            true => ServiceResult<WorkoutTemplateDto>.Failure(
                CreateEmptyDto(),
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
                CreateEmptyDto(),
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
                CreateEmptyDto(),
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
        var result = string.IsNullOrWhiteSpace(name) switch
        {
            true => false,
            false => await CheckExistsByNameInRepositoryAsync(name)
        };
        
        return result;
    }
    
    private async Task<bool> CheckExistsByNameInRepositoryAsync(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        return await repository.ExistsByNameAsync(name);
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
            errors.Add(WorkoutTemplateErrorMessages.NameRequired);
        else if (command.Name.Length < 3 || command.Name.Length > 100)
            errors.Add(WorkoutTemplateErrorMessages.NameLengthInvalid);

        if (!string.IsNullOrEmpty(command.Description) && command.Description.Length > 1000)
            errors.Add(WorkoutTemplateErrorMessages.DescriptionTooLong);

        if (command.CategoryId.IsEmpty)
            errors.Add(WorkoutTemplateErrorMessages.CategoryRequired);

        if (command.DifficultyId.IsEmpty)
            errors.Add(WorkoutTemplateErrorMessages.DifficultyRequired);

        if (command.EstimatedDurationMinutes < 5 || command.EstimatedDurationMinutes > 300)
            errors.Add(WorkoutTemplateErrorMessages.DurationInvalid);

        if (command.Tags != null && command.Tags.Count > 10)
            errors.Add(WorkoutTemplateErrorMessages.TooManyTags);

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
                errors.Add(WorkoutTemplateErrorMessages.NameLengthInvalid);
        }

        if (!string.IsNullOrEmpty(command.Description) && command.Description.Length > 1000)
            errors.Add(WorkoutTemplateErrorMessages.DescriptionTooLong);

        if (command.CategoryId != null && command.CategoryId.Value.IsEmpty)
            errors.Add(WorkoutTemplateErrorMessages.CategoryIdEmpty);

        if (command.DifficultyId != null && command.DifficultyId.Value.IsEmpty)
            errors.Add(WorkoutTemplateErrorMessages.DifficultyIdEmpty);

        if (command.EstimatedDurationMinutes.HasValue && 
            (command.EstimatedDurationMinutes < 5 || command.EstimatedDurationMinutes > 300))
            errors.Add(WorkoutTemplateErrorMessages.DurationInvalid);

        if (command.Tags != null && command.Tags.Count > 10)
            errors.Add(WorkoutTemplateErrorMessages.TooManyTags);

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

