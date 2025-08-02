using System.Linq;
using System.Reflection;
using GetFitterGetBigger.API.DTOs.Interfaces;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Base;

/// <summary>
/// Base service for pure reference data that never changes after deployment
/// Provides read-only operations with eternal caching
/// Supports both Empty pattern and non-Empty entities
/// </summary>
/// <typeparam name="TEntity">The pure reference entity type</typeparam>
/// <typeparam name="TDto">The DTO type returned by the service</typeparam>
public abstract class PureReferenceService<TEntity, TDto> : EntityServiceBase<TEntity>
    where TEntity : class, IPureReference
    where TDto : class, IEmptyDto<TDto>, new()
{
    protected readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    
    /// <summary>
    /// Initializes a new instance of the PureReferenceService class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="logger">The logger</param>
    protected PureReferenceService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEternalCacheService cacheService,
        ILogger logger)
        : base(cacheService, logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider ?? throw new ArgumentNullException(nameof(unitOfWorkProvider));
    }
    
    /// <summary>
    /// Gets all active entities
    /// </summary>
    /// <returns>A service result containing all active entities as DTOs</returns>
    public virtual async Task<ServiceResult<IEnumerable<TDto>>> GetAllAsync()
    {
        var cacheKey = GetAllCacheKey();
        var cacheService = (IEternalCacheService)_cacheService;
        var cacheResult = await cacheService.GetAsync<IEnumerable<TDto>>(cacheKey);
        
        if (cacheResult.IsHit)
        {
            _logger.LogDebug("Cache hit for {EntityType}:all", typeof(TEntity).Name);
            return ServiceResult<IEnumerable<TDto>>.Success(cacheResult.Value);
        }
        
        return await LoadAndCacheAllAsync(cacheKey);
    }
    
    private async Task<ServiceResult<IEnumerable<TDto>>> LoadAndCacheAllAsync(string cacheKey)
    {
        try
        {
            var entities = await LoadAllEntitiesAsync();
            
            // Map to DTOs
            var dtos = entities.Select(MapToDto).ToList();
            
            // Cache with eternal duration for pure reference data
            var cacheService = (IEternalCacheService)_cacheService;
            await cacheService.SetAsync(cacheKey, dtos);
            
            _logger.LogInformation("Loaded and cached {Count} {EntityType} entities", dtos.Count, typeof(TEntity).Name);
            return ServiceResult<IEnumerable<TDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading all {EntityType} entities from database", typeof(TEntity).Name);
            return ServiceResult<IEnumerable<TDto>>.Failure(
                [],
                ServiceError.InternalError($"Failed to load {typeof(TEntity).Name} data from database"));
        }
    }
    
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The entity ID in string format</param>
    /// <returns>A service result containing the entity as a DTO</returns>
    public virtual async Task<ServiceResult<TDto>> GetByIdAsync(string id)
    {
        // Validate and parse ID
        var parseResult = ValidateAndParseId(id);
        if (!parseResult.IsValid)
        {
            var errors = parseResult.Errors.Select(e => ServiceError.ValidationFailed(e)).ToArray();
            return ServiceResult<TDto>.Failure(TDto.Empty, errors);
        }
        
        var cacheKey = GetCacheKey(id);
        var cacheService = (IEternalCacheService)_cacheService;
        var cacheResult = await cacheService.GetAsync<TDto>(cacheKey);
        
        if (cacheResult.IsHit)
        {
            _logger.LogDebug("Cache hit for {EntityType}:{Id}", typeof(TEntity).Name, id);
            return ServiceResult<TDto>.Success(cacheResult.Value);
        }
        
        return await LoadAndCacheByIdAsync(id, cacheKey);
    }
    
    private async Task<ServiceResult<TDto>> LoadAndCacheByIdAsync(string id, string cacheKey)
    {
        try
        {
            var entityResult = await LoadEntityByIdAsync(id);
            
            return entityResult switch
            {
                { IsSuccess: false } => ServiceResult<TDto>.Failure(
                    TDto.Empty, 
                    entityResult.StructuredErrors.FirstOrDefault() ?? ServiceError.NotFound(typeof(TEntity).Name)),
                { Data: var entity } when !entity.IsActive => ServiceResult<TDto>.Failure(
                    TDto.Empty,
                    ServiceError.NotFound(typeof(TEntity).Name)),
                { Data: var entity } => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading {EntityType} with ID: {Id} from database", typeof(TEntity).Name, id);
            return ServiceResult<TDto>.Failure(
                TDto.Empty,
                ServiceError.InternalError($"Failed to load {typeof(TEntity).Name}"));
        }
    }
    
    private async Task<ServiceResult<TDto>> CacheAndReturnSuccessAsync(string cacheKey, TDto dto)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<TDto>.Success(dto);
    }
    
    /// <summary>
    /// Checks if an entity exists with the given ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>True if the entity exists and is active, false otherwise</returns>
    public virtual async Task<bool> ExistsAsync(string id)
    {
        var result = await GetByIdAsync(id);
        return result.IsSuccess;
    }
    
    /// <summary>
    /// Loads all entities from the database
    /// Must be implemented by derived classes
    /// </summary>
    /// <returns>A collection of all active entities</returns>
    protected abstract Task<IEnumerable<TEntity>> LoadAllEntitiesAsync();
    
    /// <summary>
    /// Loads a single entity by ID from the database
    /// Must be implemented by derived classes
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>A ServiceResult containing the entity or an error</returns>
    protected abstract Task<ServiceResult<TEntity>> LoadEntityByIdAsync(string id);
    
    /// <summary>
    /// Maps an entity to its DTO representation
    /// Must be implemented by derived classes
    /// </summary>
    /// <param name="entity">The entity to map</param>
    /// <returns>The mapped DTO</returns>
    protected abstract TDto MapToDto(TEntity entity);
    
    
    /// <summary>
    /// Validates and parses the entity ID
    /// Must be implemented by derived classes
    /// </summary>
    /// <param name="id">The ID string to validate</param>
    /// <returns>A validation result</returns>
    protected abstract ValidationResult ValidateAndParseId(string id);
    
}