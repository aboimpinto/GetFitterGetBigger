using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.Equipment;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for equipment operations
/// Equipment is a LookupTable that does NOT implement caching
/// </summary>
public class EquipmentService(IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider, ILogger<EquipmentService> logger) : IEquipmentService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<EquipmentService> _logger = logger;

    /// <summary>
    /// Gets all active equipment
    /// </summary>
    public async Task<ServiceResult<IEnumerable<EquipmentDto>>> GetAllAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entities = await repository.GetAllAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        return ServiceResult<IEnumerable<EquipmentDto>>.Success(dtos);
    }
    
    /// <summary>
    /// Gets equipment by ID using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> GetByIdAsync(EquipmentId id)
    {
        return await ServiceValidate.For<EquipmentDto>()
            .EnsureValidId(id, EquipmentErrorMessages.Validation.InvalidEquipmentId)
            .Match(
                whenValid: async () => await LoadEquipmentByIdAsync(id),
                whenInvalid: errors => ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, errors.ToArray())
            );
    }

    /// <summary>
    /// Gets equipment by name (case-insensitive)
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> GetByNameAsync(string name)
    {
        return await ServiceValidate.For<EquipmentDto>()
            .EnsureNotWhiteSpace(name, EquipmentErrorMessages.Validation.NameCannotBeEmpty)
            .Match(
                whenValid: async () => await LoadEquipmentByNameAsync(name),
                whenInvalid: errors => ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, errors.ToArray())
            );
    }
    
    /// <summary>
    /// Creates new equipment
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> CreateAsync(CreateEquipmentCommand command)
    {
        // Minimal defensive check for null command - fail early with clear error
        if (command == null)
            return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, ServiceError.ValidationFailed(EquipmentErrorMessages.Validation.RequestCannotBeNull));
            
        // Now we can trust command is not null
        return await ServiceValidate.Build<EquipmentDto>()
            .EnsureNotWhiteSpace(command.Name, EquipmentErrorMessages.Validation.NameCannotBeEmpty)
            .EnsureIsUniqueAsync(
                async () => await IsEquipmentNameUniqueAsync(command.Name),
                "Equipment", 
                command.Name)
            .MatchAsync(
                whenValid: async () => await CreateEquipmentAsync(command),
                whenInvalid: errors => ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }
    
    /// <summary>
    /// Updates existing equipment using command and strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> UpdateAsync(EquipmentId id, UpdateEquipmentCommand command)
    {
        // Minimal defensive check for null command - fail early with clear error
        if (command == null)
            return ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, ServiceError.ValidationFailed(EquipmentErrorMessages.Validation.RequestCannotBeNull));
            
        // Now we can trust command is not null
        return await ServiceValidate.Build<EquipmentDto>()
            .EnsureValidId(id, EquipmentErrorMessages.Validation.InvalidEquipmentId)
            .EnsureNotWhiteSpace(command.Name, EquipmentErrorMessages.Validation.NameCannotBeEmpty)
            .EnsureExistsAsync(
                async () => await EquipmentExistsAsync(id),
                "Equipment")
            .EnsureIsUniqueAsync(
                async () => await IsEquipmentNameUniqueForUpdateAsync(command.Name, id),
                "Equipment", 
                command.Name)
            .MatchAsync(
                whenValid: async () => await UpdateEquipmentAsync(id, command),
                whenInvalid: errors => ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }

    /// <summary>
    /// Deletes equipment using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteAsync(EquipmentId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureValidId(id, EquipmentErrorMessages.Validation.InvalidEquipmentId)
            .EnsureExistsAsync(
                async () => await EquipmentExistsAsync(id),
                "Equipment")
            .EnsureAsync(
                async () => await CanDeleteEquipmentAsync(id),
                ServiceError.DependencyExists("Equipment", "exercises that are in use"))
            .MatchAsync(
                whenValid: async () => await DeleteEquipmentAsync(id),
                whenInvalid: errors => ServiceResult<bool>.Failure(false, errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }

    /// <summary>
    /// Checks if equipment exists using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<EquipmentDto>> ExistsAsync(EquipmentId id)
    {
        return await ServiceValidate.For<EquipmentDto>()
            .EnsureValidId(id, EquipmentErrorMessages.Validation.InvalidEquipmentId)
            .Match(
                whenValid: async () => await LoadEquipmentByIdAsync(id),
                whenInvalid: errors => ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, errors.ToArray())
            );
    }

    private async Task<ServiceResult<EquipmentDto>> LoadEquipmentByIdAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        var dto = entity?.IsEmpty != false ? EquipmentDto.Empty : MapToDto(entity);
        return ServiceResult<EquipmentDto>.Success(dto);
    }
    
    private async Task<ServiceResult<EquipmentDto>> LoadEquipmentByNameAsync(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entity = await repository.GetByNameAsync(name);
        
        var dto = entity?.IsEmpty != false ? EquipmentDto.Empty : MapToDto(entity);
        return ServiceResult<EquipmentDto>.Success(dto);
    }

    private async Task<ServiceResult<EquipmentDto>> CreateEquipmentAsync(CreateEquipmentCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        var equipment = Equipment.Handler.CreateNew(command.Name.Trim());
        var createdEntity = await repository.CreateAsync(equipment);
        await unitOfWork.CommitAsync();
        
        return ServiceResult<EquipmentDto>.Success(MapToDto(createdEntity));
    }
    
    private async Task<ServiceResult<EquipmentDto>> UpdateEquipmentAsync(EquipmentId id, UpdateEquipmentCommand command)
    {
        var existingEntity = await LoadEntityForUpdateAsync(id);
        
        // Pattern matching for single exit point
        return existingEntity.IsEmpty switch
        {
            true => ServiceResult<EquipmentDto>.Failure(EquipmentDto.Empty, ServiceError.NotFound("Equipment")),
            false => await PerformUpdateAsync(existingEntity, command)
        };
    }
    
    private async Task<ServiceResult<EquipmentDto>> PerformUpdateAsync(Equipment existingEntity, UpdateEquipmentCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        var updated = Equipment.Handler.Update(existingEntity, command.Name.Trim());
        var updatedEntity = await repository.UpdateAsync(updated);
        await unitOfWork.CommitAsync();
        
        return ServiceResult<EquipmentDto>.Success(MapToDto(updatedEntity));
    }
    
    private async Task<ServiceResult<bool>> DeleteEquipmentAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        var success = await repository.DeactivateAsync(id);
        await unitOfWork.CommitAsync();
        
        return ServiceResult<bool>.Success(success);
    }
    
    private async Task<Equipment> LoadEntityForUpdateAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var entity = await repository.GetByIdAsync(id);
        return entity ?? Equipment.Empty;
    }
    
    // Positive helper methods - clear intent
    private async Task<bool> EquipmentExistsAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return await repository.ExistsAsync(id);
    }
    
    private async Task<bool> CanDeleteEquipmentAsync(EquipmentId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return !await repository.IsInUseAsync(id);
    }

    private async Task<bool> IsEquipmentNameUniqueAsync(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return !await repository.ExistsAsync(name.Trim());
    }
    
    private async Task<bool> IsEquipmentNameUniqueForUpdateAsync(string name, EquipmentId excludeId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        return !await repository.ExistsAsync(name.Trim(), excludeId);
    }
    
    private static EquipmentDto MapToDto(Equipment entity)
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