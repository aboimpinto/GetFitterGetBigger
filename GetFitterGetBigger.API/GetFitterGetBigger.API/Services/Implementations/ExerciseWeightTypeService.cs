using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for exercise weight type operations
/// </summary>
public class ExerciseWeightTypeService : PureReferenceService<ExerciseWeightType, ReferenceDataDto>, IExerciseWeightTypeService
{
    public ExerciseWeightTypeService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEternalCacheService cacheService,
        ILogger<ExerciseWeightTypeService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(ExerciseWeightTypeId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, ExerciseWeightTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await GetByIdAsync(id.ToString())
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, ExerciseWeightTypeErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await GetFromCacheOrLoadAsync(
                    GetValueCacheKey(value),
                    () => LoadByValueAsync(value),
                    value)
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByCodeAsync(string code) => 
        string.IsNullOrWhiteSpace(code)
            ? ServiceResult<ReferenceDataDto>.Failure(ReferenceDataDto.Empty, ServiceError.ValidationFailed(ExerciseWeightTypeErrorMessages.CodeCannotBeEmpty))
            : await GetFromCacheOrLoadAsync(
                GetCodeCacheKey(code),
                () => LoadByCodeAsync(code),
                code);

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
    public async Task<bool> IsValidWeightForTypeAsync(ExerciseWeightTypeId weightTypeId, decimal? weight)
    {
        if (weightTypeId.IsEmpty)
            return false;
            
        var result = await GetByIdAsync(weightTypeId);
        if (!result.IsSuccess)
            return false;
        
        // Need to load the entity to check the code
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
        var weightType = await repository.GetByIdAsync(weightTypeId);
        
        if (weightType.IsEmpty)
            return false;
        
        // Validate based on weight type code
        return weightType.Code switch
        {
            ExerciseWeightTypeCodes.BodyweightOnly => weight == null || weight == 0,
            ExerciseWeightTypeCodes.BodyweightOptional => true, // Any weight value is valid
            ExerciseWeightTypeCodes.WeightRequired => weight > 0,
            ExerciseWeightTypeCodes.MachineWeight => weight > 0,
            ExerciseWeightTypeCodes.NoWeight => weight == null || weight == 0,
            _ => false // Unknown weight type
        };
    }
    
    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    private string GetCodeCacheKey(string code) => $"{GetCacheKeyPrefix()}code:{code}";
    
    private async Task<ServiceResult<ReferenceDataDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<ExerciseWeightType>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        
        return await CacheLoad.For<ReferenceDataDto>(cacheService, cacheKey)
            .WithLogging(_logger, "ExerciseWeightType")
            .MatchAsync(
                onHit: cached => ServiceResult<ReferenceDataDto>.Success(cached),
                onMiss: async () => await LoadAndProcessEntity(loadFunc, cacheKey, identifier)
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadAndProcessEntity(
        Func<Task<ExerciseWeightType>> loadFunc,
        string cacheKey,
        string identifier)
    {
        var entity = await loadFunc();
        
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty, 
                ServiceError.NotFound(ExerciseWeightTypeErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> CacheAndReturnSuccessAsync(string cacheKey, ReferenceDataDto dto)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }
    
    private async Task<ExerciseWeightType> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
        return await repository.GetByValueAsync(value);
    }
    
    private async Task<ExerciseWeightType> LoadByCodeAsync(string code)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
        return await repository.GetByCodeAsync(code);
    }
    
    protected override async Task<IEnumerable<ExerciseWeightType>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    /// <inheritdoc/>
    protected override async Task<ServiceResult<ExerciseWeightType>> LoadEntityByIdAsync(string id)
    {
        var exerciseWeightTypeId = ExerciseWeightTypeId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ExerciseWeightType>()
            .EnsureNotEmpty(exerciseWeightTypeId, ExerciseWeightTypeErrorMessages.InvalidIdFormat)
            .Match(
                whenValid: async () => await LoadEntityFromRepository(exerciseWeightTypeId),
                whenInvalid: errors => ServiceResult<ExerciseWeightType>.Failure(
                    ExerciseWeightType.Empty,
                    ServiceError.ValidationFailed(errors.FirstOrDefault() ?? "Invalid ID format"))
            );
    }
    
    private async Task<ServiceResult<ExerciseWeightType>> LoadEntityFromRepository(ExerciseWeightTypeId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity switch
        {
            { IsEmpty: true } => ServiceResult<ExerciseWeightType>.Failure(
                ExerciseWeightType.Empty, 
                ServiceError.NotFound("ExerciseWeightType")),
            _ => ServiceResult<ExerciseWeightType>.Success(entity)
        };
    }
    
    protected override ReferenceDataDto MapToDto(ExerciseWeightType entity)
    {
        return new ReferenceDataDto
        {
            Id = entity.Id.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        return ServiceValidate.For()
            .EnsureNotWhiteSpace(id, ExerciseWeightTypeErrorMessages.IdCannotBeEmpty)
            .ToResult();
    }
}