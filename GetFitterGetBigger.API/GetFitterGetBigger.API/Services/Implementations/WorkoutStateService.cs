using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;
using CacheKeyGenerator = GetFitterGetBigger.API.Utilities.CacheKeyGenerator;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for workout state operations with integrated eternal caching
/// WorkoutStates are pure reference data that never changes after deployment
/// </summary>
public class WorkoutStateService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    ILogger<WorkoutStateService> logger) : IWorkoutStateService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IEternalCacheService _cacheService = cacheService;
    private readonly ILogger<WorkoutStateService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<WorkoutStateDto>>> GetAllAsync()
    {
        return await GetAllActiveAsync();
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<WorkoutStateDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("WorkoutStates");
        
        return await CacheLoad.For<IEnumerable<WorkoutStateDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "WorkoutStates")
            .WithAutoCacheAsync(LoadAllActiveFromDatabaseAsync);
    }
    
    /// <summary>
    /// Loads all active WorkoutStates from the database and maps to DTOs
    /// </summary>
    private async Task<ServiceResult<IEnumerable<WorkoutStateDto>>> LoadAllActiveFromDatabaseAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active workout states", dtos.Count);
        return ServiceResult<IEnumerable<WorkoutStateDto>>.Success(dtos);
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
                            var result = await LoadByIdFromDatabaseAsync(id);
                            // Convert Empty to NotFound at the API layer
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
    
    /// <summary>
    /// Loads a WorkoutState by ID from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<WorkoutStateDto>> LoadByIdFromDatabaseAsync(WorkoutStateId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<WorkoutStateDto>.Success(MapToDto(entity))
            : ServiceResult<WorkoutStateDto>.Success(WorkoutStateDto.Empty);
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
                            var result = await LoadByValueFromDatabaseAsync(value);
                            // Convert Empty to NotFound at the API layer
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
    
    /// <summary>
    /// Loads a WorkoutState by value from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<WorkoutStateDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutStateRepository>();
        var entity = await repository.GetByValueAsync(value);

        return entity.IsActive
            ? ServiceResult<WorkoutStateDto>.Success(MapToDto(entity))
            : ServiceResult<WorkoutStateDto>.Success(WorkoutStateDto.Empty);
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
    
    /// <summary>
    /// Maps a WorkoutState entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private WorkoutStateDto MapToDto(WorkoutState entity)
    {
        if (entity.IsEmpty)
            return WorkoutStateDto.Empty;
            
        return new WorkoutStateDto
        {
            Id = entity.WorkoutStateId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}