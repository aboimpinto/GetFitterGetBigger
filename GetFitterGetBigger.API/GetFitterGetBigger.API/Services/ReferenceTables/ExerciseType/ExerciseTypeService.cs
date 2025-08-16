using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.ExerciseType.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using CacheKeyGenerator = GetFitterGetBigger.API.Utilities.CacheKeyGenerator;

namespace GetFitterGetBigger.API.Services.ReferenceTables.ExerciseType;

/// <summary>
/// Service implementation for exercise type operations with integrated eternal caching
/// ExerciseTypes are pure reference data that never changes after deployment
/// NO UnitOfWork here - all data access through IExerciseTypeDataService
/// </summary>
public class ExerciseTypeService : IExerciseTypeService
{
    private readonly IExerciseTypeDataService _dataService;
    private readonly IEternalCacheService _cacheService;
    private readonly ILogger<ExerciseTypeService> _logger;

    public ExerciseTypeService(
        IExerciseTypeDataService dataService,
        IEternalCacheService cacheService,
        ILogger<ExerciseTypeService> logger)
    {
        _dataService = dataService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ExerciseTypeDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("ExerciseTypes");
        
        return await CacheLoad.For<IEnumerable<ExerciseTypeDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "ExerciseTypes")
            .WithAutoCacheAsync(() => _dataService.GetAllActiveAsync());
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ExerciseTypeDto>> GetByIdAsync(ExerciseTypeId id)
    {
        return await ServiceValidate.For<ExerciseTypeDto>()
            .EnsureNotEmpty(id, ExerciseTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromCacheAsync(id)
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ExerciseTypeDto>> GetByIdAsync(string id)
    {
        var exerciseTypeId = ExerciseTypeId.ParseOrEmpty(id);
        return await GetByIdAsync(exerciseTypeId);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ExerciseTypeDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ExerciseTypeDto>()
            .EnsureNotWhiteSpace(value, ExerciseTypeErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await LoadByValueFromCacheAsync(value)
            );
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseTypeId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, ExerciseTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await CheckExistenceAsync(id)
            );
    }
    
    /// <inheritdoc/>
    public async Task<bool> AllExistAsync(IEnumerable<string> ids)
    {
        // Parse IDs
        var exerciseTypeIds = ids.Select(id => ExerciseTypeId.ParseOrEmpty(id))
                                 .Where(id => !id.IsEmpty)
                                 .ToList();
        
        // If any ID failed to parse, not all exist
        if (exerciseTypeIds.Count != ids.Count())
            return false;
            
        // Check if all exist using data service
        return await _dataService.AllExistAsync(exerciseTypeIds);
    }
    
    /// <inheritdoc/>
    public async Task<bool> AnyIsRestTypeAsync(IEnumerable<string> ids)
    {
        // Parse IDs
        var exerciseTypeIds = ids.Select(id => ExerciseTypeId.ParseOrEmpty(id))
                                 .Where(id => !id.IsEmpty)
                                 .ToList();
        
        // Check if any is REST type using data service
        return await _dataService.AnyIsRestTypeAsync(exerciseTypeIds);
    }

    // Private helper methods for single operations
    private async Task<ServiceResult<ExerciseTypeDto>> LoadByIdFromCacheAsync(ExerciseTypeId id)
    {
        var cacheKey = CacheKeyGenerator.GetByIdKey("ExerciseTypes", id.ToString());
        
        return await CacheLoad.For<ExerciseTypeDto>(_cacheService, cacheKey)
            .WithLogging(_logger, "ExerciseType")
            .WithAutoCacheAsync(async () =>
            {
                var result = await _dataService.GetByIdAsync(id);
                // Convert Empty to NotFound at the service layer
                return result.IsSuccess && result.Data.IsEmpty
                    ? ServiceResult<ExerciseTypeDto>.Failure(
                        ExerciseTypeDto.Empty,
                        ServiceError.NotFound("ExerciseType", id.ToString()))
                    : result;
            });
    }

    private async Task<ServiceResult<ExerciseTypeDto>> LoadByValueFromCacheAsync(string value)
    {
        var cacheKey = CacheKeyGenerator.GetByValueKey("ExerciseTypes", value);
        
        return await CacheLoad.For<ExerciseTypeDto>(_cacheService, cacheKey)
            .WithLogging(_logger, "ExerciseType")
            .WithAutoCacheAsync(async () =>
            {
                var result = await _dataService.GetByValueAsync(value);
                // Convert Empty to NotFound at the service layer
                return result.IsSuccess && result.Data.IsEmpty
                    ? ServiceResult<ExerciseTypeDto>.Failure(
                        ExerciseTypeDto.Empty,
                        ServiceError.NotFound("ExerciseType", value))
                    : result;
            });
    }

    private async Task<ServiceResult<BooleanResultDto>> CheckExistenceAsync(ExerciseTypeId id)
    {
        // Leverage the GetById cache for existence checks
        var result = await GetByIdAsync(id);
        return ServiceResult<BooleanResultDto>.Success(
            BooleanResultDto.Create(result.IsSuccess && !result.Data.IsEmpty)
        );
    }
}