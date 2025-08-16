using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.MuscleGroup;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup.DataServices;

/// <summary>
/// Data service implementation for MuscleGroup database operations
/// Manages all data access concerns including UnitOfWork and Repository interactions
/// </summary>
public class MuscleGroupDataService : IMuscleGroupDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<MuscleGroupDataService> _logger;

    public MuscleGroupDataService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<MuscleGroupDataService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entities = await repository.GetAllAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active muscle groups from database", dtos.Count);
        return ServiceResult<IEnumerable<MuscleGroupDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<MuscleGroupDto>> GetByIdAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        var dto = entity?.IsEmpty != false ? MuscleGroupDto.Empty : MapToDto(entity);
        
        _logger.LogDebug("Retrieved muscle group by ID {Id}: {Found}", id, !dto.IsEmpty);
        return ServiceResult<MuscleGroupDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<MuscleGroupDto>> GetByNameAsync(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entity = await repository.GetByNameAsync(name);

        var dto = entity?.IsEmpty != false ? MuscleGroupDto.Empty : MapToDto(entity);
        
        _logger.LogDebug("Retrieved muscle group by name '{Name}': {Found}", name, !dto.IsEmpty);
        return ServiceResult<MuscleGroupDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetByBodyPartAsync(BodyPartId bodyPartId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entities = await repository.GetByBodyPartAsync(bodyPartId);
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogDebug("Retrieved {Count} muscle groups for body part '{BodyPartId}'", dtos.Count, bodyPartId);
        return ServiceResult<IEnumerable<MuscleGroupDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<MuscleGroupDto>> CreateAsync(CreateMuscleGroupCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        var muscleGroup = Models.Entities.MuscleGroup.Handler.CreateNew(command.Name.Trim(), command.BodyPartId);
        var createdEntity = await repository.CreateAsync(muscleGroup);
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Created muscle group with ID {Id} and name '{Name}'", createdEntity.Id, createdEntity.Name);
        return ServiceResult<MuscleGroupDto>.Success(MapToDto(createdEntity));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<MuscleGroupDto>> UpdateAsync(MuscleGroupId id, UpdateMuscleGroupCommand command)
    {
        var existingEntity = await LoadEntityForUpdateAsync(id);
        
        // Pattern matching for single exit point
        return existingEntity.IsEmpty switch
        {
            true => ServiceResult<MuscleGroupDto>.Success(MuscleGroupDto.Empty),
            false => await PerformUpdateAsync(existingEntity, command)
        };
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> DeleteAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        var success = await repository.DeactivateAsync(id);
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Deactivated muscle group with ID {Id}: {Success}", id, success);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(success));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var exists = await repository.ExistsAsync(id);
        
        _logger.LogDebug("Checked existence of muscle group {Id}: {Exists}", id, exists);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }

    /// <inheritdoc/>
    public async Task<bool> IsNameUniqueAsync(string name, MuscleGroupId? excludeId = null)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        // Positive assertion: check if name exists, then determine uniqueness
        var nameExists = excludeId.HasValue
            ? await repository.ExistsByNameAsync(name.Trim(), excludeId.Value)
            : await repository.ExistsByNameAsync(name.Trim());
        
        var isUnique = !nameExists;
        _logger.LogDebug("Checked name uniqueness for '{Name}' (excluding {ExcludeId}): {IsUnique}", name, excludeId, isUnique);
        return isUnique;
    }

    /// <inheritdoc/>
    public async Task<bool> CanDeleteAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        var canDelete = await repository.CanDeactivateAsync(id);
        
        _logger.LogDebug("Checked if muscle group {Id} can be deleted: {CanDelete}", id, canDelete);
        return canDelete;
    }

    private async Task<ServiceResult<MuscleGroupDto>> PerformUpdateAsync(Models.Entities.MuscleGroup existingEntity, UpdateMuscleGroupCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        var updated = Models.Entities.MuscleGroup.Handler.Update(existingEntity, command.Name.Trim(), command.BodyPartId);
        var updatedEntity = await repository.UpdateAsync(updated);
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Updated muscle group with ID {Id}", updatedEntity.Id);
        return ServiceResult<MuscleGroupDto>.Success(MapToDto(updatedEntity));
    }
    
    private async Task<Models.Entities.MuscleGroup> LoadEntityForUpdateAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entity = await repository.GetByIdAsync(id);
        return entity ?? Models.Entities.MuscleGroup.Empty;
    }
    
    /// <summary>
    /// Maps a MuscleGroup entity to its DTO representation
    /// Entity stays within the data layer - only DTO is exposed
    /// </summary>
    private static MuscleGroupDto MapToDto(Models.Entities.MuscleGroup entity)
    {
        if (entity == null || entity.IsEmpty || !entity.IsActive)
            return MuscleGroupDto.Empty;
            
        return new MuscleGroupDto
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            BodyPartId = entity.BodyPartId.ToString(),
            BodyPartName = entity.BodyPart?.Value,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}