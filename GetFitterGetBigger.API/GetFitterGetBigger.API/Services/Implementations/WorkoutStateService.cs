using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
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
    public async Task<ServiceResult<WorkoutStateDto>> GetByIdAsync(WorkoutStateId id) => 
        id.IsEmpty 
            ? ServiceResult<WorkoutStateDto>.Failure(WorkoutStateDto.Empty, ServiceError.ValidationFailed(WorkoutStateErrorMessages.InvalidIdFormat))
            : await GetByIdAsync(id.ToString());
    
    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutStateDto>> GetByValueAsync(string value) => 
        string.IsNullOrWhiteSpace(value)
            ? ServiceResult<WorkoutStateDto>.Failure(WorkoutStateDto.Empty, ServiceError.ValidationFailed(WorkoutStateErrorMessages.ValueCannotBeEmpty))
            : await GetFromCacheOrLoadAsync(
                GetValueCacheKey(value),
                () => LoadByValueAsync(value),
                value);

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<WorkoutStateDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<WorkoutState>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        var cacheResult = await cacheService.GetAsync<WorkoutStateDto>(cacheKey);
        
        if (cacheResult.IsHit)
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
        
        var result = cacheResult.IsHit
            ? ServiceResult<WorkoutStateDto>.Success(cacheResult.Value)
            : await ProcessUncachedEntity(await loadFunc(), cacheKey, identifier);
            
        return result;
    }
    
    private async Task<ServiceResult<WorkoutStateDto>> ProcessUncachedEntity(
        WorkoutState entity, string cacheKey, string identifier) =>
        entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<WorkoutStateDto>.Failure(
                WorkoutStateDto.Empty, 
                ServiceError.NotFound(WorkoutStateErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    
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
    public async Task<bool> ExistsAsync(WorkoutStateId id) => 
        !id.IsEmpty && (await GetByIdAsync(id)).IsSuccess;
    
    /// <inheritdoc/>
    public override async Task<bool> ExistsAsync(string id) => 
        await ExistsAsync(WorkoutStateId.ParseOrEmpty(id));
    
    protected override async Task<IEnumerable<WorkoutState>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    // Returns WorkoutState.Empty instead of null (Null Object Pattern)
    protected override async Task<WorkoutState> LoadEntityByIdAsync(string id)
    {
        var workoutStateId = WorkoutStateId.ParseOrEmpty(id);
        
        var result = workoutStateId.IsEmpty
            ? WorkoutState.Empty
            : await LoadFromRepository(workoutStateId);
            
        return result;
    }
    
    private async Task<WorkoutState> LoadFromRepository(WorkoutStateId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        return await repository.GetByIdAsync(id);
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
        // This is called by the base class when using the string overload
        // Since we always use the typed overload from the controller,
        // this should validate the string format
        if (string.IsNullOrWhiteSpace(id))
        {
            return ValidationResult.Failure(WorkoutStateErrorMessages.IdCannotBeEmpty);
        }
        
        // No additional validation - let the controller handle format validation
        // This allows empty GUIDs to pass through and be treated as NotFound
        
        // Valid format (including empty GUID) - let the database determine if it exists
        return ValidationResult.Success();
    }
}