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
/// Service implementation for exercise_weight_type operations
/// This service focuses solely on business logic and data access
/// Caching is handled by the wrapping ExerciseWeightTypeReferenceService layer
/// </summary>
public class ExerciseWeightTypeService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<ExerciseWeightTypeService> logger) : IExerciseWeightTypeService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<ExerciseWeightTypeService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active exercise_weight_types", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(ExerciseWeightTypeId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, ExerciseWeightTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(id)
            );
    }
    
    /// <summary>
    /// Gets a exercise_weight_type by its ID string
    /// </summary>
    /// <param name="id">The exercise_weight_type ID as a string</param>
    /// <returns>A service result containing the exercise_weight_type if found</returns>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        var exercise_weight_typeId = ExerciseWeightTypeId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(exercise_weight_typeId, ExerciseWeightTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(exercise_weight_typeId)
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromDatabaseAsync(ExerciseWeightTypeId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return (entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty,
                ServiceError.NotFound("ExerciseWeightType", id.ToString())),
            false => ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
        };
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, ExerciseWeightTypeErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await LoadByValueFromDatabaseAsync(value)
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        return (entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty,
                ServiceError.NotFound("ExerciseWeightType", value)),
            false => ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
        };
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<bool>> ExistsAsync(ExerciseWeightTypeId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, ExerciseWeightTypeErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
            });
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByCodeAsync(string code)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(code, ExerciseWeightTypeErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await LoadByCodeFromDatabaseAsync(code)
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByCodeFromDatabaseAsync(string code)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
        var entity = await repository.GetByCodeAsync(code);
        
        return (entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty,
                ServiceError.NotFound("ExerciseWeightType", code)),
            false => ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
        };
    }
    
    /// <inheritdoc/>
    public async Task<bool> IsValidWeightForTypeAsync(ExerciseWeightTypeId weightTypeId, decimal? weight)
    {
        // Early return for invalid ID
        if (weightTypeId.IsEmpty)
            return false;
            
        // Get the weight type to check its rules
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
        var weightType = await repository.GetByIdAsync(weightTypeId);
        
        if (weightType == null || weightType.IsEmpty)
            return false;
            
        // Check weight based on type code
        return weightType.Code switch
        {
            ExerciseWeightTypeCodes.BodyweightOnly => weight == null || weight == 0,
            ExerciseWeightTypeCodes.BodyweightOptional => true, // Any weight is valid
            ExerciseWeightTypeCodes.WeightRequired => weight > 0,
            ExerciseWeightTypeCodes.MachineWeight => weight > 0,
            ExerciseWeightTypeCodes.NoWeight => weight == null || weight == 0,
            _ => false
        };
    }
    
    /// <summary>
    /// Maps a ExerciseWeightType entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ReferenceDataDto MapToDto(ExerciseWeightType entity)
    {
        return new ReferenceDataDto
        {
            Id = entity.Id.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
