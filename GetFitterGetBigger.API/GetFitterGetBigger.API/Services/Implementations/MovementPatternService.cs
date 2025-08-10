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
/// Service implementation for movement_pattern operations
/// This service focuses solely on business logic and data access
/// Caching is handled by the wrapping MovementPatternReferenceService layer
/// </summary>
public class MovementPatternService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<MovementPatternService> logger) : IMovementPatternService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<MovementPatternService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active movement_patterns", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MovementPatternId id)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(id, MovementPatternErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(id))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("MovementPattern", id.ToString()))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Gets a movement_pattern by its ID string
    /// </summary>
    /// <param name="id">The movement_pattern ID as a string</param>
    /// <returns>A service result containing the movement_pattern if found</returns>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(string id)
    {
        var movementPatternId = MovementPatternId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotEmpty(movementPatternId, MovementPatternErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(movementPatternId))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("MovementPattern", movementPatternId.ToString()))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByIdFromDatabaseAsync(MovementPatternId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<ReferenceDataDto>()
            .EnsureNotWhiteSpace(value, MovementPatternErrorMessages.ValueCannotBeEmpty)
            .WithServiceResultAsync(() => LoadByValueFromDatabaseAsync(value))
            .ThenMatchDataAsync<ReferenceDataDto, ReferenceDataDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Failure(
                        ReferenceDataDto.Empty,
                        ServiceError.NotFound("MovementPattern", value))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<ReferenceDataDto>.Success(dto))
            );
    }
    
    private async Task<ServiceResult<ReferenceDataDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        return entity.IsActive
            ? ServiceResult<ReferenceDataDto>.Success(MapToDto(entity))
            : ServiceResult<ReferenceDataDto>.Success(ReferenceDataDto.Empty);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(MovementPatternId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, MovementPatternErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () =>
                {
                    using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                    var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
                    var exists = await repository.ExistsAsync(id);
                    return ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = exists });
                });
    }
    
    /// <summary>
    /// Maps a MovementPattern entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private ReferenceDataDto MapToDto(MovementPattern entity)
    {
        return new ReferenceDataDto
        {
            Id = entity.MovementPatternId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}
