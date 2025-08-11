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
/// Service implementation for kinetic chain type operations with integrated eternal caching
/// KineticChainTypes are pure reference data that never changes after deployment
/// </summary>
public class KineticChainTypeService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    ILogger<KineticChainTypeService> logger) : IKineticChainTypeService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IEternalCacheService _cacheService = cacheService;
    private readonly ILogger<KineticChainTypeService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("KineticChainTypes");
        
        return await CacheLoad.For<IEnumerable<ReferenceDataDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "KineticChainTypes")
            .WithAutoCacheAsync(LoadAllActiveFromDatabaseAsync);
    }
    
    /// <summary>
    /// Loads all active KineticChainTypes from the database and maps to DTOs
    /// </summary>
    private async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> LoadAllActiveFromDatabaseAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active kinetic chain types", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(KineticChainTypeId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, KineticChainTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByIdKey("KineticChainTypes", id.ToString());
                    
                    return await CacheLoad.For<ReferenceDataDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "KineticChainType")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByIdFromDatabaseAsync(id);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ReferenceDataDto>.Failure(
                                    ReferenceDataDto.Empty,
                                    ServiceError.NotFound("KineticChainType", id.ToString()));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Gets a kinetic chain type by its ID string
    /// </summary>
    /// <param name="id">The kinetic chain type ID as a string</param>
    /// <returns>A service result containing the kinetic chain type if found</returns>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        var kineticChainTypeId = KineticChainTypeId.ParseOrEmpty(id);
        return await GetByIdAsync(kineticChainTypeId);
    }
    
    /// <summary>
    /// Loads a KineticChainType by ID from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromDatabaseAsync(KineticChainTypeId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, KineticChainTypeErrorMessages.ValueCannotBeEmptyEntity)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByValueKey("KineticChainTypes", value);
                    
                    return await CacheLoad.For<ReferenceDataDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "KineticChainType")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByValueFromDatabaseAsync(value);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ReferenceDataDto>.Failure(
                                    ReferenceDataDto.Empty,
                                    ServiceError.NotFound("KineticChainType", value));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Loads a KineticChainType by value from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(KineticChainTypeId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, KineticChainTypeErrorMessages.InvalidIdFormat)
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
    /// Maps a KineticChainType entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ReferenceDataDto MapToDto(KineticChainType entity)
    {
        if (entity.IsEmpty)
            return ReferenceDataDto.Empty;
            
        return new ReferenceDataDto
        {
            Id = entity.KineticChainTypeId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
