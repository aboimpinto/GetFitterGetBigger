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
/// Service implementation for workout category operations with Empty pattern support
/// </summary>
public class WorkoutCategoryService : EmptyEnabledPureReferenceService<WorkoutCategory, WorkoutCategoryDto>, IWorkoutCategoryService
{
    public WorkoutCategoryService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEmptyEnabledCacheService cacheService,
        ILogger<WorkoutCategoryService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutCategoryDto>> GetByIdAsync(WorkoutCategoryId id) => 
        id.IsEmpty 
            ? ServiceResult<WorkoutCategoryDto>.Failure(CreateEmptyDto(), ServiceError.ValidationFailed("Invalid workout category ID format"))
            : await GetByIdAsync(id.ToString());
    
    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutCategoryDto>> GetByValueAsync(string value) => 
        string.IsNullOrWhiteSpace(value)
            ? ServiceResult<WorkoutCategoryDto>.Failure(CreateEmptyDto(), ServiceError.ValidationFailed("Value cannot be empty"))
            : await GetFromCacheOrLoadAsync(
                GetValueCacheKey(value),
                () => LoadByValueAsync(value),
                value);

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<WorkoutCategoryDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<WorkoutCategory>> loadFunc,
        string identifier)
    {
        var cacheService = (IEmptyEnabledCacheService)_cacheService;
        var cacheResult = await cacheService.GetAsync<WorkoutCategoryDto>(cacheKey);
        if (cacheResult.IsHit)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return ServiceResult<WorkoutCategoryDto>.Success(cacheResult.Value);
        }
        
        var entity = await loadFunc();
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<WorkoutCategoryDto>.Failure(
                CreateEmptyDto(), 
                ServiceError.NotFound("Workout category not found", identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<WorkoutCategoryDto>> CacheAndReturnSuccessAsync(string cacheKey, WorkoutCategoryDto dto)
    {
        // Use TimeSpan.MaxValue for eternal caching as per entity's cache strategy
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.MaxValue);
        return ServiceResult<WorkoutCategoryDto>.Success(dto);
    }
    
    private async Task<WorkoutCategory> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        return await repository.GetByValueAsync(value);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(WorkoutCategoryId id) => 
        !id.IsEmpty && (await GetByIdAsync(id)).IsSuccess;
    
    /// <inheritdoc/>
    public override async Task<bool> ExistsAsync(string id) => 
        await ExistsAsync(WorkoutCategoryId.ParseOrEmpty(id));
    
    protected override async Task<IEnumerable<WorkoutCategory>> LoadAllEntitiesAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    // Returns WorkoutCategory.Empty instead of null (Null Object Pattern)
    protected override async Task<WorkoutCategory> LoadEntityByIdAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string id) =>
        WorkoutCategoryId.ParseOrEmpty(id) switch
        {
            { IsEmpty: true } => WorkoutCategory.Empty,
            var workoutCategoryId => await unitOfWork.GetRepository<IWorkoutCategoryRepository>().GetByIdAsync(workoutCategoryId)
        };
    
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
    
    protected override WorkoutCategoryDto CreateEmptyDto()
    {
        return new WorkoutCategoryDto
        {
            WorkoutCategoryId = string.Empty,
            Value = string.Empty,
            Description = null,
            Icon = string.Empty,
            Color = "#000000",
            PrimaryMuscleGroups = null,
            DisplayOrder = 0,
            IsActive = false
        };
    }
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        // This is called by the base class when using the string overload
        // Since we always use the typed overload from the controller,
        // this should validate the string format
        if (string.IsNullOrWhiteSpace(id))
        {
            return ValidationResult.Failure("ID cannot be empty");
        }
        
        // No additional validation - let the controller handle format validation
        // This allows empty GUIDs to pass through and be treated as NotFound
        
        // Valid format (including empty GUID) - let the database determine if it exists
        return ValidationResult.Success();
    }
}