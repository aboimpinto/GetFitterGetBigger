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
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByIdAsync(ExecutionProtocolId id)
    {
        return await ServiceValidate.For<ExecutionProtocolDto>()
            .EnsureNotEmpty(id, ExecutionProtocolErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await GetByIdAsync(id.ToString())
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ExecutionProtocolDto>()
            .EnsureNotWhiteSpace(value, ExecutionProtocolErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await GetFromCacheOrLoadAsync(
                    GetValueCacheKey(value),
                    () => LoadByValueAsync(value),
                    value)
            );
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByCodeAsync(string code) => 
        string.IsNullOrWhiteSpace(code)
            ? ServiceResult<ExecutionProtocolDto>.Failure(ExecutionProtocolDto.Empty, ServiceError.ValidationFailed(ExecutionProtocolErrorMessages.CodeCannotBeEmpty))
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
        
        return await CacheLoad.For<ExecutionProtocolDto>(cacheService, cacheKey)
            .WithLogging(_logger, "ExecutionProtocol")
            .MatchAsync(
                onHit: cached => ServiceResult<ExecutionProtocolDto>.Success(cached),
                onMiss: async () => await LoadAndProcessEntity(loadFunc, cacheKey, identifier)
            );
    }
    
    private async Task<ServiceResult<ExecutionProtocolDto>> LoadAndProcessEntity(
        Func<Task<ExecutionProtocol>> loadFunc,
        string cacheKey,
        string identifier)
    {
        var entity = await loadFunc();
        
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<ExecutionProtocolDto>.Failure(
                ExecutionProtocolDto.Empty, 
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
    public async Task<ServiceResult<bool>> ExistsAsync(ExecutionProtocolId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, ExecutionProtocolErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
            });
    }
    
    protected override async Task<IEnumerable<ExecutionProtocol>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        return await repository.GetAllActiveAsync();
    }
    
    /// <inheritdoc/>
    protected override async Task<ServiceResult<ExecutionProtocol>> LoadEntityByIdAsync(string id)
    {
        var executionProtocolId = ExecutionProtocolId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ExecutionProtocol>()
            .EnsureNotEmpty(executionProtocolId, ExecutionProtocolErrorMessages.InvalidIdFormat)
            .Match(
                whenValid: async () => await LoadEntityFromRepository(executionProtocolId),
                whenInvalid: errors => ServiceResult<ExecutionProtocol>.Failure(
                    ExecutionProtocol.Empty,
                    ServiceError.ValidationFailed(errors.FirstOrDefault() ?? "Invalid ID format"))
            );
    }
    
    private async Task<ServiceResult<ExecutionProtocol>> LoadEntityFromRepository(ExecutionProtocolId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity switch
        {
            { IsEmpty: true } => ServiceResult<ExecutionProtocol>.Failure(
                ExecutionProtocol.Empty, 
                ServiceError.NotFound("ExecutionProtocol")),
            _ => ServiceResult<ExecutionProtocol>.Success(entity)
        };
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
    
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        return ServiceValidate.For()
            .EnsureNotWhiteSpace(id, ExecutionProtocolErrorMessages.IdCannotBeEmpty)
            .ToResult();
    }
}