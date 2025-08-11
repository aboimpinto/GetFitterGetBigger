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
/// Service implementation for muscle role operations with integrated eternal caching
/// MuscleRoles are pure reference data that never changes after deployment
/// </summary>
public class MuscleRoleService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    ILogger<MuscleRoleService> logger) : IMuscleRoleService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IEternalCacheService _cacheService = cacheService;
    private readonly ILogger<MuscleRoleService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("MuscleRoles");
        
        return await CacheLoad.For<IEnumerable<ReferenceDataDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "MuscleRoles")
            .WithAutoCacheAsync(LoadAllActiveFromDatabaseAsync);
    }
    
    /// <summary>
    /// Loads all active MuscleRoles from the database and maps to DTOs
    /// </summary>
    private async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> LoadAllActiveFromDatabaseAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active muscle roles", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
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
                            var result = await LoadByIdFromDatabaseAsync(id);
                            // Convert Empty to NotFound at the API layer
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
    
    /// <summary>
    /// Gets a muscle role by its ID string
    /// </summary>
    /// <param name="id">The muscle role ID as a string</param>
    /// <returns>A service result containing the muscle role if found</returns>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        var muscleRoleId = MuscleRoleId.ParseOrEmpty(id);
        return await GetByIdAsync(muscleRoleId);
    }
    
    /// <summary>
    /// Loads a MuscleRole by ID from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromDatabaseAsync(MuscleRoleId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
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
                            var result = await LoadByValueFromDatabaseAsync(value);
                            // Convert Empty to NotFound at the API layer
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
    
    /// <summary>
    /// Loads a MuscleRole by value from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var entity = await repository.GetByValueAsync(value);

        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);   
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
    
    /// <summary>
    /// Maps a MuscleRole entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ReferenceDataDto MapToDto(MuscleRole entity)
    {
        return new ReferenceDataDto
        {
            Id = entity.MuscleRoleId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
