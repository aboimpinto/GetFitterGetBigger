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
/// Service implementation for workout objective operations with integrated eternal caching
/// WorkoutObjectives are pure reference data that never changes after deployment
/// </summary>
public class WorkoutObjectiveService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    ILogger<WorkoutObjectiveService> logger) : IWorkoutObjectiveService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IEternalCacheService _cacheService = cacheService;
    private readonly ILogger<WorkoutObjectiveService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("WorkoutObjectives");
        
        return await CacheLoad.For<IEnumerable<ReferenceDataDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "WorkoutObjectives")
            .WithAutoCacheAsync(LoadAllActiveFromDatabaseAsync);
    }
    
    /// <summary>
    /// Loads all active WorkoutObjectives from the database and maps to DTOs
    /// </summary>
    private async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> LoadAllActiveFromDatabaseAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active workout objectives", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(WorkoutObjectiveId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, WorkoutObjectiveErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByIdKey("WorkoutObjectives", id.ToString());
                    
                    return await CacheLoad.For<ReferenceDataDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "WorkoutObjective")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByIdFromDatabaseAsync(id);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ReferenceDataDto>.Failure(
                                    ReferenceDataDto.Empty,
                                    ServiceError.NotFound("WorkoutObjective", id.ToString()));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Gets a workout objective by its ID string
    /// </summary>
    /// <param name="id">The workout objective ID as a string</param>
    /// <returns>A service result containing the workout objective if found</returns>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        var workoutObjectiveId = WorkoutObjectiveId.ParseOrEmpty(id);
        return await GetByIdAsync(workoutObjectiveId);
    }
    
    /// <summary>
    /// Loads a WorkoutObjective by ID from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromDatabaseAsync(WorkoutObjectiveId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, WorkoutObjectiveErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByValueKey("WorkoutObjectives", value);
                    
                    return await CacheLoad.For<ReferenceDataDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "WorkoutObjective")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByValueFromDatabaseAsync(value);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ReferenceDataDto>.Failure(
                                    ReferenceDataDto.Empty,
                                    ServiceError.NotFound("WorkoutObjective", value));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Loads a WorkoutObjective by value from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutObjectiveRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);   
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(WorkoutObjectiveId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, WorkoutObjectiveErrorMessages.InvalidIdFormat)
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
    /// Maps a WorkoutObjective entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ReferenceDataDto MapToDto(WorkoutObjective entity)
    {
        if (entity.IsEmpty)
            return ReferenceDataDto.Empty;
            
        return new ReferenceDataDto
        {
            Id = entity.WorkoutObjectiveId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
