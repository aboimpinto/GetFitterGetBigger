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
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
    {
        return await ServiceValidate.For<BodyPartDto>()
            .EnsureNotEmpty(id, ServiceError.ValidationFailed(BodyPartErrorMessages.InvalidIdFormat))
            .MatchAsync(
                whenValid: async () => await GetByIdAsync(id.ToString())
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<BodyPartDto>()
            .EnsureNotWhiteSpace(value, ServiceError.ValidationFailed(BodyPartErrorMessages.ValueCannotBeEmpty))
            .MatchAsync(
                whenValid: async () => await GetFromCacheOrLoadAsync(
                    GetValueCacheKey(value),
                    () => LoadByValueAsync(value),
                    value)
            );
    }

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<BodyPartDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<BodyPart>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        
        return await CacheLoad.For<BodyPartDto>(cacheService, cacheKey)
            .WithLogging(_logger, "BodyPart")
            .MatchAsync(
                onHit: cached => ServiceResult<BodyPartDto>.Success(cached),
                onMiss: async () => await LoadAndProcessEntity(loadFunc, cacheKey, identifier)
            );
    }
    
    private async Task<ServiceResult<BodyPartDto>> LoadAndProcessEntity(
        Func<Task<BodyPart>> loadFunc,
        string cacheKey,
        string identifier)
    {
        var entity = await loadFunc();
        
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<BodyPartDto>.Failure(
                BodyPartDto.Empty, 
                ServiceError.NotFound(BodyPartErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
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
    
    /// <inheritdoc/>
    protected override async Task<ServiceResult<BodyPart>> LoadEntityByIdAsync(string id)
    {
        var bodyPartId = BodyPartId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<BodyPart>()
            .EnsureNotEmpty(bodyPartId, ServiceError.InvalidFormat("BodyPartId", BodyPartErrorMessages.InvalidIdFormat))
            .Match(
                whenValid: async () => await LoadEntityFromRepository(bodyPartId),
                whenInvalid: errors => ServiceResult<BodyPart>.Failure(
                    BodyPart.Empty, 
                    ServiceError.ValidationFailed(errors.FirstOrDefault() ?? "Invalid ID format"))
            );
    }
    
    private async Task<ServiceResult<BodyPart>> LoadEntityFromRepository(BodyPartId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity switch
        {
            { IsEmpty: true } => ServiceResult<BodyPart>.Failure(
                BodyPart.Empty, 
                ServiceError.NotFound("BodyPart")),
            _ => ServiceResult<BodyPart>.Success(entity)
        };
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
        return ServiceValidate.For()
            .EnsureNotWhiteSpace(id, BodyPartErrorMessages.IdCannotBeEmpty)
            .ToResult();
    }
}