using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.MuscleRole.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using CacheKeyGenerator = GetFitterGetBigger.API.Utilities.CacheKeyGenerator;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MuscleRole;

/// <summary>
/// Service implementation for muscle role operations with integrated eternal caching
/// MuscleRoles are pure reference data that never changes after deployment
/// NO UnitOfWork here - all data access through IMuscleRoleDataService
/// </summary>
public class MuscleRoleService(
    IMuscleRoleDataService dataService,
    IEternalCacheService cacheService,
    ILogger<MuscleRoleService> logger) : IMuscleRoleService
{
    private readonly IMuscleRoleDataService _dataService = dataService;
    private readonly IEternalCacheService _cacheService = cacheService;
    private readonly ILogger<MuscleRoleService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("MuscleRoles");
        
        return await CacheLoad.For<IEnumerable<ReferenceDataDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "MuscleRoles")
            .WithAutoCacheAsync(() => _dataService.GetAllActiveAsync());
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MuscleRoleId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, MuscleRoleErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByIdKey("MuscleRoles", id.ToString());
                    
                    return await CacheLoad.For<ReferenceDataDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "MuscleRole")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await _dataService.GetByIdAsync(id);
                            // Convert Empty to NotFound at the service layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ReferenceDataDto>.Failure(
                                    ReferenceDataDto.Empty,
                                    ServiceError.NotFound("MuscleRole", id.ToString()));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        var muscleRoleId = MuscleRoleId.ParseOrEmpty(id);
        return await GetByIdAsync(muscleRoleId);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, MuscleRoleErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByValueKey("MuscleRoles", value);
                    
                    return await CacheLoad.For<ReferenceDataDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "MuscleRole")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await _dataService.GetByValueAsync(value);
                            // Convert Empty to NotFound at the service layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ReferenceDataDto>.Failure(
                                    ReferenceDataDto.Empty,
                                    ServiceError.NotFound("MuscleRole", value));
                            }
                            return result;
                        });
                }
            );
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(MuscleRoleId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, MuscleRoleErrorMessages.InvalidIdFormat)
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