using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Commands.Equipment;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
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
        await base.GetByIdAsync(id);
    
    /// <summary>
    /// Updates existing equipment using command and strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateEquipmentCommand command) =>
        await base.UpdateAsync(id, command);
    
    /// <summary>
    /// Deletes equipment using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteAsync(EquipmentId id)
    {
        var existingResult = await ExistsAsync(id);
        var result = existingResult.IsSuccess switch
        {
            false => ConvertToDeleteResult(existingResult),
            true => await ProcessEquipmentDeleteAsync(id)
        };
        
        return result;
    }
    
    private ServiceResult<bool> ConvertToDeleteResult(ServiceResult<EquipmentDto> existingResult)
    {
        return existingResult.StructuredErrors.Any() switch
        {
            true => ServiceResult<bool>.Failure(false, existingResult.StructuredErrors.ToArray()),
            false => ServiceResult<bool>.Failure(false, existingResult.Errors)
        };
    }
    
    private async Task<ServiceResult<bool>> ProcessEquipmentDeleteAsync(EquipmentId id)
    {
        var inUseResult = await CheckIfInUseAsync(id);
        return inUseResult.IsSuccess switch
        {
            false => inUseResult,
            true => await base.DeleteAsync(id)
        };
    }
    
    /// <summary>
    /// Checks if equipment exists using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> ExistsAsync(EquipmentId id) =>
        await base.ExistsAsync(id);
    
    /// <summary>
    /// Gets equipment by name (case-insensitive)
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> GetByNameAsync(string name)
    {
        var result = string.IsNullOrWhiteSpace(name) switch
        {
            true => ServiceResult<EquipmentDto>.Failure(
                CreateEmptyDto(),
                EquipmentErrorMessages.Validation.NameCannotBeEmpty),
            false => await ProcessGetByNameAsync(name)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<EquipmentDto>> ProcessGetByNameAsync(string name)
    {
        var cacheKey = GetCacheKey($"name:{name.ToLowerInvariant()}");
        var cacheService = (ICacheService)_cacheService;
        var cached = await cacheService.GetAsync<EquipmentDto>(cacheKey);
        
        var result = cached switch
        {
            not null => LogCacheHitAndReturn(cached, name),
            _ => await LoadEquipmentByNameAsync(name, cacheKey)
        };
        
        return result;
    }
    
    
    private ServiceResult<EquipmentDto> LogCacheHitAndReturn(EquipmentDto cached, string name)
    {
        _logger.LogDebug("Cache hit for Equipment by name: {Name}", name);
        return ServiceResult<EquipmentDto>.Success(cached);
    }
    
    private async Task<ServiceResult<EquipmentDto>> LoadEquipmentByNameAsync(
        string name, 
        string cacheKey)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entity = await repository.GetByNameAsync(name);
        
        var result = entity switch
        {
            null => ServiceResult<EquipmentDto>.Failure(
                CreateEmptyDto(),
                ServiceError.NotFound("Equipment")),
            { IsEmpty: true } or { IsActive: false } => ServiceResult<EquipmentDto>.Failure(
                CreateEmptyDto(),
                ServiceError.NotFound("Equipment")),
            _ => await CreateSuccessResultWithCachingAsync(entity, cacheKey)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<EquipmentDto>> CreateSuccessResultWithCachingAsync(
        Equipment entity,
        string cacheKey)
    {
        var dto = MapToDto(entity);
        var cacheService = (ICacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        return ServiceResult<EquipmentDto>.Success(dto);
    }
    
    private async Task<ServiceResult<bool>> CheckIfInUseAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var isInUse = await repository.IsInUseAsync(id);
        
        var result = isInUse switch
        {
            true => LogAndReturnInUseError(id),
            false => ServiceResult<bool>.Success(true)
        };
        
        return result;
    }
    
    private ServiceResult<bool> LogAndReturnInUseError(EquipmentId id)
    {
        _logger.LogWarning("Cannot delete equipment with ID {Id} as it is in use by exercises", id);
        return ServiceResult<bool>.Failure(
            false, 
            ServiceError.DependencyExists("Equipment", "exercises that are in use"));
    }
    
    protected override async Task<IEnumerable<Equipment>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.GetAllAsync();
    }
    
    protected override async Task<Equipment> LoadEntityByIdAsync(ISpecializedIdBase id)
    {
        var equipmentId = (EquipmentId)id;
        
        return equipmentId.IsEmpty switch
        {
            true => Equipment.Empty,
            false => await LoadFromRepositoryAsync(equipmentId)
        };
    }
    
    private async Task<Equipment> LoadFromRepositoryAsync(EquipmentId equipmentId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entity = await repository.GetByIdAsync(equipmentId);
        return entity ?? Equipment.Empty;
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
    
    
    protected override async Task<ValidationResult> ValidateCreateCommand(CreateEquipmentCommand command)
    {
        var basicValidation = ServiceValidate.For()
            .EnsureNotNull(command, EquipmentErrorMessages.Validation.RequestCannotBeNull)
            .EnsureNotWhiteSpace(command?.Name, EquipmentErrorMessages.Validation.NameCannotBeEmpty);
            
        var result = command switch
        {
            null => basicValidation.ToResult(),
            _ => await ValidateWithDuplicateCheckAsync(basicValidation, command.Name)
        };
        
        return result;
    }
    
    private async Task<ValidationResult> ValidateWithDuplicateCheckAsync(
        ServiceValidation validation,
        string name)
    {
        var enhancedValidation = await validation.EnsureAsync(
            async () => !await CheckDuplicateNameAsync(name),
            ServiceError.AlreadyExists("Equipment", name));
            
        return enhancedValidation.ToResult();
    }
    
    private async Task<bool> CheckDuplicateNameAsync(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.ExistsAsync(name.Trim());
    }
    
    protected override async Task<ValidationResult> ValidateUpdateCommand(ISpecializedIdBase id, UpdateEquipmentCommand command)
    {
        var equipmentId = (EquipmentId)id;
        
        var basicValidation = ServiceValidate.For()
            .EnsureNotNull(command, EquipmentErrorMessages.Validation.RequestCannotBeNull)
            .EnsureNotWhiteSpace(command?.Name, EquipmentErrorMessages.Validation.NameCannotBeEmpty)
            .Ensure(() => !equipmentId.IsEmpty, EquipmentErrorMessages.Validation.InvalidEquipmentId);
            
        var result = command switch
        {
            null => basicValidation.ToResult(),
            _ => await ValidateUpdateWithDuplicateCheckAsync(basicValidation, command.Name, equipmentId)
        };
        
        return result;
    }
    
    private async Task<ValidationResult> ValidateUpdateWithDuplicateCheckAsync(
        ServiceValidation validation,
        string name,
        EquipmentId excludeId)
    {
        var enhancedValidation = await validation.EnsureAsync(
            async () => !await CheckDuplicateNameAsync(name, excludeId),
            ServiceError.AlreadyExists("Equipment", name));
            
        return enhancedValidation.ToResult();
    }
    
    private async Task<bool> CheckDuplicateNameAsync(string name, EquipmentId excludeId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.ExistsAsync(name.Trim(), excludeId);
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
        ISpecializedIdBase id)
    {
        var equipmentId = (EquipmentId)id;
        if (equipmentId.IsEmpty)
            return false;
            
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.DeactivateAsync(equipmentId);
    }
}