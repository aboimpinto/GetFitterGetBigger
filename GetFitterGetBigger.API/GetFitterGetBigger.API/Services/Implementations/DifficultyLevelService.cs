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
/// Service implementation for difficulty level operations
/// TEMPORARY: Extends EmptyEnabledPureReferenceService until all entities are migrated
/// </summary>
public class DifficultyLevelService : EmptyEnabledPureReferenceService<DifficultyLevel, ReferenceDataDto>, IDifficultyLevelService
{
    public DifficultyLevelService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEmptyEnabledCacheService cacheService,
        ILogger<DifficultyLevelService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(DifficultyLevelId id) => 
        id.IsEmpty 
            ? ServiceResult<ReferenceDataDto>.Failure(CreateEmptyDto(), ServiceError.ValidationFailed(DifficultyLevelErrorMessages.InvalidIdFormat))
            : await GetByIdAsync(id.ToString());
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value) => 
        string.IsNullOrWhiteSpace(value)
            ? ServiceResult<ReferenceDataDto>.Failure(CreateEmptyDto(), ServiceError.ValidationFailed(DifficultyLevelErrorMessages.ValueCannotBeEmpty))
            : await GetFromCacheOrLoadAsync(
                GetValueCacheKey(value),
                () => LoadByValueAsync(value),
                value);

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    
    private async Task<ServiceResult<ReferenceDataDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<DifficultyLevel>> loadFunc,
        string identifier)
    {
        var cacheService = (IEmptyEnabledCacheService)_cacheService;
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
                CreateEmptyDto(), 
                ServiceError.NotFound(DifficultyLevelErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> CacheAndReturnSuccessAsync(string cacheKey, ReferenceDataDto dto)
    {
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromDays(365));
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }
    
    private async Task<DifficultyLevel> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        return await repository.GetByValueAsync(value);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(DifficultyLevelId id) => 
        !id.IsEmpty && (await GetByIdAsync(id)).IsSuccess;
    
    /// <inheritdoc/>
    public override async Task<bool> ExistsAsync(string id) => 
        await ExistsAsync(DifficultyLevelId.ParseOrEmpty(id));
    
    protected override async Task<IEnumerable<DifficultyLevel>> LoadAllEntitiesAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var repository = unitOfWork.GetRepository<IDifficultyLevelRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    // Returns DifficultyLevel.Empty instead of null (Null Object Pattern)
    protected override async Task<DifficultyLevel> LoadEntityByIdAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string id) =>
        DifficultyLevelId.ParseOrEmpty(id) switch
        {
            { IsEmpty: true } => DifficultyLevel.Empty,
            var difficultyLevelId => await unitOfWork.GetRepository<IDifficultyLevelRepository>().GetByIdAsync(difficultyLevelId)
        };
    
    protected override ReferenceDataDto MapToDto(DifficultyLevel entity)
    {
        return new ReferenceDataDto
        {
            Id = entity.DifficultyLevelId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
    
    protected override ReferenceDataDto CreateEmptyDto()
    {
        return new ReferenceDataDto();
    }
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        // This is called by the base class when using the string overload
        // Since we always use the typed overload from the controller,
        // this should validate the string format
        if (string.IsNullOrWhiteSpace(id))
        {
            return ValidationResult.Failure("ID cannot be empty");
        }
        
        // No additional validation - let the controller handle format validation
        // This allows empty GUIDs to pass through and be treated as NotFound
        
        // Valid format (including empty GUID) - let the database determine if it exists
        return ValidationResult.Success();
    }
}