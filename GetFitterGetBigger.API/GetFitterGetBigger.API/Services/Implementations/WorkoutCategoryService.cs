using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for workout_category operations
/// This service focuses solely on business logic and data access
/// Caching is handled by the wrapping WorkoutCategoryReferenceService layer
/// </summary>
public class WorkoutCategoryService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<WorkoutCategoryService> logger) : IWorkoutCategoryService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<WorkoutCategoryService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<WorkoutCategoryDto>>> GetAllAsync()
    {
        return await GetAllActiveAsync();
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<WorkoutCategoryDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active workout_categorys", dtos.Count);
        return ServiceResult<IEnumerable<WorkoutCategoryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutCategoryDto>> GetByIdAsync(WorkoutCategoryId id)
    {
        return await ServiceValidate.For<WorkoutCategoryDto>()
            .EnsureNotEmpty(id, WorkoutCategoryErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(id)
            );
    }
    
    /// <summary>
    /// Gets a workout_category by its ID string
    /// </summary>
    /// <param name="id">The workout_category ID as a string</param>
    /// <returns>A service result containing the workout_category if found</returns>
    public async Task<ServiceResult<WorkoutCategoryDto>> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return ServiceResult<WorkoutCategoryDto>.Failure(
                WorkoutCategoryDto.Empty,
                ServiceError.ValidationFailed(WorkoutCategoryErrorMessages.IdCannotBeEmpty));
        }
        
        var workout_categoryId = WorkoutCategoryId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<WorkoutCategoryDto>()
            .EnsureNotEmpty(workout_categoryId, WorkoutCategoryErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(workout_categoryId)
            );
    }
    
    private async Task<ServiceResult<WorkoutCategoryDto>> LoadByIdFromDatabaseAsync(WorkoutCategoryId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return (entity == null || entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<WorkoutCategoryDto>.Failure(
                WorkoutCategoryDto.Empty,
                ServiceError.NotFound("WorkoutCategory", id.ToString())),
            false => ServiceResult<WorkoutCategoryDto>.Success(MapToDto(entity))
        };
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<WorkoutCategoryDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<WorkoutCategoryDto>()
            .EnsureNotWhiteSpace(value, WorkoutCategoryErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await LoadByValueFromDatabaseAsync(value)
            );
    }
    
    private async Task<ServiceResult<WorkoutCategoryDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        return (entity == null || entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<WorkoutCategoryDto>.Failure(
                WorkoutCategoryDto.Empty,
                ServiceError.NotFound("WorkoutCategory", value)),
            false => ServiceResult<WorkoutCategoryDto>.Success(MapToDto(entity))
        };
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<bool>> ExistsAsync(WorkoutCategoryId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, WorkoutCategoryErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IWorkoutCategoryRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
            });
    }
    
    /// <summary>
    /// Maps a WorkoutCategory entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private WorkoutCategoryDto MapToDto(WorkoutCategory entity)
    {
        return new WorkoutCategoryDto
        {
            WorkoutCategoryId = entity.WorkoutCategoryId.ToString(),
            Value = entity.Value,
            Description = entity.Description,
            Icon = entity.Icon,
            Color = entity.Color,
            PrimaryMuscleGroups = entity.PrimaryMuscleGroups,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive
        };
    }
}
