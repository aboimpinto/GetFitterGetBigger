using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

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

    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> GetByIdAsync(WorkoutTemplateExerciseId id)
    {
        var result = id.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                CreateEmptyDto(),
                ServiceError.InvalidFormat("WorkoutTemplateExerciseId", "GUID format")),
            false => await LoadExerciseByIdAsync(id)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> LoadExerciseByIdAsync(WorkoutTemplateExerciseId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exercise = await repository.GetByIdWithDetailsAsync(id);
        
        var result = exercise.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                CreateEmptyDto(),
                ServiceError.NotFound("Workout template exercise not found")),
            false => ServiceResult<WorkoutTemplateExerciseDto>.Success(MapToDto(exercise))
        };
        
        return result;
    }

    public async Task<ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>> GetByTemplateAsync(
        WorkoutTemplateId workoutTemplateId)
    {
        var result = workoutTemplateId.IsEmpty switch
        {
            true => ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>.Failure(
                new List<WorkoutTemplateExerciseDto>(),
                ServiceError.InvalidFormat("WorkoutTemplateId", "GUID format")),
            false => await LoadExercisesByTemplateAsync(workoutTemplateId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>> LoadExercisesByTemplateAsync(
        WorkoutTemplateId workoutTemplateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exercises = await repository.GetByWorkoutTemplateAsync(workoutTemplateId);
        
        return ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>.Success(
            exercises.Select(MapToDto));
    }

    public async Task<ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>> GetByZoneAsync(
        WorkoutTemplateId workoutTemplateId,
        WorkoutZone zone)
    {
        var result = workoutTemplateId.IsEmpty switch
        {
            true => ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>.Failure(
                new List<WorkoutTemplateExerciseDto>(),
                ServiceError.InvalidFormat("WorkoutTemplateId", "GUID format")),
            false => await LoadExercisesByZoneAsync(workoutTemplateId, zone)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>> LoadExercisesByZoneAsync(
        WorkoutTemplateId workoutTemplateId,
        WorkoutZone zone)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exercises = await repository.GetByZoneAsync(workoutTemplateId, zone);
        
        return ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>.Success(
            exercises.Select(MapToDto));
    }

    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> AddExerciseAsync(AddExerciseToTemplateCommand command)
    {
        var validationResult = ValidateAddCommand(command);
        var result = validationResult.IsSuccess switch
        {
            false => validationResult,
            true => await ProcessAddExerciseAsync(command)
        };
        
        return result;
    }
    
    private ServiceResult<WorkoutTemplateExerciseDto> ValidateAddCommand(AddExerciseToTemplateCommand command)
    {
        var result = (command, command?.WorkoutTemplateId.IsEmpty ?? true, command?.ExerciseId.IsEmpty ?? true) switch
        {
            (null, _, _) => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                CreateEmptyDto(),
                ServiceError.ValidationFailed("Command cannot be null")),
            (_, true, _) => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                CreateEmptyDto(),
                ServiceError.InvalidFormat("WorkoutTemplateId", "GUID format")),
            (_, _, true) => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                CreateEmptyDto(),
                ServiceError.InvalidFormat("ExerciseId", "GUID format")),
            _ => ServiceResult<WorkoutTemplateExerciseDto>.Success(CreateEmptyDto()) // Dummy success to continue
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ProcessAddExerciseAsync(AddExerciseToTemplateCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        var templateRepository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        // Verify template exists
        var templateExists = await templateRepository.ExistsAsync(command.WorkoutTemplateId);
        var result = templateExists switch
        {
            false => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                CreateEmptyDto(),
                ServiceError.NotFound("Workout template not found")),
            true => await CreateAndAddExerciseAsync(repository, command, unitOfWork)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> CreateAndAddExerciseAsync(
        IWorkoutTemplateExerciseRepository repository,
        AddExerciseToTemplateCommand command,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        // Get sequence order if not provided
        var sequenceOrder = command.SequenceOrder ?? 
            await repository.GetMaxSequenceOrderAsync(command.WorkoutTemplateId, command.Zone) + 1;
        
        var entityResult = WorkoutTemplateExercise.Handler.CreateNew(
            command.WorkoutTemplateId,
            command.ExerciseId,
            command.Zone,
            sequenceOrder,
            command.Notes);
        
        var result = entityResult.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                CreateEmptyDto(),
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors))),
            true => await PersistExerciseAsync(repository, entityResult.Value, unitOfWork)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> PersistExerciseAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateExercise exercise,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var createdExercise = await repository.AddAsync(exercise);
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Added exercise {ExerciseId} to template {TemplateId} in zone {Zone}",
            exercise.ExerciseId, exercise.WorkoutTemplateId, exercise.Zone);
        
        return ServiceResult<WorkoutTemplateExerciseDto>.Success(MapToDto(createdExercise));
    }

    public async Task<ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>> AddExercisesAsync(
        AddExercisesToTemplateCommand command)
    {
        var validationResult = ValidateBulkAddCommand(command);
        var result = validationResult.IsSuccess switch
        {
            false => ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>.Failure(
                new List<WorkoutTemplateExerciseDto>(),
                validationResult.Errors),
            true => await ProcessBulkAddExercisesAsync(command)
        };
        
        return result;
    }
    
    private ServiceResult<bool> ValidateBulkAddCommand(AddExercisesToTemplateCommand command)
    {
        var result = (command, command?.WorkoutTemplateId.IsEmpty ?? true, command?.Exercises?.Any() ?? false) switch
        {
            (null, _, _) => ServiceResult<bool>.Failure(false, ServiceError.ValidationFailed("Command cannot be null")),
            (_, true, _) => ServiceResult<bool>.Failure(false, ServiceError.InvalidFormat("WorkoutTemplateId", "GUID format")),
            (_, _, false) => ServiceResult<bool>.Failure(false, ServiceError.ValidationFailed("No exercises provided")),
            _ => ValidateExercisesToAdd(command)
        };
        
        return result;
    }
    
    private ServiceResult<bool> ValidateExercisesToAdd(AddExercisesToTemplateCommand command)
    {
        var invalidExercises = command.Exercises
            .Where(e => e.ExerciseId.IsEmpty)
            .ToList();
        
        var result = invalidExercises.Any() switch
        {
            true => ServiceResult<bool>.Failure(false, ServiceError.ValidationFailed("One or more exercise IDs are invalid")),
            false => ServiceResult<bool>.Success(true)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>> ProcessBulkAddExercisesAsync(
        AddExercisesToTemplateCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        var templateRepository = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        // Verify template exists
        var templateExists = await templateRepository.ExistsAsync(command.WorkoutTemplateId);
        var result = templateExists switch
        {
            false => ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>.Failure(
                new List<WorkoutTemplateExerciseDto>(),
                ServiceError.NotFound("Workout template not found")),
            true => await CreateAndAddExercisesAsync(repository, command, unitOfWork)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>> CreateAndAddExercisesAsync(
        IWorkoutTemplateExerciseRepository repository,
        AddExercisesToTemplateCommand command,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var exercisesToAdd = new List<WorkoutTemplateExercise>();
        var errors = new List<string>();
        
        // Get max sequence orders for each zone
        var zoneMaxOrders = new Dictionary<WorkoutZone, int>();
        foreach (var zone in Enum.GetValues<WorkoutZone>())
        {
            zoneMaxOrders[zone] = await repository.GetMaxSequenceOrderAsync(command.WorkoutTemplateId, zone);
        }
        
        // Create exercises
        foreach (var exerciseData in command.Exercises)
        {
            var sequenceOrder = exerciseData.SequenceOrder ?? ++zoneMaxOrders[exerciseData.Zone];
            
            var entityResult = WorkoutTemplateExercise.Handler.CreateNew(
                command.WorkoutTemplateId,
                exerciseData.ExerciseId,
                exerciseData.Zone,
                sequenceOrder,
                exerciseData.Notes);
            
            if (entityResult.IsSuccess)
            {
                exercisesToAdd.Add(entityResult.Value);
            }
            else
            {
                errors.AddRange(entityResult.Errors);
            }
        }
        
        var result = errors.Any() switch
        {
            true => ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>.Failure(
                new List<WorkoutTemplateExerciseDto>(),
                ServiceError.ValidationFailed(string.Join("; ", errors))),
            false => await PersistExercisesAsync(repository, exercisesToAdd, unitOfWork)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>> PersistExercisesAsync(
        IWorkoutTemplateExerciseRepository repository,
        List<WorkoutTemplateExercise> exercises,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var createdExercises = await repository.AddRangeAsync(exercises);
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Added {Count} exercises to template {TemplateId}",
            exercises.Count, exercises.FirstOrDefault()?.WorkoutTemplateId);
        
        return ServiceResult<IEnumerable<WorkoutTemplateExerciseDto>>.Success(
            createdExercises.Select(MapToDto));
    }

    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> UpdateNotesAsync(
        WorkoutTemplateExerciseId id,
        string? notes)
    {
        var result = id.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                CreateEmptyDto(),
                ServiceError.InvalidFormat("WorkoutTemplateExerciseId", "GUID format")),
            false => await ProcessUpdateNotesAsync(id, notes)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ProcessUpdateNotesAsync(
        WorkoutTemplateExerciseId id,
        string? notes)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exercise = await repository.GetByIdWithDetailsAsync(id);
        var result = exercise.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                CreateEmptyDto(),
                ServiceError.NotFound("Workout template exercise not found")),
            false => await UpdateExerciseNotesAsync(repository, exercise, notes, unitOfWork)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> UpdateExerciseNotesAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateExercise exercise,
        string? notes,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var entityResult = WorkoutTemplateExercise.Handler.UpdateNotes(exercise, notes);
        
        var result = entityResult.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                CreateEmptyDto(),
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors))),
            true => await PersistUpdatedExerciseAsync(repository, entityResult.Value, unitOfWork)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> PersistUpdatedExerciseAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateExercise exercise,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var updatedExercise = await repository.UpdateAsync(exercise);
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Updated notes for exercise {ExerciseId}", exercise.Id);
        
        return ServiceResult<WorkoutTemplateExerciseDto>.Success(MapToDto(updatedExercise));
    }

    public async Task<ServiceResult<WorkoutTemplateExerciseDto>> ChangeZoneAsync(
        WorkoutTemplateExerciseId id,
        WorkoutZone newZone,
        int sequenceOrder)
    {
        var validationResult = ValidateChangeZoneParameters(id, sequenceOrder);
        var result = validationResult.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateExerciseDto>.Failure(CreateEmptyDto(), validationResult.Errors),
            true => await ProcessChangeZoneAsync(id, newZone, sequenceOrder)
        };
        
        return result;
    }
    
    private ServiceResult<bool> ValidateChangeZoneParameters(WorkoutTemplateExerciseId id, int sequenceOrder)
    {
        var result = (id.IsEmpty, sequenceOrder < 1) switch
        {
            (true, _) => ServiceResult<bool>.Failure(false, ServiceError.InvalidFormat("WorkoutTemplateExerciseId", "GUID format")),
            (_, true) => ServiceResult<bool>.Failure(false, ServiceError.ValidationFailed("Sequence order must be at least 1")),
            _ => ServiceResult<bool>.Success(true)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> ProcessChangeZoneAsync(
        WorkoutTemplateExerciseId id,
        WorkoutZone newZone,
        int sequenceOrder)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var exercise = await repository.GetByIdWithDetailsAsync(id);
        var result = exercise.IsEmpty switch
        {
            true => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                CreateEmptyDto(),
                ServiceError.NotFound("Workout template exercise not found")),
            false => await UpdateExerciseZoneAsync(repository, exercise, newZone, sequenceOrder, unitOfWork)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<WorkoutTemplateExerciseDto>> UpdateExerciseZoneAsync(
        IWorkoutTemplateExerciseRepository repository,
        WorkoutTemplateExercise exercise,
        WorkoutZone newZone,
        int sequenceOrder,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var entityResult = WorkoutTemplateExercise.Handler.ChangeZone(exercise, newZone, sequenceOrder);
        
        var result = entityResult.IsSuccess switch
        {
            false => ServiceResult<WorkoutTemplateExerciseDto>.Failure(
                CreateEmptyDto(),
                ServiceError.ValidationFailed(string.Join(", ", entityResult.Errors))),
            true => await PersistUpdatedExerciseAsync(repository, entityResult.Value, unitOfWork)
        };
        
        return result;
    }

    public async Task<ServiceResult<bool>> ReorderExercisesAsync(ReorderExercisesCommand command)
    {
        var validationResult = ValidateReorderCommand(command);
        var result = validationResult.IsSuccess switch
        {
            false => ServiceResult<bool>.Failure(false, validationResult.Errors),
            true => await ProcessReorderExercisesAsync(command)
        };
        
        return result;
    }
    
    private ServiceResult<bool> ValidateReorderCommand(ReorderExercisesCommand command)
    {
        var result = (command, command?.WorkoutTemplateId.IsEmpty ?? true, command?.ExerciseOrders?.Any() ?? false) switch
        {
            (null, _, _) => ServiceResult<bool>.Failure(false, ServiceError.ValidationFailed("Command cannot be null")),
            (_, true, _) => ServiceResult<bool>.Failure(false, ServiceError.InvalidFormat("WorkoutTemplateId", "GUID format")),
            (_, _, false) => ServiceResult<bool>.Failure(false, ServiceError.ValidationFailed("No exercise orders provided")),
            _ => ValidateExerciseOrders(command.ExerciseOrders)
        };
        
        return result;
    }
    
    private ServiceResult<bool> ValidateExerciseOrders(Dictionary<WorkoutTemplateExerciseId, int> exerciseOrders)
    {
        var invalidOrders = exerciseOrders
            .Where(kvp => kvp.Key.IsEmpty || kvp.Value < 1)
            .ToList();
        
        var result = invalidOrders.Any() switch
        {
            true => ServiceResult<bool>.Failure(false, ServiceError.ValidationFailed("Invalid exercise orders provided")),
            false => ServiceResult<bool>.Success(true)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<bool>> ProcessReorderExercisesAsync(ReorderExercisesCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var success = await repository.ReorderExercisesAsync(
            command.WorkoutTemplateId,
            command.Zone,
            command.ExerciseOrders);
        
        var result = success switch
        {
            false => ServiceResult<bool>.Failure(false, "Failed to reorder exercises"),
            true => await CommitReorderAsync(unitOfWork, command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<bool>> CommitReorderAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        ReorderExercisesCommand command)
    {
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Reordered {Count} exercises in zone {Zone} for template {TemplateId}",
            command.ExerciseOrders.Count, command.Zone, command.WorkoutTemplateId);
        
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> RemoveExerciseAsync(WorkoutTemplateExerciseId id)
    {
        var result = id.IsEmpty switch
        {
            true => ServiceResult<bool>.Failure(false, ServiceError.InvalidFormat("WorkoutTemplateExerciseId", "GUID format")),
            false => await ProcessRemoveExerciseAsync(id)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<bool>> ProcessRemoveExerciseAsync(WorkoutTemplateExerciseId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var deleted = await repository.DeleteAsync(id);
        var result = deleted switch
        {
            false => ServiceResult<bool>.Failure(false, ServiceError.NotFound("Workout template exercise not found")),
            true => await CommitRemoveAsync(unitOfWork, id)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<bool>> CommitRemoveAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateExerciseId id)
    {
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Removed exercise {ExerciseId} from template", id);
        
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<int>> RemoveAllExercisesAsync(WorkoutTemplateId workoutTemplateId)
    {
        var result = workoutTemplateId.IsEmpty switch
        {
            true => ServiceResult<int>.Failure(0, ServiceError.InvalidFormat("WorkoutTemplateId", "GUID format")),
            false => await ProcessRemoveAllExercisesAsync(workoutTemplateId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<int>> ProcessRemoveAllExercisesAsync(WorkoutTemplateId workoutTemplateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        var deletedCount = await repository.DeleteAllByWorkoutTemplateAsync(workoutTemplateId);
        
        var result = deletedCount > 0
            ? await CommitRemoveAllAsync(unitOfWork, workoutTemplateId, deletedCount)
            : ServiceResult<int>.Success(0); // No exercises to delete is not an error
        
        return result;
    }
    
    private async Task<ServiceResult<int>> CommitRemoveAllAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        WorkoutTemplateId workoutTemplateId,
        int deletedCount)
    {
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Removed {Count} exercises from template {TemplateId}",
            deletedCount, workoutTemplateId);
        
        return ServiceResult<int>.Success(deletedCount);
    }

    public async Task<bool> IsExerciseInUseAsync(ExerciseId exerciseId)
    {
        var result = exerciseId.IsEmpty switch
        {
            true => false,
            false => await CheckExerciseInUseAsync(exerciseId)
        };
        
        return result;
    }
    
    private async Task<bool> CheckExerciseInUseAsync(ExerciseId exerciseId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        return await repository.IsExerciseInUseAsync(exerciseId);
    }

    public async Task<int> GetTemplateCountByExerciseAsync(ExerciseId exerciseId)
    {
        var result = exerciseId.IsEmpty switch
        {
            true => 0,
            false => await LoadTemplateCountByExerciseAsync(exerciseId)
        };
        
        return result;
    }
    
    private async Task<int> LoadTemplateCountByExerciseAsync(ExerciseId exerciseId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        
        return await repository.GetTemplateCountByExerciseAsync(exerciseId);
    }

    #region Private Methods

    private static WorkoutTemplateExerciseDto CreateEmptyDto() => WorkoutTemplateExerciseDto.Empty;

    private static WorkoutTemplateExerciseDto MapToDto(WorkoutTemplateExercise exercise) =>
        new()
        {
            Id = exercise.Id.ToString(),
            WorkoutTemplateId = exercise.WorkoutTemplateId.ToString(),
            Exercise = exercise.Exercise != null ? MapExerciseToReferenceDto(exercise.Exercise) : ReferenceDataDto.Empty,
            Zone = exercise.Zone.ToString(),
            SequenceOrder = exercise.SequenceOrder,
            Notes = exercise.Notes,
            Configurations = exercise.Configurations?.Select(MapToSetConfigurationDto).ToList() ?? new()
        };

    private static ReferenceDataDto MapExerciseToReferenceDto(Exercise exercise) =>
        new()
        {
            Id = exercise.Id.ToString(),
            Value = exercise.Name,
            Description = exercise.Description
        };

    private static SetConfigurationDto MapToSetConfigurationDto(SetConfiguration config) =>
        new()
        {
            Id = config.Id.ToString(),
            WorkoutTemplateExerciseId = config.WorkoutTemplateExerciseId.ToString(),
            SetNumber = config.SetNumber,
            TargetReps = config.TargetReps,
            TargetWeight = config.TargetWeight,
            TargetDurationSeconds = config.TargetTimeSeconds,
            RestSeconds = config.RestSeconds
        };

    #endregion
}