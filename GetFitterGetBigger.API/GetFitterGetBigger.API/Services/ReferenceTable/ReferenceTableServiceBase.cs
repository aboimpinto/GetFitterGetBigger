using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTable;

/// <summary>
/// Base service implementation for reference table operations
/// </summary>
public abstract class ReferenceTableServiceBase<T> : IReferenceTableService<T> where T : class
{
    protected readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    protected readonly ICacheService _cacheService;
    protected readonly ILogger _logger;
    
    protected ReferenceTableServiceBase(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _cacheService = cacheService;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets the cache key prefix for this entity type
    /// </summary>
    protected abstract string CacheKeyPrefix { get; }
    
    /// <summary>
    /// Gets the cache duration for this entity type
    /// </summary>
    protected abstract TimeSpan CacheDuration { get; }
    
    /// <summary>
    /// Gets all entities
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var cacheKey = $"{CacheKeyPrefix}:all";
        _logger.LogDebug("[Cache] Attempting to retrieve all {EntityType} entities with key: {CacheKey}", typeof(T).Name, cacheKey);
        
        var cached = await _cacheService.GetAsync<IEnumerable<T>>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("[Cache HIT] Successfully retrieved {Count} {EntityType} entities from cache with key: {CacheKey}", 
                cached.Count(), typeof(T).Name, cacheKey);
            return cached;
        }
        
        _logger.LogInformation("[Cache MISS] No cached data found for key: {CacheKey}. Fetching from database...", cacheKey);
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var entities = await GetAllEntitiesAsync(unitOfWork);
        
        var entityList = entities.ToList();
        _logger.LogDebug("[Cache] Fetched {Count} {EntityType} entities from database", entityList.Count, typeof(T).Name);
        
        await _cacheService.SetAsync(cacheKey, entityList, CacheDuration);
        _logger.LogInformation("[Cache SET] Cached {Count} {EntityType} entities with key: {CacheKey} for duration: {Duration}", 
            entityList.Count, typeof(T).Name, cacheKey, CacheDuration);
        
        return entityList;
    }
    
    /// <summary>
    /// Gets an entity by ID
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(string id)
    {
        var cacheKey = $"{CacheKeyPrefix}:id:{id}";
        _logger.LogDebug("[Cache] Attempting to retrieve {EntityType} by ID with key: {CacheKey}", typeof(T).Name, cacheKey);
        
        var cached = await _cacheService.GetAsync<T>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("[Cache HIT] Successfully retrieved {EntityType} from cache with key: {CacheKey}", typeof(T).Name, cacheKey);
            return cached;
        }
        
        _logger.LogInformation("[Cache MISS] No cached data found for key: {CacheKey}. Fetching from database...", cacheKey);
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var entity = await GetEntityByIdAsync(unitOfWork, id);
        
        if (entity != null)
        {
            await _cacheService.SetAsync(cacheKey, entity, CacheDuration);
            _logger.LogInformation("[Cache SET] Cached {EntityType} with ID: {Id} using key: {CacheKey} for duration: {Duration}", 
                typeof(T).Name, id, cacheKey, CacheDuration);
        }
        else
        {
            _logger.LogDebug("[Cache] No {EntityType} found with ID: {Id}, nothing to cache", typeof(T).Name, id);
        }
        
        return entity;
    }
    
    /// <summary>
    /// Gets an entity by name
    /// </summary>
    public virtual async Task<T?> GetByNameAsync(string name)
    {
        var cacheKey = $"{CacheKeyPrefix}:name:{name}";
        _logger.LogDebug("[Cache] Attempting to retrieve {EntityType} by name with key: {CacheKey}", typeof(T).Name, cacheKey);
        
        var cached = await _cacheService.GetAsync<T>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("[Cache HIT] Successfully retrieved {EntityType} from cache with key: {CacheKey}", typeof(T).Name, cacheKey);
            return cached;
        }
        
        _logger.LogInformation("[Cache MISS] No cached data found for key: {CacheKey}. Fetching from database...", cacheKey);
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var entity = await GetEntityByNameAsync(unitOfWork, name);
        
        if (entity != null)
        {
            await _cacheService.SetAsync(cacheKey, entity, CacheDuration);
            _logger.LogInformation("[Cache SET] Cached {EntityType} with name: '{Name}' using key: {CacheKey} for duration: {Duration}", 
                typeof(T).Name, name, cacheKey, CacheDuration);
        }
        else
        {
            _logger.LogDebug("[Cache] No {EntityType} found with name: '{Name}', nothing to cache", typeof(T).Name, name);
        }
        
        return entity;
    }
    
    /// <summary>
    /// Gets an entity by value
    /// </summary>
    public virtual async Task<T?> GetByValueAsync(string value)
    {
        var cacheKey = $"{CacheKeyPrefix}:value:{value}";
        _logger.LogDebug("[Cache] Attempting to retrieve {EntityType} by value with key: {CacheKey}", typeof(T).Name, cacheKey);
        
        var cached = await _cacheService.GetAsync<T>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("[Cache HIT] Successfully retrieved {EntityType} from cache with key: {CacheKey}", typeof(T).Name, cacheKey);
            return cached;
        }
        
        _logger.LogInformation("[Cache MISS] No cached data found for key: {CacheKey}. Fetching from database...", cacheKey);
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var entity = await GetEntityByValueAsync(unitOfWork, value);
        
        if (entity != null)
        {
            await _cacheService.SetAsync(cacheKey, entity, CacheDuration);
            _logger.LogInformation("[Cache SET] Cached {EntityType} with value: '{Value}' using key: {CacheKey} for duration: {Duration}", 
                typeof(T).Name, value, cacheKey, CacheDuration);
        }
        else
        {
            _logger.LogDebug("[Cache] No {EntityType} found with value: '{Value}', nothing to cache", typeof(T).Name, value);
        }
        
        return entity;
    }
    
    /// <summary>
    /// Creates a new entity
    /// </summary>
    public virtual async Task<T> CreateAsync(object createDto)
    {
        _logger.LogInformation("[Cache] Creating new {EntityType}. Cache invalidation will follow after successful creation.", typeof(T).Name);
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        
        // Validate before creation
        await ValidateCreateAsync(unitOfWork, createDto);
        
        var entity = await CreateEntityAsync(unitOfWork, createDto);
        
        await unitOfWork.CommitAsync();
        _logger.LogDebug("[Cache] {EntityType} created successfully. Proceeding with cache invalidation...", typeof(T).Name);
        
        // Invalidate cache
        await InvalidateCacheAsync();
        
        return entity;
    }
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    public virtual async Task<T> UpdateAsync(string id, object updateDto)
    {
        _logger.LogInformation("[Cache] Updating {EntityType} with ID: {Id}. Cache invalidation will follow after successful update.", 
            typeof(T).Name, id);
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        
        // Check if entity exists - we'll use a method that works with writable unit of work
        var exists = await CheckEntityExistsAsync(unitOfWork, id);
        if (!exists)
        {
            throw new InvalidOperationException($"Entity with ID '{id}' not found");
        }
        
        // Validate before update
        await ValidateUpdateAsync(unitOfWork, id, updateDto);
        
        var updatedEntity = await UpdateEntityAsync(unitOfWork, id, updateDto);
        
        await unitOfWork.CommitAsync();
        _logger.LogDebug("[Cache] {EntityType} with ID: {Id} updated successfully. Proceeding with cache invalidation...", 
            typeof(T).Name, id);
        
        // Invalidate cache
        await InvalidateCacheAsync();
        
        return updatedEntity;
    }
    
    /// <summary>
    /// Deletes an entity
    /// </summary>
    public virtual async Task DeleteAsync(string id)
    {
        _logger.LogInformation("[Cache] Deleting {EntityType} with ID: {Id}. Cache invalidation will follow after successful deletion.", 
            typeof(T).Name, id);
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        
        // Check if entity exists
        var exists = await CheckEntityExistsAsync(unitOfWork, id);
        if (!exists)
        {
            throw new InvalidOperationException($"Entity with ID '{id}' not found");
        }
        
        // Validate before deletion
        await ValidateDeleteAsync(unitOfWork, id);
        
        await DeleteEntityAsync(unitOfWork, id);
        
        await unitOfWork.CommitAsync();
        _logger.LogDebug("[Cache] {EntityType} with ID: {Id} deleted successfully. Proceeding with cache invalidation...", 
            typeof(T).Name, id);
        
        // Invalidate cache
        await InvalidateCacheAsync();
    }
    
    /// <summary>
    /// Gets all entities from the repository
    /// </summary>
    protected abstract Task<IEnumerable<T>> GetAllEntitiesAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork);
    
    /// <summary>
    /// Gets an entity by ID from the repository
    /// </summary>
    protected abstract Task<T?> GetEntityByIdAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string id);
    
    /// <summary>
    /// Gets an entity by name from the repository
    /// </summary>
    protected abstract Task<T?> GetEntityByNameAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string name);
    
    /// <summary>
    /// Gets an entity by value from the repository
    /// </summary>
    protected abstract Task<T?> GetEntityByValueAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string value);
    
    /// <summary>
    /// Creates a new entity in the repository
    /// </summary>
    protected abstract Task<T> CreateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, object createDto);
    
    /// <summary>
    /// Updates an entity in the repository
    /// </summary>
    protected abstract Task<T> UpdateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id, object updateDto);
    
    /// <summary>
    /// Deletes an entity from the repository
    /// </summary>
    protected abstract Task DeleteEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id);
    
    /// <summary>
    /// Checks if entity exists (for use with writable unit of work)
    /// </summary>
    protected abstract Task<bool> CheckEntityExistsAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id);
    
    /// <summary>
    /// Validates the create operation
    /// </summary>
    protected virtual Task ValidateCreateAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, object createDto)
    {
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Validates the update operation
    /// </summary>
    protected virtual Task ValidateUpdateAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id, object updateDto)
    {
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Validates the delete operation
    /// </summary>
    protected virtual Task ValidateDeleteAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id)
    {
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Invalidates all cache entries for this entity type
    /// </summary>
    protected virtual async Task InvalidateCacheAsync()
    {
        var pattern = $"{CacheKeyPrefix}:*";
        _logger.LogInformation("[Cache INVALIDATION] Starting cache invalidation for pattern: {Pattern}", pattern);
        
        await _cacheService.RemoveByPatternAsync(pattern);
        
        _logger.LogInformation("[Cache INVALIDATION] Successfully invalidated all cache entries for {EntityType} with pattern: {Pattern}", 
            typeof(T).Name, pattern);
    }
}