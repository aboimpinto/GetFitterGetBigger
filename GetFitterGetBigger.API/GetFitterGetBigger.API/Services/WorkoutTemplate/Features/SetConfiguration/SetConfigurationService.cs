using GetFitterGetBigger.API.Constants.ErrorMessages;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using SetConfigurationEntity = GetFitterGetBigger.API.Models.Entities.SetConfiguration;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.SetConfigurations;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.WorkoutTemplate.Features.SetConfiguration;

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
                ServiceError.ValidationFailed(SetConfigurationErrorMessages.CommandCannotBeNull)),
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
                ServiceError.ValidationFailed(SetConfigurationErrorMessages.CommandCannotBeNull)),
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
                ServiceError.ValidationFailed(SetConfigurationErrorMessages.CommandCannotBeNull)),
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
                ServiceError.ValidationFailed(SetConfigurationErrorMessages.CommandCannotBeNull)),
            _ => await ValidateAndProcessBulkUpdateAsync(command)
        };

        return result;
    }

    public async Task<ServiceResult<BooleanResultDto>> ReorderSetsAsync(ReorderSetConfigurationsCommand command)
    {
        var result = command switch
        {
            null => ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
                ServiceError.ValidationFailed(SetConfigurationErrorMessages.CommandCannotBeNull)),
            _ => await ValidateAndProcessReorderAsync(command)
        };

        return result;
    }

    public async Task<ServiceResult<BooleanResultDto>> DeleteAsync(SetConfigurationId id)
    {
        var result = id.IsEmpty switch
        {
            true => ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
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

    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(SetConfigurationId id)
    {
        if (id.IsEmpty)
            return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(false));
            
        return await ServiceValidate.Build<BooleanResultDto>()
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();
                var setConfiguration = await repository.GetByIdAsync(id);
                return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(setConfiguration != null && !setConfiguration.IsEmpty));
            });
    }

    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId, int setNumber)
    {
        if (workoutTemplateExerciseId.IsEmpty)
            return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(false));
            
        if (setNumber <= 0)
            return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(false));
            
        return await ServiceValidate.Build<BooleanResultDto>()
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();
                var exists = await repository.ExistsAsync(workoutTemplateExerciseId, setNumber);
                return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
            });
    }

    // Private helper methods

    private async Task<ServiceResult<SetConfigurationDto>> LoadSetConfigurationAsync(SetConfigurationId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();
        var setConfiguration = await repository.GetByIdAsync(id);

        var result = (setConfiguration == null || setConfiguration.IsEmpty) switch
        {
            true => ServiceResult<SetConfigurationDto>.Failure(
                new SetConfigurationDto(),
                ServiceError.NotFound(SetConfigurationErrorMessages.NotFound)),
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
        var validationResult = IsCreateCommandValid(command);
        
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

        var entityResult = SetConfigurationEntity.Handler.CreateNew(
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
        SetConfigurationEntity setConfiguration)
    {
        var savedSetConfiguration = await repository.AddAsync(setConfiguration);
        await unitOfWork.CommitAsync();

        return ServiceResult<SetConfigurationDto>.Success(MapToDto(savedSetConfiguration));
    }

    private async Task<ServiceResult<IEnumerable<SetConfigurationDto>>> ValidateAndProcessBulkCreateAsync(CreateBulkSetConfigurationsCommand command)
    {
        var validationResult = IsBulkCreateCommandValid(command);
        
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

        var setConfigurations = new List<SetConfigurationEntity>();

        foreach (var setData in command.SetConfigurations)
        {
            var entityResult = SetConfigurationEntity.Handler.CreateNew(
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
        var validationResult = IsUpdateCommandValid(command);
        
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
        
        var result = (existing == null || existing.IsEmpty) switch
        {
            true => ServiceResult<SetConfigurationDto>.Failure(
                new SetConfigurationDto(),
                ServiceError.NotFound(SetConfigurationErrorMessages.NotFound)),
            false => await UpdateExistingSetConfigurationAsync(repository, unitOfWork, existing, command)
        };

        return result;
    }

    private async Task<ServiceResult<SetConfigurationDto>> UpdateExistingSetConfigurationAsync(
        ISetConfigurationRepository repository,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        SetConfigurationEntity existing,
        UpdateSetConfigurationCommand command)
    {
        var updateResult = SetConfigurationEntity.Handler.Update(
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
        SetConfigurationEntity setConfiguration)
    {
        var updatedSetConfiguration = await repository.UpdateAsync(setConfiguration);
        await unitOfWork.CommitAsync();

        return ServiceResult<SetConfigurationDto>.Success(MapToDto(updatedSetConfiguration));
    }

    private async Task<ServiceResult<int>> ValidateAndProcessBulkUpdateAsync(UpdateBulkSetConfigurationsCommand command)
    {
        var validationResult = IsBulkUpdateCommandValid(command);
        
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

        var setConfigurations = new List<SetConfigurationEntity>();

        foreach (var updateData in command.SetConfigurationUpdates)
        {
            var existing = await repository.GetByIdAsync(updateData.SetConfigurationId);
            
            if (existing == null || existing.IsEmpty)
            {
                return ServiceResult<int>.Failure(
                    0,
                    ServiceError.NotFound(string.Format(SetConfigurationErrorMessages.SetConfigurationNotFoundWithId, updateData.SetConfigurationId)));
            }

            var updateResult = SetConfigurationEntity.Handler.Update(
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

    private async Task<ServiceResult<BooleanResultDto>> ValidateAndProcessReorderAsync(ReorderSetConfigurationsCommand command)
    {
        var validationResult = IsReorderCommandValid(command);
        
        var result = validationResult.IsValid switch
        {
            false => ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
                ServiceError.ValidationFailed(string.Join("; ", validationResult.Errors))),
            true => await ProcessReorderAsync(command)
        };

        return result;
    }

    private async Task<ServiceResult<BooleanResultDto>> ProcessReorderAsync(ReorderSetConfigurationsCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();

        var success = await repository.ReorderSetsAsync(command.WorkoutTemplateExerciseId, command.SetReorders);
        
        var result = success switch
        {
            false => ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
                ServiceError.ValidationFailed(SetConfigurationErrorMessages.FailedToReorderSetConfigurations)),
            true => await CommitReorderAsync(unitOfWork)
        };

        return result;
    }

    private async Task<ServiceResult<BooleanResultDto>> CommitReorderAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork)
    {
        await unitOfWork.CommitAsync();
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true));
    }

    private async Task<ServiceResult<BooleanResultDto>> ValidateAndProcessDeleteAsync(SetConfigurationId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();

        var existing = await repository.GetByIdAsync(id);
        
        var result = (existing == null || existing.IsEmpty) switch
        {
            true => ServiceResult<BooleanResultDto>.Failure(
                BooleanResultDto.Empty,
                ServiceError.NotFound(SetConfigurationErrorMessages.NotFound)),
            false => await ProcessDeleteAsync(repository, unitOfWork, id)
        };

        return result;
    }

    private async Task<ServiceResult<BooleanResultDto>> ProcessDeleteAsync(
        ISetConfigurationRepository repository,
        IWritableUnitOfWork<FitnessDbContext> unitOfWork,
        SetConfigurationId id)
    {
        var success = await repository.DeleteAsync(id);
        await unitOfWork.CommitAsync();

        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(success));
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

        return setConfiguration != null && !setConfiguration.IsEmpty;
    }

    private async Task<bool> CheckSetConfigurationExistsAsync(WorkoutTemplateExerciseId workoutTemplateExerciseId, int setNumber)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<ISetConfigurationRepository>();

        return await repository.ExistsAsync(workoutTemplateExerciseId, setNumber);
    }

    // Validation methods

    private ValidationResult IsCreateCommandValid(CreateSetConfigurationCommand command)
    {
        var errors = new List<string>();

        if (command.WorkoutTemplateExerciseId.IsEmpty)
            errors.Add(SetConfigurationErrorMessages.WorkoutTemplateExerciseIdRequired);

        if (command.SetNumber.HasValue && command.SetNumber.Value <= 0)
            errors.Add(SetConfigurationErrorMessages.SetNumberMustBeGreaterThanZero);

        if (command.RestSeconds < 0)
            errors.Add(SetConfigurationErrorMessages.RestSecondsCannotBeNegative);

        if (command.TargetWeight.HasValue && command.TargetWeight.Value < 0)
            errors.Add(SetConfigurationErrorMessages.TargetWeightCannotBeNegative);

        if (command.TargetTimeSeconds.HasValue && command.TargetTimeSeconds.Value <= 0)
            errors.Add(SetConfigurationErrorMessages.TargetTimeSecondsMustBeGreaterThanZero);

        if (command.UserId.IsEmpty)
            errors.Add(SetConfigurationErrorMessages.UserIdRequired);

        return new ValidationResult(errors);
    }

    private ValidationResult IsBulkCreateCommandValid(CreateBulkSetConfigurationsCommand command)
    {
        var errors = new List<string>();

        if (command.WorkoutTemplateExerciseId.IsEmpty)
            errors.Add(SetConfigurationErrorMessages.WorkoutTemplateExerciseIdRequired);

        if (!command.SetConfigurations.Any())
            errors.Add(SetConfigurationErrorMessages.AtLeastOneSetConfigurationRequired);

        if (command.UserId.IsEmpty)
            errors.Add(SetConfigurationErrorMessages.UserIdRequired);

        // Validate individual set configurations
        var setNumbers = new HashSet<int>();
        foreach (var setData in command.SetConfigurations)
        {
            if (setData.SetNumber <= 0)
                errors.Add(SetConfigurationErrorMessages.SetNumberMustBeGreaterThanZero);

            if (!setNumbers.Add(setData.SetNumber))
                errors.Add(string.Format(SetConfigurationErrorMessages.DuplicateSetNumberFound, setData.SetNumber));

            if (setData.RestSeconds < 0)
                errors.Add(string.Format(SetConfigurationErrorMessages.RestSecondsCannotBeNegativeForSet, setData.SetNumber));

            if (setData.TargetWeight.HasValue && setData.TargetWeight.Value < 0)
                errors.Add(string.Format(SetConfigurationErrorMessages.TargetWeightCannotBeNegativeForSet, setData.SetNumber));

            if (setData.TargetTimeSeconds.HasValue && setData.TargetTimeSeconds.Value <= 0)
                errors.Add(string.Format(SetConfigurationErrorMessages.TargetTimeSecondsMustBeGreaterThanZeroForSet, setData.SetNumber));
        }

        return new ValidationResult(errors);
    }

    private ValidationResult IsUpdateCommandValid(UpdateSetConfigurationCommand command)
    {
        var errors = new List<string>();

        if (command.SetConfigurationId.IsEmpty)
            errors.Add(SetConfigurationErrorMessages.SetConfigurationIdRequired);

        if (command.RestSeconds < 0)
            errors.Add(SetConfigurationErrorMessages.RestSecondsCannotBeNegative);

        if (command.TargetWeight.HasValue && command.TargetWeight.Value < 0)
            errors.Add(SetConfigurationErrorMessages.TargetWeightCannotBeNegative);

        if (command.TargetTimeSeconds.HasValue && command.TargetTimeSeconds.Value <= 0)
            errors.Add(SetConfigurationErrorMessages.TargetTimeSecondsMustBeGreaterThanZero);

        if (command.UserId.IsEmpty)
            errors.Add(SetConfigurationErrorMessages.UserIdRequired);

        return new ValidationResult(errors);
    }

    private ValidationResult IsBulkUpdateCommandValid(UpdateBulkSetConfigurationsCommand command)
    {
        var errors = new List<string>();

        if (command.WorkoutTemplateExerciseId.IsEmpty)
            errors.Add(SetConfigurationErrorMessages.WorkoutTemplateExerciseIdRequired);

        if (!command.SetConfigurationUpdates.Any())
            errors.Add(SetConfigurationErrorMessages.AtLeastOneSetConfigurationUpdateRequired);

        if (command.UserId.IsEmpty)
            errors.Add(SetConfigurationErrorMessages.UserIdRequired);

        // Validate individual updates
        foreach (var updateData in command.SetConfigurationUpdates)
        {
            if (updateData.SetConfigurationId.IsEmpty)
                errors.Add(SetConfigurationErrorMessages.SetConfigurationIdRequiredForAllUpdates);

            if (updateData.RestSeconds < 0)
                errors.Add(string.Format(SetConfigurationErrorMessages.RestSecondsCannotBeNegativeForSet, updateData.SetConfigurationId));

            if (updateData.TargetWeight.HasValue && updateData.TargetWeight.Value < 0)
                errors.Add(string.Format(SetConfigurationErrorMessages.TargetWeightCannotBeNegativeForSet, updateData.SetConfigurationId));

            if (updateData.TargetTimeSeconds.HasValue && updateData.TargetTimeSeconds.Value <= 0)
                errors.Add(string.Format(SetConfigurationErrorMessages.TargetTimeSecondsMustBeGreaterThanZeroForSet, updateData.SetConfigurationId));
        }

        return new ValidationResult(errors);
    }

    private ValidationResult IsReorderCommandValid(ReorderSetConfigurationsCommand command)
    {
        var errors = new List<string>();

        if (command.WorkoutTemplateExerciseId.IsEmpty)
            errors.Add(SetConfigurationErrorMessages.WorkoutTemplateExerciseIdRequired);

        if (!command.SetReorders.Any())
            errors.Add(SetConfigurationErrorMessages.AtLeastOneSetReorderRequired);

        if (command.UserId.IsEmpty)
            errors.Add(SetConfigurationErrorMessages.UserIdRequired);

        // Validate set numbers
        var setNumbers = new HashSet<int>();
        foreach (var reorder in command.SetReorders)
        {
            if (reorder.Key.IsEmpty)
                errors.Add(SetConfigurationErrorMessages.SetConfigurationIdRequiredForAllReorders);

            if (reorder.Value <= 0)
                errors.Add(SetConfigurationErrorMessages.SetNumberMustBeGreaterThanZero);

            if (!setNumbers.Add(reorder.Value))
                errors.Add(string.Format(SetConfigurationErrorMessages.DuplicateSetNumberInReorder, reorder.Value));
        }

        return new ValidationResult(errors);
    }

    // Mapping methods

    private static SetConfigurationDto MapToDto(SetConfigurationEntity setConfiguration) => new()
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