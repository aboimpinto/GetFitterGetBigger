using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for workout state operations
/// </summary>
public class WorkoutStateService : PureReferenceService<WorkoutState, WorkoutStateDto>, IWorkoutStateService
{
    public WorkoutStateService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEternalCacheService cacheService,
        ILogger<WorkoutStateService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutStateDto>> GetByIdAsync(WorkoutStateId id)
    {
        return await ServiceValidate.For<WorkoutStateDto>()
            .EnsureNotEmpty(id, WorkoutStateErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await GetByIdAsync(id.ToString())
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutStateDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<WorkoutStateDto>()
            .EnsureNotWhiteSpace(value, WorkoutStateErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await GetFromCacheOrLoadAsync(
                    GetValueCacheKey(value),
                    () => LoadByValueAsync(value),
                    value)
            );
    }

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<WorkoutStateDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<WorkoutState>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        
        return await CacheLoad.For<WorkoutStateDto>(cacheService, cacheKey)
            .WithLogging(_logger, "WorkoutState")
            .MatchAsync(
                onHit: cached => ServiceResult<WorkoutStateDto>.Success(cached),
                onMiss: async () => await LoadAndProcessEntity(loadFunc, cacheKey, identifier)
            );
    }
    
    private async Task<ServiceResult<WorkoutStateDto>> LoadAndProcessEntity(
        Func<Task<WorkoutState>> loadFunc,
        string cacheKey,
        string identifier)
    {
        var entity = await loadFunc();
        
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<WorkoutStateDto>.Failure(
                WorkoutStateDto.Empty, 
                ServiceError.NotFound(WorkoutStateErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<WorkoutStateDto>> CacheAndReturnSuccessAsync(string cacheKey, WorkoutStateDto dto)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<WorkoutStateDto>.Success(dto);
    }
    
    private async Task<WorkoutState> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        return await repository.GetByValueAsync(value);
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
    
    protected override async Task<IEnumerable<WorkoutState>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    /// <inheritdoc/>
    protected override async Task<ServiceResult<WorkoutState>> LoadEntityByIdAsync(string id)
    {
        var workoutStateId = WorkoutStateId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<WorkoutState>()
            .EnsureNotEmpty(workoutStateId, WorkoutStateErrorMessages.InvalidIdFormat)
            .Match(
                whenValid: async () => await LoadEntityFromRepository(workoutStateId),
                whenInvalid: errors => ServiceResult<WorkoutState>.Failure(
                    WorkoutState.Empty,
                    ServiceError.ValidationFailed(errors.FirstOrDefault() ?? "Invalid ID format"))
            );
    }
    
    private async Task<ServiceResult<WorkoutState>> LoadEntityFromRepository(WorkoutStateId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity switch
        {
            { IsEmpty: true } => ServiceResult<WorkoutState>.Failure(
                WorkoutState.Empty, 
                ServiceError.NotFound("WorkoutState")),
            _ => ServiceResult<WorkoutState>.Success(entity)
        };
    }
    
    protected override WorkoutStateDto MapToDto(WorkoutState entity)
    {
        return new WorkoutStateDto
        {
            Id = entity.Id,
            Value = entity.Value,
            Description = entity.Description
        };
    }
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        return ServiceValidate.For()
            .EnsureNotWhiteSpace(id, WorkoutStateErrorMessages.IdCannotBeEmpty)
            .ToResult();
    }
}