using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Base;

/// <summary>
/// Generic base service for enhanced reference data entities with strongly-typed IDs.
/// Eliminates string conversions and provides type-safe ID handling.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TDto">The DTO type</typeparam>
/// <typeparam name="TId">The specialized ID type</typeparam>
/// <typeparam name="TCreateCommand">The create command type</typeparam>
/// <typeparam name="TUpdateCommand">The update command type</typeparam>
public abstract class EnhancedReferenceServiceGeneric<TEntity, TDto, TId, TCreateCommand, TUpdateCommand>
    where TEntity : class, IEntity<TId>, ITrackedEntity, ICacheableEntity
    where TDto : class, new()
    where TId : struct, ISpecializedId<TId>
    where TCreateCommand : class
    where TUpdateCommand : class
{
    protected readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    protected readonly ICacheService _cacheService;
    protected readonly ILogger _logger;
    
    protected EnhancedReferenceServiceGeneric(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider ?? throw new ArgumentNullException(nameof(unitOfWorkProvider));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Gets all active entities
    /// </summary>
    public virtual async Task<ServiceResult<IEnumerable<TDto>>> GetAllAsync()
    {
        var cacheKey = GetCacheKey("all");
        var cacheService = (ICacheService)_cacheService;
        var cached = await cacheService.GetAsync<IEnumerable<TDto>>(cacheKey);
        
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for {EntityType}:all", typeof(TEntity).Name);
            return ServiceResult<IEnumerable<TDto>>.Success(cached);
        }
        
        var entities = await LoadAllEntitiesAsync();
        var dtos = entities.Select(MapToDto).ToList();
        
        await cacheService.SetAsync(cacheKey, dtos);
        
        _logger.LogInformation("Loaded and cached {Count} {EntityType} entities", dtos.Count, typeof(TEntity).Name);
        return ServiceResult<IEnumerable<TDto>>.Success(dtos);
    }
    
    /// <summary>
    /// Gets an entity by its strongly-typed ID
    /// </summary>
    public virtual async Task<ServiceResult<TDto>> GetByIdAsync(TId id)
    {
        if (id.IsEmpty)
        {
            return ServiceResult<TDto>.Failure(
                CreateEmptyDto(), 
                ServiceError.InvalidFormat("ID", GetExpectedIdFormat()));
        }
        
        return await LoadEntityAsync(id);
    }
    
    /// <summary>
    /// Creates a new entity
    /// </summary>
    public virtual async Task<ServiceResult<TDto>> CreateAsync(TCreateCommand command)
    {
        var validationResult = await ValidateCreateCommand(command);
        if (!validationResult.IsValid)
        {
            if (validationResult.ServiceError != null)
            {
                return ServiceResult<TDto>.Failure(
                    CreateEmptyDto(),
                    validationResult.ServiceError);
            }
            return ServiceResult<TDto>.Failure(
                CreateEmptyDto(),
                validationResult.Errors);
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        
        var entity = await CreateEntityAsync(unitOfWork, command);
        await unitOfWork.CommitAsync();
        
        await InvalidateCacheAsync();
        
        var dto = MapToDto(entity);
        
        _logger.LogInformation("Created new {EntityType} with ID: {Id}", typeof(TEntity).Name, entity.Id);
        return ServiceResult<TDto>.Success(dto);
    }
    
    /// <summary>
    /// Updates an existing entity using strongly-typed ID
    /// </summary>
    public virtual async Task<ServiceResult<TDto>> UpdateAsync(TId id, TUpdateCommand command)
    {
        // No more string conversion!
        var existingResult = await ExistsAsync(id);
        if (!existingResult.IsSuccess)
            return existingResult;
        
        var validationResult = await ValidateUpdateCommand(id, command);
        if (!validationResult.IsValid)
        {
            if (validationResult.ServiceError != null)
            {
                return ServiceResult<TDto>.Failure(
                    CreateEmptyDto(),
                    validationResult.ServiceError);
            }
            return ServiceResult<TDto>.Failure(
                CreateEmptyDto(),
                validationResult.Errors);
        }
        
        var existingEntity = await LoadEntityByIdAsync(id);
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        
        var updatedEntity = await UpdateEntityAsync(unitOfWork, existingEntity, command);
        await unitOfWork.CommitAsync();
        
        await InvalidateCacheAsync();
        
        var dto = MapToDto(updatedEntity);
        
        _logger.LogInformation("Updated {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
        return ServiceResult<TDto>.Success(dto);
    }
    
    /// <summary>
    /// Deletes an entity (soft delete)
    /// </summary>
    public virtual async Task<ServiceResult<bool>> DeleteAsync(TId id)
    {
        var existingResult = await ExistsAsync(id);
        var result = existingResult.IsSuccess switch
        {
            false => ConvertToDeleteResult(existingResult),
            true => await ProcessDeleteAsync(id)
        };
        
        return result;
    }
    
    /// <summary>
    /// Converts a failed TDto result to a boolean result for delete operations
    /// </summary>
    private ServiceResult<bool> ConvertToDeleteResult(ServiceResult<TDto> existingResult)
    {
        return existingResult.StructuredErrors.Any() switch
        {
            true => ServiceResult<bool>.Failure(false, existingResult.StructuredErrors.ToArray()),
            false => ServiceResult<bool>.Failure(false, existingResult.Errors)
        };
    }
    
    /// <summary>
    /// Processes the delete operation after entity existence is confirmed
    /// </summary>
    private async Task<ServiceResult<bool>> ProcessDeleteAsync(TId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        
        var deleted = await DeleteEntityAsync(unitOfWork, id);
        await unitOfWork.CommitAsync();
        
        await InvalidateCacheAsync();
        
        _logger.LogInformation("Deleted {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
        
        return ServiceResult<bool>.Success(true);
    }
    
    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    [Obsolete("Use ExistsAsync instead. This method will be removed in the next version. The new ExistsAsync returns ServiceResult<TDto> for consistent error handling.")]
    public virtual async Task<bool> ExistsAsyncBool(TId id)
    {
        var result = await GetByIdAsync(id);
        return result.IsSuccess;
    }
    
    /// <summary>
    /// Checks if an entity exists with the given ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>Success with the entity DTO if it exists, Failure with error details if not</returns>
    public virtual async Task<ServiceResult<TDto>> ExistsAsync(TId id)
    {
        return await GetByIdAsync(id);
    }
    
    // Protected helper methods
    
    protected virtual async Task<ServiceResult<TDto>> LoadEntityAsync(TId id)
    {
        var cacheResult = await TryLoadFromCacheAsync(id);
        return cacheResult.IsHit
            ? ServiceResult<TDto>.Success(cacheResult.Value)
            : await LoadEntityFromDatabaseAsync(id);
    }
    
    private async Task<CacheResult<TDto>> TryLoadFromCacheAsync(TId id)
    {
        var cacheKey = GetCacheKey(id.ToString());
        var cacheService = (ICacheService)_cacheService;
        var cachedDto = await cacheService.GetAsync<TDto>(cacheKey);
        
        if (cachedDto != null)
        {
            _logger.LogDebug("Cache hit for {EntityType}:{Id}", typeof(TEntity).Name, id);
            return CacheResult<TDto>.Hit(cachedDto);
        }
        
        return CacheResult<TDto>.Miss();
    }
    
    protected virtual async Task<ServiceResult<TDto>> LoadEntityFromDatabaseAsync(TId id)
    {
        var entity = await LoadEntityByIdAsync(id);
        
        return IsEntityValidForReturn(entity) switch
        {
            false => ServiceResult<TDto>.Failure(CreateEmptyDto(), ServiceError.NotFound(typeof(TEntity).Name)),
            true => await MapAndCacheEntityAsync(entity, id)
        };
    }
    
    protected virtual bool IsEntityValidForReturn(TEntity entity)
    {
        // Check if entity is empty
        var entityType = entity.GetType();
        var isEmptyProperty = entityType.GetProperty("IsEmpty");
        if (isEmptyProperty?.GetValue(entity) is bool isEmpty && isEmpty)
            return false;
            
        return entity.IsActive;
    }
    
    protected virtual async Task<ServiceResult<TDto>> MapAndCacheEntityAsync(TEntity entity, TId id)
    {
        var dto = MapToDto(entity);
        
        var cacheKey = GetCacheKey(id.ToString());
        var cacheService = (ICacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        
        return ServiceResult<TDto>.Success(dto);
    }
    
    protected virtual async Task InvalidateCacheAsync()
    {
        var cachePattern = GetCacheKeyPattern();
        var cacheService = (ICacheService)_cacheService;
        await cacheService.RemoveByPatternAsync(cachePattern);
        _logger.LogDebug("Invalidated cache for pattern: {Pattern}", cachePattern);
    }
    
    protected virtual string GetCacheKey(string suffix) =>
        $"{typeof(TEntity).Name}:{suffix}";
        
    protected virtual string GetCacheKeyPattern() =>
        $"{typeof(TEntity).Name}:*";
    
    // Abstract methods that must be implemented by derived classes
    
    /// <summary>
    /// Loads all active entities from the repository
    /// </summary>
    protected abstract Task<IEnumerable<TEntity>> LoadAllEntitiesAsync();
    
    /// <summary>
    /// Loads an entity by its strongly-typed ID
    /// </summary>
    protected abstract Task<TEntity> LoadEntityByIdAsync(TId id);
    
    /// <summary>
    /// Maps an entity to its DTO representation
    /// </summary>
    protected abstract TDto MapToDto(TEntity entity);
    
    /// <summary>
    /// Creates an empty DTO instance
    /// </summary>
    protected abstract TDto CreateEmptyDto();
    
    /// <summary>
    /// Gets the expected ID format for error messages (e.g., "equipment-{guid}")
    /// </summary>
    protected abstract string GetExpectedIdFormat();
    
    /// <summary>
    /// Validates the create command
    /// </summary>
    protected abstract Task<ValidationResult> ValidateCreateCommand(TCreateCommand command);
    
    /// <summary>
    /// Validates the update command
    /// </summary>
    protected abstract Task<ValidationResult> ValidateUpdateCommand(TId id, TUpdateCommand command);
    
    /// <summary>
    /// Creates a new entity in the repository
    /// </summary>
    protected abstract Task<TEntity> CreateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, TCreateCommand command);
    
    /// <summary>
    /// Updates an existing entity in the repository
    /// </summary>
    protected abstract Task<TEntity> UpdateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, TEntity existingEntity, TUpdateCommand command);
    
    /// <summary>
    /// Deletes (soft delete) an entity in the repository
    /// </summary>
    protected abstract Task<bool> DeleteEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, TId id);
    
    // Helper record for cache results
    protected record CacheResult<T>
    {
        public bool IsHit { get; init; }
        public T? Value { get; init; }
        
        public static CacheResult<T> Hit(T value) => new() { IsHit = true, Value = value };
        public static CacheResult<T> Miss() => new() { IsHit = false, Value = default };
    }
}