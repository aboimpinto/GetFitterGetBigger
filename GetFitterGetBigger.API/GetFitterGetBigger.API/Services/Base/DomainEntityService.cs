using GetFitterGetBigger.API.DTOs.Interfaces;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Base;

/// <summary>
/// ARCHITECTURAL NOTE: This class represents the third tier of our three-tier entity architecture.
/// 
/// While PureReferenceService and EnhancedReferenceService work well as generic base classes,
/// domain entities have such unique and complex business logic that they cannot be effectively
/// abstracted into a generic pattern. Each domain entity (Exercise, WorkoutTemplate, etc.) 
/// requires its own custom implementation.
/// 
/// This class exists as:
/// 1. A reference implementation showing the pattern for domain entity services
/// 2. Documentation of the third tier in our architecture
/// 3. A template that could be copied (not inherited) when creating new domain services
/// 
/// In practice, domain entity services like ExerciseService implement their service
/// interfaces directly without inheriting from this base class.
/// </summary>
/// <typeparam name="TEntity">The domain entity type</typeparam>
/// <typeparam name="TDto">The DTO type returned by the service</typeparam>
/// <typeparam name="TCreateCommand">The command type for creating entities</typeparam>
/// <typeparam name="TUpdateCommand">The command type for updating entities</typeparam>
public abstract class DomainEntityService<TEntity, TDto, TCreateCommand, TUpdateCommand> : EntityServiceBase<TEntity>
    where TEntity : class, IDomainEntity
    where TDto : class, IEmptyDto<TDto>, new()
{
    protected readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    
    /// <summary>
    /// Initializes a new instance of the DomainEntityService class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="logger">The logger</param>
    protected DomainEntityService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger logger)
        : base(cacheService, logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider ?? throw new ArgumentNullException(nameof(unitOfWorkProvider));
    }
    
    /// <summary>
    /// Gets all entities (with optional filtering)
    /// Note: Domain entities typically don't return ALL records due to volume
    /// </summary>
    /// <returns>A service result containing filtered entities as DTOs</returns>
    public virtual async Task<ServiceResult<IEnumerable<TDto>>> GetAllAsync()
    {
        try
        {
            // For domain entities, we typically don't cache the full list
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var entities = await LoadAllEntitiesAsync(unitOfWork);
            
            // Map to DTOs
            var dtos = entities.Select(MapToDto).ToList();
            
            _logger.LogInformation("Loaded {Count} {EntityType} entities", dtos.Count, typeof(TEntity).Name);
            return ServiceResult<IEnumerable<TDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading {EntityType} entities", typeof(TEntity).Name);
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
                    TDto.Empty,
                    parseResult.Errors);
            }
            
            // For domain entities, we might use short-lived caching
            TDto? dto = null;
            
            // Check if caching is enabled for this entity type
            var cacheDuration = GetCacheDuration();
            if (cacheDuration.HasValue)
            {
                var cacheKey = GetCacheKey(id);
                var cacheService = (ICacheService)_cacheService;
                dto = await cacheService.GetAsync<TDto>(cacheKey);
                
                if (dto != null)
                {
                    _logger.LogDebug("Cache hit for {EntityType}:{Id}", typeof(TEntity).Name, id);
                    return ServiceResult<TDto>.Success(dto);
                }
            }
            
            // Load from database
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var entity = await LoadEntityByIdAsync(unitOfWork, id);
            
            if (entity == null || !entity.IsActive)
            {
                return ServiceResult<TDto>.Failure(
                    TDto.Empty,
                    $"{typeof(TEntity).Name} not found");
            }
            
            // Map to DTO
            dto = MapToDto(entity);
            
            // Cache if applicable
            if (cacheDuration.HasValue)
            {
                var cacheService = (ICacheService)_cacheService;
                await cacheService.SetAsync(GetCacheKey(id), dto);
            }
            
            return ServiceResult<TDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
            return ServiceResult<TDto>.Failure(
                TDto.Empty,
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
            // Validate command using read-only UoW
            ValidationResult validationResult;
            using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
            {
                validationResult = await ValidateCreateCommand(command, readOnlyUow);
            }
            
            if (!validationResult.IsValid)
            {
                return ServiceResult<TDto>.Failure(
                    TDto.Empty,
                    validationResult.Errors);
            }
            
            // Create entity using writable UoW
            using var writableUow = _unitOfWorkProvider.CreateWritable();
            
            var entity = await CreateEntityAsync(writableUow, command);
            await writableUow.CommitAsync();
            
            // Clear any related caches if needed
            await InvalidateRelatedCachesAsync(entity);
            
            // Map to DTO
            var dto = MapToDto(entity);
            
            _logger.LogInformation("Created new {EntityType} with ID: {Id}", typeof(TEntity).Name, entity.Id);
            return ServiceResult<TDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {EntityType}", typeof(TEntity).Name);
            return ServiceResult<TDto>.Failure(
                TDto.Empty,
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
                    TDto.Empty,
                    parseResult.Errors);
            }
            
            // Validate command using read-only UoW
            ValidationResult validationResult;
            using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
            {
                validationResult = await ValidateUpdateCommand(id, command, readOnlyUow);
            }
            
            if (!validationResult.IsValid)
            {
                return ServiceResult<TDto>.Failure(
                    TDto.Empty,
                    validationResult.Errors);
            }
            
            // Update entity using writable UoW
            using var writableUow = _unitOfWorkProvider.CreateWritable();
            
            var existingEntity = await LoadEntityByIdAsync(writableUow, id);
            if (existingEntity == null)
            {
                return ServiceResult<TDto>.Failure(
                    TDto.Empty,
                    $"{typeof(TEntity).Name} not found");
            }
            
            var updatedEntity = await UpdateEntityAsync(writableUow, existingEntity, command);
            await writableUow.CommitAsync();
            
            // Clear any related caches if needed
            await InvalidateRelatedCachesAsync(updatedEntity);
            
            // Map to DTO
            var dto = MapToDto(updatedEntity);
            
            _logger.LogInformation("Updated {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
            return ServiceResult<TDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
            return ServiceResult<TDto>.Failure(
                TDto.Empty,
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
            
            // Check if entity exists and can be deleted
            var entity = await LoadEntityByIdAsync(unitOfWork, id);
            if (entity == null)
            {
                return ServiceResult<bool>.Failure(false, $"{typeof(TEntity).Name} not found");
            }
            
            // Check if deletion is allowed
            var canDelete = await CanDeleteEntityAsync(unitOfWork, entity);
            if (!canDelete.IsValid)
            {
                return ServiceResult<bool>.Failure(false, canDelete.Errors);
            }
            
            // Perform delete
            var deleted = await DeleteEntityAsync(unitOfWork, id);
            if (!deleted)
            {
                return ServiceResult<bool>.Failure(false, $"Failed to delete {typeof(TEntity).Name}");
            }
            
            await unitOfWork.CommitAsync();
            
            // Clear any related caches if needed
            await InvalidateRelatedCachesAsync(entity);
            
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
    /// Invalidates caches related to the given entity
    /// Override this to invalidate specific related caches
    /// </summary>
    /// <param name="entity">The entity that was modified</param>
    protected virtual async Task InvalidateRelatedCachesAsync(TEntity entity)
    {
        // Remove the specific entity from cache
        var cacheService = (ICacheService)_cacheService;
        await cacheService.RemoveAsync(GetCacheKey(entity.Id.ToString()));
        
        // Derived classes can override to invalidate additional caches
    }
    
    /// <summary>
    /// Checks if an entity can be deleted
    /// Override this to add business rules for deletion
    /// </summary>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="entity">The entity to check</param>
    /// <returns>A validation result</returns>
    protected virtual Task<ValidationResult> CanDeleteEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, TEntity entity)
    {
        // Default implementation allows all deletions
        return Task.FromResult(ValidationResult.Success());
    }
    
    // Abstract methods that must be implemented by derived classes
    
    protected abstract Task<IEnumerable<TEntity>> LoadAllEntitiesAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork);
    protected abstract Task<TEntity?> LoadEntityByIdAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string id);
    protected abstract Task<TEntity?> LoadEntityByIdAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id);
    protected abstract TDto MapToDto(TEntity entity);
    protected abstract ValidationResult ValidateAndParseId(string id);
    protected abstract Task<ValidationResult> ValidateCreateCommand(TCreateCommand command, IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork);
    protected abstract Task<ValidationResult> ValidateUpdateCommand(string id, TUpdateCommand command, IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork);
    protected abstract Task<TEntity> CreateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, TCreateCommand command);
    protected abstract Task<TEntity> UpdateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, TEntity existingEntity, TUpdateCommand command);
    protected abstract Task<bool> DeleteEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id);
}