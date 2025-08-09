using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for workout_state operations
/// This service focuses solely on business logic and data access
/// Caching is handled by the wrapping WorkoutStateReferenceService layer
/// </summary>
public class WorkoutStateService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<WorkoutStateService> logger) : IWorkoutStateService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<WorkoutStateService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<WorkoutStateDto>>> GetAllAsync()
    {
        return await GetAllActiveAsync();
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<WorkoutStateDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active workout_states", dtos.Count);
        return ServiceResult<IEnumerable<WorkoutStateDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutStateDto>> GetByIdAsync(WorkoutStateId id)
    {
        return await ServiceValidate.For<WorkoutStateDto>()
            .EnsureNotEmpty(id, WorkoutStateErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(id)
            );
    }
    
    /// <summary>
    /// Gets a workout_state by its ID string
    /// </summary>
    /// <param name="id">The workout_state ID as a string</param>
    /// <returns>A service result containing the workout_state if found</returns>
    public async Task<ServiceResult<WorkoutStateDto>> GetByIdAsync(string id)
    {
        var workout_stateId = WorkoutStateId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<WorkoutStateDto>()
            .EnsureNotEmpty(workout_stateId, WorkoutStateErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(workout_stateId)
            );
    }
    
    private async Task<ServiceResult<WorkoutStateDto>> LoadByIdFromDatabaseAsync(WorkoutStateId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return (entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<WorkoutStateDto>.Failure(
                WorkoutStateDto.Empty,
                ServiceError.NotFound("WorkoutState", id.ToString())),
            false => ServiceResult<WorkoutStateDto>.Success(MapToDto(entity))
        };
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutStateDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<WorkoutStateDto>()
            .EnsureNotWhiteSpace(value, WorkoutStateErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await LoadByValueFromDatabaseAsync(value)
            );
    }
    
    private async Task<ServiceResult<WorkoutStateDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        return (entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<WorkoutStateDto>.Failure(
                WorkoutStateDto.Empty,
                ServiceError.NotFound("WorkoutState", value)),
            false => ServiceResult<WorkoutStateDto>.Success(MapToDto(entity))
        };
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<bool>> ExistsAsync(WorkoutStateId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, WorkoutStateErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
            });
    }
    
    /// <summary>
    /// Maps a WorkoutState entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private WorkoutStateDto MapToDto(WorkoutState entity)
    {
        return new WorkoutStateDto
        {
            Id = entity.WorkoutStateId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
