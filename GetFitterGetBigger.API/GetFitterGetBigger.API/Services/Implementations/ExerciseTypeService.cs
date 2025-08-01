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
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for exercise type operations
/// TEMPORARY: Extends EmptyEnabledPureReferenceService until all entities are migrated
/// </summary>
public class ExerciseTypeService : PureReferenceService<ExerciseType, ReferenceDataDto>, IExerciseTypeService
{
    public ExerciseTypeService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEternalCacheService cacheService,
        ILogger<ExerciseTypeService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(ExerciseTypeId id) => 
        id.IsEmpty 
            ? ServiceResult<ReferenceDataDto>.Failure(ReferenceDataDto.Empty, ServiceError.ValidationFailed(ExerciseTypeErrorMessages.InvalidIdFormat))
            : await GetByIdAsync(id.ToString());
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value) => 
        string.IsNullOrWhiteSpace(value)
            ? ServiceResult<ReferenceDataDto>.Failure(ReferenceDataDto.Empty, ServiceError.ValidationFailed(ExerciseTypeErrorMessages.ValueCannotBeEmpty))
            : await GetFromCacheOrLoadAsync(
                GetValueCacheKey(value),
                () => LoadByValueAsync(value),
                value);

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(ExerciseTypeId id) => 
        !id.IsEmpty && (await GetByIdAsync(id)).IsSuccess;
    
    /// <inheritdoc/>
    public override async Task<bool> ExistsAsync(string id) => 
        await ExistsAsync(ExerciseTypeId.ParseOrEmpty(id));

    /// <inheritdoc/>
    public async Task<bool> AllExistAsync(IEnumerable<string> ids)
    {
        foreach (var idString in ids)
        {
            var id = ExerciseTypeId.ParseOrEmpty(idString);
            if (id.IsEmpty || !(await ExistsAsync(id)))
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> AnyIsRestTypeAsync(IEnumerable<string> ids)
    {
        foreach (var idString in ids)
        {
            var id = ExerciseTypeId.ParseOrEmpty(idString);
            if (!id.IsEmpty)
            {
                var result = await GetByIdAsync(id);
                if (result.IsSuccess && result.Data.Value.ToLowerInvariant() == "rest")
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<ReferenceDataDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<ExerciseType>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        
        return await CacheLoad.For<ReferenceDataDto>(cacheService, cacheKey)
            .WithLogging(_logger, "ExerciseType")
            .MatchAsync(
                onHit: cached => ServiceResult<ReferenceDataDto>.Success(cached),
                onMiss: async () => await LoadAndProcessEntity(loadFunc, cacheKey, identifier)
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadAndProcessEntity(
        Func<Task<ExerciseType>> loadFunc,
        string cacheKey,
        string identifier)
    {
        var entity = await loadFunc();
        
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty, 
                ServiceError.NotFound(ExerciseTypeErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> CacheAndReturnSuccessAsync(string cacheKey, ReferenceDataDto dto)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }
    
    private async Task<ExerciseType> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        return await repository.GetByValueAsync(value) ?? ExerciseType.Empty;
    }
    
    protected override async Task<IEnumerable<ExerciseType>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    // Returns ExerciseType.Empty instead of null (Null Object Pattern)
    protected override async Task<ExerciseType> LoadEntityByIdAsync(string id)
    {
        var exerciseTypeId = ExerciseTypeId.ParseOrEmpty(id);
        if (exerciseTypeId.IsEmpty)
            return ExerciseType.Empty;
            
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        return await repository.GetByIdAsync(exerciseTypeId);
    }
    
    protected override ReferenceDataDto MapToDto(ExerciseType entity)
    {
        return new ReferenceDataDto
        {
            Id = entity.Id,
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
            return ValidationResult.Failure(ExerciseTypeErrorMessages.IdCannotBeEmpty);
        }
        
        // No additional validation - let the controller handle format validation
        // This allows empty GUIDs to pass through and be treated as NotFound
        
        // Valid format (including empty GUID) - let the database determine if it exists
        return ValidationResult.Success();
    }
}