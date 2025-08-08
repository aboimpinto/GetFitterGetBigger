using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for kinetic chain type operations
/// TEMPORARY: Extends EmptyEnabledPureReferenceService until all entities are migrated
/// </summary>
public class KineticChainTypeService : PureReferenceService<KineticChainType, ReferenceDataDto>, IKineticChainTypeService
{
    public KineticChainTypeService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEternalCacheService cacheService,
        ILogger<KineticChainTypeService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(KineticChainTypeId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, KineticChainTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await GetByIdAsync(id.ToString())
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, KineticChainTypeErrorMessages.ValueCannotBeEmptyEntity)
            .MatchAsync(
                whenValid: async () => await GetFromCacheOrLoadAsync(
                    GetValueCacheKey(value),
                    () => LoadByValueAsync(value),
                    value)
            );
    }

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<ReferenceDataDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<KineticChainType>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        
        return await CacheLoad.For<ReferenceDataDto>(cacheService, cacheKey)
            .WithLogging(_logger, "KineticChainType")
            .MatchAsync(
                onHit: cached => ServiceResult<ReferenceDataDto>.Success(cached),
                onMiss: async () => await LoadAndProcessEntity(loadFunc, cacheKey, identifier)
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadAndProcessEntity(
        Func<Task<KineticChainType>> loadFunc,
        string cacheKey,
        string identifier)
    {
        var entity = await loadFunc();
        
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty, 
                ServiceError.NotFound(KineticChainTypeErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> CacheAndReturnSuccessAsync(string cacheKey, ReferenceDataDto dto)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<bool>> ExistsAsync(KineticChainTypeId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, KineticChainTypeErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
            });
    }

    private async Task<KineticChainType> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
        return await repository.GetByValueAsync(value);
    }

    /// <inheritdoc/>
    protected override async Task<ServiceResult<KineticChainType>> LoadEntityByIdAsync(string id)
    {
        var kineticChainTypeId = KineticChainTypeId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<KineticChainType>()
            .EnsureNotEmpty(kineticChainTypeId, KineticChainTypeErrorMessages.InvalidIdFormat)
            .Match(
                whenValid: async () => await LoadEntityFromRepository(kineticChainTypeId),
                whenInvalid: errors => ServiceResult<KineticChainType>.Failure(
                    KineticChainType.Empty,
                    ServiceError.ValidationFailed(errors.FirstOrDefault() ?? "Invalid ID format"))
            );
    }
    
    private async Task<ServiceResult<KineticChainType>> LoadEntityFromRepository(KineticChainTypeId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity switch
        {
            { IsEmpty: true } => ServiceResult<KineticChainType>.Failure(
                KineticChainType.Empty, 
                ServiceError.NotFound("KineticChainType")),
            _ => ServiceResult<KineticChainType>.Success(entity)
        };
    }

    /// <inheritdoc/>
    protected override async Task<IEnumerable<KineticChainType>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
        return await repository.GetAllActiveAsync();
    }

    /// <inheritdoc/>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var parsedId = KineticChainTypeId.ParseOrEmpty(id);
        return ServiceValidate.For()
            .EnsureNotEmpty(parsedId, KineticChainTypeErrorMessages.InvalidIdFormat)
            .ToResult();
    }

    /// <inheritdoc/>
    protected override ReferenceDataDto MapToDto(KineticChainType entity) =>
        new()
        {
            Id = entity.Id,
            Value = entity.Value,
            Description = entity.Description
        };
}