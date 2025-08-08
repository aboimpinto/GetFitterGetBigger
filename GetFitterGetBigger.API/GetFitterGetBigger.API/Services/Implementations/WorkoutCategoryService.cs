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
/// Service implementation for workout category operations with Empty pattern support
/// </summary>
public class WorkoutCategoryService : PureReferenceService<WorkoutCategory, WorkoutCategoryDto>, IWorkoutCategoryService
{
    public WorkoutCategoryService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEternalCacheService cacheService,
        ILogger<WorkoutCategoryService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutCategoryDto>> GetByIdAsync(WorkoutCategoryId id)
    {
        return await ServiceValidate.For<WorkoutCategoryDto>()
            .EnsureNotEmpty(id, WorkoutCategoryErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await GetByIdAsync(id.ToString())
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutCategoryDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<WorkoutCategoryDto>()
            .EnsureNotWhiteSpace(value, WorkoutCategoryErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await GetFromCacheOrLoadAsync(
                    GetValueCacheKey(value),
                    () => LoadByValueAsync(value),
                    value)
            );
    }

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<WorkoutCategoryDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<WorkoutCategory>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        
        return await CacheLoad.For<WorkoutCategoryDto>(cacheService, cacheKey)
            .WithLogging(_logger, "WorkoutCategory")
            .MatchAsync(
                onHit: cached => ServiceResult<WorkoutCategoryDto>.Success(cached),
                onMiss: async () => await LoadAndProcessEntity(loadFunc, cacheKey, identifier)
            );
    }
    
    private async Task<ServiceResult<WorkoutCategoryDto>> LoadAndProcessEntity(
        Func<Task<WorkoutCategory>> loadFunc,
        string cacheKey,
        string identifier)
    {
        var entity = await loadFunc();
        
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<WorkoutCategoryDto>.Failure(
                WorkoutCategoryDto.Empty, 
                ServiceError.NotFound(WorkoutCategoryErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<WorkoutCategoryDto>> CacheAndReturnSuccessAsync(string cacheKey, WorkoutCategoryDto dto)
    {
        // Use TimeSpan.MaxValue for eternal caching as per entity's cache strategy
        var cacheService = (IEternalCacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<WorkoutCategoryDto>.Success(dto);
    }
    
    private async Task<WorkoutCategory> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        return await repository.GetByValueAsync(value);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<bool>> ExistsAsync(WorkoutCategoryId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, WorkoutCategoryErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
            });
    }
    
    protected override async Task<IEnumerable<WorkoutCategory>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    /// <inheritdoc/>
    protected override async Task<ServiceResult<WorkoutCategory>> LoadEntityByIdAsync(string id)
    {
        var workoutCategoryId = WorkoutCategoryId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<WorkoutCategory>()
            .EnsureNotEmpty(workoutCategoryId, WorkoutCategoryErrorMessages.InvalidIdFormat)
            .Match(
                whenValid: async () => await LoadEntityFromRepository(workoutCategoryId),
                whenInvalid: errors => ServiceResult<WorkoutCategory>.Failure(
                    WorkoutCategory.Empty,
                    ServiceError.ValidationFailed(errors.FirstOrDefault() ?? "Invalid ID format"))
            );
    }
    
    private async Task<ServiceResult<WorkoutCategory>> LoadEntityFromRepository(WorkoutCategoryId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity switch
        {
            { IsEmpty: true } => ServiceResult<WorkoutCategory>.Failure(
                WorkoutCategory.Empty, 
                ServiceError.NotFound("WorkoutCategory")),
            _ => ServiceResult<WorkoutCategory>.Success(entity)
        };
    }
    
    protected override WorkoutCategoryDto MapToDto(WorkoutCategory entity)
    {
        return new WorkoutCategoryDto
        {
            WorkoutCategoryId = entity.WorkoutCategoryId.ToString(),
            Value = entity.Value,
            Description = entity.Description,
            Icon = entity.Icon,
            Color = entity.Color,
            PrimaryMuscleGroups = entity.PrimaryMuscleGroups,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive
        };
    }
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        return ServiceValidate.For()
            .EnsureNotWhiteSpace(id, WorkoutCategoryErrorMessages.IdCannotBeEmpty)
            .ToResult();
    }
}