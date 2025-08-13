using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.MovementPattern.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using CacheKeyGenerator = GetFitterGetBigger.API.Utilities.CacheKeyGenerator;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MovementPattern;

/// <summary>
/// Service implementation for movement pattern operations with integrated eternal caching
/// MovementPatterns are pure reference data that never changes after deployment
/// NO UnitOfWork here - all data access through IMovementPatternDataService
/// </summary>
public class MovementPatternService : IMovementPatternService
{
    private readonly IMovementPatternDataService _dataService;
    private readonly IEternalCacheService _cacheService;
    private readonly ILogger<MovementPatternService> _logger;

    public MovementPatternService(
        IMovementPatternDataService dataService,
        IEternalCacheService cacheService,
        ILogger<MovementPatternService> logger)
    {
        _dataService = dataService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("MovementPatterns");
        
        return await CacheLoad.For<IEnumerable<ReferenceDataDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "MovementPatterns")
            .WithAutoCacheAsync(() => _dataService.GetAllActiveAsync());
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MovementPatternId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, MovementPatternErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByIdKey("MovementPatterns", id.ToString());
                    
                    return await CacheLoad.For<ReferenceDataDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "MovementPattern")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await _dataService.GetByIdAsync(id);
                            // Convert Empty to NotFound at the service layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ReferenceDataDto>.Failure(
                                    ReferenceDataDto.Empty,
                                    ServiceError.NotFound("MovementPattern", id.ToString()));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        var movementPatternId = MovementPatternId.ParseOrEmpty(id);
        return await GetByIdAsync(movementPatternId);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, MovementPatternErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByValueKey("MovementPatterns", value);
                    
                    return await CacheLoad.For<ReferenceDataDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "MovementPattern")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await _dataService.GetByValueAsync(value);
                            // Convert Empty to NotFound at the service layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ReferenceDataDto>.Failure(
                                    ReferenceDataDto.Empty,
                                    ServiceError.NotFound("MovementPattern", value));
                            }
                            return result;
                        });
                }
            );
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(MovementPatternId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, MovementPatternErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    // Leverage the GetById cache for existence checks
                    var result = await GetByIdAsync(id);
                    return ServiceResult<BooleanResultDto>.Success(
                        BooleanResultDto.Create(result.IsSuccess && !result.Data.IsEmpty)
                    );
                }
            );
    }
}