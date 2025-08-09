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
/// Reference service for MuscleRole that provides caching infrastructure
/// This service delegates actual business logic to IMuscleRoleService
/// Entities stay within the service layer - only DTOs are exposed
/// </summary>
public class MuscleRoleReferenceService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    IMuscleRoleService muscle_roleService,
    ILogger<MuscleRoleReferenceService> logger) :
    PureReferenceService<MuscleRole, ReferenceDataDto>(unitOfWorkProvider, cacheService, logger)
{
    private readonly IMuscleRoleService _muscle_roleService = muscle_roleService;

    /// <summary>
    /// Loads all DTOs by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> LoadAllDtosAsync()
    {
        // Delegate to the actual service which handles entity-to-DTO mapping internally
        return await _muscle_roleService.GetAllActiveAsync();
    }
    
    /// <summary>
    /// Loads a DTO by ID by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<ReferenceDataDto>> LoadDtoByIdAsync(string id)
    {
        var muscle_roleId = MuscleRoleId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(muscle_roleId, MuscleRoleErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await _muscle_roleService.GetByIdAsync(muscle_roleId)
            );
    }
    
    /// <summary>
    /// Validates and parses the ID using ServiceValidate fluent API
    /// </summary>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var muscle_roleId = MuscleRoleId.ParseOrEmpty(id);
        
        return ServiceValidate.For()
            .EnsureNotEmpty(muscle_roleId, MuscleRoleErrorMessages.InvalidIdFormat)
            .ToResult();
    }
}
