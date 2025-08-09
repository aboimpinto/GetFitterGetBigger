using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables;

/// <summary>
/// Reference service for MetricType that provides caching infrastructure
/// This service delegates actual business logic to IMetricTypeService
/// Entities stay within the service layer - only DTOs are exposed
/// </summary>
public class MetricTypeReferenceService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    IMetricTypeService metric_typeService,
    ILogger<MetricTypeReferenceService> logger) :
    PureReferenceService<MetricType, ReferenceDataDto>(unitOfWorkProvider, cacheService, logger)
{
    private readonly IMetricTypeService _metric_typeService = metric_typeService;

    /// <summary>
    /// Loads all DTOs by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> LoadAllDtosAsync()
    {
        // Delegate to the actual service which handles entity-to-DTO mapping internally
        return await _metric_typeService.GetAllActiveAsync();
    }
    
    /// <summary>
    /// Loads a DTO by ID by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<ReferenceDataDto>> LoadDtoByIdAsync(string id)
    {
        var metric_typeId = MetricTypeId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(metric_typeId, MetricTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await _metric_typeService.GetByIdAsync(metric_typeId)
            );
    }
    
    /// <summary>
    /// Validates and parses the ID using ServiceValidate fluent API
    /// </summary>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var metric_typeId = MetricTypeId.ParseOrEmpty(id);
        
        return ServiceValidate.For()
            .EnsureNotEmpty(metric_typeId, MetricTypeErrorMessages.InvalidIdFormat)
            .ToResult();
    }
}
