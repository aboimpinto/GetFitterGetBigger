using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
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
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(ExerciseWeightTypeId id) => 
        id.IsEmpty 
            ? ServiceResult<ReferenceDataDto>.Failure(ReferenceDataDto.Empty, ServiceError.ValidationFailed(ExerciseWeightTypeErrorMessages.InvalidIdFormat))
            : await GetByIdAsync(id.ToString());
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value) => 
        string.IsNullOrWhiteSpace(value)
            ? ServiceResult<ReferenceDataDto>.Failure(ReferenceDataDto.Empty, ServiceError.ValidationFailed(ExerciseWeightTypeErrorMessages.ValueCannotBeEmpty))
            : await GetFromCacheOrLoadAsync(
                GetValueCacheKey(value),
                () => LoadByValueAsync(value),
                value);
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByCodeAsync(string code) => 
        string.IsNullOrWhiteSpace(code)
            ? ServiceResult<ReferenceDataDto>.Failure(ReferenceDataDto.Empty, ServiceError.ValidationFailed(ExerciseWeightTypeErrorMessages.CodeCannotBeEmpty))
            : await GetFromCacheOrLoadAsync(
                GetCodeCacheKey(code),
                () => LoadByCodeAsync(code),
                code);

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(ExerciseWeightTypeId id) => 
        !id.IsEmpty && (await GetByIdAsync(id)).IsSuccess;
    
    /// <inheritdoc/>
    public override async Task<bool> ExistsAsync(string id) => 
        await ExistsAsync(ExerciseWeightTypeId.ParseOrEmpty(id));
    
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
        var cacheResult = await cacheService.GetAsync<ReferenceDataDto>(cacheKey);
        if (cacheResult.IsHit)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return ServiceResult<ReferenceDataDto>.Success(cacheResult.Value);
        }
        
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
    
    // Returns ExerciseWeightType.Empty instead of null (Null Object Pattern)
    protected override async Task<ExerciseWeightType> LoadEntityByIdAsync(string id)
    {
        var exerciseWeightTypeId = ExerciseWeightTypeId.ParseOrEmpty(id);
        if (exerciseWeightTypeId.IsEmpty)
            return ExerciseWeightType.Empty;
            
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseWeightTypeRepository>();
        return await repository.GetByIdAsync(exerciseWeightTypeId);
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
        // This is called by the base class when using the string overload
        // Since we always use the typed overload from the controller,
        // this should validate the string format
        if (string.IsNullOrWhiteSpace(id))
        {
            return ValidationResult.Failure(ExerciseWeightTypeErrorMessages.IdCannotBeEmpty);
        }
        
        // No additional validation - let the controller handle format validation
        // This allows empty GUIDs to pass through and be treated as NotFound
        
        // Valid format (including empty GUID) - let the database determine if it exists
        return ValidationResult.Success();
    }
}