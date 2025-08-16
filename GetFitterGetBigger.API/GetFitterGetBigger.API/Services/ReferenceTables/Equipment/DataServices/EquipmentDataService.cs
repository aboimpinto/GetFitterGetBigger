using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.Equipment;
using GetFitterGetBigger.API.Services.Results;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.ReferenceTables.Equipment.DataServices;

/// <summary>
/// Data service implementation for Equipment database operations
/// Manages all data access concerns including UnitOfWork and Repository interactions
/// </summary>
public class EquipmentDataService : IEquipmentDataService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly ILogger<EquipmentDataService> _logger;

    public EquipmentDataService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ILogger<EquipmentDataService> logger)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllActiveAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entities = await repository.GetAllAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        
        _logger.LogInformation("Loaded {Count} active equipment from database", dtos.Count);
        return ServiceResult<IEnumerable<EquipmentDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        var dto = entity?.IsEmpty != false ? EquipmentDto.Empty : MapToDto(entity);
        
        _logger.LogDebug("Retrieved equipment by ID {Id}: {Found}", id, !dto.IsEmpty);
        return ServiceResult<EquipmentDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<EquipmentDto>> GetByNameAsync(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entity = await repository.GetByNameAsync(name);

        var dto = entity?.IsEmpty != false ? EquipmentDto.Empty : MapToDto(entity);
        
        _logger.LogDebug("Retrieved equipment by name '{Name}': {Found}", name, !dto.IsEmpty);
        return ServiceResult<EquipmentDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        var equipment = Models.Entities.Equipment.Handler.CreateNew(command.Name.Trim());
        var createdEntity = await repository.CreateAsync(equipment);
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Created equipment with ID {Id} and name '{Name}'", createdEntity.EquipmentId, createdEntity.Name);
        return ServiceResult<EquipmentDto>.Success(MapToDto(createdEntity));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateEquipmentCommand command)
    {
        var existingEntity = await LoadEntityForUpdateAsync(id);
        
        // Pattern matching for single exit point
        return existingEntity.IsEmpty switch
        {
            true => ServiceResult<EquipmentDto>.Success(EquipmentDto.Empty),
            false => await PerformUpdateAsync(existingEntity, command)
        };
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> DeleteAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        var success = await repository.DeactivateAsync(id);
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Deactivated equipment with ID {Id}: {Success}", id, success);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(success));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BooleanResultDto>> ExistsAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var exists = await repository.ExistsAsync(id);
        
        _logger.LogDebug("Checked existence of equipment {Id}: {Exists}", id, exists);
        return ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(exists));
    }

    /// <inheritdoc/>
    public async Task<bool> IsNameUniqueAsync(string name, EquipmentId? excludeId = null)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        // Positive assertion: check if name exists, then determine uniqueness
        var nameExists = excludeId.HasValue
            ? await repository.ExistsAsync(name.Trim(), excludeId.Value)
            : await repository.ExistsAsync(name.Trim());
        
        var isUnique = !nameExists;
        _logger.LogDebug("Checked name uniqueness for '{Name}' (excluding {ExcludeId}): {IsUnique}", name, excludeId, isUnique);
        return isUnique;
    }

    /// <inheritdoc/>
    public async Task<bool> CanDeleteAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        // Positive assertion: check if equipment is in use, then determine if can delete
        var isInUse = await repository.IsInUseAsync(id);
        var canDelete = !isInUse;
        
        _logger.LogDebug("Checked if equipment {Id} can be deleted: {CanDelete}", id, canDelete);
        return canDelete;
    }

    private async Task<ServiceResult<EquipmentDto>> PerformUpdateAsync(Models.Entities.Equipment existingEntity, UpdateEquipmentCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        var updated = Models.Entities.Equipment.Handler.Update(existingEntity, command.Name.Trim());
        var updatedEntity = await repository.UpdateAsync(updated);
        await unitOfWork.CommitAsync();
        
        _logger.LogInformation("Updated equipment with ID {Id}", updatedEntity.EquipmentId);
        return ServiceResult<EquipmentDto>.Success(MapToDto(updatedEntity));
    }
    
    private async Task<Models.Entities.Equipment> LoadEntityForUpdateAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entity = await repository.GetByIdAsync(id);
        return entity ?? Models.Entities.Equipment.Empty;
    }
    
    /// <summary>
    /// Maps an Equipment entity to its DTO representation
    /// Entity stays within the data layer - only DTO is exposed
    /// </summary>
    private static EquipmentDto MapToDto(Models.Entities.Equipment entity)
    {
        if (entity == null || entity.IsEmpty)
            return EquipmentDto.Empty;
            
        return new EquipmentDto
        {
            Id = entity.EquipmentId.ToString(),
            Name = entity.Name,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}