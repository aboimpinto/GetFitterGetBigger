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
/// Service implementation for exercise type operations with integrated eternal caching
/// ExerciseTypes are pure reference data that never changes after deployment
/// </summary>
public class ExerciseTypeService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    ILogger<ExerciseTypeService> logger) : IExerciseTypeService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IEternalCacheService _cacheService = cacheService;
    private readonly ILogger<ExerciseTypeService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ExerciseTypeDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("ExerciseTypes");
        
        return await CacheLoad.For<IEnumerable<ExerciseTypeDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "ExerciseTypes")
            .WithAutoCacheAsync(LoadAllActiveFromDatabaseAsync);
    }
    
    /// <summary>
    /// Loads all active ExerciseTypes from the database and maps to DTOs
    /// </summary>
    private async Task<ServiceResult<IEnumerable<ExerciseTypeDto>>> LoadAllActiveFromDatabaseAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active exercise types", dtos.Count);
        return ServiceResult<IEnumerable<ExerciseTypeDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ExerciseTypeDto>> GetByIdAsync(ExerciseTypeId id)
    {
        return await ServiceValidate.For<ExerciseTypeDto>()
            .EnsureNotEmpty(id, ExerciseTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByIdKey("ExerciseTypes", id.ToString());
                    
                    return await CacheLoad.For<ExerciseTypeDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "ExerciseType")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByIdFromDatabaseAsync(id);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ExerciseTypeDto>.Failure(
                                    ExerciseTypeDto.Empty,
                                    ServiceError.NotFound("ExerciseType", id.ToString()));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Gets an exercise type by its ID string
    /// </summary>
    /// <param name="id">The exercise type ID as a string</param>
    /// <returns>A service result containing the exercise type if found</returns>
    public async Task<ServiceResult<ExerciseTypeDto>> GetByIdAsync(string id)
    {
        var exerciseTypeId = ExerciseTypeId.ParseOrEmpty(id);
        return await GetByIdAsync(exerciseTypeId);
    }
    
    /// <summary>
    /// Loads an ExerciseType by ID from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ExerciseTypeDto>> LoadByIdFromDatabaseAsync(ExerciseTypeId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<ExerciseTypeDto>.Success(MapToDto(entity))
            : ServiceResult<ExerciseTypeDto>.Success(ExerciseTypeDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ExerciseTypeDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ExerciseTypeDto>()
            .EnsureNotWhiteSpace(value, ExerciseTypeErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByValueKey("ExerciseTypes", value);
                    
                    return await CacheLoad.For<ExerciseTypeDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "ExerciseType")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByValueFromDatabaseAsync(value);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ExerciseTypeDto>.Failure(
                                    ExerciseTypeDto.Empty,
                                    ServiceError.NotFound("ExerciseType", value));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Loads an ExerciseType by value from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ExerciseTypeDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        var entity = await repository.GetByValueAsync(value);

        return entity.IsActive
            ? ServiceResult<ExerciseTypeDto>.Success(MapToDto(entity))
            : ServiceResult<ExerciseTypeDto>.Success(ExerciseTypeDto.Empty);   
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExerciseTypeId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, ExerciseTypeErrorMessages.InvalidIdFormat)
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
            
        // Check if all exist
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        
        foreach (var id in exerciseTypeIds)
        {
            var exists = await repository.ExistsAsync(id);
            if (!exists)
                return false;
        }
        
        return true;
    }
    
    /// <inheritdoc/>
    public Task<bool> AnyIsRestTypeAsync(IEnumerable<string> ids)
    {
        // The REST exercise type has a fixed ID that never changes
        const string REST_EXERCISE_TYPE_ID = "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a";
        
        // Simply check if any of the provided IDs match the REST type ID
        return Task.FromResult(ids.Any(id => string.Equals(id, REST_EXERCISE_TYPE_ID, StringComparison.OrdinalIgnoreCase)));
    }
    
    /// <summary>
    /// Maps an ExerciseType entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ExerciseTypeDto MapToDto(ExerciseType entity)
    {
        if (entity.IsEmpty)
            return ExerciseTypeDto.Empty;
            
        return new ExerciseTypeDto
        {
            Id = entity.ExerciseTypeId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
