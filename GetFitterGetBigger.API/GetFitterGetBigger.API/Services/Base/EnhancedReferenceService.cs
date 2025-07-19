using System.Reflection;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Base;

/// <summary>
/// Base service for enhanced reference data that can be modified by administrators
/// Provides read and write operations with cache invalidation
/// </summary>
/// <typeparam name="TEntity">The enhanced reference entity type</typeparam>
/// <typeparam name="TDto">The DTO type returned by the service</typeparam>
/// <typeparam name="TCreateCommand">The command type for creating entities</typeparam>
/// <typeparam name="TUpdateCommand">The command type for updating entities</typeparam>
public abstract class EnhancedReferenceService<TEntity, TDto, TCreateCommand, TUpdateCommand> : EntityServiceBase<TEntity>
    where TEntity : class, IEnhancedReference
    where TDto : class
{
    protected readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    
    /// <summary>
    /// Initializes a new instance of the EnhancedReferenceService class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="logger">The logger</param>
    protected EnhancedReferenceService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
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
        var cacheService = (ICacheService)_cacheService;
        var cached = await cacheService.GetAsync<IEnumerable<TDto>>(cacheKey);
        
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for {EntityType}:all", typeof(TEntity).Name);
            return ServiceResult<IEnumerable<TDto>>.Success(cached);
        }
        
        // Load from database
        var entities = await LoadAllEntitiesAsync();
        
        // Map to DTOs
        var dtos = entities.Select(MapToDto).ToList();
        
        // Cache with automatic 24-hour expiration for enhanced reference data
        await cacheService.SetAsync(cacheKey, dtos);
        
        _logger.LogInformation("Loaded and cached {Count} {EntityType} entities", dtos.Count, typeof(TEntity).Name);
        return ServiceResult<IEnumerable<TDto>>.Success(dtos);
    }
    
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>A service result containing the entity as a DTO</returns>
    public virtual async Task<ServiceResult<TDto>> GetByIdAsync(ISpecializedIdBase id)
    {
        return id.IsEmpty switch
        {
            true => ServiceResult<TDto>.Failure(CreateEmptyDto(), ServiceError.ValidationFailed("Invalid ID provided")),
            false => await LoadEntityAsync(id)
        };
    }
    
    /// <summary>
    /// Loads an entity, checking cache first
    /// </summary>
    protected virtual async Task<ServiceResult<TDto>> LoadEntityAsync(ISpecializedIdBase id)
    {
        var cacheResult = await TryLoadFromCacheAsync(id);
        return cacheResult.IsHit switch
        {
            true => ServiceResult<TDto>.Success(cacheResult.Value),
            false => await LoadEntityFromDatabaseAsync(id)
        };
    }
    
    /// <summary>
    /// Attempts to load entity from cache
    /// </summary>
    private async Task<CacheResult<TDto>> TryLoadFromCacheAsync(ISpecializedIdBase id)
    {
        var cacheKey = GetCacheKey(id.ToString());
        var cacheService = (ICacheService)_cacheService;
        var cachedDto = await cacheService.GetAsync<TDto>(cacheKey);
        
        return cachedDto switch
        {
            not null => LogCacheHitAndReturn(cachedDto, id),
            null => CacheResult<TDto>.Miss()
        };
    }
    
    private CacheResult<TDto> LogCacheHitAndReturn(TDto cachedDto, ISpecializedIdBase id)
    {
        _logger.LogDebug("Cache hit for {EntityType}:{Id}", typeof(TEntity).Name, id.ToString());
        return CacheResult<TDto>.Hit(cachedDto);
    }
    
    /// <summary>
    /// Loads entity from database and processes it
    /// </summary>
    protected virtual async Task<ServiceResult<TDto>> LoadEntityFromDatabaseAsync(ISpecializedIdBase id)
    {
        var entity = await LoadEntityByIdAsync(id);
        
        return IsEntityValidForReturn(entity) switch
        {
            false => ServiceResult<TDto>.Failure(CreateEmptyDto(), ServiceError.NotFound(typeof(TEntity).Name)),
            true => await MapAndCacheEntityAsync(entity, id)
        };
    }
    
    /// <summary>
    /// Checks if entity is valid for returning to client
    /// </summary>
    protected virtual bool IsEntityValidForReturn(TEntity entity)
    {
        // Check if entity is empty (for entities implementing IEmptyEntity)
        var entityType = entity.GetType();
        var isEmptyProperty = entityType.GetProperty("IsEmpty");
        if (isEmptyProperty?.GetValue(entity) is bool isEmpty && isEmpty)
            return false;
            
        return entity.IsActive;
    }
    
    /// <summary>
    /// Maps entity to DTO and caches it
    /// </summary>
    protected virtual async Task<ServiceResult<TDto>> MapAndCacheEntityAsync(TEntity entity, ISpecializedIdBase id)
    {
        var dto = MapToDto(entity);
        
        // Cache with automatic 24-hour expiration for enhanced reference data
        var cacheKey = GetCacheKey(id.ToString());
        var cacheService = (ICacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        
        return ServiceResult<TDto>.Success(dto);
    }
    
    /// <summary>
    /// Creates a new entity
    /// </summary>
    /// <param name="command">The creation command</param>
    /// <returns>A service result containing the created entity as a DTO</returns>
    public virtual async Task<ServiceResult<TDto>> CreateAsync(TCreateCommand command)
    {
        var validationResult = await ValidateCreateCommand(command);
        
        return validationResult switch
        {
            { IsValid: false, ServiceError: not null } => 
                ServiceResult<TDto>.Failure(CreateEmptyDto(), validationResult.ServiceError),
            { IsValid: false } => 
                ServiceResult<TDto>.Failure(CreateEmptyDto(), validationResult.Errors),
            { IsValid: true } => 
                await CreateEntityAndReturnDtoAsync(command)
        };
    }
    
    private async Task<ServiceResult<TDto>> CreateEntityAndReturnDtoAsync(TCreateCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        
        var entity = await CreateEntityAsync(unitOfWork, command);
        await unitOfWork.CommitAsync();
        
        await InvalidateCacheAsync();
        
        var dto = MapToDto(entity);
        _logger.LogInformation("Created new {EntityType} with ID: {Id}", typeof(TEntity).Name, entity.Id);
        
        return ServiceResult<TDto>.Success(dto);
    }
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="command">The update command</param>
    /// <returns>A service result containing the updated entity as a DTO</returns>
    public virtual async Task<ServiceResult<TDto>> UpdateAsync(ISpecializedIdBase id, TUpdateCommand command)
    {
        var existingResult = await ExistsAsync(id);
        var result = existingResult.IsSuccess switch
        {
            false => existingResult,
            true => await ProcessUpdateAsync(id, command)
        };
        
        return result;
    }
    
    /// <summary>
    /// Processes the update operation after entity existence is confirmed
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="command">The update command</param>
    /// <returns>A service result containing the updated entity as a DTO</returns>
    private async Task<ServiceResult<TDto>> ProcessUpdateAsync(ISpecializedIdBase id, TUpdateCommand command)
    {
        var validationResult = await ValidateUpdateCommand(id, command);
        
        var result = validationResult switch
        {
            { IsValid: false, ServiceError: not null } => 
                ServiceResult<TDto>.Failure(CreateEmptyDto(), validationResult.ServiceError),
            { IsValid: false } => 
                ServiceResult<TDto>.Failure(CreateEmptyDto(), validationResult.Errors),
            { IsValid: true } => 
                await UpdateEntityAndReturnDtoAsync(id, command)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<TDto>> UpdateEntityAndReturnDtoAsync(ISpecializedIdBase id, TUpdateCommand command)
    {
        var existingEntity = await LoadEntityByIdAsync(id);
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var updatedEntity = await UpdateEntityAsync(unitOfWork, existingEntity, command);
        await unitOfWork.CommitAsync();
        
        await InvalidateCacheAsync();
        
        var dto = MapToDto(updatedEntity);
        _logger.LogInformation("Updated {EntityType} with ID: {Id}", typeof(TEntity).Name, id.ToString());
        
        return ServiceResult<TDto>.Success(dto);
    }
    
    /// <summary>
    /// Deletes an entity (soft delete - sets IsActive to false)
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>A service result indicating success or failure</returns>
    public virtual async Task<ServiceResult<bool>> DeleteAsync(ISpecializedIdBase id)
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
    /// <param name="existingResult">The result from GetByIdAsync</param>
    /// <returns>A boolean service result with the same error information</returns>
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
    /// <param name="id">The entity ID</param>
    /// <returns>A service result indicating success or failure</returns>
    private async Task<ServiceResult<bool>> ProcessDeleteAsync(ISpecializedIdBase id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        
        var deleted = await DeleteEntityAsync(unitOfWork, id);
        await unitOfWork.CommitAsync();
        
        await InvalidateCacheAsync();
        
        _logger.LogInformation("Deleted {EntityType} with ID: {Id}", typeof(TEntity).Name, id.ToString());
        
        return ServiceResult<bool>.Success(true);
    }
    
    /// <summary>
    /// Checks if an entity exists with the given ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>True if the entity exists and is active, false otherwise</returns>
    [Obsolete("Use ExistsAsync instead. This method will be removed in the next version. The new ExistsAsync returns ServiceResult<TDto> for consistent error handling.")]
    public virtual async Task<bool> ExistsAsyncBool(ISpecializedIdBase id)
    {
        var result = await GetByIdAsync(id);
        return result.IsSuccess;
    }
    
    /// <summary>
    /// Checks if an entity exists with the given ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>Success with the entity DTO if it exists, Failure with error details if not</returns>
    public virtual async Task<ServiceResult<TDto>> ExistsAsync(ISpecializedIdBase id)
    {
        return await GetByIdAsync(id);
    }
    
    // Abstract methods that must be implemented by derived classes
    
    /// <summary>
    /// Loads all active entities from the repository
    /// </summary>
    /// <returns>Collection of entities (never null, use Empty pattern)</returns>
    protected abstract Task<IEnumerable<TEntity>> LoadAllEntitiesAsync();
    
    /// <summary>
    /// Loads an entity by ID using ReadOnlyUnitOfWork internally
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>The entity or Empty if not found</returns>
    protected abstract Task<TEntity> LoadEntityByIdAsync(ISpecializedIdBase id);
    
    /// <summary>
    /// Maps an entity to its DTO representation
    /// </summary>
    protected abstract TDto MapToDto(TEntity entity);
    
    /// <summary>
    /// Creates an empty DTO instance
    /// </summary>
    protected abstract TDto CreateEmptyDto();
    
    /// <summary>
    /// Validates the create command
    /// </summary>
    protected abstract Task<ValidationResult> ValidateCreateCommand(TCreateCommand command);
    
    /// <summary>
    /// Validates the update command
    /// </summary>
    protected abstract Task<ValidationResult> ValidateUpdateCommand(ISpecializedIdBase id, TUpdateCommand command);
    
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
    protected abstract Task<bool> DeleteEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, ISpecializedIdBase id);
}