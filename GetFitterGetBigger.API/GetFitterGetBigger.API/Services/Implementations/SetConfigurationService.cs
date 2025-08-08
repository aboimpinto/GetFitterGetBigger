using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.SetConfigurations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service for managing set configurations
/// </summary>
public class SetConfigurationService : ISetConfigurationService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<SetConfigurationService> _logger;

    public SetConfigurationService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<SetConfigurationService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    public async Task<ServiceResult<SetConfigurationDto>> GetByIdAsync(SetConfigurationId id)
    {
        var result = id.IsEmpty switch
        {
            true => ServiceResult<SetConfigurationDto>.Failure(
                new SetConfigurationDto(),
                ServiceError.InvalidFormat("SetConfigurationId", "GUID format")),
            false => await LoadSetConfigurationAsync(id)
        };

        return result;
    }

    public async Task<ServiceResult<IEnumerable<SetConfigurationDto>>> GetByWorkoutTemplateExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId)
    {
        var result = workoutTemplateExerciseId.IsEmpty switch
        {
            true => ServiceResult<IEnumerable<SetConfigurationDto>>.Failure(
                new List<SetConfigurationDto>(),
                ServiceError.InvalidFormat("WorkoutTemplateExerciseId", "GUID format")),
            false => await LoadSetConfigurationsByExerciseAsync(workoutTemplateExerciseId)
        };

        return result;
    }

    public async Task<ServiceResult<IEnumerable<SetConfigurationDto>>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId)
    {
        var result = workoutTemplateId.IsEmpty switch
        {
            true => ServiceResult<IEnumerable<SetConfigurationDto>>.Failure(
                new List<SetConfigurationDto>(),
                ServiceError.InvalidFormat("WorkoutTemplateId", "GUID format")),
            false => await LoadSetConfigurationsByTemplateAsync(workoutTemplateId)
        };

        return result;
    }

    public async Task<ServiceResult<SetConfigurationDto>> CreateAsync(CreateSetConfigurationCommand command)
    {
        var result = command switch
        {
            null => ServiceResult<SetConfigurationDto>.Failure(
                new SetConfigurationDto(),
                ServiceError.ValidationFailed("Command cannot be null")),
            _ => await ValidateAndProcessCreateAsync(command)
        };

        return result;
    }

    public async Task<ServiceResult<IEnumerable<SetConfigurationDto>>> CreateBulkAsync(CreateBulkSetConfigurationsCommand command)
    {
        var result = command switch
        {
            null => ServiceResult<IEnumerable<SetConfigurationDto>>.Failure(
                new List<SetConfigurationDto>(),
                ServiceError.ValidationFailed("Command cannot be null")),
            _ => await ValidateAndProcessBulkCreateAsync(command)
        };

        return result;
    }

    public async Task<ServiceResult<SetConfigurationDto>> UpdateAsync(UpdateSetConfigurationCommand command)
    {
        var result = command switch
        {
            null => ServiceResult<SetConfigurationDto>.Failure(
                new SetConfigurationDto(),
                ServiceError.ValidationFailed("Command cannot be null")),
            _ => await ValidateAndProcessUpdateAsync(command)
        };

        return result;
    }

    public async Task<ServiceResult<int>> UpdateBulkAsync(UpdateBulkSetConfigurationsCommand command)
    {
        var result = command switch
        {
            null => ServiceResult<int>.Failure(
                0,
                ServiceError.ValidationFailed("Command cannot be null")),
            _ => await ValidateAndProcessBulkUpdateAsync(command)
        };

        return result;
    }

    public async Task<ServiceResult<bool>> ReorderSetsAsync(ReorderSetConfigurationsCommand command)
    {
        var result = command switch
        {
            null => ServiceResult<bool>.Failure(
                false,
                ServiceError.ValidationFailed("Command cannot be null")),
            _ => await ValidateAndProcessReorderAsync(command)
        };

        return result;
    }

    public async Task<ServiceResult<bool>> DeleteAsync(SetConfigurationId id)
    {
        var result = id.IsEmpty switch
        {
            true => ServiceResult<bool>.Failure(
                false,
                ServiceError.InvalidFormat("SetConfigurationId", "GUID format")),
            false => await ValidateAndProcessDeleteAsync(id)
        };

        return result;
    }

    public async Task<ServiceResult<int>> DeleteByWorkoutTemplateExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId)
    {
        var result = workoutTemplateExerciseId.IsEmpty switch
        {
            true => ServiceResult<int>.Failure(
                0,
                ServiceError.InvalidFormat("WorkoutTemplateExerciseId", "GUID format")),
            false => await ValidateAndProcessBulkDeleteAsync(workoutTemplateExerciseId)
        };

        return result;
    }

    public async Task<ServiceResult<bool>> ExistsAsync(SetConfigurationId id)
    {
        if (id.IsEmpty)
            return ServiceResult<bool>.Success(false);
            
        return await ServiceValidate.Build<bool>()
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();
                var setConfiguration = await repository.GetByIdAsync(id);
                return ServiceResult<bool>.Success(!setConfiguration.IsEmpty);
            });
    }

    public async Task<ServiceResult<bool>> ExistsAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId, int setNumber)
    {
        if (workoutTemplateExerciseId.IsEmpty)
            return ServiceResult<bool>.Success(false);
            
        if (setNumber <= 0)
            return ServiceResult<bool>.Success(false);
            
        return await ServiceValidate.Build<bool>()
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();
                var exists = await repository.ExistsAsync(workoutTemplateExerciseId, setNumber);
                return ServiceResult<bool>.Success(exists);
            });
    }

    // Private helper methods

    private async Task<ServiceResult<SetConfigurationDto>> LoadSetConfigurationAsync(SetConfigurationId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();
        var setConfiguration = await repository.GetByIdAsync(id);

        var result = setConfiguration.IsEmpty switch
        {
            true => ServiceResult<SetConfigurationDto>.Failure(
                new SetConfigurationDto(),
                ServiceError.NotFound("Set configuration not found")),
            false => ServiceResult<SetConfigurationDto>.Success(MapToDto(setConfiguration))
        };

        return result;
    }

    private async Task<ServiceResult<IEnumerable<SetConfigurationDto>>> LoadSetConfigurationsByExerciseAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();
        var setConfigurations = await repository.GetByWorkoutTemplateExerciseAsync(workoutTemplateExerciseId);

        return ServiceResult<IEnumerable<SetConfigurationDto>>.Success(setConfigurations.Select(MapToDto));
    }

    private async Task<ServiceResult<IEnumerable<SetConfigurationDto>>> LoadSetConfigurationsByTemplateAsync(WorkoutTemplateId workoutTemplateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();
        var setConfigurations = await repository.GetByWorkoutTemplateAsync(workoutTemplateId);

        return ServiceResult<IEnumerable<SetConfigurationDto>>.Success(setConfigurations.Select(MapToDto));
    }

    private async Task<ServiceResult<SetConfigurationDto>> ValidateAndProcessCreateAsync(CreateSetConfigurationCommand command)
    {
        var validationResult = ValidateCreateCommand(command);
        
        var result = validationResult.IsValid switch
        {
            false => ServiceResult<SetConfigurationDto>.Failure(
                new SetConfigurationDto(),
                ServiceError.ValidationFailed(string.Join("; ", validationResult.Errors))),
            true => await ProcessCreateAsync(command)
        };

        return result;
    }

    private async Task<ServiceResult<SetConfigurationDto>> ProcessCreateAsync(CreateSetConfigurationCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();

        // Get next set number if not provided
        var setNumber = command.SetNumber ?? (await repository.GetMaxSetNumberAsync(command.WorkoutTemplateExerciseId) + 1);

        var entityResult = SetConfiguration.Handler.CreateNew(
            command.WorkoutTemplateExerciseId,
            setNumber,
            command.TargetReps,
            command.TargetWeight,
            command.TargetTimeSeconds,
            command.RestSeconds);

        var result = entityResult.IsSuccess switch
        {
            false => ServiceResult<SetConfigurationDto>.Failure(
                new SetConfigurationDto(),
                ServiceError.ValidationFailed(string.Join("; ", entityResult.Errors))),
            true => await SaveSetConfigurationAsync(repository, unitOfWork, entityResult.Value!)
        };

        return result;
    }

    private async Task<ServiceResult<SetConfigurationDto>> SaveSetConfigurationAsync(
        ISetConfigurationRepository repository,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        SetConfiguration setConfiguration)
    {
        var savedSetConfiguration = await repository.AddAsync(setConfiguration);
        await unitOfWork.CommitAsync();

        return ServiceResult<SetConfigurationDto>.Success(MapToDto(savedSetConfiguration));
    }

    private async Task<ServiceResult<IEnumerable<SetConfigurationDto>>> ValidateAndProcessBulkCreateAsync(CreateBulkSetConfigurationsCommand command)
    {
        var validationResult = ValidateBulkCreateCommand(command);
        
        var result = validationResult.IsValid switch
        {
            false => ServiceResult<IEnumerable<SetConfigurationDto>>.Failure(
                new List<SetConfigurationDto>(),
                ServiceError.ValidationFailed(string.Join("; ", validationResult.Errors))),
            true => await ProcessBulkCreateAsync(command)
        };

        return result;
    }

    private async Task<ServiceResult<IEnumerable<SetConfigurationDto>>> ProcessBulkCreateAsync(CreateBulkSetConfigurationsCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();

        var setConfigurations = new List<SetConfiguration>();

        foreach (var setData in command.SetConfigurations)
        {
            var entityResult = SetConfiguration.Handler.CreateNew(
                command.WorkoutTemplateExerciseId,
                setData.SetNumber,
                setData.TargetReps,
                setData.TargetWeight,
                setData.TargetTimeSeconds,
                setData.RestSeconds);

            if (!entityResult.IsSuccess)
            {
                return ServiceResult<IEnumerable<SetConfigurationDto>>.Failure(
                    new List<SetConfigurationDto>(),
                    ServiceError.ValidationFailed(string.Join("; ", entityResult.Errors)));
            }

            setConfigurations.Add(entityResult.Value!);
        }

        var savedSetConfigurations = await repository.AddRangeAsync(setConfigurations);
        await unitOfWork.CommitAsync();

        return ServiceResult<IEnumerable<SetConfigurationDto>>.Success(savedSetConfigurations.Select(MapToDto));
    }

    private async Task<ServiceResult<SetConfigurationDto>> ValidateAndProcessUpdateAsync(UpdateSetConfigurationCommand command)
    {
        var validationResult = ValidateUpdateCommand(command);
        
        var result = validationResult.IsValid switch
        {
            false => ServiceResult<SetConfigurationDto>.Failure(
                new SetConfigurationDto(),
                ServiceError.ValidationFailed(string.Join("; ", validationResult.Errors))),
            true => await ProcessUpdateAsync(command)
        };

        return result;
    }

    private async Task<ServiceResult<SetConfigurationDto>> ProcessUpdateAsync(UpdateSetConfigurationCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();

        var existing = await repository.GetByIdAsync(command.SetConfigurationId);
        
        var result = existing.IsEmpty switch
        {
            true => ServiceResult<SetConfigurationDto>.Failure(
                new SetConfigurationDto(),
                ServiceError.NotFound("Set configuration not found")),
            false => await UpdateExistingSetConfigurationAsync(repository, unitOfWork, existing, command)
        };

        return result;
    }

    private async Task<ServiceResult<SetConfigurationDto>> UpdateExistingSetConfigurationAsync(
        ISetConfigurationRepository repository,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        SetConfiguration existing,
        UpdateSetConfigurationCommand command)
    {
        var updateResult = SetConfiguration.Handler.Update(
            existing,
            command.TargetReps,
            command.TargetWeight,
            command.TargetTimeSeconds,
            command.RestSeconds);

        var result = updateResult.IsSuccess switch
        {
            false => ServiceResult<SetConfigurationDto>.Failure(
                new SetConfigurationDto(),
                ServiceError.ValidationFailed(string.Join("; ", updateResult.Errors))),
            true => await SaveUpdatedSetConfigurationAsync(repository, unitOfWork, updateResult.Value!)
        };

        return result;
    }

    private async Task<ServiceResult<SetConfigurationDto>> SaveUpdatedSetConfigurationAsync(
        ISetConfigurationRepository repository,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        SetConfiguration setConfiguration)
    {
        var updatedSetConfiguration = await repository.UpdateAsync(setConfiguration);
        await unitOfWork.CommitAsync();

        return ServiceResult<SetConfigurationDto>.Success(MapToDto(updatedSetConfiguration));
    }

    private async Task<ServiceResult<int>> ValidateAndProcessBulkUpdateAsync(UpdateBulkSetConfigurationsCommand command)
    {
        var validationResult = ValidateBulkUpdateCommand(command);
        
        var result = validationResult.IsValid switch
        {
            false => ServiceResult<int>.Failure(
                0,
                ServiceError.ValidationFailed(string.Join("; ", validationResult.Errors))),
            true => await ProcessBulkUpdateAsync(command)
        };

        return result;
    }

    private async Task<ServiceResult<int>> ProcessBulkUpdateAsync(UpdateBulkSetConfigurationsCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();

        var setConfigurations = new List<SetConfiguration>();

        foreach (var updateData in command.SetConfigurationUpdates)
        {
            var existing = await repository.GetByIdAsync(updateData.SetConfigurationId);
            
            if (existing.IsEmpty)
            {
                return ServiceResult<int>.Failure(
                    0,
                    ServiceError.NotFound($"Set configuration {updateData.SetConfigurationId} not found"));
            }

            var updateResult = SetConfiguration.Handler.Update(
                existing,
                updateData.TargetReps,
                updateData.TargetWeight,
                updateData.TargetTimeSeconds,
                updateData.RestSeconds);

            if (!updateResult.IsSuccess)
            {
                return ServiceResult<int>.Failure(
                    0,
                    ServiceError.ValidationFailed(string.Join("; ", updateResult.Errors)));
            }

            setConfigurations.Add(updateResult.Value!);
        }

        var updatedCount = await repository.UpdateRangeAsync(setConfigurations);
        await unitOfWork.CommitAsync();

        return ServiceResult<int>.Success(updatedCount);
    }

    private async Task<ServiceResult<bool>> ValidateAndProcessReorderAsync(ReorderSetConfigurationsCommand command)
    {
        var validationResult = ValidateReorderCommand(command);
        
        var result = validationResult.IsValid switch
        {
            false => ServiceResult<bool>.Failure(
                false,
                ServiceError.ValidationFailed(string.Join("; ", validationResult.Errors))),
            true => await ProcessReorderAsync(command)
        };

        return result;
    }

    private async Task<ServiceResult<bool>> ProcessReorderAsync(ReorderSetConfigurationsCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();

        var success = await repository.ReorderSetsAsync(command.WorkoutTemplateExerciseId, command.SetReorders);
        
        var result = success switch
        {
            false => ServiceResult<bool>.Failure(
                false,
                ServiceError.ValidationFailed("Failed to reorder set configurations")),
            true => await CommitReorderAsync(unitOfWork)
        };

        return result;
    }

    private async Task<ServiceResult<bool>> CommitReorderAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        await unitOfWork.CommitAsync();
        return ServiceResult<bool>.Success(true);
    }

    private async Task<ServiceResult<bool>> ValidateAndProcessDeleteAsync(SetConfigurationId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();

        var existing = await repository.GetByIdAsync(id);
        
        var result = existing.IsEmpty switch
        {
            true => ServiceResult<bool>.Failure(
                false,
                ServiceError.NotFound("Set configuration not found")),
            false => await ProcessDeleteAsync(repository, unitOfWork, id)
        };

        return result;
    }

    private async Task<ServiceResult<bool>> ProcessDeleteAsync(
        ISetConfigurationRepository repository,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        SetConfigurationId id)
    {
        var success = await repository.DeleteAsync(id);
        await unitOfWork.CommitAsync();

        return ServiceResult<bool>.Success(success);
    }

    private async Task<ServiceResult<int>> ValidateAndProcessBulkDeleteAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();

        var deletedCount = await repository.DeleteByWorkoutTemplateExerciseAsync(workoutTemplateExerciseId);
        await unitOfWork.CommitAsync();

        return ServiceResult<int>.Success(deletedCount);
    }

    private async Task<bool> CheckSetConfigurationExistsAsync(SetConfigurationId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();
        var setConfiguration = await repository.GetByIdAsync(id);

        return !setConfiguration.IsEmpty;
    }

    private async Task<bool> CheckSetConfigurationExistsAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId, int setNumber)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();

        return await repository.ExistsAsync(workoutTemplateExerciseId, setNumber);
    }

    // Validation methods

    private ValidationResult ValidateCreateCommand(CreateSetConfigurationCommand command)
    {
        var errors = new List<string>();

        if (command.WorkoutTemplateExerciseId.IsEmpty)
            errors.Add("WorkoutTemplateExerciseId is required");

        if (command.SetNumber.HasValue && command.SetNumber.Value <= 0)
            errors.Add("SetNumber must be greater than 0");

        if (command.RestSeconds < 0)
            errors.Add("RestSeconds cannot be negative");

        if (command.TargetWeight.HasValue && command.TargetWeight.Value < 0)
            errors.Add("TargetWeight cannot be negative");

        if (command.TargetTimeSeconds.HasValue && command.TargetTimeSeconds.Value <= 0)
            errors.Add("TargetTimeSeconds must be greater than 0");

        if (command.UserId.IsEmpty)
            errors.Add("UserId is required");

        return new ValidationResult(errors);
    }

    private ValidationResult ValidateBulkCreateCommand(CreateBulkSetConfigurationsCommand command)
    {
        var errors = new List<string>();

        if (command.WorkoutTemplateExerciseId.IsEmpty)
            errors.Add("WorkoutTemplateExerciseId is required");

        if (!command.SetConfigurations.Any())
            errors.Add("At least one set configuration is required");

        if (command.UserId.IsEmpty)
            errors.Add("UserId is required");

        // Validate individual set configurations
        var setNumbers = new HashSet<int>();
        foreach (var setData in command.SetConfigurations)
        {
            if (setData.SetNumber <= 0)
                errors.Add($"SetNumber {setData.SetNumber} must be greater than 0");

            if (!setNumbers.Add(setData.SetNumber))
                errors.Add($"Duplicate SetNumber {setData.SetNumber} found");

            if (setData.RestSeconds < 0)
                errors.Add($"RestSeconds cannot be negative for set {setData.SetNumber}");

            if (setData.TargetWeight.HasValue && setData.TargetWeight.Value < 0)
                errors.Add($"TargetWeight cannot be negative for set {setData.SetNumber}");

            if (setData.TargetTimeSeconds.HasValue && setData.TargetTimeSeconds.Value <= 0)
                errors.Add($"TargetTimeSeconds must be greater than 0 for set {setData.SetNumber}");
        }

        return new ValidationResult(errors);
    }

    private ValidationResult ValidateUpdateCommand(UpdateSetConfigurationCommand command)
    {
        var errors = new List<string>();

        if (command.SetConfigurationId.IsEmpty)
            errors.Add("SetConfigurationId is required");

        if (command.RestSeconds < 0)
            errors.Add("RestSeconds cannot be negative");

        if (command.TargetWeight.HasValue && command.TargetWeight.Value < 0)
            errors.Add("TargetWeight cannot be negative");

        if (command.TargetTimeSeconds.HasValue && command.TargetTimeSeconds.Value <= 0)
            errors.Add("TargetTimeSeconds must be greater than 0");

        if (command.UserId.IsEmpty)
            errors.Add("UserId is required");

        return new ValidationResult(errors);
    }

    private ValidationResult ValidateBulkUpdateCommand(UpdateBulkSetConfigurationsCommand command)
    {
        var errors = new List<string>();

        if (command.WorkoutTemplateExerciseId.IsEmpty)
            errors.Add("WorkoutTemplateExerciseId is required");

        if (!command.SetConfigurationUpdates.Any())
            errors.Add("At least one set configuration update is required");

        if (command.UserId.IsEmpty)
            errors.Add("UserId is required");

        // Validate individual updates
        foreach (var updateData in command.SetConfigurationUpdates)
        {
            if (updateData.SetConfigurationId.IsEmpty)
                errors.Add("SetConfigurationId is required for all updates");

            if (updateData.RestSeconds < 0)
                errors.Add($"RestSeconds cannot be negative for set {updateData.SetConfigurationId}");

            if (updateData.TargetWeight.HasValue && updateData.TargetWeight.Value < 0)
                errors.Add($"TargetWeight cannot be negative for set {updateData.SetConfigurationId}");

            if (updateData.TargetTimeSeconds.HasValue && updateData.TargetTimeSeconds.Value <= 0)
                errors.Add($"TargetTimeSeconds must be greater than 0 for set {updateData.SetConfigurationId}");
        }

        return new ValidationResult(errors);
    }

    private ValidationResult ValidateReorderCommand(ReorderSetConfigurationsCommand command)
    {
        var errors = new List<string>();

        if (command.WorkoutTemplateExerciseId.IsEmpty)
            errors.Add("WorkoutTemplateExerciseId is required");

        if (!command.SetReorders.Any())
            errors.Add("At least one set reorder is required");

        if (command.UserId.IsEmpty)
            errors.Add("UserId is required");

        // Validate set numbers
        var setNumbers = new HashSet<int>();
        foreach (var reorder in command.SetReorders)
        {
            if (reorder.Key.IsEmpty)
                errors.Add("SetConfigurationId is required for all reorders");

            if (reorder.Value <= 0)
                errors.Add($"SetNumber {reorder.Value} must be greater than 0");

            if (!setNumbers.Add(reorder.Value))
                errors.Add($"Duplicate SetNumber {reorder.Value} in reorder command");
        }

        return new ValidationResult(errors);
    }

    // Mapping methods

    private static SetConfigurationDto MapToDto(SetConfiguration setConfiguration) => new()
    {
        Id = setConfiguration.Id.ToString(),
        SetNumber = setConfiguration.SetNumber,
        TargetReps = setConfiguration.TargetReps,
        TargetWeight = setConfiguration.TargetWeight,
        TargetTime = setConfiguration.TargetTimeSeconds,
        RestSeconds = setConfiguration.RestSeconds,
        CreatedAt = DateTime.UtcNow, // TODO: Add timestamps to entity
        UpdatedAt = DateTime.UtcNow  // TODO: Add timestamps to entity
    };

    // Helper class for validation results
    private class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; }

        public ValidationResult(List<string> errors)
        {
            Errors = errors ?? new List<string>();
        }
    }
}