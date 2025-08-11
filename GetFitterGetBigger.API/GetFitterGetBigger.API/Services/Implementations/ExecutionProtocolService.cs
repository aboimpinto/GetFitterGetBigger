using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;
using CacheKeyGenerator = GetFitterGetBigger.API.Utilities.CacheKeyGenerator;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for execution protocol operations with integrated eternal caching
/// ExecutionProtocols are pure reference data that never changes after deployment
/// </summary>
public class ExecutionProtocolService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    ILogger<ExecutionProtocolService> logger) : IExecutionProtocolService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly IEternalCacheService _cacheService = cacheService;
    private readonly ILogger<ExecutionProtocolService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ExecutionProtocolDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("ExecutionProtocols");
        
        return await CacheLoad.For<IEnumerable<ExecutionProtocolDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "ExecutionProtocols")
            .WithAutoCacheAsync(LoadAllActiveFromDatabaseAsync);
    }
    
    /// <summary>
    /// Loads all active ExecutionProtocols from the database and maps to DTOs
    /// </summary>
    private async Task<ServiceResult<IEnumerable<ExecutionProtocolDto>>> LoadAllActiveFromDatabaseAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active execution protocols", dtos.Count);
        return ServiceResult<IEnumerable<ExecutionProtocolDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByIdAsync(ExecutionProtocolId id)
    {
        return await ServiceValidate.For<ExecutionProtocolDto>()
            .EnsureNotEmpty(id, ExecutionProtocolErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByIdKey("ExecutionProtocols", id.ToString());
                    
                    return await CacheLoad.For<ExecutionProtocolDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "ExecutionProtocol")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByIdFromDatabaseAsync(id);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ExecutionProtocolDto>.Failure(
                                    ExecutionProtocolDto.Empty,
                                    ServiceError.NotFound("ExecutionProtocol", id.ToString()));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Gets an execution protocol by its ID string
    /// </summary>
    /// <param name="id">The execution protocol ID as a string</param>
    /// <returns>A service result containing the execution protocol if found</returns>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByIdAsync(string id)
    {
        var executionProtocolId = ExecutionProtocolId.ParseOrEmpty(id);
        return await GetByIdAsync(executionProtocolId);
    }
    
    /// <summary>
    /// Loads an ExecutionProtocol by ID from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ExecutionProtocolDto>> LoadByIdFromDatabaseAsync(ExecutionProtocolId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<ExecutionProtocolDto>.Success(MapToDto(entity))
            : ServiceResult<ExecutionProtocolDto>.Success(ExecutionProtocolDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ExecutionProtocolDto>()
            .EnsureNotWhiteSpace(value, ExecutionProtocolErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByValueKey("ExecutionProtocols", value);
                    
                    return await CacheLoad.For<ExecutionProtocolDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "ExecutionProtocol")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByValueFromDatabaseAsync(value);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ExecutionProtocolDto>.Failure(
                                    ExecutionProtocolDto.Empty,
                                    ServiceError.NotFound("ExecutionProtocol", value));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Loads an ExecutionProtocol by value from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ExecutionProtocolDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        return entity.IsActive
            ? ServiceResult<ExecutionProtocolDto>.Success(MapToDto(entity))
            : ServiceResult<ExecutionProtocolDto>.Success(ExecutionProtocolDto.Empty);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExecutionProtocolId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, ExecutionProtocolErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    // Leverage the GetById cache for existence checks
                    var result = await GetByIdAsync(id);
                    return ServiceResult<BooleanResultDto>.Success(
                        BooleanResultDto.Create(result.IsSuccess && !result.Data.IsEmpty)
                    );
                }
            );
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByCodeAsync(string code)
    {
        return await ServiceValidate.For<ExecutionProtocolDto>()
            .EnsureNotWhiteSpace(code, ExecutionProtocolErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () =>
                {
                    var cacheKey = CacheKeyGenerator.GetByCodeKey("ExecutionProtocols", code);
                    
                    return await CacheLoad.For<ExecutionProtocolDto>(_cacheService, cacheKey)
                        .WithLogging(_logger, "ExecutionProtocol")
                        .WithAutoCacheAsync(async () =>
                        {
                            var result = await LoadByCodeFromDatabaseAsync(code);
                            // Convert Empty to NotFound at the API layer
                            if (result.IsSuccess && result.Data.IsEmpty)
                            {
                                return ServiceResult<ExecutionProtocolDto>.Failure(
                                    ExecutionProtocolDto.Empty,
                                    ServiceError.NotFound("ExecutionProtocol", code));
                            }
                            return result;
                        });
                }
            );
    }
    
    /// <summary>
    /// Loads an ExecutionProtocol by code from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<ExecutionProtocolDto>> LoadByCodeFromDatabaseAsync(string code)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entity = await repository.GetByCodeAsync(code);
        
        return entity.IsActive
            ? ServiceResult<ExecutionProtocolDto>.Success(MapToDto(entity))
            : ServiceResult<ExecutionProtocolDto>.Success(ExecutionProtocolDto.Empty);
    }
    
    /// <summary>
    /// Maps an ExecutionProtocol entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ExecutionProtocolDto MapToDto(ExecutionProtocol entity)
    {
        if (entity.IsEmpty)
            return ExecutionProtocolDto.Empty;
            
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
}