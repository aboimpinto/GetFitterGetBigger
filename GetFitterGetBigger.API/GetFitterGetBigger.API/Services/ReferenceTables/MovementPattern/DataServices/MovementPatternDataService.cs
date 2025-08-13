using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MovementPattern.DataServices;

/// <summary>
/// Data service implementation for MovementPattern database operations
/// Manages all data access concerns including UnitOfWork and Repository interactions
/// </summary>
public class MovementPatternDataService : IMovementPatternDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<MovementPatternDataService> _logger;

    public MovementPatternDataService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<MovementPatternDataService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active movement patterns from database", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MovementPatternId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        var dto = entity.IsActive ? MapToDto(entity) : ReferenceDataDto.Empty;
        
        _logger.LogDebug("Retrieved movement pattern by ID {Id}: {Found}", id, !dto.IsEmpty);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
        var entity = await repository.GetByValueAsync(value);

        var dto = entity.IsActive ? MapToDto(entity) : ReferenceDataDto.Empty;
        
        _logger.LogDebug("Retrieved movement pattern by value '{Value}': {Found}", value, !dto.IsEmpty);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(MovementPatternId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMovementPatternRepository>();
        var exists = await repository.ExistsAsync(id);
        
        _logger.LogDebug("Checked existence of movement pattern {Id}: {Exists}", id, exists);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    /// <summary>
    /// Maps a MovementPattern entity to its DTO representation
    /// Entity stays within the data layer - only DTO is exposed
    /// </summary>
    private static ReferenceDataDto MapToDto(Models.Entities.MovementPattern entity)
    {
        if (entity.IsEmpty)
            return ReferenceDataDto.Empty;
            
        return new ReferenceDataDto
        {
            Id = entity.MovementPatternId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}