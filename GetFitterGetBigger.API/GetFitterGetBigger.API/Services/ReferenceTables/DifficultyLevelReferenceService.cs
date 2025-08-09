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
/// Reference service for DifficultyLevel that provides caching infrastructure
/// This service delegates actual business logic to IDifficultyLevelService
/// Entities stay within the service layer - only DTOs are exposed
/// </summary>
public class DifficultyLevelReferenceService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    IDifficultyLevelService difficultyLevelService,
    ILogger<DifficultyLevelReferenceService> logger) :
    PureReferenceService<DifficultyLevel, ReferenceDataDto>(unitOfWorkProvider, cacheService, logger)
{
    private readonly IDifficultyLevelService _difficultyLevelService = difficultyLevelService;

    /// <summary>
    /// Loads all DTOs by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> LoadAllDtosAsync()
    {
        // Delegate to the actual service which handles entity-to-DTO mapping internally
        return await _difficultyLevelService.GetAllActiveAsync();
    }
    
    /// <summary>
    /// Loads a DTO by ID by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<ReferenceDataDto>> LoadDtoByIdAsync(string id)
    {
        var difficultyLevelId = DifficultyLevelId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(difficultyLevelId, DifficultyLevelErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await _difficultyLevelService.GetByIdAsync(difficultyLevelId)
            );
    }
    
    /// <summary>
    /// Validates and parses the ID using ServiceValidate fluent API
    /// </summary>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var difficultyLevelId = DifficultyLevelId.ParseOrEmpty(id);
        
        return ServiceValidate.For()
            .EnsureNotEmpty(difficultyLevelId, DifficultyLevelErrorMessages.InvalidIdFormat)
            .ToResult();
    }
}