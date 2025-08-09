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
/// Reference service for MovementPattern that provides caching infrastructure
/// This service delegates actual business logic to IMovementPatternService
/// Entities stay within the service layer - only DTOs are exposed
/// </summary>
public class MovementPatternReferenceService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    IMovementPatternService movement_patternService,
    ILogger<MovementPatternReferenceService> logger) :
    PureReferenceService<MovementPattern, ReferenceDataDto>(unitOfWorkProvider, cacheService, logger)
{
    private readonly IMovementPatternService _movement_patternService = movement_patternService;

    /// <summary>
    /// Loads all DTOs by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> LoadAllDtosAsync()
    {
        // Delegate to the actual service which handles entity-to-DTO mapping internally
        return await _movement_patternService.GetAllActiveAsync();
    }
    
    /// <summary>
    /// Loads a DTO by ID by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<ReferenceDataDto>> LoadDtoByIdAsync(string id)
    {
        var movement_patternId = MovementPatternId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(movement_patternId, MovementPatternErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await _movement_patternService.GetByIdAsync(movement_patternId)
            );
    }
    
    /// <summary>
    /// Validates and parses the ID using ServiceValidate fluent API
    /// </summary>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var movement_patternId = MovementPatternId.ParseOrEmpty(id);
        
        return ServiceValidate.For()
            .EnsureNotEmpty(movement_patternId, MovementPatternErrorMessages.InvalidIdFormat)
            .ToResult();
    }
}
