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
/// Service implementation for difficulty level operations with integrated eternal caching
/// DifficultyLevels are pure reference data that never changes after deployment
/// </summary>
public class DifficultyLevelService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    ILogger<DifficultyLevelService> logger) : IDifficultyLevelService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IEternalCacheService _cacheService = cacheService;
    private readonly ILogger<DifficultyLevelService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("DifficultyLevels");
        
        return await CacheLoad.For<IEnumerable<ReferenceDataDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "DifficultyLevels")
            .WithAutoCacheAsync(LoadAllActiveFromDatabaseAsync);
    }
    
    /// <summary>
    /// Loads all active DifficultyLevels from the database and maps to DTOs
    /// </summary>
    private async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> LoadAllActiveFromDatabaseAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active difficulty levels", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(DifficultyLevelId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, DifficultyLevelErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByIdKey("DifficultyLevels", id.ToString());
                    
                    return await CacheLoad.For<ReferenceDataDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "DifficultyLevel")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByIdFromDatabaseAsync(id);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ReferenceDataDto>.Failure(
                                    ReferenceDataDto.Empty,
                                    ServiceError.NotFound("DifficultyLevel", id.ToString()));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Loads a DifficultyLevel by ID from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromDatabaseAsync(DifficultyLevelId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, DifficultyLevelErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByValueKey("DifficultyLevels", value);
                    
                    return await CacheLoad.For<ReferenceDataDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "DifficultyLevel")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByValueFromDatabaseAsync(value);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ReferenceDataDto>.Failure(
                                    ReferenceDataDto.Empty,
                                    ServiceError.NotFound("DifficultyLevel", value));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Loads a DifficultyLevel by value from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        var entity = await repository.GetByValueAsync(value);

        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);   
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(DifficultyLevelId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, DifficultyLevelErrorMessages.InvalidIdFormat)
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
    /// Maps a DifficultyLevel entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ReferenceDataDto MapToDto(DifficultyLevel entity)
    {
        if (entity.IsEmpty)
            return ReferenceDataDto.Empty;
            
        return new ReferenceDataDto
        {
            Id = entity.DifficultyLevelId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}