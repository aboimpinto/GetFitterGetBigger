using GetFitterGetBigger.API.DTOs.Interfaces;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Base;

/// <summary>
/// Base service for pure reference data that never changes after deployment
/// Provides read-only operations with eternal caching
/// Works with DTOs only - entities stay within service implementations
/// </summary>
/// <typeparam name="TEntity">The pure reference entity type (for compatibility - will be removed)</typeparam>
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
    /// Gets all active entities as DTOs
    /// Orchestrates cache checking and loading with automatic caching on miss
    /// </summary>
    /// <returns>A service result containing all active entities as DTOs</returns>
    public virtual async Task<ServiceResult<IEnumerable<TDto>>> GetAllAsync()
    {
        var cacheKey = GetAllCacheKey();
        var cacheService = (IEternalCacheService)_cacheService;
        
        return await CacheLoad.For<IEnumerable<TDto>>(cacheService, cacheKey)
            .WithLogging(_logger, typeof(TEntity).Name)
            .WithAutoCacheAsync(LoadAllDtosAsync);
    }
    
    /// <summary>
    /// Gets a DTO by its ID
    /// Orchestrates validation, cache checking and loading with automatic caching on miss
    /// </summary>
    /// <param name="id">The entity ID in string format</param>
    /// <returns>A service result containing the entity as a DTO</returns>
    public virtual async Task<ServiceResult<TDto>> GetByIdAsync(string id)
    {
        return await ServiceValidate.For<TDto>()
            .ValidateWith(() => ValidateAndParseId(id))
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = GetCacheKey(id);
                    var cacheService = (IEternalCacheService)_cacheService;
                    
                    return await CacheLoad.For<TDto>(cacheService, cacheKey)
                        .WithLogging(_logger, typeof(TEntity).Name)
                        .WithAutoCacheAsync(() => LoadDtoByIdAsync(id));
                }
            );
    }
    
    // ==================== NEW DTO-ONLY METHODS ====================
    
    /// <summary>
    /// Loads all DTOs from the underlying service
    /// This method only loads data - caching is handled by GetAllAsync
    /// Entities stay within service implementations
    /// </summary>
    /// <returns>A ServiceResult containing all DTOs</returns>
    protected virtual async Task<ServiceResult<IEnumerable<TDto>>> LoadAllDtosAsync()
    {
        // Default implementation for backward compatibility
        // New services should override this method
        try
        {
#pragma warning disable CS0618 // Type or member is obsolete - intentional for backward compatibility
            var entities = await LoadAllEntitiesAsync();
            var dtos = entities.Select(MapToDto).ToList();
#pragma warning restore CS0618 // Type or member is obsolete
            return ServiceResult<IEnumerable<TDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading all {EntityType} DTOs", typeof(TEntity).Name);
            return ServiceResult<IEnumerable<TDto>>.Failure(
                Enumerable.Empty<TDto>(),
                ServiceError.InternalError($"Failed to load {typeof(TEntity).Name} data"));
        }
    }
    
    /// <summary>
    /// Loads a single DTO by ID from the underlying service
    /// This method only loads data - caching is handled by GetByIdAsync
    /// Entities stay within service implementations
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>A ServiceResult containing the DTO</returns>
    protected virtual async Task<ServiceResult<TDto>> LoadDtoByIdAsync(string id)
    {
        // Default implementation for backward compatibility
        // New services should override this method
        try
        {
#pragma warning disable CS0618 // Type or member is obsolete - intentional for backward compatibility
            var entityResult = await LoadEntityByIdAsync(id);
#pragma warning restore CS0618 // Type or member is obsolete
            
            if (!entityResult.IsSuccess)
            {
                return ServiceResult<TDto>.Failure(
                    TDto.Empty,
                    entityResult.StructuredErrors.ToArray());
            }
            
            if (!entityResult.Data.IsActive)
            {
                return ServiceResult<TDto>.Failure(
                    TDto.Empty,
                    ServiceError.NotFound(typeof(TEntity).Name));
            }
            
#pragma warning disable CS0618 // Type or member is obsolete - intentional for backward compatibility
            var dto = MapToDto(entityResult.Data);
#pragma warning restore CS0618 // Type or member is obsolete
            return ServiceResult<TDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading {EntityType} DTO with ID: {Id}", typeof(TEntity).Name, id);
            return ServiceResult<TDto>.Failure(
                TDto.Empty,
                ServiceError.InternalError($"Failed to load {typeof(TEntity).Name}"));
        }
    }
    
    // ==================== OBSOLETE ENTITY-BASED METHODS ====================
    
    /// <summary>
    /// Loads all entities from the database
    /// OBSOLETE: Use LoadAllDtosAsync instead - entities should not leave the service layer
    /// </summary>
    [Obsolete("Use LoadAllDtosAsync instead. Entities should not leave the service layer.", false)]
    protected virtual Task<IEnumerable<TEntity>> LoadAllEntitiesAsync()
    {
        // Default empty implementation for new services
        return Task.FromResult(Enumerable.Empty<TEntity>());
    }
    
    /// <summary>
    /// Loads a single entity by ID from the database
    /// OBSOLETE: Use LoadDtoByIdAsync instead - entities should not leave the service layer
    /// </summary>
    [Obsolete("Use LoadDtoByIdAsync instead. Entities should not leave the service layer.", false)]
    protected virtual Task<ServiceResult<TEntity>> LoadEntityByIdAsync(string id)
    {
        // Default empty implementation for new services
        // Return a dummy entity to satisfy the interface
        // This will be removed once all services are migrated
        return Task.FromResult(ServiceResult<TEntity>.Failure(
            default!,
            ServiceError.NotFound(typeof(TEntity).Name)));
    }
    
    /// <summary>
    /// Maps an entity to its DTO representation
    /// OBSOLETE: DTOs should be loaded directly from the service layer
    /// </summary>
    [Obsolete("DTOs should be loaded directly from the service layer, not mapped from entities.", false)]
    protected virtual TDto MapToDto(TEntity entity)
    {
        // Default empty implementation for new services
        return TDto.Empty;
    }
    
    /// <summary>
    /// Validates and parses the entity ID
    /// Must be implemented by derived classes
    /// </summary>
    /// <param name="id">The ID string to validate</param>
    /// <returns>A validation result</returns>
    protected abstract ValidationResult ValidateAndParseId(string id);
}