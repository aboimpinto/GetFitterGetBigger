using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Validation;
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
/// Service implementation for muscle role operations
/// </summary>
public class MuscleRoleService : PureReferenceService<MuscleRole, ReferenceDataDto>, IMuscleRoleService
{
    public MuscleRoleService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEternalCacheService cacheService,
        ILogger<MuscleRoleService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MuscleRoleId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, ServiceError.ValidationFailed(MuscleRoleErrorMessages.InvalidIdFormat))
            .MatchAsync(
                whenValid: async () => await GetByIdAsync(id.ToString())
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, ServiceError.ValidationFailed(MuscleRoleErrorMessages.ValueCannotBeEmpty))
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
        Func<Task<MuscleRole>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        
        return await CacheLoad.For<ReferenceDataDto>(cacheService, cacheKey)
            .WithLogging(_logger, "MuscleRole")
            .MatchAsync(
                onHit: cached => ServiceResult<ReferenceDataDto>.Success(cached),
                onMiss: async () => await LoadAndProcessEntity(loadFunc, cacheKey, identifier)
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadAndProcessEntity(
        Func<Task<MuscleRole>> loadFunc,
        string cacheKey,
        string identifier)
    {
        var entity = await loadFunc();
        
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<ReferenceDataDto>.Failure(
                ReferenceDataDto.Empty, 
                ServiceError.NotFound(MuscleRoleErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> CacheAndReturnSuccessAsync(string cacheKey, ReferenceDataDto dto)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }

    private async Task<MuscleRole> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        return await repository.GetByValueAsync(value);
    }
    
    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(MuscleRoleId id) => 
        await ExistsAsync(id.ToString());

    /// <inheritdoc/>
    protected override async Task<ServiceResult<MuscleRole>> LoadEntityByIdAsync(string id)
    {
        var muscleRoleId = MuscleRoleId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<MuscleRole>()
            .EnsureNotEmpty(muscleRoleId, ServiceError.InvalidFormat("MuscleRoleId", MuscleRoleErrorMessages.InvalidIdFormat))
            .Match(
                whenValid: async () => await LoadEntityFromRepository(muscleRoleId),
                whenInvalid: errors => ServiceResult<MuscleRole>.Failure(
                    MuscleRole.Empty,
                    ServiceError.ValidationFailed(errors.FirstOrDefault() ?? "Invalid ID format"))
            );
    }
    
    private async Task<ServiceResult<MuscleRole>> LoadEntityFromRepository(MuscleRoleId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity switch
        {
            { IsEmpty: true } => ServiceResult<MuscleRole>.Failure(
                MuscleRole.Empty, 
                ServiceError.NotFound("MuscleRole")),
            _ => ServiceResult<MuscleRole>.Success(entity)
        };
    }

    /// <inheritdoc/>
    protected override async Task<IEnumerable<MuscleRole>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        return await repository.GetAllActiveAsync();
    }

    /// <inheritdoc/>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var parsedId = MuscleRoleId.ParseOrEmpty(id);
        return ServiceValidate.For()
            .EnsureNotEmpty(parsedId, MuscleRoleErrorMessages.InvalidIdFormat)
            .ToResult();
    }

    /// <inheritdoc/>
    protected override ReferenceDataDto MapToDto(MuscleRole entity) =>
        new()
        {
            Id = entity.Id,
            Value = entity.Value,
            Description = entity.Description
        };

    protected override string GetCacheKeyPrefix() => "musclerole:";
}