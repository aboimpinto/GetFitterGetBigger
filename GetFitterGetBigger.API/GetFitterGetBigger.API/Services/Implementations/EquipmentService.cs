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
    public async Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id) =>
        await GetByIdAsync(id.ToString());
    
    /// <summary>
    /// Updates existing equipment using command and strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateEquipmentCommand command) =>
        await UpdateAsync(id.ToString(), command);
    
    /// <summary>
    /// Deletes equipment using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteAsync(EquipmentId id)
    {
        // Validate using existing method
        var existingResult = await GetByIdAsync(id);
        if (!existingResult.IsSuccess)
            return ServiceResult<bool>.Failure(false, existingResult.StructuredErrors.First());
        
        // Check if in use
        var inUseResult = await CheckIfInUseAsync(id);
        if (!inUseResult.IsSuccess)
            return inUseResult;
        
        // Delegate to base class delete
        var deleteResult = await DeleteAsync(id.ToString());
        return deleteResult.IsSuccess 
            ? ServiceResult<bool>.Success(true)
            : ServiceResult<bool>.Failure(false, deleteResult.StructuredErrors.First());
    }
    
    /// <summary>
    /// Checks if equipment exists using strongly-typed ID
    /// </summary>
    public async Task<bool> ExistsAsync(EquipmentId id) =>
        await ExistsAsync(id.ToString());
    
    /// <summary>
    /// Gets equipment by name (case-insensitive)
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult<EquipmentDto>.Failure(
                CreateEmptyDto(),
                EquipmentErrorMessages.Validation.NameCannotBeEmpty);
        
        var cacheKey = GetCacheKey($"name:{name.ToLowerInvariant()}");
        var cacheService = (ICacheService)_cacheService;
        var cached = await cacheService.GetAsync<EquipmentDto>(cacheKey);
        
        return cached switch
        {
            not null => LogCacheHitAndReturn(cached, name),
            _ => await LoadEquipmentByNameAsync(name, cacheKey, cacheService)
        };
    }
    
    // Helper methods
    
    private ServiceResult<EquipmentDto> LogCacheHitAndReturn(EquipmentDto cached, string name)
    {
        _logger.LogDebug("Cache hit for Equipment by name: {Name}", name);
        return ServiceResult<EquipmentDto>.Success(cached);
    }
    
    private async Task<ServiceResult<EquipmentDto>> LoadEquipmentByNameAsync(
        string name, 
        string cacheKey, 
        ICacheService cacheService)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entity = await repository.GetByNameAsync(name);
        
        if (entity == null || entity.IsEmpty || !entity.IsActive)
            return ServiceResult<EquipmentDto>.Failure(
                CreateEmptyDto(),
                ServiceError.NotFound("Equipment"));
        
        var dto = MapToDto(entity);
        await cacheService.SetAsync(cacheKey, dto);
        
        return ServiceResult<EquipmentDto>.Success(dto);
    }
    
    private async Task<ServiceResult<bool>> CheckIfInUseAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        if (await repository.IsInUseAsync(id))
        {
            _logger.LogWarning("Cannot delete equipment with ID {Id} as it is in use by exercises", id);
            return ServiceResult<bool>.Failure(
                false, 
                ServiceError.DependencyExists("Equipment", "exercises that are in use"));
        }
        
        return ServiceResult<bool>.Success(true);
    }
    
    // Abstract method implementations
    
    protected override async Task<IEnumerable<Equipment>> LoadAllEntitiesAsync(
        IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.GetAllAsync();
    }
    
    protected override async Task<Equipment?> LoadEntityByIdAsync(
        IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, 
        string id)
    {
        var equipmentId = EquipmentId.ParseOrEmpty(id);
        if (equipmentId.IsEmpty)
            return null;
            
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.GetByIdAsync(equipmentId);
    }
    
    protected override async Task<Equipment?> LoadEntityByIdAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork, 
        string id)
    {
        var equipmentId = EquipmentId.ParseOrEmpty(id);
        if (equipmentId.IsEmpty)
            return null;
            
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.GetByIdAsync(equipmentId);
    }
    
    protected override EquipmentDto MapToDto(Equipment entity) =>
        new()
        {
            Id = entity.EquipmentId.ToString(),
            Name = entity.Name,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    
    protected override EquipmentDto CreateEmptyDto() =>
        new()
        {
            Id = string.Empty,
            Name = string.Empty,
            IsActive = false,
            CreatedAt = DateTime.MinValue,
            UpdatedAt = null
        };
    
    protected override ValidationResult ValidateAndParseId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return ValidationResult.Failure(EquipmentErrorMessages.Validation.IdCannotBeEmpty);
            
        var equipmentId = EquipmentId.ParseOrEmpty(id);
        return equipmentId.IsEmpty && !string.IsNullOrWhiteSpace(id)
            ? ValidationResult.Failure(string.Format(EquipmentErrorMessages.Validation.InvalidIdFormat, id))
            : ValidationResult.Success();
    }
    
    protected override async Task<ValidationResult> ValidateCreateCommand(CreateEquipmentCommand command)
    {
        if (command == null)
            return ValidationResult.Failure(EquipmentErrorMessages.Validation.RequestCannotBeNull);
            
        if (string.IsNullOrWhiteSpace(command.Name))
            return ValidationResult.Failure(EquipmentErrorMessages.Validation.NameCannotBeEmpty);
            
        // Check for duplicate name
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        return await repository.ExistsAsync(command.Name.Trim())
            ? ValidationResult.Failure(string.Format(EquipmentErrorMessages.BusinessRules.DuplicateNameFormat, command.Name))
            : ValidationResult.Success();
    }
    
    protected override async Task<ValidationResult> ValidateUpdateCommand(string id, UpdateEquipmentCommand command)
    {
        if (command == null)
            return ValidationResult.Failure(EquipmentErrorMessages.Validation.RequestCannotBeNull);
            
        if (string.IsNullOrWhiteSpace(command.Name))
            return ValidationResult.Failure(EquipmentErrorMessages.Validation.NameCannotBeEmpty);
            
        var equipmentId = EquipmentId.ParseOrEmpty(id);
        if (equipmentId.IsEmpty)
            return ValidationResult.Failure(EquipmentErrorMessages.Validation.InvalidEquipmentId);
            
        // Check for duplicate name (excluding current)
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        return await repository.ExistsAsync(command.Name.Trim(), equipmentId)
            ? ValidationResult.Failure(string.Format(EquipmentErrorMessages.BusinessRules.DuplicateNameFormat, command.Name))
            : ValidationResult.Success();
    }
    
    protected override async Task<Equipment> CreateEntityAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork, 
        CreateEquipmentCommand command)
    {
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var equipment = Equipment.Handler.CreateNew(command.Name.Trim());
        return await repository.CreateAsync(equipment);
    }
    
    protected override async Task<Equipment> UpdateEntityAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork, 
        Equipment existingEntity, 
        UpdateEquipmentCommand command)
    {
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var updated = Equipment.Handler.Update(existingEntity, command.Name.Trim());
        return await repository.UpdateAsync(updated);
    }
    
    protected override async Task<bool> DeleteEntityAsync(
        IWritableUnitOfWork<FitnessDbContext> unitOfWork, 
        string id)
    {
        var equipmentId = EquipmentId.ParseOrEmpty(id);
        if (equipmentId.IsEmpty)
            return false;
            
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.DeactivateAsync(equipmentId);
    }
}