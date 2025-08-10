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
/// Service implementation for body part operations with integrated eternal caching
/// BodyParts are pure reference data that never changes after deployment
/// </summary>
public class BodyPartService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    ILogger<BodyPartService> logger) : IBodyPartService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IEternalCacheService _cacheService = cacheService;
    private readonly ILogger<BodyPartService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<BodyPartDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("BodyParts");
        
        return await CacheLoad.For<IEnumerable<BodyPartDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "BodyParts")
            .WithAutoCacheAsync(LoadAllActiveFromDatabaseAsync);
    }
    
    /// <summary>
    /// Loads all active BodyParts from the database and maps to DTOs
    /// </summary>
    private async Task<ServiceResult<IEnumerable<BodyPartDto>>> LoadAllActiveFromDatabaseAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active body parts", dtos.Count);
        return ServiceResult<IEnumerable<BodyPartDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
    {
        return await ServiceValidate.For<BodyPartDto>()
            .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByIdKey("BodyParts", id.ToString());
                    
                    return await CacheLoad.For<BodyPartDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "BodyPart")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByIdFromDatabaseAsync(id);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<BodyPartDto>.Failure(
                                    BodyPartDto.Empty,
                                    ServiceError.NotFound("BodyPart", id.ToString()));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Gets a body part by its ID string
    /// </summary>
    /// <param name="id">The body part ID as a string</param>
    /// <returns>A service result containing the body part if found</returns>
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(string id)
    {
        var bodyPartId = BodyPartId.ParseOrEmpty(id);
        return await GetByIdAsync(bodyPartId);
    }
    
    /// <summary>
    /// Loads a BodyPart by ID from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<BodyPartDto>> LoadByIdFromDatabaseAsync(BodyPartId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<BodyPartDto>.Success(MapToDto(entity))
            : ServiceResult<BodyPartDto>.Success(BodyPartDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<BodyPartDto>()
            .EnsureNotWhiteSpace(value, BodyPartErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByValueKey("BodyParts", value);
                    
                    return await CacheLoad.For<BodyPartDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "BodyPart")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByValueFromDatabaseAsync(value);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<BodyPartDto>.Failure(
                                    BodyPartDto.Empty,
                                    ServiceError.NotFound("BodyPart", value));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Loads a BodyPart by value from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<BodyPartDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entity = await repository.GetByValueAsync(value);

        return entity.IsActive
            ? ServiceResult<BodyPartDto>.Success(MapToDto(entity))
            : ServiceResult<BodyPartDto>.Success(BodyPartDto.Empty);   
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(BodyPartId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
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
    /// Maps a BodyPart entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private BodyPartDto MapToDto(BodyPart entity)
    {
        if (entity.IsEmpty)
            return BodyPartDto.Empty;
            
        return new BodyPartDto
        {
            Id = entity.Id,
            Value = entity.Value,
            Description = entity.Description
        };
    }
}