using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Models.Validation;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
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
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MuscleRoleId id) => 
        id.IsEmpty 
            ? ServiceResult<ReferenceDataDto>.Failure(ReferenceDataDto.Empty, ServiceError.ValidationFailed(MuscleRoleErrorMessages.InvalidIdFormat))
            : await GetByIdAsync(id.ToString());
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value) => 
        string.IsNullOrWhiteSpace(value)
            ? ServiceResult<ReferenceDataDto>.Failure(ReferenceDataDto.Empty, ServiceError.ValidationFailed(MuscleRoleErrorMessages.ValueCannotBeEmpty))
            : await GetFromCacheOrLoadAsync(
                GetValueCacheKey(value),
                () => LoadByValueAsync(value),
                value);

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<ReferenceDataDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<MuscleRole>> loadFunc,
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
    protected override async Task<MuscleRole> LoadEntityByIdAsync(string id)
    {
        var muscleRoleId = MuscleRoleId.ParseOrEmpty(id);
        if (muscleRoleId.IsEmpty)
            return MuscleRole.Empty;
            
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        return await repository.GetByIdAsync(muscleRoleId);
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
        return parsedId.IsEmpty 
            ? ValidationResult.Failure(MuscleRoleErrorMessages.InvalidIdFormat) 
            : ValidationResult.Success();
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