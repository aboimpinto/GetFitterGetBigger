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
/// Reference service for BodyPart that provides caching infrastructure
/// This service delegates actual business logic to IBodyPartService
/// Entities stay within the service layer - only DTOs are exposed
/// </summary>
public class BodyPartReferenceService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    IBodyPartService bodyPartService,
    ILogger<BodyPartReferenceService> logger) :
    PureReferenceService<BodyPart, BodyPartDto>(unitOfWorkProvider, cacheService, logger)
{
    private readonly IBodyPartService _bodyPartService = bodyPartService;

    /// <summary>
    /// Loads all DTOs by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<IEnumerable<BodyPartDto>>> LoadAllDtosAsync()
    {
        // Delegate to the actual service which handles entity-to-DTO mapping internally
        return await _bodyPartService.GetAllActiveAsync();
    }
    
    /// <summary>
    /// Loads a DTO by ID by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<BodyPartDto>> LoadDtoByIdAsync(string id)
    {
        var bodyPartId = BodyPartId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<BodyPartDto>()
            .EnsureNotEmpty(bodyPartId, BodyPartErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await _bodyPartService.GetByIdAsync(bodyPartId)
            );
    }
    
    /// <summary>
    /// Validates and parses the ID using ServiceValidate fluent API
    /// </summary>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var bodyPartId = BodyPartId.ParseOrEmpty(id);
        
        return ServiceValidate.For()
            .EnsureNotEmpty(bodyPartId, BodyPartErrorMessages.InvalidIdFormat)
            .ToResult();
    }
}