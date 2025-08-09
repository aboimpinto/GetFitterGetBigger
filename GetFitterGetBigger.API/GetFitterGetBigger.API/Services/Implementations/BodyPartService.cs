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
/// Service implementation for body part operations
/// This service focuses solely on business logic and data access
/// Caching is handled by the wrapping PureReferenceService layer
/// </summary>
public class BodyPartService(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    ILogger<BodyPartService> logger) : IBodyPartService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<BodyPartService> _logger = logger;

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<BodyPartDto>>> GetAllActiveAsync()
    {
        return await ServiceValidate.Build<IEnumerable<BodyPartDto>>()
            .WhenValidAsync(async () => await LoadAllActiveFromDatabaseAsync());
    }
    
    /// <summary>
    /// Loads all active BodyParts from the database and maps to DTOs
    /// </summary>
    private async Task<ServiceResult<IEnumerable<BodyPartDto>>> LoadAllActiveFromDatabaseAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active body parts", dtos.Count);
        return ServiceResult<IEnumerable<BodyPartDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
    {
        return await ServiceValidate.For<BodyPartDto>()
            .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(id))
            .ThenMatchDataAsync<BodyPartDto, BodyPartDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<BodyPartDto>.Failure(
                        BodyPartDto.Empty,
                        ServiceError.NotFound("BodyPart", id.ToString()))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<BodyPartDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Gets a body part by its ID string
    /// </summary>
    /// <param name="id">The body part ID as a string</param>
    /// <returns>A service result containing the body part if found</returns>
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(string id)
    {
        var bodyPartId = BodyPartId.ParseOrEmpty(id);
        
        return await ServiceValidate.For<BodyPartDto>()
            .EnsureNotEmpty(bodyPartId, BodyPartErrorMessages.InvalidIdFormat)
            .WithServiceResultAsync(() => LoadByIdFromDatabaseAsync(bodyPartId))
            .ThenMatchDataAsync<BodyPartDto, BodyPartDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<BodyPartDto>.Failure(
                        BodyPartDto.Empty,
                        ServiceError.NotFound("BodyPart", id))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<BodyPartDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Loads a BodyPart by ID from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<BodyPartDto>> LoadByIdFromDatabaseAsync(BodyPartId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity.IsActive
            ? ServiceResult<BodyPartDto>.Success(MapToDto(entity))
            : ServiceResult<BodyPartDto>.Success(BodyPartDto.Empty);
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<BodyPartDto>()
            .EnsureNotWhiteSpace(value, BodyPartErrorMessages.ValueCannotBeEmpty)
            .WithServiceResultAsync(() => LoadByValueFromDatabaseAsync(value))
            .ThenMatchDataAsync<BodyPartDto, BodyPartDto>(
                whenEmpty: () => Task.FromResult(
                    ServiceResult<BodyPartDto>.Failure(
                        BodyPartDto.Empty,
                        ServiceError.NotFound("BodyPart", value))),
                whenNotEmpty: dto => Task.FromResult(
                    ServiceResult<BodyPartDto>.Success(dto))
            );
    }
    
    /// <summary>
    /// Loads a BodyPart by value from the database and maps to DTO
    /// </summary>
    private async Task<ServiceResult<BodyPartDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entity = await repository.GetByValueAsync(value);

        return entity.IsActive
            ? ServiceResult<BodyPartDto>.Success(MapToDto(entity))
            : ServiceResult<BodyPartDto>.Success(BodyPartDto.Empty);   
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(BodyPartId id)
    {
        return await ServiceValidate.For<BooleanResultDto>()
            .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
            .MatchAsync(
                whenValid: async () => await CheckExistsInDatabaseAsync(id)
            );
    }
    
    /// <summary>
    /// Checks if a BodyPart exists in the database by ID
    /// </summary>
    private async Task<ServiceResult<BooleanResultDto>> CheckExistsInDatabaseAsync(BodyPartId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var exists = await repository.ExistsAsync(id);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    /// <summary>
    /// Maps a BodyPart entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private BodyPartDto MapToDto(BodyPart entity)
    {
        if (entity.IsEmpty)
            return BodyPartDto.Empty;
            
        return new BodyPartDto
        {
            Id = entity.Id,
            Value = entity.Value,
            Description = entity.Description
        };
    }
}