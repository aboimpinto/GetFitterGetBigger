using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Validation;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
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
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(KineticChainTypeId id) => 
        id.IsEmpty 
            ? ServiceResult<ReferenceDataDto>.Failure(ReferenceDataDto.Empty, ServiceError.ValidationFailed(KineticChainTypeErrorMessages.InvalidIdFormat))
            : await GetByIdAsync(id.ToString());
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value) => 
        string.IsNullOrWhiteSpace(value)
            ? ServiceResult<ReferenceDataDto>.Failure(ReferenceDataDto.Empty, ServiceError.ValidationFailed(KineticChainTypeErrorMessages.ValueCannotBeEmptyEntity))
            : await GetFromCacheOrLoadAsync(
                GetValueCacheKey(value),
                () => LoadByValueAsync(value),
                value);

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<ReferenceDataDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<KineticChainType>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        var cacheResult = await cacheService.GetAsync<ReferenceDataDto>(cacheKey);
        if (cacheResult.IsHit)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return ServiceResult<ReferenceDataDto>.Success(cacheResult.Value);
        }
        
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
    public async Task<bool> ExistsAsync(KineticChainTypeId id) => 
        await ExistsAsync(id.ToString());

    private async Task<KineticChainType> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
        return await repository.GetByValueAsync(value);
    }

    /// <inheritdoc/>
    protected override async Task<KineticChainType> LoadEntityByIdAsync(string id)
    {
        var kineticChainTypeId = KineticChainTypeId.ParseOrEmpty(id);
        if (kineticChainTypeId.IsEmpty)
            return KineticChainType.Empty;
            
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IKineticChainTypeRepository>();
        return await repository.GetByIdAsync(kineticChainTypeId);
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
        return parsedId.IsEmpty 
            ? ValidationResult.Failure(KineticChainTypeErrorMessages.InvalidIdFormat) 
            : ValidationResult.Success();
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