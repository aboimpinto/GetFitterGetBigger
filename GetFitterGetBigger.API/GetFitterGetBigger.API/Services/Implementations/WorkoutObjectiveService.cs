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
/// Service implementation for managing workout objectives reference data
/// </summary>
public class WorkoutObjectiveService : IWorkoutObjectiveService
{
    private const string CacheKeyPrefix = "WorkoutObjectives";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ICacheService _cacheService;
    private readonly ILogger<WorkoutObjectiveService> _logger;
    
    public WorkoutObjectiveService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger<WorkoutObjectiveService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider ?? throw new ArgumentNullException(nameof(unitOfWorkProvider));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<WorkoutObjective>> GetAllAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey(CacheKeyPrefix);
        var cached = await _cacheService.GetAsync<IEnumerable<WorkoutObjective>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        var objectives = await repository.GetAllActiveAsync();
        var objectiveList = objectives.ToList();
        
        await _cacheService.SetAsync(cacheKey, objectiveList, CacheDuration);
        _logger.LogDebug("Cached {Count} workout objectives with key: {CacheKey}", objectiveList.Count, cacheKey);
        
        return objectiveList;
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<ReferenceDataDto>> GetAllAsDtosAsync()
    {
        var objectives = await GetAllAsync();
        return objectives.Select(wo => new ReferenceDataDto
        {
            Id = wo.Id.ToString(),
            Value = wo.Value,
            Description = wo.Description
        });
    }
    
    /// <inheritdoc />
    public async Task<WorkoutObjective?> GetByIdAsync(WorkoutObjectiveId id)
    {
        if (id.IsEmpty)
        {
            _logger.LogWarning("Attempted to get workout objective with empty ID");
            return null;
        }
        
        var cacheKey = CacheKeyGenerator.GetByIdKey(CacheKeyPrefix, id.ToString());
        var cached = await _cacheService.GetAsync<WorkoutObjective>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        var objective = await repository.GetByIdAsync(id);
        
        if (objective != null && objective.IsActive)
        {
            await _cacheService.SetAsync(cacheKey, objective, CacheDuration);
            _logger.LogDebug("Cached workout objective with key: {CacheKey}", cacheKey);
        }
        
        return objective?.IsActive == true ? objective : null;
    }
    
    /// <inheritdoc />
    public async Task<ReferenceDataDto?> GetByIdAsDtoAsync(string id)
    {
        var objectiveId = WorkoutObjectiveId.From(id);
        if (objectiveId.IsEmpty)
        {
            _logger.LogWarning("Invalid workout objective ID format: {Id}", id);
            return null;
        }
        
        var objective = await GetByIdAsync(objectiveId);
        if (objective == null) return null;
        
        return new ReferenceDataDto
        {
            Id = objective.Id.ToString(),
            Value = objective.Value,
            Description = objective.Description
        };
    }
    
    /// <inheritdoc />
    public async Task<WorkoutObjective?> GetByValueAsync(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _logger.LogWarning("Attempted to get workout objective with empty value");
            return null;
        }
        
        var cacheKey = CacheKeyGenerator.GetByValueKey(CacheKeyPrefix, value);
        var cached = await _cacheService.GetAsync<WorkoutObjective>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        var objective = await repository.GetByValueAsync(value);
        
        if (objective != null)
        {
            await _cacheService.SetAsync(cacheKey, objective, CacheDuration);
            _logger.LogDebug("Cached workout objective with key: {CacheKey}", cacheKey);
        }
        
        return objective;
    }
    
    /// <inheritdoc />
    public async Task<bool> ExistsAsync(WorkoutObjectiveId id)
    {
        if (id.IsEmpty) return false;
        
        var objective = await GetByIdAsync(id);
        return objective != null;
    }
}