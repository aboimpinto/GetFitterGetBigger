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
/// Reference service for WorkoutCategory that provides caching infrastructure
/// This service delegates actual business logic to IWorkoutCategoryService
/// Entities stay within the service layer - only DTOs are exposed
/// </summary>
public class WorkoutCategoryReferenceService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    IWorkoutCategoryService workout_categoryService,
    ILogger<WorkoutCategoryReferenceService> logger) :
    PureReferenceService<WorkoutCategory, WorkoutCategoryDto>(unitOfWorkProvider, cacheService, logger)
{
    private readonly IWorkoutCategoryService _workout_categoryService = workout_categoryService;

    /// <summary>
    /// Loads all DTOs by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<IEnumerable<WorkoutCategoryDto>>> LoadAllDtosAsync()
    {
        // Delegate to the actual service which handles entity-to-DTO mapping internally
        return await _workout_categoryService.GetAllAsync();
    }
    
    /// <summary>
    /// Loads a DTO by ID by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<WorkoutCategoryDto>> LoadDtoByIdAsync(string id)
    {
        var workout_categoryId = WorkoutCategoryId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<WorkoutCategoryDto>()
            .EnsureNotEmpty(workout_categoryId, WorkoutCategoryErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await _workout_categoryService.GetByIdAsync(workout_categoryId)
            );
    }
    
    /// <summary>
    /// Validates and parses the ID using ServiceValidate fluent API
    /// </summary>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var workout_categoryId = WorkoutCategoryId.ParseOrEmpty(id);
        
        return ServiceValidate.For()
            .EnsureNotEmpty(workout_categoryId, WorkoutCategoryErrorMessages.InvalidIdFormat)
            .ToResult();
    }
}
