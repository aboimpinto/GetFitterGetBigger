using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for body part operations
/// TEMPORARY: Extends EmptyEnabledPureReferenceService until all entities are migrated
/// </summary>
public class BodyPartService : PureReferenceService<BodyPart, BodyPartDto>, IBodyPartService
{
    public BodyPartService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEternalCacheService cacheService,
        ILogger<BodyPartService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<BodyPartDto>>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id) => 
        id.IsEmpty 
            ? ServiceResult<BodyPartDto>.Failure(BodyPartDto.Empty, ServiceError.ValidationFailed(BodyPartErrorMessages.InvalidIdFormat))
            : await GetByIdAsync(id.ToString());
    
    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value) => 
        string.IsNullOrWhiteSpace(value)
            ? ServiceResult<BodyPartDto>.Failure(BodyPartDto.Empty, ServiceError.ValidationFailed(BodyPartErrorMessages.ValueCannotBeEmpty))
            : await GetFromCacheOrLoadAsync(
                GetValueCacheKey(value),
                () => LoadByValueAsync(value),
                value);

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<BodyPartDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<BodyPart>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        var cacheResult = await cacheService.GetAsync<BodyPartDto>(cacheKey);
        
        if (cacheResult.IsHit)
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
        
        var result = cacheResult.IsHit
            ? ServiceResult<BodyPartDto>.Success(cacheResult.Value)
            : await ProcessUncachedEntity(await loadFunc(), cacheKey, identifier);
            
        return result;
    }
    
    private async Task<ServiceResult<BodyPartDto>> ProcessUncachedEntity(
        BodyPart entity, string cacheKey, string identifier) =>
        entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<BodyPartDto>.Failure(
                BodyPartDto.Empty, 
                ServiceError.NotFound(BodyPartErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    
    private async Task<ServiceResult<BodyPartDto>> CacheAndReturnSuccessAsync(string cacheKey, BodyPartDto dto)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<BodyPartDto>.Success(dto);
    }
    
    private async Task<BodyPart> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        return await repository.GetByValueAsync(value);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(BodyPartId id) => 
        !id.IsEmpty && (await GetByIdAsync(id)).IsSuccess;
    
    /// <inheritdoc/>
    public override async Task<bool> ExistsAsync(string id) => 
        await ExistsAsync(BodyPartId.ParseOrEmpty(id));
    
    protected override async Task<IEnumerable<BodyPart>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    // Returns BodyPart.Empty instead of null (Null Object Pattern)
    protected override async Task<BodyPart> LoadEntityByIdAsync(string id)
    {
        var bodyPartId = BodyPartId.ParseOrEmpty(id);
        
        var result = bodyPartId.IsEmpty
            ? BodyPart.Empty
            : await LoadFromRepository(bodyPartId);
            
        return result;
    }
    
    private async Task<BodyPart> LoadFromRepository(BodyPartId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        return await repository.GetByIdAsync(id);
    }
    
    protected override BodyPartDto MapToDto(BodyPart entity)
    {
        return new BodyPartDto
        {
            Id = entity.Id,
            Value = entity.Value,
            Description = entity.Description
        };
    }
    
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        // This is called by the base class when using the string overload
        // Since we always use the typed overload from the controller,
        // this should validate the string format
        if (string.IsNullOrWhiteSpace(id))
        {
            return ValidationResult.Failure(BodyPartErrorMessages.IdCannotBeEmpty);
        }
        
        // No additional validation - let the controller handle format validation
        // This allows empty GUIDs to pass through and be treated as NotFound
        
        // Valid format (including empty GUID) - let the database determine if it exists
        return ValidationResult.Success();
    }
}