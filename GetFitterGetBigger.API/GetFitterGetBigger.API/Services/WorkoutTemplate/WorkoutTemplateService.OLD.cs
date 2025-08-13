/*
 * OLD VERSION - REPLACED BY NEW REFACTORED SERVICE
 * This file is kept for reference only and should not be compiled
 * The new implementation no longer uses UnitOfWork or Repository patterns directly
 * Following FEAT-029 architectural changes
 */

#if FALSE // Exclude from compilation

using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Extensions;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Equipment;
using Olimpo.EntityFramework.Persistency;
using GetFitterGetBigger.API.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate;

/// <summary>
/// Service implementation for WorkoutTemplate operations
/// </summary>
public class WorkoutTemplateService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IWorkoutStateService workoutStateService,
    IEquipmentRequirementsService equipmentRequirementsService,
    StateTransitionHandler stateTransitionHandler,
    DuplicationHandler duplicationHandler,
    SearchQueryBuilder searchQueryBuilder,
    SuggestionHandler suggestionHandler,
    ILogger<WorkoutTemplateService> logger) : IWorkoutTemplateService
{

    public async Task<ServiceResult<WorkoutTemplateDto>> GetByIdAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.Build<WorkoutTemplateDto>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () => await LoadWorkoutTemplateByIdAsync(id));
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> LoadWorkoutTemplateByIdAsync(WorkoutTemplateId id)
    {
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var workoutTemplate = await repository.GetByIdWithDetailsAsync(id);
        
        if (workoutTemplate.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.NotFound(WorkoutTemplateErrorMessages.NotFound));
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
        // Delegate to SearchQueryBuilder
        return await searchQueryBuilder.SearchAsync(
            page,
            pageSize,
            namePattern,
            categoryId,
            objectiveId,
            difficultyId,
            stateId,
            sortBy,
            sortOrder);
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
        var draftStateResult = await workoutStateService.GetByValueAsync("Draft");
        
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

        var result = entityResult.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateDto>.Failure(
                WorkoutTemplateDto.Empty,
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors))),
            true => await PersistWorkoutTemplateAsync(entityResult.Value)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateDto>> PersistWorkoutTemplateAsync(WorkoutTemplateEntity workoutTemplate)
    {
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var createdTemplate = await repository.AddAsync(workoutTemplate);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Created workout template {TemplateId}", 
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
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
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
        WorkoutTemplateEntity existingTemplate,
        UpdateWorkoutTemplateCommand command,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId id)
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
        WorkoutTemplateEntity workoutTemplate,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId id)
    {
        var updatedTemplate = await repository.UpdateAsync(workoutTemplate);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Updated workout template {TemplateId}", id);

        return ServiceResult<WorkoutTemplateDto>.Success(updatedTemplate.ToDto());
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> ChangeStateAsync(WorkoutTemplateId id, WorkoutStateId newStateId)
    {
        // Delegate to StateTransitionHandler
        return await stateTransitionHandler.ChangeStateAsync(id, newStateId);
    }

    public async Task<ServiceResult<WorkoutTemplateDto>> DuplicateAsync(WorkoutTemplateId id, string newName)
    {
        // Delegate to DuplicationHandler
        return await duplicationHandler.DuplicateAsync(id, newName);
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
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
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
        using var unitOfWork = unitOfWorkProvider.CreateWritable();
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
        logger.LogInformation(logMessage, id);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> ExistsAsync(WorkoutTemplateId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, WorkoutTemplateErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
            });
    }


    public async Task<bool> ExistsByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
            
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
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
            
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
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
        using var unitOfWork = unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        return await repository.ExistsAsync(id);
    }

    public async Task<ServiceResult<IEnumerable<ExerciseDto>>> GetSuggestedExercisesAsync(
        WorkoutCategoryId categoryId,
        IEnumerable<ExerciseId> existingExerciseIds,
        int maxSuggestions = 10)
    {
        // Delegate to SuggestionHandler
        return await suggestionHandler.GetSuggestedExercisesAsync(
            categoryId,
            existingExerciseIds,
            maxSuggestions);
    }
    
    public async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetRequiredEquipmentAsync(WorkoutTemplateId id)
    {
        // Delegate to specialized equipment requirements service
        return await equipmentRequirementsService.GetRequiredEquipmentAsync(id);
    }


}


#endif // FALSE
