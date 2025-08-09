using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables;

/// <summary>
/// Reference service for ExecutionProtocol that provides caching infrastructure
/// This service delegates actual business logic to IExecutionProtocolService
/// Entities stay within the service layer - only DTOs are exposed
/// </summary>
public class ExecutionProtocolReferenceService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    IExecutionProtocolService execution_protocolService,
    ILogger<ExecutionProtocolReferenceService> logger) :
    PureReferenceService<ExecutionProtocol, ExecutionProtocolDto>(unitOfWorkProvider, cacheService, logger)
{
    private readonly IExecutionProtocolService _execution_protocolService = execution_protocolService;

    /// <summary>
    /// Loads all DTOs by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<IEnumerable<ExecutionProtocolDto>>> LoadAllDtosAsync()
    {
        // Delegate to the actual service which handles entity-to-DTO mapping internally
        return await _execution_protocolService.GetAllActiveAsync();
    }
    
    /// <summary>
    /// Loads a DTO by ID by delegating to the actual service
    /// Entities never leave the service layer
    /// </summary>
    protected override async Task<ServiceResult<ExecutionProtocolDto>> LoadDtoByIdAsync(string id)
    {
        var execution_protocolId = ExecutionProtocolId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ExecutionProtocolDto>()
            .EnsureNotEmpty(execution_protocolId, ExecutionProtocolErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await _execution_protocolService.GetByIdAsync(execution_protocolId)
            );
    }
    
    /// <summary>
    /// Validates and parses the ID using ServiceValidate fluent API
    /// </summary>
    protected override ValidationResult ValidateAndParseId(string id)
    {
        var execution_protocolId = ExecutionProtocolId.ParseOrEmpty(id);
        
        return ServiceValidate.For()
            .EnsureNotEmpty(execution_protocolId, ExecutionProtocolErrorMessages.InvalidIdFormat)
            .ToResult();
    }
}
