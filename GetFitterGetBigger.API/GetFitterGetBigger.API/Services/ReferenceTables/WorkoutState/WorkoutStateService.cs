using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using CacheKeyGenerator = GetFitterGetBigger.API.Utilities.CacheKeyGenerator;

namespace GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState;

/// <summary>
/// Service implementation for workout state operations with integrated eternal caching
/// WorkoutStates are pure reference data that never changes after deployment
/// NO UnitOfWork here - all data access through IWorkoutStateDataService
/// </summary>
public class WorkoutStateService(
    IWorkoutStateDataService dataService,
    IEternalCacheService cacheService,
    ILogger<WorkoutStateService> logger) : IWorkoutStateService
{
    private readonly IWorkoutStateDataService _dataService = dataService;
    private readonly IEternalCacheService _cacheService = cacheService;
    private readonly ILogger<WorkoutStateService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<WorkoutStateDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("WorkoutStates");
        
        return await CacheLoad.For<IEnumerable<WorkoutStateDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "WorkoutStates")
            .WithAutoCacheAsync(async () => await _dataService.GetAllActiveAsync());
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutStateDto>> GetByIdAsync(WorkoutStateId id)
    {
        return await ServiceValidate.For<WorkoutStateDto>()
            .EnsureNotEmpty(id, WorkoutStateErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByIdKey("WorkoutStates", id.ToString());
                    
                    return await CacheLoad.For<WorkoutStateDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "WorkoutState")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await _dataService.GetByIdAsync(id);
                            // Convert Empty to NotFound at the service layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<WorkoutStateDto>.Failure(
                                    WorkoutStateDto.Empty,
                                    ServiceError.NotFound("WorkoutState", id.ToString()));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Gets a workout state by its ID string
    /// </summary>
    /// <param name="id">The workout state ID as a string</param>
    /// <returns>A service result containing the workout state if found</returns>
    public async Task<ServiceResult<WorkoutStateDto>> GetByIdAsync(string id)
    {
        var workoutStateId = WorkoutStateId.ParseOrEmpty(id);
        return await GetByIdAsync(workoutStateId);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutStateDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<WorkoutStateDto>()
            .EnsureNotWhiteSpace(value, WorkoutStateErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByValueKey("WorkoutStates", value);
                    
                    return await CacheLoad.For<WorkoutStateDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "WorkoutState")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await _dataService.GetByValueAsync(value);
                            // Convert Empty to NotFound at the service layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<WorkoutStateDto>.Failure(
                                    WorkoutStateDto.Empty,
                                    ServiceError.NotFound("WorkoutState", value));
                            }
                            return result;
                        });
                }
            );
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutStateId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, WorkoutStateErrorMessages.InvalidIdFormat)
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