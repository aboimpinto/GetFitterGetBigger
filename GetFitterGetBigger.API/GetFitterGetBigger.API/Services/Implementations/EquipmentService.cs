using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTable;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for equipment operations
/// </summary>
public class EquipmentService : ReferenceTableServiceBase<Equipment>, IEquipmentService
{
    public EquipmentService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger<EquipmentService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }
    
    protected override string CacheKeyPrefix => "Equipment";
    protected override TimeSpan CacheDuration => TimeSpan.FromHours(1); // Dynamic table
    
    /// <summary>
    /// Gets all equipment as DTOs
    /// </summary>
    public async Task<IEnumerable<ReferenceDataDto>> GetAllAsDtosAsync()
    {
        var equipment = await GetAllAsync();
        return equipment.Select(e => new ReferenceDataDto
        {
            Id = e.Id.ToString(),
            Value = e.Name
        });
    }
    
    /// <summary>
    /// Gets equipment by ID as DTO
    /// </summary>
    public async Task<ReferenceDataDto?> GetByIdAsDtoAsync(string id)
    {
        var equipment = await GetByIdAsync(id);
        if (equipment == null || !equipment.IsActive)
            return null;
            
        return new ReferenceDataDto
        {
            Id = equipment.Id.ToString(),
            Value = equipment.Name
        };
    }
    
    /// <summary>
    /// Creates equipment and returns as DTO
    /// </summary>
    public async Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentDto request)
    {
        var created = await CreateAsync(request);
        
        return new EquipmentDto
        {
            Id = created.Id.ToString(),
            Name = created.Name,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };
    }
    
    /// <summary>
    /// Updates equipment and returns as DTO
    /// </summary>
    public async Task<EquipmentDto> UpdateEquipmentAsync(string id, UpdateEquipmentDto request)
    {
        var updated = await UpdateAsync(id, request);
        
        return new EquipmentDto
        {
            Id = updated.Id.ToString(),
            Name = updated.Name,
            IsActive = updated.IsActive,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        };
    }
    
    /// <summary>
    /// Deactivates equipment
    /// </summary>
    public async Task DeactivateAsync(string id)
    {
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        if (!EquipmentId.TryParse(id, out var equipmentId))
        {
            throw new ArgumentException($"Invalid ID format. Expected format: 'equipment-{{guid}}', got: '{id}'");
        }
        
        // Check if equipment exists
        var existing = await repository.GetByIdAsync(equipmentId);
        if (existing == null || !existing.IsActive)
        {
            throw new InvalidOperationException($"Equipment with ID '{id}' not found or already inactive");
        }
        
        
        // Check if equipment is in use
        if (await repository.IsInUseAsync(equipmentId))
        {
            _logger.LogWarning("Cannot deactivate equipment '{Name}' (ID: {Id}) as it is in use by exercises", existing.Name, id);
            throw new InvalidOperationException("Cannot deactivate equipment that is in use by exercises");
        }
        
        // Deactivate
        var deactivated = await repository.DeactivateAsync(equipmentId);
        if (!deactivated)
        {
            throw new InvalidOperationException($"Failed to deactivate equipment with ID '{id}'");
        }
        
        await unitOfWork.CommitAsync();
        
        // Invalidate cache
        await InvalidateCacheAsync();
    }
    
    protected override async Task<IEnumerable<Equipment>> GetAllEntitiesAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.GetAllAsync();
    }
    
    protected override async Task<Equipment?> GetEntityByIdAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string id)
    {
        if (!EquipmentId.TryParse(id, out var equipmentId))
        {
            throw new ArgumentException($"Invalid ID format. Expected format: 'equipment-{{guid}}', got: '{id}'");
        }
            
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.GetByIdAsync(equipmentId);
    }
    
    protected override async Task<Equipment?> GetEntityByNameAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string name)
    {
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.GetByNameAsync(name);
    }
    
    protected override async Task<Equipment?> GetEntityByValueAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string value)
    {
        // For equipment, value is the same as name
        return await GetEntityByNameAsync(unitOfWork, value);
    }
    
    protected override async Task<Equipment> CreateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, object createDto)
    {
        if (createDto is not CreateEquipmentDto request)
            throw new ArgumentException("Invalid DTO type");
            
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        // Check for duplicate name
        if (await repository.ExistsAsync(request.Name))
        {
            throw new InvalidOperationException($"Equipment with the name '{request.Name}' already exists");
        }
        
        // Create the equipment
        var equipment = Equipment.Handler.CreateNew(request.Name.Trim());
        return await repository.CreateAsync(equipment);
    }
    
    protected override async Task<Equipment> UpdateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id, object updateDto)
    {
        if (updateDto is not UpdateEquipmentDto request)
            throw new ArgumentException("Invalid DTO type");
            
        if (!EquipmentId.TryParse(id, out var equipmentId))
            throw new ArgumentException($"Invalid ID format. Expected format: 'equipment-{{guid}}', got: '{id}'");
            
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        // Get existing equipment
        var existing = await repository.GetByIdAsync(equipmentId);
        if (existing == null || !existing.IsActive)
        {
            throw new InvalidOperationException($"Equipment with ID '{id}' not found or inactive");
        }
        
        // Check for duplicate name (excluding current)
        if (request.Name != null && await repository.ExistsAsync(request.Name, equipmentId))
        {
            throw new InvalidOperationException($"Equipment with the name '{request.Name}' already exists");
        }
        
        // Update the equipment
        var updated = Equipment.Handler.Update(existing, request.Name!.Trim());
        return await repository.UpdateAsync(updated);
    }
    
    protected override Task DeleteEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id)
    {
        // Use DeactivateAsync instead
        throw new NotSupportedException("Use DeactivateAsync method instead");
    }
    
    protected override async Task<bool> CheckEntityExistsAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id)
    {
        if (!EquipmentId.TryParse(id, out var equipmentId))
        {
            throw new ArgumentException($"Invalid ID format. Expected format: 'equipment-{{guid}}', got: '{id}'");
        }
            
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entity = await repository.GetByIdAsync(equipmentId);
        return entity != null && entity.IsActive;
    }
}