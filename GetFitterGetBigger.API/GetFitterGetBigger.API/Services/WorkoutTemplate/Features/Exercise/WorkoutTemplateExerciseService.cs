using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using ExerciseEntity = GetFitterGetBigger.API.Models.Entities.Exercise;
using SetConfigurationEntity = GetFitterGetBigger.API.Models.Entities.SetConfiguration;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;

/// <summary>
/// Service implementation for managing exercises within workout templates
/// </summary>
public class WorkoutTemplateExerciseService : IWorkoutTemplateExerciseService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<WorkoutTemplateExerciseService> _logger;

    public WorkoutTemplateExerciseService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<WorkoutTemplateExerciseService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WorkoutTemplateExerciseListDto>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId)
    {
        var result = workoutTemplateId.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateExerciseListDto>.Failure(
                new WorkoutTemplateExerciseListDto(),
                ServiceError.InvalidFormat("WorkoutTemplateId", "GUID format")),
            false => await LoadWorkoutTemplateExercisesAsync(workoutTemplateId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseListDto>> LoadWorkoutTemplateExercisesAsync(WorkoutTemplateId workoutTemplateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exercises = (await repository.GetByWorkoutTemplateAsync(workoutTemplateId)).ToList();

        var dto = new WorkoutTemplateExerciseListDto
        {
            WorkoutTemplateId = workoutTemplateId.ToString(),
            WarmupExercises = exercises
                .Where(e => e.Zone == WorkoutZone.Warmup)
                .OrderBy(e => e.SequenceOrder)
                .Select(MapToDto)
                .ToList(),
            MainExercises = exercises
                .Where(e => e.Zone == WorkoutZone.Main)
                .OrderBy(e => e.SequenceOrder)
                .Select(MapToDto)
                .ToList(),
            CooldownExercises = exercises
                .Where(e => e.Zone == WorkoutZone.Cooldown)
                .OrderBy(e => e.SequenceOrder)
                .Select(MapToDto)
                .ToList(),
            TotalEstimatedDurationMinutes = CalculateEstimatedDuration(exercises)
        };

        return ServiceResult<WorkoutTemplateExerciseListDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> GetByIdAsync(WorkoutTemplateExerciseId exerciseId)
    {
        var result = exerciseId.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.InvalidFormat("WorkoutTemplateExerciseId", "GUID format")),
            false => await LoadWorkoutTemplateExerciseByIdAsync(exerciseId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> LoadWorkoutTemplateExerciseByIdAsync(WorkoutTemplateExerciseId exerciseId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exercise = await repository.GetByIdWithDetailsAsync(exerciseId);

        if (exercise == null || exercise.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.ExerciseNotFound));
        }
        
        var result = ServiceResult<WorkoutTemplateExerciseDto>.Success(MapToDto(exercise));
        
        return result;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> AddExerciseAsync(AddExerciseToTemplateCommand command)
    {
        var result = command switch
        {
            null => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CommandCannotBeNull)),
            _ => await ValidateAndProcessAddExerciseAsync(command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ValidateAndProcessAddExerciseAsync(AddExerciseToTemplateCommand command)
    {
        var validationResult = IsAddExerciseCommandValid(command);
        var result = validationResult.IsSuccess switch
        {
            false => validationResult,
            true => await ProcessAddExerciseAsync(command)
        };
        
        return result;
    }
    
    private ServiceResult<WorkoutTemplateExerciseDto> IsAddExerciseCommandValid(AddExerciseToTemplateCommand command)
    {
        var result = (command.WorkoutTemplateId.IsEmpty, command.ExerciseId.IsEmpty, 
                     string.IsNullOrWhiteSpace(command.Zone), command.UserId.IsEmpty) switch
        {
            (true, _, _, _) or (_, true, _, _) or (_, _, true, _) or (_, _, _, true) => 
                ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                    new WorkoutTemplateExerciseDto(),
                    ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)),
            _ => IsValidZone(command.Zone) switch
            {
                false => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                    new WorkoutTemplateExerciseDto(),
                    ServiceError.ValidationFailed(string.Format(WorkoutTemplateExerciseErrorMessages.InvalidZoneWarmupMainCooldown, command.Zone))),
                true => ServiceResult<WorkoutTemplateExerciseDto>.Success(new WorkoutTemplateExerciseDto()) // Dummy success
            }
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ProcessAddExerciseAsync(AddExerciseToTemplateCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        
        var permissionResult = await ValidateTemplatePermissionsAsync(unitOfWork, command.WorkoutTemplateId, command.UserId);
        var result = permissionResult.IsSuccess switch
        {
            false => permissionResult,
            true => await ValidateExerciseAndCreateAsync(unitOfWork, command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ValidateTemplatePermissionsAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId workoutTemplateId,
        UserId userId)
    {
        var templateRepo = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        var template = await templateRepo.GetByIdAsync(workoutTemplateId);
        
        if (template == null || template.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound));
        }
        
        if (template.WorkoutState?.Value != WorkoutTemplateExerciseErrorMessages.DraftStateRequired)
        {
            return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CanOnlyAddExercisesToDraftTemplates));
        }
        
        var result = ServiceResult<WorkoutTemplateExerciseDto>.Success(new WorkoutTemplateExerciseDto());
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ValidateExerciseAndCreateAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        AddExerciseToTemplateCommand command)
    {
        var exerciseRepo = unitOfWork.GetRepository<IExerciseRepository>();
        var exercise = await exerciseRepo.GetByIdAsync(command.ExerciseId);
        
        if (exercise == null || exercise.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.ExerciseNotFound));
        }
        
        var result = await CreateWorkoutTemplateExerciseAsync(unitOfWork, command);
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> CreateWorkoutTemplateExerciseAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        AddExerciseToTemplateCommand command)
    {
        var exerciseTemplateRepo = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        var sequenceOrder = command.SequenceOrder ?? 
            await exerciseTemplateRepo.GetMaxSequenceOrderAsync(command.WorkoutTemplateId, Enum.Parse<WorkoutZone>(command.Zone)) + 1;

        var workoutTemplateExercise = WorkoutTemplateExercise.Handler.CreateNew(
            command.WorkoutTemplateId,
            command.ExerciseId,
            Enum.Parse<WorkoutZone>(command.Zone),
            sequenceOrder,
            command.Notes);

        var result = workoutTemplateExercise.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.ValidationFailed(string.Join(", ", workoutTemplateExercise.Errors))),
            true => await PersistWorkoutTemplateExerciseAsync(exerciseTemplateRepo, workoutTemplateExercise.Value, unitOfWork)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> PersistWorkoutTemplateExerciseAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateExercise workoutTemplateExercise,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        await repository.AddAsync(workoutTemplateExercise);
        await unitOfWork.CommitAsync();

        var created = await repository.GetByIdWithDetailsAsync(workoutTemplateExercise.Id);
        
        return ServiceResult<WorkoutTemplateExerciseDto>.Success(MapToDto(created));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> UpdateExerciseAsync(UpdateTemplateExerciseCommand command)
    {
        var result = command switch
        {
            null => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CommandCannotBeNull)),
            _ => await ValidateAndProcessUpdateExerciseAsync(command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ValidateAndProcessUpdateExerciseAsync(UpdateTemplateExerciseCommand command)
    {
        var result = (command.WorkoutTemplateExerciseId.IsEmpty, command.UserId.IsEmpty) switch
        {
            (true, _) or (_, true) => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)),
            _ => await ProcessUpdateExerciseAsync(command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ProcessUpdateExerciseAsync(UpdateTemplateExerciseCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var exerciseTemplateRepo = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exerciseTemplate = await exerciseTemplateRepo.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId);
        if (exerciseTemplate == null || exerciseTemplate.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound));
        }
        
        var result = await ValidateAndPerformUpdateAsync(unitOfWork, exerciseTemplate, command, exerciseTemplateRepo);
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ValidateAndPerformUpdateAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateExercise exerciseTemplate,
        UpdateTemplateExerciseCommand command,
        IWorkoutTemplateExerciseRepository exerciseTemplateRepo)
    {
        var templateRepo = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        var template = await templateRepo.GetByIdAsync(exerciseTemplate.WorkoutTemplateId);
        
        var result = template.WorkoutState.Value != WorkoutTemplateExerciseErrorMessages.DraftStateRequired
            ? ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CanOnlyUpdateExercisesInDraftTemplates))
            : await PerformUpdateAsync(exerciseTemplate, command, exerciseTemplateRepo, unitOfWork);
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> PerformUpdateAsync(
        WorkoutTemplateExercise exerciseTemplate,
        UpdateTemplateExerciseCommand command,
        IWorkoutTemplateExerciseRepository exerciseTemplateRepo,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var updated = WorkoutTemplateExercise.Handler.UpdateNotes(exerciseTemplate, command.Notes);
        var result = updated.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.ValidationFailed(string.Join(", ", updated.Errors))),
            true => await SaveUpdatedExerciseAsync(updated.Value, exerciseTemplateRepo, unitOfWork)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> SaveUpdatedExerciseAsync(
        WorkoutTemplateExercise updatedExercise,
        IWorkoutTemplateExerciseRepository exerciseTemplateRepo,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        await exerciseTemplateRepo.UpdateAsync(updatedExercise);
        await unitOfWork.CommitAsync();

        var reloaded = await exerciseTemplateRepo.GetByIdWithDetailsAsync(updatedExercise.Id);
        
        return ServiceResult<WorkoutTemplateExerciseDto>.Success(MapToDto(reloaded));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BooleanResultDto>> RemoveExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId)
    {
        var result = workoutTemplateExerciseId.IsEmpty switch
        {
            true => ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.InvalidExerciseId)),
            false => await ProcessRemoveExerciseAsync(workoutTemplateExerciseId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> ProcessRemoveExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var exerciseTemplateRepo = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exerciseTemplate = await exerciseTemplateRepo.GetByIdWithDetailsAsync(workoutTemplateExerciseId);
        if (exerciseTemplate == null || exerciseTemplate.IsEmpty)
        {
            return ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound));
        }
        
        var result = await ValidateAndRemoveExerciseAsync(unitOfWork, exerciseTemplate, exerciseTemplateRepo, workoutTemplateExerciseId);
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> ValidateAndRemoveExerciseAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateExercise exerciseTemplate,
        IWorkoutTemplateExerciseRepository exerciseTemplateRepo,
        WorkoutTemplateExerciseId workoutTemplateExerciseId)
    {
        var templateRepo = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        var template = await templateRepo.GetByIdAsync(exerciseTemplate.WorkoutTemplateId);
        
        var result = template.WorkoutState.Value != WorkoutTemplateExerciseErrorMessages.DraftStateRequired
            ? ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CanOnlyRemoveExercisesFromDraftTemplates))
            : await PerformRemoveExerciseAsync(exerciseTemplateRepo, workoutTemplateExerciseId, unitOfWork);
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> PerformRemoveExerciseAsync(
        IWorkoutTemplateExerciseRepository exerciseTemplateRepo,
        WorkoutTemplateExerciseId workoutTemplateExerciseId,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        await exerciseTemplateRepo.DeleteAsync(workoutTemplateExerciseId);
        await unitOfWork.CommitAsync();

        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BooleanResultDto>> ReorderExercisesAsync(ReorderTemplateExercisesCommand command)
    {
        var result = command switch
        {
            null => ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CommandCannotBeNull)),
            _ => await ValidateAndProcessReorderAsync(command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> ValidateAndProcessReorderAsync(ReorderTemplateExercisesCommand command)
    {
        var validationResult = IsReorderCommandValid(command);
        var result = validationResult.IsSuccess switch
        {
            false => validationResult,
            true => await ProcessReorderExercisesAsync(command)
        };
        
        return result;
    }
    
    private ServiceResult<BooleanResultDto> IsReorderCommandValid(ReorderTemplateExercisesCommand command)
    {
        var result = (command.WorkoutTemplateId.IsEmpty, string.IsNullOrWhiteSpace(command.Zone),
                     command.ExerciseIds == null || command.ExerciseIds.Count == 0, command.UserId.IsEmpty) switch
        {
            (true, _, _, _) or (_, true, _, _) or (_, _, true, _) or (_, _, _, true) => 
                ServiceResult<BooleanResultDto>.Failure(
                    BooleanResultDto.Empty,
                    ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)),
            _ => IsValidZone(command.Zone) switch
            {
                false => ServiceResult<BooleanResultDto>.Failure(
                    BooleanResultDto.Empty,
                    ServiceError.ValidationFailed(string.Format(WorkoutTemplateExerciseErrorMessages.InvalidZone, command.Zone))),
                true => ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)) // Dummy success
            }
        };
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> ProcessReorderExercisesAsync(ReorderTemplateExercisesCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        
        var templateValidationResult = await ValidateTemplateForReorderAsync(unitOfWork, command.WorkoutTemplateId, command.UserId);
        var result = templateValidationResult.IsSuccess switch
        {
            false => templateValidationResult,
            true => await PerformReorderAsync(unitOfWork, command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> ValidateTemplateForReorderAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId workoutTemplateId,
        UserId userId)
    {
        var templateRepo = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        var template = await templateRepo.GetByIdAsync(workoutTemplateId);
        
        if (template == null || template.IsEmpty)
        {
            return ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound));
        }
        
        if (template.WorkoutState?.Value != WorkoutTemplateExerciseErrorMessages.DraftStateRequired)
        {
            return ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CanOnlyReorderExercisesInDraftTemplates));
        }
        
        var result = ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true));
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> PerformReorderAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        ReorderTemplateExercisesCommand command)
    {
        var exerciseTemplateRepo = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        await exerciseTemplateRepo.ReorderExercisesAsync(
            command.WorkoutTemplateId, 
            Enum.Parse<WorkoutZone>(command.Zone), 
            command.ExerciseIds.ToDictionary(id => id, id => command.ExerciseIds.IndexOf(id) + 1));
        
        await unitOfWork.CommitAsync();

        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> ChangeExerciseZoneAsync(ChangeExerciseZoneCommand command)
    {
        var result = command switch
        {
            null => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CommandCannotBeNull)),
            _ => await ValidateAndProcessChangeZoneAsync(command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ValidateAndProcessChangeZoneAsync(ChangeExerciseZoneCommand command)
    {
        var validationResult = IsChangeZoneCommandValid(command);
        var result = validationResult.IsSuccess switch
        {
            false => validationResult,
            true => await ProcessChangeZoneAsync(command)
        };
        
        return result;
    }
    
    private ServiceResult<WorkoutTemplateExerciseDto> IsChangeZoneCommandValid(ChangeExerciseZoneCommand command)
    {
        var result = (command.WorkoutTemplateExerciseId.IsEmpty, string.IsNullOrWhiteSpace(command.NewZone),
                     command.UserId.IsEmpty) switch
        {
            (true, _, _) or (_, true, _) or (_, _, true) => 
                ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                    new WorkoutTemplateExerciseDto(),
                    ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)),
            _ => IsValidZone(command.NewZone) switch
            {
                false => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                    new WorkoutTemplateExerciseDto(),
                    ServiceError.ValidationFailed(string.Format(WorkoutTemplateExerciseErrorMessages.InvalidZone, command.NewZone))),
                true => ServiceResult<WorkoutTemplateExerciseDto>.Success(new WorkoutTemplateExerciseDto()) // Dummy success
            }
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ProcessChangeZoneAsync(ChangeExerciseZoneCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var exerciseTemplateRepo = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exerciseTemplate = await exerciseTemplateRepo.GetByIdWithDetailsAsync(command.WorkoutTemplateExerciseId);
        if (exerciseTemplate == null || exerciseTemplate.IsEmpty)
        {
            return ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound));
        }
        
        var result = await ValidateAndPerformZoneChangeAsync(unitOfWork, exerciseTemplate, command, exerciseTemplateRepo);
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ValidateAndPerformZoneChangeAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateExercise exerciseTemplate,
        ChangeExerciseZoneCommand command,
        IWorkoutTemplateExerciseRepository exerciseTemplateRepo)
    {
        var templateRepo = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        var template = await templateRepo.GetByIdAsync(exerciseTemplate.WorkoutTemplateId);
        
        var result = template.WorkoutState.Value != WorkoutTemplateExerciseErrorMessages.DraftStateRequired
            ? ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CanOnlyChangeZonesInDraftTemplates))
            : await PerformZoneChangeAsync(exerciseTemplate, command, exerciseTemplateRepo, unitOfWork);
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> PerformZoneChangeAsync(
        WorkoutTemplateExercise exerciseTemplate,
        ChangeExerciseZoneCommand command,
        IWorkoutTemplateExerciseRepository exerciseTemplateRepo,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var newSequenceOrder = command.NewSequenceOrder ?? 
            await exerciseTemplateRepo.GetMaxSequenceOrderAsync(exerciseTemplate.WorkoutTemplateId, Enum.Parse<WorkoutZone>(command.NewZone)) + 1;

        var updated = WorkoutTemplateExercise.Handler.ChangeZone(exerciseTemplate, Enum.Parse<WorkoutZone>(command.NewZone), newSequenceOrder);
        var result = updated.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                new WorkoutTemplateExerciseDto(),
                ServiceError.ValidationFailed(string.Join(", ", updated.Errors))),
            true => await SaveZoneChangeAsync(updated.Value, exerciseTemplateRepo, unitOfWork)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> SaveZoneChangeAsync(
        WorkoutTemplateExercise updatedExercise,
        IWorkoutTemplateExerciseRepository exerciseTemplateRepo,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        await exerciseTemplateRepo.UpdateAsync(updatedExercise);
        await unitOfWork.CommitAsync();

        var reloaded = await exerciseTemplateRepo.GetByIdWithDetailsAsync(updatedExercise.Id);
        
        return ServiceResult<WorkoutTemplateExerciseDto>.Success(MapToDto(reloaded));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> DuplicateExercisesAsync(DuplicateTemplateExercisesCommand command)
    {
        var result = command switch
        {
            null => ServiceResult<int>.Failure(
                0,
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CommandCannotBeNull)),
            _ => await ValidateAndProcessDuplicateAsync(command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<int>> ValidateAndProcessDuplicateAsync(DuplicateTemplateExercisesCommand command)
    {
        var result = (command.SourceTemplateId.IsEmpty, command.TargetTemplateId.IsEmpty, command.UserId.IsEmpty) switch
        {
            (true, _, _) or (_, true, _) or (_, _, true) => 
                ServiceResult<int>.Failure(
                    0,
                    ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.InvalidCommandParameters)),
            _ => await ProcessDuplicateExercisesAsync(command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<int>> ProcessDuplicateExercisesAsync(DuplicateTemplateExercisesCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        
        var validationResult = await ValidateTemplatesForDuplicateAsync(unitOfWork, command);
        var result = validationResult.IsSuccess switch
        {
            false => validationResult,
            true => await PerformDuplicateAsync(unitOfWork, command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<int>> ValidateTemplatesForDuplicateAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        DuplicateTemplateExercisesCommand command)
    {
        var templateRepo = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var sourceTemplate = await templateRepo.GetByIdAsync(command.SourceTemplateId);
        
        if (sourceTemplate == null || sourceTemplate.IsEmpty)
        {
            return ServiceResult<int>.Failure(
                0,
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.SourceTemplateNotFound));
        }
        
        var result = await ValidateTargetTemplateAsync(templateRepo, command);
        
        return result;
    }
    
    private async Task<ServiceResult<int>> ValidateTargetTemplateAsync(
        IWorkoutTemplateRepository templateRepo,
        DuplicateTemplateExercisesCommand command)
    {
        var targetTemplate = await templateRepo.GetByIdAsync(command.TargetTemplateId);
        
        if (targetTemplate == null || targetTemplate.IsEmpty)
        {
            return ServiceResult<int>.Failure(
                0,
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.TargetTemplateNotFound));
        }
        
        if (targetTemplate.WorkoutState?.Value != WorkoutTemplateExerciseErrorMessages.DraftStateRequired)
        {
            return ServiceResult<int>.Failure(
                0,
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.CanOnlyDuplicateExercisesToDraftTemplates));
        }
        
        var result = ServiceResult<int>.Success(0);
        
        return result;
    }
    
    private async Task<ServiceResult<int>> PerformDuplicateAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        DuplicateTemplateExercisesCommand command)
    {
        var exerciseTemplateRepo = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        var sourceExercises = (await exerciseTemplateRepo.GetByWorkoutTemplateAsync(
            command.SourceTemplateId)).ToList();

        var result = sourceExercises.Count == 0
            ? ServiceResult<int>.Success(0)
            : await DuplicateExercisesAndConfigurationsAsync(unitOfWork, sourceExercises, command);
        
        return result;
    }
    
    private async Task<ServiceResult<int>> DuplicateExercisesAndConfigurationsAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        List<WorkoutTemplateExercise> sourceExercises,
        DuplicateTemplateExercisesCommand command)
    {
        var exerciseTemplateRepo = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        var duplicatedExercises = CreateDuplicateExercises(sourceExercises, command.TargetTemplateId);

        var result = duplicatedExercises.Count > 0
            ? await SaveDuplicatedExercisesAsync(
                exerciseTemplateRepo, 
                duplicatedExercises, 
                sourceExercises, 
                command.IncludeSetConfigurations, 
                unitOfWork)
            : ServiceResult<int>.Success(0);
        
        return result;
    }
    
    private List<WorkoutTemplateExercise> CreateDuplicateExercises(
        List<WorkoutTemplateExercise> sourceExercises,
        WorkoutTemplateId targetTemplateId)
    {
        var duplicatedExercises = new List<WorkoutTemplateExercise>();
        
        foreach (var sourceExercise in sourceExercises)
        {
            var newExercise = WorkoutTemplateExercise.Handler.CreateNew(
                targetTemplateId,
                sourceExercise.ExerciseId,
                sourceExercise.Zone,
                sourceExercise.SequenceOrder,
                sourceExercise.Notes);

            if (newExercise.IsSuccess)
            {
                duplicatedExercises.Add(newExercise.Value);
            }
        }
        
        return duplicatedExercises;
    }
    
    private async Task<ServiceResult<int>> SaveDuplicatedExercisesAsync(
        IWorkoutTemplateExerciseRepository exerciseTemplateRepo,
        List<WorkoutTemplateExercise> duplicatedExercises,
        List<WorkoutTemplateExercise> sourceExercises,
        bool includeSetConfigurations,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        await exerciseTemplateRepo.AddRangeAsync(duplicatedExercises);

        if (includeSetConfigurations)
        {
            await DuplicateSetConfigurationsAsync(unitOfWork, sourceExercises, duplicatedExercises);
        }

        await unitOfWork.CommitAsync();
        
        return ServiceResult<int>.Success(duplicatedExercises.Count);
    }
    
    private async Task DuplicateSetConfigurationsAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        List<WorkoutTemplateExercise> sourceExercises,
        List<WorkoutTemplateExercise> duplicatedExercises)
    {
        var setConfigRepo = unitOfWork.GetRepository<ISetConfigurationRepository>();
        
        for (int i = 0; i < sourceExercises.Count && i < duplicatedExercises.Count; i++)
        {
            if (sourceExercises[i].Configurations.Count > 0)
            {
                var newConfigs = sourceExercises[i].Configurations
                    .Select(config => SetConfigurationEntity.Handler.CreateNew(
                        duplicatedExercises[i].Id,
                        config.SetNumber,
                        config.TargetReps,
                        config.TargetWeight,
                        config.TargetTimeSeconds,
                        config.RestSeconds))
                    .Where(result => result.IsSuccess)
                    .Select(result => result.Value)
                    .ToList();

                if (newConfigs.Count > 0)
                {
                    await setConfigRepo.AddRangeAsync(newConfigs);
                }
            }
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<List<ExerciseDto>>> GetExerciseSuggestionsAsync(
        WorkoutTemplateId workoutTemplateId, 
        string zone, 
        int maxSuggestions = 5)
    {
        var result = (workoutTemplateId.IsEmpty, string.IsNullOrWhiteSpace(zone)) switch
        {
            (true, _) or (_, true) => ServiceResult<List<ExerciseDto>>.Failure(
                new List<ExerciseDto>(),
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrZone)),
            _ => await LoadExerciseSuggestionsAsync(workoutTemplateId, zone, maxSuggestions)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<List<ExerciseDto>>> LoadExerciseSuggestionsAsync(
        WorkoutTemplateId workoutTemplateId, 
        string zone, 
        int maxSuggestions)
    {
        // TODO: Implement exercise suggestion logic based on:
        // - Template objectives
        // - Existing exercises (to avoid duplicates)
        // - Zone requirements (warmup vs main vs cooldown)
        // - User's exercise history
        // For now, return empty list

        _logger.LogInformation(
            "Exercise suggestions requested for template {TemplateId} zone {Zone}",
            workoutTemplateId, zone);

        var suggestions = new List<ExerciseDto>();
        return await Task.FromResult(ServiceResult<List<ExerciseDto>>.Success(suggestions));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<BooleanResultDto>> ValidateExercisesAsync(
        WorkoutTemplateId workoutTemplateId, 
        List<ExerciseId> exerciseIds)
    {
        var result = (workoutTemplateId.IsEmpty, exerciseIds is null || exerciseIds.Count == 0) switch
        {
            (true, _) or (_, true) => ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
                ServiceError.ValidationFailed(WorkoutTemplateExerciseErrorMessages.InvalidTemplateIdOrExerciseList)),
            _ => await PerformExerciseValidationAsync(workoutTemplateId, exerciseIds!)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> PerformExerciseValidationAsync(
        WorkoutTemplateId workoutTemplateId, 
        List<ExerciseId> exerciseIds)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        
        var templateValidationResult = await ValidateTemplateExistsAsync(unitOfWork, workoutTemplateId);
        var result = templateValidationResult.IsSuccess switch
        {
            false => templateValidationResult,
            true => await ValidateAllExercisesExistAsync(unitOfWork, exerciseIds)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> ValidateTemplateExistsAsync(
        IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId workoutTemplateId)
    {
        var templateRepo = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        var template = await templateRepo.GetByIdAsync(workoutTemplateId);
        
        if (template == null || template.IsEmpty)
        {
            return ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
                ServiceError.NotFound(WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound));
        }
        
        var result = ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true));
        
        return result;
    }
    
    private async Task<ServiceResult<BooleanResultDto>> ValidateAllExercisesExistAsync(
        IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork,
        List<ExerciseId> exerciseIds)
    {
        var exerciseRepo = unitOfWork.GetRepository<IExerciseRepository>();
        var result = ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true));
        
        foreach (var exerciseId in exerciseIds)
        {
            var exercise = await exerciseRepo.GetByIdAsync(exerciseId);
            if (exercise.IsEmpty)
            {
                result = ServiceResult<BooleanResultDto>.Failure(
                    BooleanResultDto.Empty,
                    ServiceError.NotFound(string.Format(WorkoutTemplateExerciseErrorMessages.ExerciseNotFoundWithId, exerciseId)));
                break;
            }
        }
        
        return result;
    }

    private static bool IsValidZone(string zone)
    {
        return Enum.TryParse<WorkoutZone>(zone, out _);
    }

    private static int CalculateEstimatedDuration(List<WorkoutTemplateExercise> exercises)
    {
        // Simple estimation: 
        // - Warmup exercises: 1 minute each
        // - Main exercises: 3 minutes per set (including rest)
        // - Cooldown exercises: 1 minute each
        var warmupMinutes = exercises.Count(e => e.Zone == WorkoutZone.Warmup);
        var mainMinutes = exercises
            .Where(e => e.Zone == WorkoutZone.Main)
            .Sum(e => e.Configurations.Count * 3);
        var cooldownMinutes = exercises.Count(e => e.Zone == WorkoutZone.Cooldown);

        return warmupMinutes + mainMinutes + cooldownMinutes;
    }

    private static WorkoutTemplateExerciseDto MapToDto(WorkoutTemplateExercise entity)
    {
        return new WorkoutTemplateExerciseDto
        {
            Id = entity.Id.ToString(),
            Exercise = entity.Exercise != null ? MapExerciseToDto(entity.Exercise) : ExerciseDto.Empty,
            Zone = entity.Zone.ToString(),
            SequenceOrder = entity.SequenceOrder,
            Notes = entity.Notes,
            SetConfigurations = entity.Configurations
                .OrderBy(c => c.SetNumber)
                .Select(MapSetConfigurationToDto)
                .ToList(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static ExerciseDto MapExerciseToDto(ExerciseEntity entity)
    {
        // Using simplified mapping that matches the existing ExerciseDto structure
        return new ExerciseDto
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Description = entity.Description,
            Difficulty = entity.Difficulty != null ? new ReferenceDataDto 
            { 
                Id = entity.Difficulty.Id.ToString(), 
                Value = entity.Difficulty.Value 
            } : ReferenceDataDto.Empty
        };
    }

    private static SetConfigurationDto MapSetConfigurationToDto(SetConfigurationEntity entity)
    {
        return new SetConfigurationDto
        {
            Id = entity.Id.ToString(),
            SetNumber = entity.SetNumber,
            TargetReps = entity.TargetReps,
            TargetWeight = entity.TargetWeight,
            TargetTime = entity.TargetTimeSeconds,
            RestSeconds = entity.RestSeconds,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}