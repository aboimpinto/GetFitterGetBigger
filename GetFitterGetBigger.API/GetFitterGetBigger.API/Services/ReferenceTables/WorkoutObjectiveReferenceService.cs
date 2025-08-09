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
/// Reference service for WorkoutObjective that provides caching infrastructure
/// This service delegates actual business logic to IWorkoutObjectiveService
/// Entities stay within the service layer - only DTOs are exposed
/// </summary>
public class WorkoutObjectiveReferenceService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    IWorkoutObjectiveService workout_objectiveService,
    ILogger<WorkoutObjectiveReferenceService> logger) :
    PureReferenceService<WorkoutObjective, ReferenceDataDto>(unitOfWorkProvider, cacheService, logger)
{
    private readonly IWorkoutObjectiveService _workout_objectiveService = workout_objectiveService;

    /// <summary>
    /// Loads all DTOs by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> LoadAllDtosAsync()
    {
        // Delegate to the actual service which handles entity-to-DTO mapping internally
        return await _workout_objectiveService.GetAllActiveAsync();
    }
    
    /// <summary>
    /// Loads a DTO by ID by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<ReferenceDataDto>> LoadDtoByIdAsync(string id)
    {
        var workout_objectiveId = WorkoutObjectiveId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(workout_objectiveId, WorkoutObjectiveErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await _workout_objectiveService.GetByIdAsync(workout_objectiveId)
            );
    }
    
    /// <summary>
    /// Validates and parses the ID using ServiceValidate fluent API
    /// </summary>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var workout_objectiveId = WorkoutObjectiveId.ParseOrEmpty(id);
        
        return ServiceValidate.For()
            .EnsureNotEmpty(workout_objectiveId, WorkoutObjectiveErrorMessages.InvalidIdFormat)
            .ToResult();
    }
}
