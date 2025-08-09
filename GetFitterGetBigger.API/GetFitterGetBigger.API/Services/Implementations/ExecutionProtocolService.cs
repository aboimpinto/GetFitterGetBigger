using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for execution_protocol operations
/// This service focuses solely on business logic and data access
/// Caching is handled by the wrapping ExecutionProtocolReferenceService layer
/// </summary>
public class ExecutionProtocolService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<ExecutionProtocolService> logger) : IExecutionProtocolService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<ExecutionProtocolService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ExecutionProtocolDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active execution_protocols", dtos.Count);
        return ServiceResult<IEnumerable<ExecutionProtocolDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByIdAsync(ExecutionProtocolId id)
    {
        return await ServiceValidate.For<ExecutionProtocolDto>()
            .EnsureNotEmpty(id, ExecutionProtocolErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(id)
            );
    }
    
    /// <summary>
    /// Gets a execution_protocol by its ID string
    /// </summary>
    /// <param name="id">The execution_protocol ID as a string</param>
    /// <returns>A service result containing the execution_protocol if found</returns>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByIdAsync(string id)
    {
        var execution_protocolId = ExecutionProtocolId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ExecutionProtocolDto>()
            .EnsureNotEmpty(execution_protocolId, ExecutionProtocolErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(execution_protocolId)
            );
    }
    
    private async Task<ServiceResult<ExecutionProtocolDto>> LoadByIdFromDatabaseAsync(ExecutionProtocolId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return (entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<ExecutionProtocolDto>.Failure(
                ExecutionProtocolDto.Empty,
                ServiceError.NotFound("ExecutionProtocol", id.ToString())),
            false => ServiceResult<ExecutionProtocolDto>.Success(MapToDto(entity))
        };
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ExecutionProtocolDto>()
            .EnsureNotWhiteSpace(value, ExecutionProtocolErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await LoadByValueFromDatabaseAsync(value)
            );
    }
    
    private async Task<ServiceResult<ExecutionProtocolDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        return (entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<ExecutionProtocolDto>.Failure(
                ExecutionProtocolDto.Empty,
                ServiceError.NotFound("ExecutionProtocol", value)),
            false => ServiceResult<ExecutionProtocolDto>.Success(MapToDto(entity))
        };
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
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByCodeAsync(string code)
    {
        return await ServiceValidate.For<ExecutionProtocolDto>()
            .EnsureNotWhiteSpace(code, ExecutionProtocolErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await LoadByCodeFromDatabaseAsync(code)
            );
    }
    
    private async Task<ServiceResult<ExecutionProtocolDto>> LoadByCodeFromDatabaseAsync(string code)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var entity = await repository.GetByCodeAsync(code);
        
        return (entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<ExecutionProtocolDto>.Failure(
                ExecutionProtocolDto.Empty,
                ServiceError.NotFound("ExecutionProtocol", code)),
            false => ServiceResult<ExecutionProtocolDto>.Success(MapToDto(entity))
        };
    }
    
    /// <summary>
    /// Maps a ExecutionProtocol entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ExecutionProtocolDto MapToDto(ExecutionProtocol entity)
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
}
