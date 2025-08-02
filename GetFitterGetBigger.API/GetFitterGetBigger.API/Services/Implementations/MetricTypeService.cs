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
/// Service implementation for metric type operations
/// TEMPORARY: Extends EmptyEnabledPureReferenceService until all entities are migrated
/// </summary>
public class MetricTypeService : PureReferenceService<MetricType, ReferenceDataDto>, IMetricTypeService
{
    private const string ValueCacheKeySuffix = "value:";
    
    public MetricTypeService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEternalCacheService cacheService,
        ILogger<MetricTypeService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MetricTypeId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, ServiceError.ValidationFailed(MetricTypeErrorMessages.InvalidIdFormat))
            .MatchAsync(
                whenValid: async () => await GetByIdAsync(id.ToString())
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, ServiceError.ValidationFailed(MetricTypeErrorMessages.ValueCannotBeEmpty))
            .MatchAsync(
                whenValid: async () => await GetFromCacheOrLoadAsync(
                    GetValueCacheKey(value),
                    () => LoadByValueAsync(value),
                    value)
            );
    }

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}{ValueCacheKeySuffix}{value}";
    
    private async Task<ServiceResult<ReferenceDataDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<MetricType>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        
        return await CacheLoad.For<ReferenceDataDto>(cacheService, cacheKey)
            .WithLogging(_logger, "MetricType")
            .MatchAsync(
                onHit: cached => ServiceResult<ReferenceDataDto>.Success(cached),
                onMiss: async () => await LoadAndProcessEntity(loadFunc, cacheKey, identifier)
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadAndProcessEntity(
        Func<Task<MetricType>> loadFunc,
        string cacheKey,
        string identifier)
    {
        var entity = await loadFunc();
        
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty, 
                ServiceError.NotFound(MetricTypeErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> CacheAndReturnSuccessAsync(string cacheKey, ReferenceDataDto dto)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }
    
    private async Task<MetricType> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMetricTypeRepository>();
        return await repository.GetByValueAsync(value);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(MetricTypeId id) => 
        !id.IsEmpty && (await GetByIdAsync(id)).IsSuccess;
    
    /// <inheritdoc/>
    public override async Task<bool> ExistsAsync(string id) => 
        await ExistsAsync(MetricTypeId.ParseOrEmpty(id));
    
    protected override async Task<IEnumerable<MetricType>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMetricTypeRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    /// <inheritdoc/>
    protected override async Task<ServiceResult<MetricType>> LoadEntityByIdAsync(string id)
    {
        var metricTypeId = MetricTypeId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<MetricType>()
            .EnsureNotEmpty(metricTypeId, ServiceError.InvalidFormat("MetricTypeId", MetricTypeErrorMessages.InvalidIdFormat))
            .Match(
                whenValid: async () => await LoadEntityFromRepository(metricTypeId),
                whenInvalid: errors => ServiceResult<MetricType>.Failure(
                    MetricType.Empty,
                    ServiceError.ValidationFailed(errors.FirstOrDefault() ?? "Invalid ID format"))
            );
    }
    
    private async Task<ServiceResult<MetricType>> LoadEntityFromRepository(MetricTypeId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMetricTypeRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity switch
        {
            { IsEmpty: true } => ServiceResult<MetricType>.Failure(
                MetricType.Empty, 
                ServiceError.NotFound("MetricType")),
            _ => ServiceResult<MetricType>.Success(entity)
        };
    }
    
    protected override ReferenceDataDto MapToDto(MetricType entity)
    {
        return new ReferenceDataDto
        {
            Id = entity.Id,
            Value = entity.Value,
            Description = entity.Description
        };
    }
    
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        return ServiceValidate.For()
            .EnsureNotWhiteSpace(id, MetricTypeErrorMessages.IdCannotBeEmpty)
            .ToResult();
    }
}