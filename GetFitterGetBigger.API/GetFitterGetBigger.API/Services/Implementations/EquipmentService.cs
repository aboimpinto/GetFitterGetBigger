using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Commands.Equipment;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for equipment operations
/// </summary>
public class EquipmentService : EnhancedReferenceService<Equipment, EquipmentDto, CreateEquipmentCommand, UpdateEquipmentCommand>, IEquipmentService
{
    public EquipmentService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger<EquipmentService> logger)
        : base(unitOfWorkProvider, cacheService, logger)
    {
    }
    
    /// <summary>
    /// Gets equipment by ID using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id)
    {
        // Let the base class handle validation through ValidateAndParseId
        return await GetByIdAsync(id.ToString());
    }
    
    /// <summary>
    /// Creates new equipment using command
    /// </summary>
    public override async Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command)
    {
        return await base.CreateAsync(command);
    }
    
    /// <summary>
    /// Updates existing equipment using command and strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateEquipmentCommand command)
    {
        return await base.UpdateAsync(id.ToString(), command);
    }
    
    /// <summary>
    /// Deletes equipment using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteAsync(EquipmentId id)
    {
        try
        {
            // Validate ID
            if (id.IsEmpty)
            {
                return ServiceResult<bool>.Failure(false, EquipmentErrorMessages.IdCannotBeEmpty);
            }
            
            using var unitOfWork = _unitOfWorkProvider.CreateWritable();
            var repository = unitOfWork.GetRepository<IEquipmentRepository>();
            
            // Check if equipment exists
            var existingEntity = await repository.GetByIdAsync(id);
            if (existingEntity == null)
            {
                return ServiceResult<bool>.Failure(false, ServiceError.NotFound("Equipment"));
            }
            
            // Check if equipment is in use
            if (await repository.IsInUseAsync(id))
            {
                _logger.LogWarning("Cannot delete equipment with ID {Id} as it is in use by exercises", id);
                return ServiceResult<bool>.Failure(false, ServiceError.DependencyExists("Equipment", "exercises that reference it"));
            }
            
            // Perform soft delete
            var deleted = await repository.DeactivateAsync(id);
            if (!deleted)
            {
                return ServiceResult<bool>.Failure(false, EquipmentErrorMessages.FailedToDelete);
            }
            
            await unitOfWork.CommitAsync();
            
            // Invalidate caches
            await InvalidateCacheAsync();
            
            _logger.LogInformation("Deleted Equipment with ID: {Id}", id);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Equipment with ID: {Id}", id);
            return ServiceResult<bool>.Failure(false, "Failed to delete Equipment");
        }
    }
    
    /// <summary>
    /// Checks if equipment exists using strongly-typed ID
    /// </summary>
    public async Task<bool> ExistsAsync(EquipmentId id)
    {
        return await base.ExistsAsync(id.ToString());
    }
    
    /// <summary>
    /// Gets equipment by name (case-insensitive)
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> GetByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return ServiceResult<EquipmentDto>.Failure(
                    CreateEmptyDto(),
                    EquipmentErrorMessages.NameCannotBeEmpty);
            }
            
            var cacheKey = GetCacheKey($"name:{name.ToLowerInvariant()}");
            var cacheService = (ICacheService)_cacheService;
            var cached = await cacheService.GetAsync<EquipmentDto>(cacheKey);
            
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for Equipment by name: {Name}", name);
                return ServiceResult<EquipmentDto>.Success(cached);
            }
            
            // Load from database
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IEquipmentRepository>();
            var entity = await repository.GetByNameAsync(name);
            
            if (entity == null || entity.IsEmpty || !entity.IsActive)
            {
                return ServiceResult<EquipmentDto>.Failure(
                    CreateEmptyDto(),
                    ServiceError.NotFound("Equipment"));
            }
            
            // Map to DTO
            var dto = MapToDto(entity);
            
            // Cache with 1-hour expiration for enhanced reference data
            await cacheService.SetAsync(cacheKey, dto);
            
            return ServiceResult<EquipmentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Equipment by name: {Name}", name);
            return ServiceResult<EquipmentDto>.Failure(
                CreateEmptyDto(),
                EquipmentErrorMessages.FailedToLoad);
        }
    }
    
    // Abstract method implementations
    
    protected override async Task<IEnumerable<Equipment>> LoadAllEntitiesAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.GetAllAsync();
    }
    
    protected override async Task<Equipment?> LoadEntityByIdAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string id)
    {
        var equipmentId = EquipmentId.ParseOrEmpty(id);
        if (equipmentId.IsEmpty)
            return null;
            
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.GetByIdAsync(equipmentId);
    }
    
    protected override async Task<Equipment?> LoadEntityByIdAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id)
    {
        var equipmentId = EquipmentId.ParseOrEmpty(id);
        if (equipmentId.IsEmpty)
            return null;
            
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.GetByIdAsync(equipmentId);
    }
    
    protected override EquipmentDto MapToDto(Equipment entity)
    {
        return new EquipmentDto
        {
            Id = entity.EquipmentId.ToString(),
            Name = entity.Name,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    
    protected override EquipmentDto CreateEmptyDto()
    {
        return new EquipmentDto
        {
            Id = string.Empty,
            Name = string.Empty,
            IsActive = false,
            CreatedAt = DateTime.MinValue,
            UpdatedAt = null
        };
    }
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return ValidationResult.Failure(EquipmentErrorMessages.IdCannotBeEmpty);
            
        var equipmentId = EquipmentId.ParseOrEmpty(id);
        // If ParseOrEmpty returns Empty for a non-empty string, it means invalid format
        if (equipmentId.IsEmpty && !string.IsNullOrWhiteSpace(id))
            return ValidationResult.Failure(string.Format(EquipmentErrorMessages.InvalidIdFormat, id));
            
        return ValidationResult.Success();
    }
    
    protected override async Task<ValidationResult> ValidateCreateCommand(CreateEquipmentCommand command)
    {
        if (command == null)
            return ValidationResult.Failure(EquipmentErrorMessages.RequestCannotBeNull);
            
        if (string.IsNullOrWhiteSpace(command.Name))
            return ValidationResult.Failure(EquipmentErrorMessages.NameCannotBeEmpty);
            
        // Check for duplicate name
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        if (await repository.ExistsAsync(command.Name.Trim()))
            return ValidationResult.Failure(string.Format(EquipmentErrorMessages.DuplicateNameFormat, command.Name));
            
        return ValidationResult.Success();
    }
    
    protected override async Task<ValidationResult> ValidateUpdateCommand(string id, UpdateEquipmentCommand command)
    {
        if (command == null)
            return ValidationResult.Failure(EquipmentErrorMessages.RequestCannotBeNull);
            
        if (string.IsNullOrWhiteSpace(command.Name))
            return ValidationResult.Failure(EquipmentErrorMessages.NameCannotBeEmpty);
            
        var equipmentId = EquipmentId.ParseOrEmpty(id);
        if (equipmentId.IsEmpty)
            return ValidationResult.Failure(EquipmentErrorMessages.InvalidEquipmentId);
            
        // Check for duplicate name (excluding current)
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        if (await repository.ExistsAsync(command.Name.Trim(), equipmentId))
            return ValidationResult.Failure(string.Format(EquipmentErrorMessages.DuplicateNameFormat, command.Name));
            
        return ValidationResult.Success();
    }
    
    protected override async Task<Equipment> CreateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, CreateEquipmentCommand command)
    {
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var equipment = Equipment.Handler.CreateNew(command.Name.Trim());
        return await repository.CreateAsync(equipment);
    }
    
    protected override async Task<Equipment> UpdateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, Equipment existingEntity, UpdateEquipmentCommand command)
    {
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var updated = Equipment.Handler.Update(existingEntity, command.Name.Trim());
        return await repository.UpdateAsync(updated);
    }
    
    protected override async Task<bool> DeleteEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id)
    {
        // This method is not used when DeleteAsync(EquipmentId) is overridden
        // But we need to provide an implementation for the abstract method
        var equipmentId = EquipmentId.ParseOrEmpty(id);
        if (equipmentId.IsEmpty)
            return false;
            
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.DeactivateAsync(equipmentId);
    }
}