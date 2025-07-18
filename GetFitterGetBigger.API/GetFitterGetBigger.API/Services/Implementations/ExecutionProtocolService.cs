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
/// Service implementation for execution protocol operations
/// </summary>
public class ExecutionProtocolService : PureReferenceService<ExecutionProtocol, ExecutionProtocolDto>, IExecutionProtocolService
{
    public ExecutionProtocolService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        IEternalCacheService cacheService,
        ILogger<ExecutionProtocolService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ExecutionProtocolDto>>> GetAllActiveAsync()
    {
        return await GetAllAsync();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByIdAsync(ExecutionProtocolId id) => 
        id.IsEmpty 
            ? ServiceResult<ExecutionProtocolDto>.Failure(CreateEmptyDto(), ServiceError.ValidationFailed(ExecutionProtocolErrorMessages.InvalidIdFormat))
            : await GetByIdAsync(id.ToString());
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByValueAsync(string value) => 
        string.IsNullOrWhiteSpace(value)
            ? ServiceResult<ExecutionProtocolDto>.Failure(CreateEmptyDto(), ServiceError.ValidationFailed(ExecutionProtocolErrorMessages.ValueCannotBeEmpty))
            : await GetFromCacheOrLoadAsync(
                GetValueCacheKey(value),
                () => LoadByValueAsync(value),
                value);

    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByCodeAsync(string code) => 
        string.IsNullOrWhiteSpace(code)
            ? ServiceResult<ExecutionProtocolDto>.Failure(CreateEmptyDto(), ServiceError.ValidationFailed(ExecutionProtocolErrorMessages.CodeCannotBeEmpty))
            : await GetFromCacheOrLoadAsync(
                GetCodeCacheKey(code),
                () => LoadByCodeAsync(code),
                code);

    private string GetValueCacheKey(string value) => $"{GetCacheKeyPrefix()}value:{value}";
    private string GetCodeCacheKey(string code) => $"{GetCacheKeyPrefix()}code:{code.ToLowerInvariant()}";
    
    private async Task<ServiceResult<ExecutionProtocolDto>> GetFromCacheOrLoadAsync(
        string cacheKey, 
        Func<Task<ExecutionProtocol>> loadFunc,
        string identifier)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        var cacheResult = await cacheService.GetAsync<ExecutionProtocolDto>(cacheKey);
        if (cacheResult.IsHit)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return ServiceResult<ExecutionProtocolDto>.Success(cacheResult.Value);
        }
        
        var entity = await loadFunc();
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<ExecutionProtocolDto>.Failure(
                CreateEmptyDto(), 
                ServiceError.NotFound(ExecutionProtocolErrorMessages.NotFound, identifier)),
            _ => await CacheAndReturnSuccessAsync(cacheKey, MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<ExecutionProtocolDto>> CacheAndReturnSuccessAsync(string cacheKey, ExecutionProtocolDto dto)
    {
        var cacheService = (IEternalCacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<ExecutionProtocolDto>.Success(dto);
    }
    
    private async Task<ExecutionProtocol> LoadByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        return await repository.GetByValueAsync(value);
    }
    
    private async Task<ExecutionProtocol> LoadByCodeAsync(string code)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        return await repository.GetByCodeAsync(code);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(ExecutionProtocolId id) => 
        !id.IsEmpty && (await GetByIdAsync(id)).IsSuccess;
    
    /// <inheritdoc/>
    public override async Task<bool> ExistsAsync(string id) => 
        await ExistsAsync(ExecutionProtocolId.ParseOrEmpty(id));
    
    protected override async Task<IEnumerable<ExecutionProtocol>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    protected override async Task<ExecutionProtocol> LoadEntityByIdAsync(string id)
    {
        var executionProtocolId = ExecutionProtocolId.ParseOrEmpty(id);
        if (executionProtocolId.IsEmpty)
            return ExecutionProtocol.Empty;
            
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        return await repository.GetByIdAsync(executionProtocolId);
    }
    
    protected override ExecutionProtocolDto MapToDto(ExecutionProtocol entity)
    {
        return new ExecutionProtocolDto
        {
            ExecutionProtocolId = entity.ExecutionProtocolId.ToString(),
            Value = entity.Value,
            Description = entity.Description,
            Code = entity.Code,
            TimeBase = entity.TimeBase,
            RepBase = entity.RepBase,
            RestPattern = entity.RestPattern,
            IntensityLevel = entity.IntensityLevel,
            DisplayOrder = entity.DisplayOrder,
            IsActive = entity.IsActive
        };
    }
    
    protected override ExecutionProtocolDto CreateEmptyDto()
    {
        return new ExecutionProtocolDto
        {
            ExecutionProtocolId = string.Empty,
            Value = string.Empty,
            Description = null,
            Code = string.Empty,
            TimeBase = false,
            RepBase = false,
            RestPattern = null,
            IntensityLevel = null,
            DisplayOrder = 0,
            IsActive = false
        };
    }
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        // This is called by the base class when using the string overload
        // Since we always use the typed overload from the controller,
        // this should validate the string format
        if (string.IsNullOrWhiteSpace(id))
        {
            return ValidationResult.Failure(ExecutionProtocolErrorMessages.IdCannotBeEmpty);
        }
        
        // No additional validation - let the controller handle format validation
        return ValidationResult.Success();
    }
}