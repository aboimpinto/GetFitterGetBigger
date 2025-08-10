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
        return await ServiceValidate.Build<IEnumerable<ExecutionProtocolDto>>()
            .WhenValidAsync(async () => await LoadAllActiveFromDatabaseAsync());
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
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(id))
            .ThenMatchDataAsync<ExecutionProtocolDto, ExecutionProtocolDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ExecutionProtocolDto>.Failure(
                        ExecutionProtocolDto.Empty,
                        ServiceError.NotFound("ExecutionProtocol", id.ToString()))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ExecutionProtocolDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Gets a execution_protocol by its ID string
    /// </summary>
    /// <param name="id">The execution_protocol ID as a string</param>
    /// <returns>A service result containing the execution_protocol if found</returns>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByIdAsync(string id)
    {
        var executionProtocolId = ExecutionProtocolId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ExecutionProtocolDto>()
            .EnsureNotEmpty(executionProtocolId, ExecutionProtocolErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(executionProtocolId))
            .ThenMatchDataAsync<ExecutionProtocolDto, ExecutionProtocolDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ExecutionProtocolDto>.Failure(
                        ExecutionProtocolDto.Empty,
                        ServiceError.NotFound("ExecutionProtocol", id))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ExecutionProtocolDto>.Success(dto))
            );
    }
    
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
            .WithServiceResultAsync(() => LoadByValueFromDatabaseAsync(value))
            .ThenMatchDataAsync<ExecutionProtocolDto, ExecutionProtocolDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ExecutionProtocolDto>.Failure(
                        ExecutionProtocolDto.Empty,
                        ServiceError.NotFound("ExecutionProtocol", value))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ExecutionProtocolDto>.Success(dto))
            );
    }
    
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
                whenValid: async () => await CheckExistsInDatabaseAsync(id)
            );
    }
    
    /// <summary>
    /// Checks if an ExecutionProtocol exists in the database by ID
    /// </summary>
    private async Task<ServiceResult<BooleanResultDto>> CheckExistsInDatabaseAsync(ExecutionProtocolId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IExecutionProtocolRepository>();
        var exists = await repository.ExistsAsync(id);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ExecutionProtocolDto>> GetByCodeAsync(string code)
    {
        return await ServiceValidate.For<ExecutionProtocolDto>()
            .EnsureNotWhiteSpace(code, ExecutionProtocolErrorMessages.ValueCannotBeEmpty)
            .WithServiceResultAsync(() => LoadByCodeFromDatabaseAsync(code))
            .ThenMatchDataAsync<ExecutionProtocolDto, ExecutionProtocolDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ExecutionProtocolDto>.Failure(
                        ExecutionProtocolDto.Empty,
                        ServiceError.NotFound("ExecutionProtocol", code))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ExecutionProtocolDto>.Success(dto))
            );
    }
    
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
    /// Maps a ExecutionProtocol entity to its DTO representation
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