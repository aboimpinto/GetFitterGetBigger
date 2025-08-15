using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables.ExecutionProtocol.DataServices;

/// <summary>
/// Data service implementation for ExecutionProtocol database operations
/// Manages all data access concerns including UnitOfWork and Repository interactions
/// ExecutionProtocols are pure reference data (read-only) that never changes after deployment
/// </summary>
public class ExecutionProtocolDataService : IExecutionProtocolDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<ExecutionProtocolDataService> _logger;

    public ExecutionProtocolDataService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<ExecutionProtocolDataService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ExecutionProtocolDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active execution protocols from database", dtos.Count);
        return ServiceResult<IEnumerable<ExecutionProtocolDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByIdAsync(ExecutionProtocolId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        // Pattern matching - repository returns Empty (never null)
        var dto = entity switch
        {
            { IsActive: false } => MapToDto(Models.Entities.ExecutionProtocol.Empty),
            _ => MapToDto(entity)
        };
        
        _logger.LogDebug("Retrieved execution protocol by ID {Id}: {Found}", id, !dto.IsEmpty);
        return ServiceResult<ExecutionProtocolDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        // Pattern matching for clean code
        var dto = entity switch
        {
            { IsActive: false } => MapToDto(Models.Entities.ExecutionProtocol.Empty),
            _ => MapToDto(entity)
        };
        
        _logger.LogDebug("Retrieved execution protocol by value '{Value}': {Found}", value, !dto.IsEmpty);
        return ServiceResult<ExecutionProtocolDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByCodeAsync(string code)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entity = await repository.GetByCodeAsync(code);
        
        // Pattern matching approach
        var dto = entity switch
        {
            { IsActive: false } => MapToDto(Models.Entities.ExecutionProtocol.Empty),
            _ => MapToDto(entity)
        };
        
        _logger.LogDebug("Retrieved execution protocol by code '{Code}': {Found}", code, !dto.IsEmpty);
        return ServiceResult<ExecutionProtocolDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(ExecutionProtocolId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        // Pattern matching for existence check
        var exists = entity is { IsActive: true };
        
        _logger.LogDebug("Checked existence of execution protocol {Id}: {Exists}", id, exists);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }

    /// <summary>
    /// Maps an ExecutionProtocol entity to its DTO representation
    /// Entity stays within the data layer - only DTO is exposed
    /// </summary>
    private static ExecutionProtocolDto MapToDto(Models.Entities.ExecutionProtocol entity)
    {
        if (entity == null || entity.IsEmpty)
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