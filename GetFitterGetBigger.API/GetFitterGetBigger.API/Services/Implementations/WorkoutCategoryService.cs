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
/// Service implementation for workout category operations with integrated eternal caching
/// WorkoutCategories are pure reference data that never changes after deployment
/// </summary>
public class WorkoutCategoryService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    ILogger<WorkoutCategoryService> logger) : IWorkoutCategoryService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
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
            .WithAutoCacheAsync(LoadAllActiveFromDatabaseAsync);
    }
    
    /// <summary>
    /// Loads all active WorkoutCategories from the database and maps to DTOs
    /// </summary>
    private async Task<ServiceResult<IEnumerable<WorkoutCategoryDto>>> LoadAllActiveFromDatabaseAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active workout categories", dtos.Count);
        return ServiceResult<IEnumerable<WorkoutCategoryDto>>.Success(dtos);
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
                            var result = await LoadByIdFromDatabaseAsync(id);
                            // Convert Empty to NotFound at the API layer
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
    
    /// <summary>
    /// Loads a WorkoutCategory by ID from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<WorkoutCategoryDto>> LoadByIdFromDatabaseAsync(WorkoutCategoryId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<WorkoutCategoryDto>.Success(MapToDto(entity))
            : ServiceResult<WorkoutCategoryDto>.Success(WorkoutCategoryDto.Empty);
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
                            var result = await LoadByValueFromDatabaseAsync(value);
                            // Convert Empty to NotFound at the API layer
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
    
    /// <summary>
    /// Loads a WorkoutCategory by value from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<WorkoutCategoryDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var entity = await repository.GetByValueAsync(value);

        return entity.IsActive
            ? ServiceResult<WorkoutCategoryDto>.Success(MapToDto(entity))
            : ServiceResult<WorkoutCategoryDto>.Success(WorkoutCategoryDto.Empty);   
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
    
    /// <summary>
    /// Maps a WorkoutCategory entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private WorkoutCategoryDto MapToDto(WorkoutCategory entity)
    {
        if (entity.IsEmpty)
            return WorkoutCategoryDto.Empty;
            
        return new WorkoutCategoryDto
        {
            WorkoutCategoryId = entity.WorkoutCategoryId.ToString(),
            Value = entity.Value,
            Description = entity.Description,
            Icon = entity.Icon,
            Color = entity.Color,
            PrimaryMuscleGroups = entity.PrimaryMuscleGroups,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive
        };
    }
}
