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
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(ExerciseTypeId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, ExerciseTypeErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await GetByIdAsync(id.ToString())
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, ExerciseTypeErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await GetFromCacheOrLoadAsync(
                    GetValueCacheKey(value),
                    () => LoadByValueAsync(value),
                    value)
            );
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
        foreach (var idString in ids)
        {
            var id = ExerciseTypeId.ParseOrEmpty(idString);
            if (id.IsEmpty || !(await ExistsAsync(id)).Data)
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
    
    /// <inheritdoc/>
    protected override async Task<ServiceResult<ExerciseType>> LoadEntityByIdAsync(string id)
    {
        var exerciseTypeId = ExerciseTypeId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ExerciseType>()
            .EnsureNotEmpty(exerciseTypeId, ExerciseTypeErrorMessages.InvalidIdFormat)
            .Match(
                whenValid: async () => await LoadEntityFromRepository(exerciseTypeId),
                whenInvalid: errors => ServiceResult<ExerciseType>.Failure(
                    ExerciseType.Empty,
                    ServiceError.ValidationFailed(errors.FirstOrDefault() ?? "Invalid ID format"))
            );
    }
    
    private async Task<ServiceResult<ExerciseType>> LoadEntityFromRepository(ExerciseTypeId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExerciseTypeRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity switch
        {
            { IsEmpty: true } => ServiceResult<ExerciseType>.Failure(
                ExerciseType.Empty, 
                ServiceError.NotFound("ExerciseType")),
            _ => ServiceResult<ExerciseType>.Success(entity)
        };
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
        return ServiceValidate.For()
            .EnsureNotWhiteSpace(id, ExerciseTypeErrorMessages.IdCannotBeEmpty)
            .ToResult();
    }
}