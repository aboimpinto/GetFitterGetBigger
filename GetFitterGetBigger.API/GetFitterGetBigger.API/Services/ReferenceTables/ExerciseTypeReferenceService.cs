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
/// Reference service for ExerciseType that provides caching infrastructure
/// This service delegates actual business logic to IExerciseTypeService
/// Entities stay within the service layer - only DTOs are exposed
/// </summary>
public class ExerciseTypeReferenceService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    IExerciseTypeService exercise_typeService,
    ILogger<ExerciseTypeReferenceService> logger) :
    PureReferenceService<ExerciseType, ReferenceDataDto>(unitOfWorkProvider, cacheService, logger)
{
    private readonly IExerciseTypeService _exercise_typeService = exercise_typeService;

    /// <summary>
    /// Loads all DTOs by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> LoadAllDtosAsync()
    {
        // Delegate to the actual service which handles entity-to-DTO mapping internally
        return await _exercise_typeService.GetAllActiveAsync();
    }
    
    /// <summary>
    /// Loads a DTO by ID by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<ReferenceDataDto>> LoadDtoByIdAsync(string id)
    {
        var exercise_typeId = ExerciseTypeId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(exercise_typeId, ExerciseTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await _exercise_typeService.GetByIdAsync(exercise_typeId)
            );
    }
    
    /// <summary>
    /// Validates and parses the ID using ServiceValidate fluent API
    /// </summary>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var exercise_typeId = ExerciseTypeId.ParseOrEmpty(id);
        
        return ServiceValidate.For()
            .EnsureNotEmpty(exercise_typeId, ExerciseTypeErrorMessages.InvalidIdFormat)
            .ToResult();
    }
}
