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
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(id))
            .ThenMatchDataAsync<WorkoutStateDto, WorkoutStateDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<WorkoutStateDto>.Failure(
                        WorkoutStateDto.Empty,
                        ServiceError.NotFound("WorkoutState", id.ToString()))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<WorkoutStateDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Gets a workout_state by its ID string
    /// </summary>
    /// <param name="id">The workout_state ID as a string</param>
    /// <returns>A service result containing the workout_state if found</returns>
    public async Task<ServiceResult<WorkoutStateDto>> GetByIdAsync(string id)
    {
        var workoutStateId = WorkoutStateId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<WorkoutStateDto>()
            .EnsureNotEmpty(workoutStateId, WorkoutStateErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(workoutStateId)
            );
    }
    
    private async Task<ServiceResult<WorkoutStateDto>> LoadByIdFromDatabaseAsync(WorkoutStateId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        // Database layer: Return what we find - let API layer decide HTTP response
        return entity.IsActive
            ? ServiceResult<WorkoutStateDto>.Success(MapToDto(entity))
            : ServiceResult<WorkoutStateDto>.Success(WorkoutStateDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutStateDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<WorkoutStateDto>()
            .EnsureNotWhiteSpace(value, WorkoutStateErrorMessages.ValueCannotBeEmpty)
            .WithServiceResultAsync(() => LoadByValueFromDatabaseAsync(value))
            .ThenMatchDataAsync<WorkoutStateDto, WorkoutStateDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<WorkoutStateDto>.Failure(
                        WorkoutStateDto.Empty,
                        ServiceError.NotFound("WorkoutState", value))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<WorkoutStateDto>.Success(dto))
            );
    }
    
    private async Task<ServiceResult<WorkoutStateDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        // Database layer: Return what we find - let API layer decide HTTP response
        return entity.IsActive
            ? ServiceResult<WorkoutStateDto>.Success(MapToDto(entity))
            : ServiceResult<WorkoutStateDto>.Success(WorkoutStateDto.Empty);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutStateId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, WorkoutStateErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                    var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
                    var exists = await repository.ExistsAsync(id);
                    return ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = exists });
                });
    }
    
    /// <summary>
    /// Maps a WorkoutState entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private WorkoutStateDto MapToDto(WorkoutState entity)
    {
        if (entity.IsEmpty)
            return WorkoutStateDto.Empty;
            
        return new WorkoutStateDto
        {
            Id = entity.WorkoutStateId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
