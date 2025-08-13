using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.BodyPart.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using CacheKeyGenerator = GetFitterGetBigger.API.Utilities.CacheKeyGenerator;

namespace GetFitterGetBigger.API.Services.ReferenceTables.BodyPart;

/// <summary>
/// Service implementation for body part operations with integrated eternal caching
/// BodyParts are pure reference data that never changes after deployment
/// NO UnitOfWork here - all data access through IBodyPartDataService
/// </summary>
public class BodyPartService : IBodyPartService
{
    private readonly IBodyPartDataService _dataService;
    private readonly IEternalCacheService _cacheService;
    private readonly ILogger<BodyPartService> _logger;

    public BodyPartService(
        IBodyPartDataService dataService,
        IEternalCacheService cacheService,
        ILogger<BodyPartService> logger)
    {
        _dataService = dataService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<BodyPartDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("BodyParts");
        
        return await CacheLoad.For<IEnumerable<BodyPartDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "BodyParts")
            .WithAutoCacheAsync(() => _dataService.GetAllActiveAsync());
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
    {
        return await ServiceValidate.For<BodyPartDto>()
            .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByIdKey("BodyParts", id.ToString());
                    
                    return await CacheLoad.For<BodyPartDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "BodyPart")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await _dataService.GetByIdAsync(id);
                            // Convert Empty to NotFound at the service layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<BodyPartDto>.Failure(
                                    BodyPartDto.Empty,
                                    ServiceError.NotFound("BodyPart", id.ToString()));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(string id)
    {
        var bodyPartId = BodyPartId.ParseOrEmpty(id);
        return await GetByIdAsync(bodyPartId);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<BodyPartDto>()
            .EnsureNotWhiteSpace(value, BodyPartErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByValueKey("BodyParts", value);
                    
                    return await CacheLoad.For<BodyPartDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "BodyPart")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await _dataService.GetByValueAsync(value);
                            // Convert Empty to NotFound at the service layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<BodyPartDto>.Failure(
                                    BodyPartDto.Empty,
                                    ServiceError.NotFound("BodyPart", value));
                            }
                            return result;
                        });
                }
            );
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(BodyPartId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
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