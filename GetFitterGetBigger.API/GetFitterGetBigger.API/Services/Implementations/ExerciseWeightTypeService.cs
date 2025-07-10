using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Utilities;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for exercise weight type operations
/// </summary>
public class ExerciseWeightTypeService : IExerciseWeightTypeService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ExerciseWeightTypeService> _logger;
    
    private const string CacheKeyPrefix = "ExerciseWeightTypes";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24); // Static reference table
    
    public ExerciseWeightTypeService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger<ExerciseWeightTypeService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _cacheService = cacheService;
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<ExerciseWeightType>> GetAllAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey(CacheKeyPrefix);
        
        var cached = await _cacheService.GetAsync<IEnumerable<ExerciseWeightType>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Retrieved {Count} exercise weight types from cache", cached.Count());
            return cached;
        }
        
        using var uow = _unitOfWorkProvider.CreateReadOnly();
        var repository = uow.GetRepository<IExerciseWeightTypeRepository>();
        var weightTypes = await repository.GetAllActiveAsync();
        
        var weightTypesList = weightTypes.ToList();
        await _cacheService.SetAsync(cacheKey, weightTypesList, CacheDuration);
        _logger.LogInformation("Cached {Count} exercise weight types", weightTypesList.Count);
        
        return weightTypesList;
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<ReferenceDataDto>> GetAllAsDtosAsync()
    {
        var weightTypes = await GetAllAsync();
        return weightTypes.Select(MapToDto);
    }
    
    /// <inheritdoc/>
    public async Task<ExerciseWeightType?> GetByIdAsync(ExerciseWeightTypeId id)
    {
        var cacheKey = CacheKeyGenerator.GetByIdKey(CacheKeyPrefix, id.ToString());
        
        var cached = await _cacheService.GetAsync<ExerciseWeightType>(cacheKey);
        if (cached != null)
        {
            return cached;
        }
        
        using var uow = _unitOfWorkProvider.CreateReadOnly();
        var repository = uow.GetRepository<IExerciseWeightTypeRepository>();
        var weightType = await repository.GetByIdAsync(id);
        
        if (weightType != null && weightType.IsActive)
        {
            await _cacheService.SetAsync(cacheKey, weightType, CacheDuration);
        }
        
        return weightType;
    }
    
    /// <inheritdoc/>
    public async Task<ReferenceDataDto?> GetByIdAsDtoAsync(string id)
    {
        if (!ExerciseWeightTypeId.TryParse(id, out var weightTypeId))
        {
            return null;
        }
        
        var weightType = await GetByIdAsync(weightTypeId);
        return weightType != null ? MapToDto(weightType) : null;
    }
    
    /// <inheritdoc/>
    public async Task<ExerciseWeightType?> GetByValueAsync(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        
        var cacheKey = CacheKeyGenerator.GetByValueKey(CacheKeyPrefix, value);
        
        var cached = await _cacheService.GetAsync<ExerciseWeightType>(cacheKey);
        if (cached != null)
        {
            return cached;
        }
        
        using var uow = _unitOfWorkProvider.CreateReadOnly();
        var repository = uow.GetRepository<IExerciseWeightTypeRepository>();
        var weightType = await repository.GetByValueAsync(value);
        
        if (weightType != null && weightType.IsActive)
        {
            await _cacheService.SetAsync(cacheKey, weightType, CacheDuration);
        }
        
        return weightType;
    }
    
    /// <inheritdoc/>
    public async Task<ReferenceDataDto?> GetByValueAsDtoAsync(string value)
    {
        var weightType = await GetByValueAsync(value);
        return weightType != null ? MapToDto(weightType) : null;
    }
    
    /// <inheritdoc/>
    public async Task<ExerciseWeightType?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }
        
        var cacheKey = $"{CacheKeyPrefix}:code:{code}";
        
        var cached = await _cacheService.GetAsync<ExerciseWeightType>(cacheKey);
        if (cached != null)
        {
            return cached;
        }
        
        using var uow = _unitOfWorkProvider.CreateReadOnly();
        var repository = uow.GetRepository<IExerciseWeightTypeRepository>();
        var weightType = await repository.GetByCodeAsync(code);
        
        if (weightType != null && weightType.IsActive)
        {
            await _cacheService.SetAsync(cacheKey, weightType, CacheDuration);
        }
        
        return weightType;
    }
    
    /// <inheritdoc/>
    public async Task<ReferenceDataDto?> GetByCodeAsDtoAsync(string code)
    {
        var weightType = await GetByCodeAsync(code);
        return weightType != null ? MapToDto(weightType) : null;
    }
    
    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(ExerciseWeightTypeId id)
    {
        var weightType = await GetByIdAsync(id);
        return weightType != null && weightType.IsActive;
    }
    
    /// <inheritdoc/>
    public async Task<bool> IsValidWeightForTypeAsync(ExerciseWeightTypeId weightTypeId, decimal? weight)
    {
        var weightType = await GetByIdAsync(weightTypeId);
        if (weightType == null)
        {
            return false;
        }
        
        // Validate based on weight type code
        return weightType.Code switch
        {
            "BODYWEIGHT_ONLY" => weight == null || weight == 0,
            "BODYWEIGHT_OPTIONAL" => true, // Any weight value is valid
            "WEIGHT_REQUIRED" => weight > 0,
            "MACHINE_WEIGHT" => weight > 0,
            "NO_WEIGHT" => weight == null || weight == 0,
            _ => false // Unknown weight type
        };
    }
    
    private static ReferenceDataDto MapToDto(ExerciseWeightType weightType)
    {
        return new ReferenceDataDto
        {
            Id = weightType.Id.ToString(),
            Value = weightType.Value,
            Description = weightType.Description
        };
    }
}