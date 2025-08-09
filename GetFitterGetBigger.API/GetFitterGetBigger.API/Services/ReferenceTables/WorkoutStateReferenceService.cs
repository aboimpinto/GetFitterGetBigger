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
/// Reference service for WorkoutState that provides caching infrastructure
/// This service delegates actual business logic to IWorkoutStateService
/// Entities stay within the service layer - only DTOs are exposed
/// </summary>
public class WorkoutStateReferenceService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    IWorkoutStateService workout_stateService,
    ILogger<WorkoutStateReferenceService> logger) :
    PureReferenceService<WorkoutState, WorkoutStateDto>(unitOfWorkProvider, cacheService, logger)
{
    private readonly IWorkoutStateService _workout_stateService = workout_stateService;

    /// <summary>
    /// Loads all DTOs by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<IEnumerable<WorkoutStateDto>>> LoadAllDtosAsync()
    {
        // Delegate to the actual service which handles entity-to-DTO mapping internally
        return await _workout_stateService.GetAllAsync();
    }
    
    /// <summary>
    /// Loads a DTO by ID by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<WorkoutStateDto>> LoadDtoByIdAsync(string id)
    {
        var workout_stateId = WorkoutStateId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<WorkoutStateDto>()
            .EnsureNotEmpty(workout_stateId, WorkoutStateErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await _workout_stateService.GetByIdAsync(workout_stateId)
            );
    }
    
    /// <summary>
    /// Validates and parses the ID using ServiceValidate fluent API
    /// </summary>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var workout_stateId = WorkoutStateId.ParseOrEmpty(id);
        
        return ServiceValidate.For()
            .EnsureNotEmpty(workout_stateId, WorkoutStateErrorMessages.InvalidIdFormat)
            .ToResult();
    }
}
