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
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(id)
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
            .MatchAsync(
                whenValid: async () => await LoadByIdFromDatabaseAsync(bodyPartId)
            );
    }
    
    private async Task<ServiceResult<BodyPartDto>> LoadByIdFromDatabaseAsync(BodyPartId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return (entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<BodyPartDto>.Failure(
                BodyPartDto.Empty,
                ServiceError.NotFound("BodyPart", id.ToString())),
            false => ServiceResult<BodyPartDto>.Success(MapToDto(entity))
        };
    }
    
    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value)
    {
        return await ServiceValidate.For<BodyPartDto>()
            .EnsureNotWhiteSpace(value, BodyPartErrorMessages.ValueCannotBeEmpty)
            .MatchAsync(
                whenValid: async () => await LoadByValueFromDatabaseAsync(value)
            );
    }
    
    private async Task<ServiceResult<BodyPartDto>> LoadByValueFromDatabaseAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entity = await repository.GetByValueAsync(value);
        
        return (entity.IsEmpty || !entity.IsActive) switch
        {
            true => ServiceResult<BodyPartDto>.Failure(
                BodyPartDto.Empty,
                ServiceError.NotFound("BodyPart", value)),
            false => ServiceResult<BodyPartDto>.Success(MapToDto(entity))
        };
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<bool>> ExistsAsync(BodyPartId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, BodyPartErrorMessages.InvalidIdFormat)
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IBodyPartRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
            });
    }
    
    /// <summary>
    /// Maps a BodyPart entity to its DTO representation
    /// Entity stays within the service layer - only DTO is exposed
    /// </summary>
    private BodyPartDto MapToDto(BodyPart entity)
    {
        return new BodyPartDto
        {
            Id = entity.Id,
            Value = entity.Value,
            Description = entity.Description
        };
    }
}