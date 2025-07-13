using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Utilities;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;
using GetFitterGetBigger.API.Models;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for managing execution protocols reference data
/// </summary>
public class ExecutionProtocolService : IExecutionProtocolService
{
    private const string CacheKeyPrefix = "ExecutionProtocols";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ExecutionProtocolService> _logger;
    
    public ExecutionProtocolService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger<ExecutionProtocolService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider ?? throw new ArgumentNullException(nameof(unitOfWorkProvider));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<ExecutionProtocol>> GetAllAsync()
    {
        var cacheKey = CacheKeyGenerator.GetAllKey(CacheKeyPrefix);
        var cached = await _cacheService.GetAsync<IEnumerable<ExecutionProtocol>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var protocols = await repository.GetAllActiveAsync();
        var protocolList = protocols.ToList();
        
        await _cacheService.SetAsync(cacheKey, protocolList, CacheDuration);
        _logger.LogDebug("Cached {Count} execution protocols with key: {CacheKey}", protocolList.Count, cacheKey);
        
        return protocolList;
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<ExecutionProtocolDto>> GetAllAsDtosAsync()
    {
        var protocols = await GetAllAsync();
        return protocols.Select(ep => new ExecutionProtocolDto
        {
            ExecutionProtocolId = ep.Id.ToString(),
            Value = ep.Value,
            Description = ep.Description,
            Code = ep.Code,
            TimeBase = ep.TimeBase,
            RepBase = ep.RepBase,
            RestPattern = ep.RestPattern,
            IntensityLevel = ep.IntensityLevel,
            DisplayOrder = ep.DisplayOrder,
            IsActive = ep.IsActive
        });
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<ExecutionProtocolDto>> GetAllAsExecutionProtocolDtosAsync(bool includeInactive = false)
    {
        var cacheKey = CacheKeyGenerator.GetAllKey($"{CacheKeyPrefix}_{(includeInactive ? "All" : "Active")}");
        var cached = await _cacheService.GetAsync<IEnumerable<ExecutionProtocolDto>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        
        var protocols = includeInactive 
            ? await repository.GetAllAsync() 
            : await repository.GetAllActiveAsync();
            
        var dtoList = protocols
            .OrderBy(ep => ep.DisplayOrder)
            .Select(ep => new ExecutionProtocolDto
            {
                ExecutionProtocolId = ep.Id.ToString(),
                Value = ep.Value,
                Description = ep.Description,
                Code = ep.Code,
                TimeBase = ep.TimeBase,
                RepBase = ep.RepBase,
                RestPattern = ep.RestPattern,
                IntensityLevel = ep.IntensityLevel,
                DisplayOrder = ep.DisplayOrder,
                IsActive = ep.IsActive
            })
            .ToList();
        
        await _cacheService.SetAsync(cacheKey, dtoList, CacheDuration);
        _logger.LogDebug("Cached {Count} execution protocol DTOs with key: {CacheKey}", dtoList.Count, cacheKey);
        
        return dtoList;
    }
    
    /// <inheritdoc />
    public async Task<ExecutionProtocol?> GetByIdAsync(ExecutionProtocolId id)
    {
        if (id.IsEmpty)
        {
            _logger.LogWarning("Attempted to get execution protocol with empty ID");
            return null;
        }
        
        var cacheKey = CacheKeyGenerator.GetByIdKey(CacheKeyPrefix, id.ToString());
        var cached = await _cacheService.GetAsync<ExecutionProtocol>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var protocol = await repository.GetByIdAsync(id);
        
        if (protocol != null && protocol.IsActive)
        {
            await _cacheService.SetAsync(cacheKey, protocol, CacheDuration);
            _logger.LogDebug("Cached execution protocol with key: {CacheKey}", cacheKey);
        }
        
        return protocol?.IsActive == true ? protocol : null;
    }
    
    /// <inheritdoc />
    public async Task<ExecutionProtocolDto?> GetByIdAsDtoAsync(string id)
    {
        var protocolId = ExecutionProtocolId.From(id);
        if (protocolId.IsEmpty)
        {
            _logger.LogWarning("Invalid execution protocol ID format: {Id}", id);
            return null;
        }
        
        var protocol = await GetByIdAsync(protocolId);
        if (protocol == null) return null;
        
        return new ExecutionProtocolDto
        {
            ExecutionProtocolId = protocol.Id.ToString(),
            Value = protocol.Value,
            Description = protocol.Description,
            Code = protocol.Code,
            TimeBase = protocol.TimeBase,
            RepBase = protocol.RepBase,
            RestPattern = protocol.RestPattern,
            IntensityLevel = protocol.IntensityLevel,
            DisplayOrder = protocol.DisplayOrder,
            IsActive = protocol.IsActive
        };
    }
    
    /// <inheritdoc />
    public async Task<ExecutionProtocolDto?> GetByIdAsExecutionProtocolDtoAsync(string id, bool includeInactive = false)
    {
        var protocolId = ExecutionProtocolId.From(id);
        if (protocolId.IsEmpty)
        {
            _logger.LogWarning("Invalid execution protocol ID format: {Id}", id);
            return null;
        }
        
        var cacheKey = CacheKeyGenerator.GetByIdKey($"{CacheKeyPrefix}_{(includeInactive ? "All" : "Active")}", id);
        var cached = await _cacheService.GetAsync<ExecutionProtocolDto>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var protocol = await repository.GetByIdAsync(protocolId);
        
        if (protocol == null || (!includeInactive && !protocol.IsActive))
        {
            return null;
        }
        
        var dto = new ExecutionProtocolDto
        {
            ExecutionProtocolId = protocol.Id.ToString(),
            Value = protocol.Value,
            Description = protocol.Description,
            Code = protocol.Code,
            TimeBase = protocol.TimeBase,
            RepBase = protocol.RepBase,
            RestPattern = protocol.RestPattern,
            IntensityLevel = protocol.IntensityLevel,
            DisplayOrder = protocol.DisplayOrder,
            IsActive = protocol.IsActive
        };
        
        await _cacheService.SetAsync(cacheKey, dto, CacheDuration);
        _logger.LogDebug("Cached execution protocol DTO with key: {CacheKey}", cacheKey);
        
        return dto;
    }
    
    /// <inheritdoc />
    public async Task<ExecutionProtocol?> GetByValueAsync(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _logger.LogWarning("Attempted to get execution protocol with empty value");
            return null;
        }
        
        var cacheKey = CacheKeyGenerator.GetByValueKey(CacheKeyPrefix, value);
        var cached = await _cacheService.GetAsync<ExecutionProtocol>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var protocol = await repository.GetByValueAsync(value);
        
        if (protocol != null)
        {
            await _cacheService.SetAsync(cacheKey, protocol, CacheDuration);
            _logger.LogDebug("Cached execution protocol with key: {CacheKey}", cacheKey);
        }
        
        return protocol;
    }
    
    /// <inheritdoc />
    public async Task<ExecutionProtocol?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("Attempted to get execution protocol with empty code");
            return null;
        }
        
        var cacheKey = $"{CacheKeyPrefix}:byCode:{code.ToLowerInvariant()}";
        var cached = await _cacheService.GetAsync<ExecutionProtocol>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var protocol = await repository.GetByCodeAsync(code);
        
        if (protocol != null)
        {
            await _cacheService.SetAsync(cacheKey, protocol, CacheDuration);
            _logger.LogDebug("Cached execution protocol with key: {CacheKey}", cacheKey);
        }
        
        return protocol;
    }
    
    /// <inheritdoc />
    public async Task<ExecutionProtocolDto?> GetByCodeAsDtoAsync(string code)
    {
        var protocol = await GetByCodeAsync(code);
        if (protocol == null) return null;
        
        return new ExecutionProtocolDto
        {
            ExecutionProtocolId = protocol.Id.ToString(),
            Value = protocol.Value,
            Description = protocol.Description,
            Code = protocol.Code,
            TimeBase = protocol.TimeBase,
            RepBase = protocol.RepBase,
            RestPattern = protocol.RestPattern,
            IntensityLevel = protocol.IntensityLevel,
            DisplayOrder = protocol.DisplayOrder,
            IsActive = protocol.IsActive
        };
    }
    
    /// <inheritdoc />
    public async Task<bool> ExistsAsync(ExecutionProtocolId id)
    {
        if (id.IsEmpty) return false;
        
        var protocol = await GetByIdAsync(id);
        return protocol != null;
    }
}