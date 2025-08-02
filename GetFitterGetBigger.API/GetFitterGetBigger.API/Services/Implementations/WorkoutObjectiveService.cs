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
/// Service implementation for workout objective operations
/// </summary>
public class WorkoutObjectiveService : PureReferenceService<WorkoutObjective, ReferenceDataDto>, IWorkoutObjectiveService
{
    public WorkoutObjectiveService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEternalCacheService cacheService,
        ILogger<WorkoutObjectiveService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(WorkoutObjectiveId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, ServiceError.ValidationFailed(WorkoutObjectiveErrorMessages.InvalidIdFormat))
            .MatchAsync(
                whenValid: async () => await GetByIdAsync(id.ToString())
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, ServiceError.ValidationFailed(WorkoutObjectiveErrorMessages.ValueCannotBeEmpty))
            .MatchAsync(
                whenValid: async () => await GetFromCacheOrLoadAsync(
                    GetValueCacheKey(value),
                    () => LoadByValueAsync(value),
                    value)
            );
    }

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<ReferenceDataDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<WorkoutObjective>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        
        return await CacheLoad.For<ReferenceDataDto>(cacheService, cacheKey)
            .WithLogging(_logger, "WorkoutObjective")
            .MatchAsync(
                onHit: cached => ServiceResult<ReferenceDataDto>.Success(cached),
                onMiss: async () => await LoadAndProcessEntity(loadFunc, cacheKey, identifier)
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadAndProcessEntity(
        Func<Task<WorkoutObjective>> loadFunc,
        string cacheKey,
        string identifier)
    {
        var entity = await loadFunc();
        
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty, 
                ServiceError.NotFound(WorkoutObjectiveErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> CacheAndReturnSuccessAsync(string cacheKey, ReferenceDataDto dto)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }
    
    private async Task<WorkoutObjective> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        return await repository.GetByValueAsync(value);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(WorkoutObjectiveId id) => 
        !id.IsEmpty && (await GetByIdAsync(id)).IsSuccess;
    
    /// <inheritdoc/>
    public override async Task<bool> ExistsAsync(string id) => 
        await ExistsAsync(WorkoutObjectiveId.ParseOrEmpty(id));
    
    protected override async Task<IEnumerable<WorkoutObjective>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    /// <inheritdoc/>
    protected override async Task<ServiceResult<WorkoutObjective>> LoadEntityByIdAsync(string id)
    {
        var workoutObjectiveId = WorkoutObjectiveId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<WorkoutObjective>()
            .EnsureNotEmpty(workoutObjectiveId, ServiceError.InvalidFormat("WorkoutObjectiveId", WorkoutObjectiveErrorMessages.InvalidIdFormat))
            .Match(
                whenValid: async () => await LoadEntityFromRepository(workoutObjectiveId),
                whenInvalid: errors => ServiceResult<WorkoutObjective>.Failure(
                    WorkoutObjective.Empty,
                    ServiceError.ValidationFailed(errors.FirstOrDefault() ?? "Invalid ID format"))
            );
    }
    
    private async Task<ServiceResult<WorkoutObjective>> LoadEntityFromRepository(WorkoutObjectiveId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity switch
        {
            { IsEmpty: true } => ServiceResult<WorkoutObjective>.Failure(
                WorkoutObjective.Empty, 
                ServiceError.NotFound("WorkoutObjective")),
            _ => ServiceResult<WorkoutObjective>.Success(entity)
        };
    }
    
    protected override ReferenceDataDto MapToDto(WorkoutObjective entity)
    {
        return new ReferenceDataDto
        {
            Id = entity.Id,
            Value = entity.Value,
            Description = entity.Description
        };
    }
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        return ServiceValidate.For()
            .EnsureNotWhiteSpace(id, WorkoutObjectiveErrorMessages.IdCannotBeEmpty)
            .ToResult();
    }
}