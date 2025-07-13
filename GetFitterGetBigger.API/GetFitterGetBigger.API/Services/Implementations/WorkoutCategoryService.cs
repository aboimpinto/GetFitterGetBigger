using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Utilities;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;
using GetFitterGetBigger.API.Models;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for managing workout categories reference data
/// </summary>
public class WorkoutCategoryService : IWorkoutCategoryService
{
    private const string CacheKeyPrefix = "WorkoutCategories";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ICacheService _cacheService;
    private readonly ILogger<WorkoutCategoryService> _logger;
    
    public WorkoutCategoryService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger<WorkoutCategoryService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider ?? throw new ArgumentNullException(nameof(unitOfWorkProvider));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutCategory>> GetAllAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey(CacheKeyPrefix);
        var cached = await _cacheService.GetAsync<IEnumerable<WorkoutCategory>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var categories = await repository.GetAllActiveAsync();
        var categoryList = categories.ToList();
        
        await _cacheService.SetAsync(cacheKey, categoryList, CacheDuration);
        _logger.LogDebug("Cached {Count} workout categories with key: {CacheKey}", categoryList.Count, cacheKey);
        
        return categoryList;
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutCategoryDto>> GetAllAsDtosAsync()
    {
        var categories = await GetAllAsync();
        return categories.Select(wc => new WorkoutCategoryDto
        {
            WorkoutCategoryId = wc.Id.ToString(),
            Value = wc.Value,
            Description = wc.Description,
            Icon = wc.Icon,
            Color = wc.Color,
            PrimaryMuscleGroups = wc.PrimaryMuscleGroups,
            DisplayOrder = wc.DisplayOrder,
            IsActive = wc.IsActive
        });
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutCategoryDto>> GetAllAsWorkoutCategoryDtosAsync(bool includeInactive = false)
    {
        var cacheKey = CacheKeyGenerator.GetAllKey($"{CacheKeyPrefix}_{(includeInactive ? "All" : "Active")}");
        var cached = await _cacheService.GetAsync<IEnumerable<WorkoutCategoryDto>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        
        var categories = includeInactive 
            ? await repository.GetAllAsync() 
            : await repository.GetAllActiveAsync();
            
        var dtoList = categories
            .OrderBy(wc => wc.DisplayOrder)
            .Select(wc => new WorkoutCategoryDto
            {
                WorkoutCategoryId = wc.Id.ToString(),
                Value = wc.Value,
                Description = wc.Description,
                Icon = wc.Icon,
                Color = wc.Color,
                PrimaryMuscleGroups = wc.PrimaryMuscleGroups,
                DisplayOrder = wc.DisplayOrder,
                IsActive = wc.IsActive
            })
            .ToList();
        
        await _cacheService.SetAsync(cacheKey, dtoList, CacheDuration);
        _logger.LogDebug("Cached {Count} workout category DTOs with key: {CacheKey}", dtoList.Count, cacheKey);
        
        return dtoList;
    }
    
    /// <inheritdoc />
    public async Task<WorkoutCategory?> GetByIdAsync(WorkoutCategoryId id)
    {
        if (id.IsEmpty)
        {
            _logger.LogWarning("Attempted to get workout category with empty ID");
            return null;
        }
        
        var cacheKey = CacheKeyGenerator.GetByIdKey(CacheKeyPrefix, id.ToString());
        var cached = await _cacheService.GetAsync<WorkoutCategory>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var category = await repository.GetByIdAsync(id);
        
        if (category != null && category.IsActive)
        {
            await _cacheService.SetAsync(cacheKey, category, CacheDuration);
            _logger.LogDebug("Cached workout category with key: {CacheKey}", cacheKey);
        }
        
        return category?.IsActive == true ? category : null;
    }
    
    /// <inheritdoc />
    public async Task<WorkoutCategoryDto?> GetByIdAsDtoAsync(string id)
    {
        var categoryId = WorkoutCategoryId.From(id);
        if (categoryId.IsEmpty)
        {
            _logger.LogWarning("Invalid workout category ID format: {Id}", id);
            return null;
        }
        
        var category = await GetByIdAsync(categoryId);
        if (category == null) return null;
        
        return new WorkoutCategoryDto
        {
            WorkoutCategoryId = category.Id.ToString(),
            Value = category.Value,
            Description = category.Description,
            Icon = category.Icon,
            Color = category.Color,
            PrimaryMuscleGroups = category.PrimaryMuscleGroups,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };
    }
    
    /// <inheritdoc />
    public async Task<WorkoutCategoryDto?> GetByIdAsWorkoutCategoryDtoAsync(string id, bool includeInactive = false)
    {
        var categoryId = WorkoutCategoryId.From(id);
        if (categoryId.IsEmpty)
        {
            _logger.LogWarning("Invalid workout category ID format: {Id}", id);
            return null;
        }
        
        var cacheKey = CacheKeyGenerator.GetByIdKey($"{CacheKeyPrefix}_{(includeInactive ? "All" : "Active")}", id);
        var cached = await _cacheService.GetAsync<WorkoutCategoryDto>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var category = await repository.GetByIdAsync(categoryId);
        
        if (category == null || (!includeInactive && !category.IsActive))
        {
            return null;
        }
        
        var dto = new WorkoutCategoryDto
        {
            WorkoutCategoryId = category.Id.ToString(),
            Value = category.Value,
            Description = category.Description,
            Icon = category.Icon,
            Color = category.Color,
            PrimaryMuscleGroups = category.PrimaryMuscleGroups,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };
        
        await _cacheService.SetAsync(cacheKey, dto, CacheDuration);
        _logger.LogDebug("Cached workout category DTO with key: {CacheKey}", cacheKey);
        
        return dto;
    }
    
    /// <inheritdoc />
    public async Task<WorkoutCategory?> GetByValueAsync(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _logger.LogWarning("Attempted to get workout category with empty value");
            return null;
        }
        
        var cacheKey = CacheKeyGenerator.GetByValueKey(CacheKeyPrefix, value);
        var cached = await _cacheService.GetAsync<WorkoutCategory>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var category = await repository.GetByValueAsync(value);
        
        if (category != null)
        {
            await _cacheService.SetAsync(cacheKey, category, CacheDuration);
            _logger.LogDebug("Cached workout category with key: {CacheKey}", cacheKey);
        }
        
        return category;
    }
    
    /// <inheritdoc />
    public async Task<bool> ExistsAsync(WorkoutCategoryId id)
    {
        if (id.IsEmpty) return false;
        
        var category = await GetByIdAsync(id);
        return category != null;
    }
}