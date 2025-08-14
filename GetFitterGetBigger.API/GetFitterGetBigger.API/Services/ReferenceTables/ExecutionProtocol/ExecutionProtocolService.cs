using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.ExecutionProtocol.DataServices;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using CacheKeyGenerator = GetFitterGetBigger.API.Utilities.CacheKeyGenerator;

namespace GetFitterGetBigger.API.Services.ReferenceTables.ExecutionProtocol;

/// <summary>
/// Service implementation for execution protocol operations with integrated eternal caching
/// ExecutionProtocols are pure reference data that never changes after deployment
/// NO UnitOfWork here - all data access through IExecutionProtocolDataService
/// </summary>
public class ExecutionProtocolService(
    IExecutionProtocolDataService dataService,
    IEternalCacheService cacheService,
    ILogger<ExecutionProtocolService> logger) : IExecutionProtocolService
{
    private readonly IExecutionProtocolDataService _dataService = dataService;
    private readonly IEternalCacheService _cacheService = cacheService;
    private readonly ILogger<ExecutionProtocolService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ExecutionProtocolDto>>> GetAllActiveAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey("ExecutionProtocols");
        
        return await CacheLoad.For<IEnumerable<ExecutionProtocolDto>>(_cacheService, cacheKey)
            .WithLogging(_logger, "ExecutionProtocols")
            .WithAutoCacheAsync(async () => await _dataService.GetAllActiveAsync());
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
                            var result = await _dataService.GetByIdAsync(id);
                            // Convert Empty to NotFound at the service layer
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
                            var result = await _dataService.GetByValueAsync(value);
                            // Convert Empty to NotFound at the service layer
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
                            var result = await _dataService.GetByCodeAsync(code);
                            // Convert Empty to NotFound at the service layer
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
}