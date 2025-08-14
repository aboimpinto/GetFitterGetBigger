using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutCategory.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using CacheKeyGenerator = GetFitterGetBigger.API.Utilities.CacheKeyGenerator;

namespace GetFitterGetBigger.API.Services.ReferenceTables.WorkoutCategory;

/// <summary>
/// Service implementation for workout category operations with integrated eternal caching
/// WorkoutCategories are pure reference data that never changes after deployment
/// NO UnitOfWork here - all data access through IWorkoutCategoryDataService
/// </summary>
public class WorkoutCategoryService(
    IWorkoutCategoryDataService dataService,
    IEternalCacheService cacheService,
    ILogger<WorkoutCategoryService> logger) : IWorkoutCategoryService
{
    private readonly IWorkoutCategoryDataService _dataService = dataService;
    private readonly IEternalCacheService _cacheService = cacheService;
    private readonly ILogger<WorkoutCategoryService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<WorkoutCategoryDto>>> GetAllAsync()
    {
        return await GetAllActiveAsync();
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<WorkoutCategoryDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("WorkoutCategories");
        
        return await CacheLoad.For<IEnumerable<WorkoutCategoryDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "WorkoutCategories")
            .WithAutoCacheAsync(async () => await _dataService.GetAllActiveAsync());
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutCategoryDto>> GetByIdAsync(WorkoutCategoryId id)
    {
        return await ServiceValidate.For<WorkoutCategoryDto>()
            .EnsureNotEmpty(id, WorkoutCategoryErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByIdKey("WorkoutCategories", id.ToString());
                    
                    return await CacheLoad.For<WorkoutCategoryDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "WorkoutCategory")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await _dataService.GetByIdAsync(id);
                            // Convert Empty to NotFound at the service layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<WorkoutCategoryDto>.Failure(
                                    WorkoutCategoryDto.Empty,
                                    ServiceError.NotFound("WorkoutCategory", id.ToString()));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Gets a workout category by its ID string
    /// </summary>
    /// <param name="id">The workout category ID as a string</param>
    /// <returns>A service result containing the workout category if found</returns>
    public async Task<ServiceResult<WorkoutCategoryDto>> GetByIdAsync(string id)
    {
        var workoutCategoryId = WorkoutCategoryId.ParseOrEmpty(id);
        return await GetByIdAsync(workoutCategoryId);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutCategoryDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<WorkoutCategoryDto>()
            .EnsureNotWhiteSpace(value, WorkoutCategoryErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByValueKey("WorkoutCategories", value);
                    
                    return await CacheLoad.For<WorkoutCategoryDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "WorkoutCategory")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await _dataService.GetByValueAsync(value);
                            // Convert Empty to NotFound at the service layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<WorkoutCategoryDto>.Failure(
                                    WorkoutCategoryDto.Empty,
                                    ServiceError.NotFound("WorkoutCategory", value));
                            }
                            return result;
                        });
                }
            );
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutCategoryId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, WorkoutCategoryErrorMessages.InvalidIdFormat)
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