using GetFitterGetBigger.API.Models;
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
        try
        {
            var cacheKey = GetAllCacheKey();
            var cached = await _cacheService.GetAsync<IEnumerable<TDto>>(cacheKey);
            
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for {EntityType}:all", typeof(TEntity).Name);
                return ServiceResult<IEnumerable<TDto>>.Success(cached);
            }
            
            // Load from database
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var entities = await LoadAllEntitiesAsync(unitOfWork);
            
            // Map to DTOs
            var dtos = entities.Select(MapToDto).ToList();
            
            // Cache with moderate TTL
            var cacheDuration = GetCacheDuration() ?? TimeSpan.FromHours(1);
            await _cacheService.SetAsync(cacheKey, dtos, cacheDuration);
            
            _logger.LogInformation("Loaded and cached {Count} {EntityType} entities", dtos.Count, typeof(TEntity).Name);
            return ServiceResult<IEnumerable<TDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading all {EntityType} entities", typeof(TEntity).Name);
            return ServiceResult<IEnumerable<TDto>>.Failure(
                Enumerable.Empty<TDto>(),
                $"Failed to load {typeof(TEntity).Name} data");
        }
    }
    
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The entity ID in string format</param>
    /// <returns>A service result containing the entity as a DTO</returns>
    public virtual async Task<ServiceResult<TDto>> GetByIdAsync(string id)
    {
        try
        {
            // Validate and parse ID
            var parseResult = ValidateAndParseId(id);
            if (!parseResult.IsValid)
            {
                return ServiceResult<TDto>.Failure(
                    CreateEmptyDto(),
                    parseResult.Errors);
            }
            
            var cacheKey = GetCacheKey(id);
            var cached = await _cacheService.GetAsync<TDto>(cacheKey);
            
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for {EntityType}:{Id}", typeof(TEntity).Name, id);
                return ServiceResult<TDto>.Success(cached);
            }
            
            // Load from database
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var entity = await LoadEntityByIdAsync(unitOfWork, id);
            
            if (entity == null || !entity.IsActive)
            {
                return ServiceResult<TDto>.Failure(
                    CreateEmptyDto(),
                    $"{typeof(TEntity).Name} not found");
            }
            
            // Map to DTO
            var dto = MapToDto(entity);
            
            // Cache with moderate TTL
            var cacheDuration = GetCacheDuration() ?? TimeSpan.FromHours(1);
            await _cacheService.SetAsync(cacheKey, dto, cacheDuration);
            
            return ServiceResult<TDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
            return ServiceResult<TDto>.Failure(
                CreateEmptyDto(),
                $"Failed to load {typeof(TEntity).Name}");
        }
    }
    
    /// <summary>
    /// Creates a new entity
    /// </summary>
    /// <param name="command">The creation command</param>
    /// <returns>A service result containing the created entity as a DTO</returns>
    public virtual async Task<ServiceResult<TDto>> CreateAsync(TCreateCommand command)
    {
        try
        {
            // Validate command
            var validationResult = await ValidateCreateCommand(command);
            if (!validationResult.IsValid)
            {
                return ServiceResult<TDto>.Failure(
                    CreateEmptyDto(),
                    validationResult.Errors);
            }
            
            using var unitOfWork = _unitOfWorkProvider.CreateWritable();
            
            // Create entity
            var entity = await CreateEntityAsync(unitOfWork, command);
            await unitOfWork.CommitAsync();
            
            // Invalidate caches
            await InvalidateAllCachesAsync();
            
            // Map to DTO
            var dto = MapToDto(entity);
            
            _logger.LogInformation("Created new {EntityType} with ID: {Id}", typeof(TEntity).Name, entity.Id);
            return ServiceResult<TDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {EntityType}", typeof(TEntity).Name);
            return ServiceResult<TDto>.Failure(
                CreateEmptyDto(),
                $"Failed to create {typeof(TEntity).Name}");
        }
    }
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="command">The update command</param>
    /// <returns>A service result containing the updated entity as a DTO</returns>
    public virtual async Task<ServiceResult<TDto>> UpdateAsync(string id, TUpdateCommand command)
    {
        try
        {
            // Validate ID
            var parseResult = ValidateAndParseId(id);
            if (!parseResult.IsValid)
            {
                return ServiceResult<TDto>.Failure(
                    CreateEmptyDto(),
                    parseResult.Errors);
            }
            
            // Validate command
            var validationResult = await ValidateUpdateCommand(id, command);
            if (!validationResult.IsValid)
            {
                return ServiceResult<TDto>.Failure(
                    CreateEmptyDto(),
                    validationResult.Errors);
            }
            
            using var unitOfWork = _unitOfWorkProvider.CreateWritable();
            
            // Load existing entity
            var existingEntity = await LoadEntityByIdAsync(unitOfWork, id);
            if (existingEntity == null)
            {
                return ServiceResult<TDto>.Failure(
                    CreateEmptyDto(),
                    $"{typeof(TEntity).Name} not found");
            }
            
            // Update entity
            var updatedEntity = await UpdateEntityAsync(unitOfWork, existingEntity, command);
            await unitOfWork.CommitAsync();
            
            // Invalidate caches
            await InvalidateAllCachesAsync();
            
            // Map to DTO
            var dto = MapToDto(updatedEntity);
            
            _logger.LogInformation("Updated {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
            return ServiceResult<TDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
            return ServiceResult<TDto>.Failure(
                CreateEmptyDto(),
                $"Failed to update {typeof(TEntity).Name}");
        }
    }
    
    /// <summary>
    /// Deletes an entity (soft delete - sets IsActive to false)
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>A service result indicating success or failure</returns>
    public virtual async Task<ServiceResult<bool>> DeleteAsync(string id)
    {
        try
        {
            // Validate ID
            var parseResult = ValidateAndParseId(id);
            if (!parseResult.IsValid)
            {
                return ServiceResult<bool>.Failure(false, parseResult.Errors);
            }
            
            using var unitOfWork = _unitOfWorkProvider.CreateWritable();
            
            // Perform delete
            var deleted = await DeleteEntityAsync(unitOfWork, id);
            if (!deleted)
            {
                return ServiceResult<bool>.Failure(false, $"{typeof(TEntity).Name} not found");
            }
            
            await unitOfWork.CommitAsync();
            
            // Invalidate caches
            await InvalidateAllCachesAsync();
            
            _logger.LogInformation("Deleted {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
            return ServiceResult<bool>.Failure(false, $"Failed to delete {typeof(TEntity).Name}");
        }
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
    
    // Abstract methods that must be implemented by derived classes
    
    protected abstract Task<IEnumerable<TEntity>> LoadAllEntitiesAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork);
    protected abstract Task<TEntity?> LoadEntityByIdAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string id);
    protected abstract Task<TEntity?> LoadEntityByIdAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id);
    protected abstract TDto MapToDto(TEntity entity);
    protected abstract TDto CreateEmptyDto();
    protected abstract ValidationResult ValidateAndParseId(string id);
    protected abstract Task<ValidationResult> ValidateCreateCommand(TCreateCommand command);
    protected abstract Task<ValidationResult> ValidateUpdateCommand(string id, TUpdateCommand command);
    protected abstract Task<TEntity> CreateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, TCreateCommand command);
    protected abstract Task<TEntity> UpdateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, TEntity existingEntity, TUpdateCommand command);
    protected abstract Task<bool> DeleteEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id);
}