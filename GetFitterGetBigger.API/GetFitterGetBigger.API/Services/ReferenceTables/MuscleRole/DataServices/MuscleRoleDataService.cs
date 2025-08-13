using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MuscleRole.DataServices;

/// <summary>
/// Data service implementation for MuscleRole database operations
/// Manages all data access concerns including UnitOfWork and Repository interactions
/// </summary>
public class MuscleRoleDataService : IMuscleRoleDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<MuscleRoleDataService> _logger;

    public MuscleRoleDataService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<MuscleRoleDataService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<ReferenceDataDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var entities = await repository.GetAllActiveAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active muscle roles from database", dtos.Count);
        return ServiceResult<IEnumerable<ReferenceDataDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByIdAsync(MuscleRoleId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        var dto = entity.IsActive ? MapToDto(entity) : ReferenceDataDto.Empty;
        
        _logger.LogDebug("Retrieved muscle role by ID {Id}: {Found}", id, !dto.IsEmpty);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ReferenceDataDto>> GetByValueAsync(string value)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var entity = await repository.GetByValueAsync(value);

        var dto = entity.IsActive ? MapToDto(entity) : ReferenceDataDto.Empty;
        
        _logger.LogDebug("Retrieved muscle role by value '{Value}': {Found}", value, !dto.IsEmpty);
        return ServiceResult<ReferenceDataDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(MuscleRoleId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleRoleRepository>();
        var exists = await repository.ExistsAsync(id);
        
        _logger.LogDebug("Checked existence of muscle role {Id}: {Exists}", id, exists);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }
    
    /// <summary>
    /// Maps a MuscleRole entity to its DTO representation
    /// Entity stays within the data layer - only DTO is exposed
    /// </summary>
    private static ReferenceDataDto MapToDto(Models.Entities.MuscleRole entity)
    {
        if (entity.IsEmpty)
            return ReferenceDataDto.Empty;
            
        return new ReferenceDataDto
        {
            Id = entity.MuscleRoleId.ToString(),
            Value = entity.Value,
            Description = entity.Description
        };
    }
}