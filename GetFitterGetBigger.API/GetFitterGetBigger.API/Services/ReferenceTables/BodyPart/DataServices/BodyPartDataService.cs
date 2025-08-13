using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables.BodyPart.DataServices;

/// <summary>
/// Data service implementation for BodyPart database operations
/// Manages all data access concerns including UnitOfWork and Repository interactions
/// </summary>
public class BodyPartDataService : IBodyPartDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<BodyPartDataService> _logger;

    public BodyPartDataService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<BodyPartDataService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<BodyPartDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active body parts from database", dtos.Count);
        return ServiceResult<IEnumerable<BodyPartDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByIdAsync(BodyPartId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        var dto = entity.IsActive ? MapToDto(entity) : BodyPartDto.Empty;
        
        _logger.LogDebug("Retrieved body part by ID {Id}: {Found}", id, !dto.IsEmpty);
        return ServiceResult<BodyPartDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BodyPartDto>> GetByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var entity = await repository.GetByValueAsync(value);

        var dto = entity.IsActive ? MapToDto(entity) : BodyPartDto.Empty;
        
        _logger.LogDebug("Retrieved body part by value '{Value}': {Found}", value, !dto.IsEmpty);
        return ServiceResult<BodyPartDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(BodyPartId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IBodyPartRepository>();
        var exists = await repository.ExistsAsync(id);
        
        _logger.LogDebug("Checked existence of body part {Id}: {Exists}", id, exists);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    /// <summary>
    /// Maps a BodyPart entity to its DTO representation
    /// Entity stays within the data layer - only DTO is exposed
    /// </summary>
    private static BodyPartDto MapToDto(Models.Entities.BodyPart entity)
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