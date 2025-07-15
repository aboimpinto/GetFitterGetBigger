using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Base;

/// <summary>
/// TEMPORARY: Base service for pure reference data that supports the Empty pattern
/// This class will be merged with PureReferenceService once all entities are migrated
/// </summary>
/// <typeparam name="TEntity">The pure reference entity type that implements IEmptyEntity</typeparam>
/// <typeparam name="TDto">The DTO type returned by the service</typeparam>
public abstract class EmptyEnabledPureReferenceService<TEntity, TDto> : EntityServiceBase<TEntity>
    where TEntity : class, IPureReference, IEmptyEntity<TEntity>
    where TDto : class
{
    protected readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    
    /// <summary>
    /// Initializes a new instance of the EmptyEnabledPureReferenceService class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="logger">The logger</param>
    protected EmptyEnabledPureReferenceService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEmptyEnabledCacheService cacheService,
        ILogger logger)
        : base(cacheService, logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }
    
    /// <summary>
    /// Gets all active entities
    /// </summary>
    /// <returns>A service result containing all active entities as DTOs</returns>
    public virtual async Task<ServiceResult<IEnumerable<TDto>>> GetAllAsync()
    {
        var cacheKey = GetAllCacheKey();
        var cacheService = (IEmptyEnabledCacheService)_cacheService;
        var cacheResult = await cacheService.GetAsync<IEnumerable<TDto>>(cacheKey);
        
        return cacheResult switch
        {
            { IsHit: true } => LogCacheHitAndReturn(cacheResult.Value),
            _ => await LoadAndCacheAllAsync(cacheKey)
        };
    }
    
    private ServiceResult<IEnumerable<TDto>> LogCacheHitAndReturn(IEnumerable<TDto> cached)
    {
        _logger.LogDebug("Cache hit for {EntityType}:all", typeof(TEntity).Name);
        return ServiceResult<IEnumerable<TDto>>.Success(cached);
    }
    
    private async Task<ServiceResult<IEnumerable<TDto>>> LoadAndCacheAllAsync(string cacheKey)
    {
        var loadResult = await LoadAllFromDatabaseAsync();
        
        return loadResult switch
        {
            { IsSuccess: true } => await CacheAndReturnAllAsync(cacheKey, loadResult.Data),
            _ => loadResult
        };
    }
    
    private async Task<ServiceResult<IEnumerable<TDto>>> LoadAllFromDatabaseAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var loadResult = await SafeExecuteAsync(
            async () => await LoadAllEntitiesAsync(unitOfWork),
            "Error loading all {EntityType} entities from database");
            
        return loadResult switch
        {
            { IsSuccess: true } => ServiceResult<IEnumerable<TDto>>.Success(
                [.. loadResult.Data.Select(MapToDto)]),
            _ => ServiceResult<IEnumerable<TDto>>.Failure(
                [],
                ServiceError.InternalError($"Failed to load {typeof(TEntity).Name} data from database"))
        };
    }
    
    private async Task<(bool IsSuccess, T Data)> SafeExecuteAsync<T>(
        Func<Task<T>> operation,
        string errorMessageTemplate) where T : class
    {
        try
        {
            var result = await operation();
            return (true, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, errorMessageTemplate, typeof(TEntity).Name);
            return (false, null!);
        }
    }
    
    private async Task<ServiceResult<IEnumerable<TDto>>> CacheAndReturnAllAsync(string cacheKey, IEnumerable<TDto> dtos)
    {
        var dtoList = dtos.ToList();
        await _cacheService.SetAsync(cacheKey, dtoList, TimeSpan.FromDays(365));
        
        _logger.LogInformation("Loaded and cached {Count} {EntityType} entities", dtoList.Count, typeof(TEntity).Name);
        return ServiceResult<IEnumerable<TDto>>.Success(dtoList);
    }
    
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The entity ID in string format</param>
    /// <returns>A service result containing the entity as a DTO</returns>
    public virtual async Task<ServiceResult<TDto>> GetByIdAsync(string id)
    {
        return ValidateAndParseId(id) switch
        {
            { IsValid: false } validationResult => CreateValidationFailure(validationResult),
            _ => await LoadByIdWithCachingAsync(id)
        };
    }
    
    private ServiceResult<TDto> CreateValidationFailure(ValidationResult validationResult)
    {
        // Convert multiple validation errors to ServiceErrors
        var errors = validationResult.Errors.Select(e => ServiceError.ValidationFailed(e)).ToArray();
        return ServiceResult<TDto>.Failure(CreateEmptyDto(), errors);
    }
    
    private async Task<ServiceResult<TDto>> LoadByIdWithCachingAsync(string id)
    {
        var cacheKey = GetCacheKey(id);
        var cacheService = (IEmptyEnabledCacheService)_cacheService;
        var cacheResult = await cacheService.GetAsync<TDto>(cacheKey);
        
        return cacheResult switch
        {
            { IsHit: true } => LogCacheHitAndReturn(cacheResult.Value, id),
            _ => await LoadAndCacheByIdAsync(id, cacheKey)
        };
    }
    
    private ServiceResult<TDto> LogCacheHitAndReturn(TDto cached, string id)
    {
        _logger.LogDebug("Cache hit for {EntityType}:{Id}", typeof(TEntity).Name, id);
        return ServiceResult<TDto>.Success(cached);
    }
    
    private async Task<ServiceResult<TDto>> LoadAndCacheByIdAsync(string id, string cacheKey)
    {
        var loadResult = await LoadByIdFromDatabaseAsync(id);
        
        return loadResult switch
        {
            { IsEmpty: true } => CreateNotFoundResult(),
            { IsActive: false } => CreateNotFoundResult(),
            _ => await CacheAndReturnSuccess(cacheKey, MapToDto(loadResult))
        };
    }
    
    private async Task<TEntity> LoadByIdFromDatabaseAsync(string id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var loadResult = await SafeExecuteAsync(
            async () => await LoadEntityByIdAsync(unitOfWork, id),
            $"Error loading {{EntityType}} with ID: {id} from database");
            
        return loadResult.IsSuccess ? loadResult.Data : TEntity.Empty;
    }
    
    private ServiceResult<TDto> CreateNotFoundResult()
    {
        return ServiceResult<TDto>.Failure(CreateEmptyDto(), ServiceError.NotFound(typeof(TEntity).Name));
    }
    
    private async Task<ServiceResult<TDto>> CacheAndReturnSuccess(string cacheKey, TDto dto)
    {
        // Cache forever (until app restart)
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromDays(365));
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
    /// <param name="unitOfWork">The read-only unit of work</param>
    /// <returns>A collection of all active entities</returns>
    protected abstract Task<IEnumerable<TEntity>> LoadAllEntitiesAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork);
    
    /// <summary>
    /// Loads a single entity by ID from the database
    /// Must be implemented by derived classes
    /// NOTE: Returns TEntity.Empty instead of null when not found
    /// </summary>
    /// <param name="unitOfWork">The read-only unit of work</param>
    /// <param name="id">The entity ID</param>
    /// <returns>The entity if found, TEntity.Empty otherwise</returns>
    protected abstract Task<TEntity> LoadEntityByIdAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string id);
    
    /// <summary>
    /// Maps an entity to its DTO representation
    /// Must be implemented by derived classes
    /// </summary>
    /// <param name="entity">The entity to map</param>
    /// <returns>The mapped DTO</returns>
    protected abstract TDto MapToDto(TEntity entity);
    
    /// <summary>
    /// Creates an empty DTO instance for failure cases
    /// Must be implemented by derived classes
    /// </summary>
    /// <returns>An empty DTO instance</returns>
    protected abstract TDto CreateEmptyDto();
    
    /// <summary>
    /// Validates and parses the entity ID
    /// Must be implemented by derived classes
    /// </summary>
    /// <param name="id">The ID string to validate</param>
    /// <returns>A validation result</returns>
    protected abstract ValidationResult ValidateAndParseId(string id);
}