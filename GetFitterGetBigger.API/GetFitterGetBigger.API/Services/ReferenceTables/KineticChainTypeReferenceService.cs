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
/// Reference service for KineticChainType that provides caching infrastructure
/// This service delegates actual business logic to IKineticChainTypeService
/// Entities stay within the service layer - only DTOs are exposed
/// </summary>
public class KineticChainTypeReferenceService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    IKineticChainTypeService kinetic_chain_typeService,
    ILogger<KineticChainTypeReferenceService> logger) :
    PureReferenceService<KineticChainType, ReferenceDataDto>(unitOfWorkProvider, cacheService, logger)
{
    private readonly IKineticChainTypeService _kinetic_chain_typeService = kinetic_chain_typeService;

    /// <summary>
    /// Loads all DTOs by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> LoadAllDtosAsync()
    {
        // Delegate to the actual service which handles entity-to-DTO mapping internally
        return await _kinetic_chain_typeService.GetAllActiveAsync();
    }
    
    /// <summary>
    /// Loads a DTO by ID by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<ReferenceDataDto>> LoadDtoByIdAsync(string id)
    {
        var kinetic_chain_typeId = KineticChainTypeId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(kinetic_chain_typeId, KineticChainTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await _kinetic_chain_typeService.GetByIdAsync(kinetic_chain_typeId)
            );
    }
    
    /// <summary>
    /// Validates and parses the ID using ServiceValidate fluent API
    /// </summary>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var kinetic_chain_typeId = KineticChainTypeId.ParseOrEmpty(id);
        
        return ServiceValidate.For()
            .EnsureNotEmpty(kinetic_chain_typeId, KineticChainTypeErrorMessages.InvalidIdFormat)
            .ToResult();
    }
}
