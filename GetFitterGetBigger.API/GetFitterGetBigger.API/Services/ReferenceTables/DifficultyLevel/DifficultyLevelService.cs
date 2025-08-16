using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.DifficultyLevel.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using CacheKeyGenerator = GetFitterGetBigger.API.Utilities.CacheKeyGenerator;

namespace GetFitterGetBigger.API.Services.ReferenceTables.DifficultyLevel;

/// <summary>
/// Service implementation for difficulty level operations with integrated eternal caching
/// DifficultyLevels are pure reference data that never changes after deployment
/// NO UnitOfWork here - all data access through IDifficultyLevelDataService
/// </summary>
public class DifficultyLevelService : IDifficultyLevelService
{
    private readonly IDifficultyLevelDataService _dataService;
    private readonly IEternalCacheService _cacheService;
    private readonly ILogger<DifficultyLevelService> _logger;

    public DifficultyLevelService(
        IDifficultyLevelDataService dataService,
        IEternalCacheService cacheService,
        ILogger<DifficultyLevelService> logger)
    {
        _dataService = dataService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("DifficultyLevels");
        
        return await CacheLoad.For<IEnumerable<ReferenceDataDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "DifficultyLevels")
            .WithAutoCacheAsync(() => _dataService.GetAllActiveAsync());
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(DifficultyLevelId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, DifficultyLevelErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromCacheAsync(id)
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        var difficultyLevelId = DifficultyLevelId.ParseOrEmpty(id);
        return await GetByIdAsync(difficultyLevelId);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, DifficultyLevelErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await LoadByValueFromCacheAsync(value)
            );
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(DifficultyLevelId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, DifficultyLevelErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await CheckExistenceAsync(id)
            );
    }

    // Private helper methods for single operations
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromCacheAsync(DifficultyLevelId id)
    {
        var cacheKey = CacheKeyGenerator.GetByIdKey("DifficultyLevels", id.ToString());
        
        return await CacheLoad.For<ReferenceDataDto>(_cacheService, cacheKey)
            .WithLogging(_logger, "DifficultyLevel")
            .WithAutoCacheAsync(async () =>
            {
                var result = await _dataService.GetByIdAsync(id);
                // Convert Empty to NotFound at the service layer
                return result.IsSuccess && result.Data.IsEmpty
                    ? ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("DifficultyLevel", id.ToString()))
                    : result;
            });
    }

    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromCacheAsync(string value)
    {
        var cacheKey = CacheKeyGenerator.GetByValueKey("DifficultyLevels", value);
        
        return await CacheLoad.For<ReferenceDataDto>(_cacheService, cacheKey)
            .WithLogging(_logger, "DifficultyLevel")
            .WithAutoCacheAsync(async () =>
            {
                var result = await _dataService.GetByValueAsync(value);
                // Convert Empty to NotFound at the service layer
                return result.IsSuccess && result.Data.IsEmpty
                    ? ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("DifficultyLevel", value))
                    : result;
            });
    }

    private async Task<ServiceResult<BooleanResultDto>> CheckExistenceAsync(DifficultyLevelId id)
    {
        // Leverage the GetById cache for existence checks
        var result = await GetByIdAsync(id);
        return ServiceResult<BooleanResultDto>.Success(
            BooleanResultDto.Create(result.IsSuccess && !result.Data.IsEmpty)
        );
    }
}