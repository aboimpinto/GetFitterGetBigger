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
public class BodyPartService : EmptyEnabledPureReferenceService<BodyPart, BodyPartDto>, IBodyPartService
{
    public BodyPartService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEmptyEnabledCacheService cacheService,
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
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id) => id switch
    {
        { IsEmpty: true } => ServiceResult<BodyPartDto>.Failure(
            CreateEmptyDto(),
            "Invalid body part ID format. Expected format: 'bodypart-{guid}'"),
        _ => await GetByIdAsync(id.ToString())
    };
    
    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value) => 
        string.IsNullOrWhiteSpace(value)
            ? ServiceResult<BodyPartDto>.Failure(CreateEmptyDto(), "Body part value cannot be empty")
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
        var cacheService = (IEmptyEnabledCacheService)_cacheService;
        var cacheResult = await cacheService.GetAsync<BodyPartDto>(cacheKey);
        if (cacheResult.IsHit)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return ServiceResult<BodyPartDto>.Success(cacheResult.Value);
        }
        
        var entity = await loadFunc();
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<BodyPartDto>.Failure(
                CreateEmptyDto(), 
                $"Body part with value '{identifier}' not found"),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<BodyPartDto>> CacheAndReturnSuccessAsync(string cacheKey, BodyPartDto dto)
    {
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromDays(365));
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
    
    protected override async Task<IEnumerable<BodyPart>> LoadAllEntitiesAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    // Returns BodyPart.Empty instead of null (Null Object Pattern)
    protected override async Task<BodyPart> LoadEntityByIdAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string id) =>
        BodyPartId.ParseOrEmpty(id) switch
        {
            { IsEmpty: true } => BodyPart.Empty,
            var bodyPartId => await unitOfWork.GetRepository<IBodyPartRepository>().GetByIdAsync(bodyPartId)
        };
    
    protected override BodyPartDto MapToDto(BodyPart entity)
    {
        return new BodyPartDto
        {
            Id = entity.Id,
            Value = entity.Value,
            Description = entity.Description
        };
    }
    
    protected override BodyPartDto CreateEmptyDto()
    {
        return new BodyPartDto();
    }
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return ValidationResult.Failure("Body part ID cannot be empty");
        }
        
        var parsedId = BodyPartId.ParseOrEmpty(id);
        if (parsedId.IsEmpty)
        {
            return ValidationResult.Failure($"Invalid body part ID format. Expected format: 'bodypart-{{guid}}', got: '{id}'");
        }
        
        return ValidationResult.Success();
    }
}