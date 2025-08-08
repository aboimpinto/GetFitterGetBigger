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
/// Service implementation for movement pattern operations
/// Extends EmptyEnabledPureReferenceService for pure reference data with eternal caching
/// </summary>
public class MovementPatternService : PureReferenceService<MovementPattern, ReferenceDataDto>, IMovementPatternService
{
    public MovementPatternService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEternalCacheService cacheService,
        ILogger<MovementPatternService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MovementPatternId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, MovementPatternErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await GetByIdAsync(id.ToString())
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, MovementPatternErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await GetFromCacheOrLoadAsync(
                    GetValueCacheKey(value),
                    () => LoadByValueAsync(value),
                    value)
            );
    }

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<ReferenceDataDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<MovementPattern>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        
        return await CacheLoad.For<ReferenceDataDto>(cacheService, cacheKey)
            .WithLogging(_logger, "MovementPattern")
            .MatchAsync(
                onHit: cached => ServiceResult<ReferenceDataDto>.Success(cached),
                onMiss: async () => await LoadAndProcessEntity(loadFunc, cacheKey, identifier)
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadAndProcessEntity(
        Func<Task<MovementPattern>> loadFunc,
        string cacheKey,
        string identifier)
    {
        var entity = await loadFunc();
        
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty, 
                ServiceError.NotFound(MovementPatternErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> CacheAndReturnSuccessAsync(string cacheKey, ReferenceDataDto dto)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }
    
    private async Task<MovementPattern> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
        return await repository.GetByValueAsync(value);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<bool>> ExistsAsync(MovementPatternId id)
    {
        if (id.IsEmpty)
            return ServiceResult<bool>.Success(false);
            
        return await ServiceValidate.Build<bool>()
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
            });
    }
    
    protected override async Task<IEnumerable<MovementPattern>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    /// <inheritdoc/>
    protected override async Task<ServiceResult<MovementPattern>> LoadEntityByIdAsync(string id)
    {
        var movementPatternId = MovementPatternId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<MovementPattern>()
            .EnsureNotEmpty(movementPatternId, MovementPatternErrorMessages.InvalidIdFormat)
            .Match(
                whenValid: async () => await LoadEntityFromRepository(movementPatternId),
                whenInvalid: errors => ServiceResult<MovementPattern>.Failure(
                    MovementPattern.Empty,
                    ServiceError.ValidationFailed(errors.FirstOrDefault() ?? "Invalid ID format"))
            );
    }
    
    private async Task<ServiceResult<MovementPattern>> LoadEntityFromRepository(MovementPatternId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity switch
        {
            { IsEmpty: true } => ServiceResult<MovementPattern>.Failure(
                MovementPattern.Empty, 
                ServiceError.NotFound("MovementPattern")),
            _ => ServiceResult<MovementPattern>.Success(entity)
        };
    }
    
    protected override ReferenceDataDto MapToDto(MovementPattern entity)
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
            .EnsureNotWhiteSpace(id, MovementPatternErrorMessages.IdCannotBeEmpty)
            .ToResult();
    }
}