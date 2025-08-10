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
/// Service implementation for exercise_type operations
/// This service focuses solely on business logic and data access
/// Caching is handled by the wrapping ExerciseTypeReferenceService layer
/// </summary>
public class ExerciseTypeService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<ExerciseTypeService> logger) : IExerciseTypeService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<ExerciseTypeService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active exercise_types", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(ExerciseTypeId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, ExerciseTypeErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(id))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("ExerciseType", id.ToString()))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Gets a exercise_type by its ID string
    /// </summary>
    /// <param name="id">The exercise_type ID as a string</param>
    /// <returns>A service result containing the exercise_type if found</returns>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        var exercise_typeId = ExerciseTypeId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(exercise_typeId, ExerciseTypeErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(exercise_typeId))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("ExerciseType", id))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromDatabaseAsync(ExerciseTypeId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        // Clean separation: business logic only
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, ExerciseTypeErrorMessages.ValueCannotBeEmpty)
            .WithServiceResultAsync(() => LoadByValueFromDatabaseAsync(value))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("ExerciseType", value))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        // Clean separation: business logic only
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<bool>> ExistsAsync(ExerciseTypeId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, ExerciseTypeErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
            });
    }
    
    /// <inheritdoc/>
    public async Task<bool> AllExistAsync(IEnumerable<string> ids)
    {
        // Parse IDs
        var exerciseTypeIds = ids.Select(id => ExerciseTypeId.ParseOrEmpty(id))
                                 .Where(id => !id.IsEmpty)
                                 .ToList();
        
        // If any ID failed to parse, not all exist
        if (exerciseTypeIds.Count != ids.Count())
            return false;
            
        // Check if all exist
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        
        foreach (var id in exerciseTypeIds)
        {
            var exists = await repository.ExistsAsync(id);
            if (!exists)
                return false;
        }
        
        return true;
    }
    
    /// <inheritdoc/>
    public Task<bool> AnyIsRestTypeAsync(IEnumerable<string> ids)
    {
        // The REST exercise type has a fixed ID that never changes
        const string REST_EXERCISE_TYPE_ID = "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a";
        
        // Simply check if any of the provided IDs match the REST type ID
        return Task.FromResult(ids.Any(id => string.Equals(id, REST_EXERCISE_TYPE_ID, StringComparison.OrdinalIgnoreCase)));
    }
    
    /// <summary>
    /// Maps a ExerciseType entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ReferenceDataDto MapToDto(ExerciseType entity)
    {
        return new ReferenceDataDto
        {
            Id = entity.ExerciseTypeId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
