using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Cache;
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
        return await ServiceValidate.Build<bool>()
            .Ensure(() => !id.IsEmpty, EquipmentErrorMessages.Validation.InvalidEquipmentId)
            .EnsureExistsAsync(
                async () => (await ExistsAsync(id)).IsSuccess,
                ServiceError.NotFound("Equipment"))
            .EnsureAsync(
                async () => !await IsInUseAsync(id),
                ServiceError.DependencyExists("Equipment", "exercises that are in use"))
            .MatchAsync(
                whenValid: async () => await PerformDeleteAsync(id),
                whenInvalid: (IReadOnlyList<ServiceError> errors) => ServiceResult<bool>.Failure(false, errors.ToArray())
            );
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
        return await ServiceValidate.For<EquipmentDto>()
            .EnsureNotWhiteSpace(name, EquipmentErrorMessages.Validation.NameCannotBeEmpty)
            .Match(
                whenValid: async () => await ProcessGetByNameAsync(name),
                whenInvalid: errors => ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, errors.ToArray())
            );
    }

    private async Task<ServiceResult<EquipmentDto>> ProcessGetByNameAsync(string name)
    {
        var cacheKey = GetCacheKey($"name:{name.ToLowerInvariant()}");
        var cacheService = (ICacheService)_cacheService;

        return await CacheLoad.For<EquipmentDto>(cacheService, cacheKey)
            .WithLogging(_logger, "Equipment by name")
            .MatchAsync(
                onHit: cached => ServiceResult<EquipmentDto>.Success(cached),
                onMiss: async () => await LoadEquipmentByNameAsync(name, cacheKey)
            );
    }

    private async Task<ServiceResult<EquipmentDto>> LoadEquipmentByNameAsync(string name, string cacheKey)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entity = await repository.GetByNameAsync(name);

        var result = entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, ServiceError.NotFound("Equipment")),
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

    private async Task<bool> IsInUseAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.IsInUseAsync(id);
    }

    private async Task<ServiceResult<bool>> PerformDeleteAsync(EquipmentId id)
    {
        // Use the base class DeleteAsync which handles the UnitOfWork properly
        return await base.DeleteAsync(id);
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
        return (entity == null || entity.IsEmpty) ? Equipment.Empty : entity;
    }

    protected override EquipmentDto MapToDto(Equipment entity)
    {
        if (entity == null)
            return EquipmentDto.Empty;
            
        return new()
        {
            Id = entity.EquipmentId.ToString(),
            Name = entity.Name,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override async Task<ValidationResult> ValidateCreateCommand(CreateEquipmentCommand command)
    {
        var validation = await ServiceValidate.Build()
            .EnsureNotNull(command, EquipmentErrorMessages.Validation.RequestCannotBeNull)
            .EnsureNotWhiteSpace(command?.Name, EquipmentErrorMessages.Validation.NameCannotBeEmpty)
            .EnsureAsync(
                async () => command == null || !await CheckDuplicateNameAsync(command.Name),
                ServiceError.AlreadyExists("Equipment", command?.Name ?? string.Empty))
            .ToValidationResultAsync();

        return validation;
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

        var validation = await ServiceValidate.Build()
            .EnsureNotNull(command, EquipmentErrorMessages.Validation.RequestCannotBeNull)
            .EnsureNotWhiteSpace(command?.Name, EquipmentErrorMessages.Validation.NameCannotBeEmpty)
            .Ensure(() => !equipmentId.IsEmpty, EquipmentErrorMessages.Validation.InvalidEquipmentId)
            .EnsureAsync(
                async () => command == null || !await CheckDuplicateNameAsync(command.Name, equipmentId),
                ServiceError.AlreadyExists("Equipment", command?.Name ?? string.Empty))
            .ToValidationResultAsync();

        return validation;
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

    /// <summary>
    /// Checks if an equipment entity exists and is active
    /// </summary>
    protected override async Task<bool> CheckEntityExistsAsync(ISpecializedIdBase id)
    {
        var equipmentId = (EquipmentId)id;
        if (equipmentId.IsEmpty)
            return false;

        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.ExistsAsync(equipmentId);
    }

    /// <summary>
    /// Validates if the equipment ID is properly formed and not empty
    /// </summary>
    protected override bool IsValidId(ISpecializedIdBase id)
    {
        if (id is not EquipmentId equipmentId)
            return false;

        return !equipmentId.IsEmpty;
    }
}