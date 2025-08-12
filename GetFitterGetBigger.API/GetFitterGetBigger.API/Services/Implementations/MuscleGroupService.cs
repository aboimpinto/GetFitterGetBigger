using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.MuscleGroup;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for muscle group operations
/// MuscleGroup is a LookupTable that does NOT implement caching
/// </summary>
public class MuscleGroupService(IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider, ILogger<MuscleGroupService> logger, IBodyPartService bodyPartService) : IMuscleGroupService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider = unitOfWorkProvider;
    private readonly ILogger<MuscleGroupService> _logger = logger;
    private readonly IBodyPartService _bodyPartService = bodyPartService;
    
    /// <summary>
    /// Gets all active muscle groups
    /// </summary>
    public async Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetAllAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entities = await repository.GetAllAsync();
        
        var dtos = entities.Select(MapToDto).ToList();
        return ServiceResult<IEnumerable<MuscleGroupDto>>.Success(dtos);
    }
    
    /// <summary>
    /// Gets muscle group by ID using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<MuscleGroupDto>> GetByIdAsync(MuscleGroupId id)
    {
        // Handle empty ID validation directly to ensure correct error code
        if (id.IsEmpty)
        {
            return ServiceResult<MuscleGroupDto>.Failure(
                MuscleGroupDto.Empty, 
                ServiceError.InvalidFormat("MuscleGroupId", "GUID format"));
        }
        
        return await LoadMuscleGroupByIdAsync(id);
    }
    
    /// <summary>
    /// Creates new muscle group
    /// </summary>
    public async Task<ServiceResult<MuscleGroupDto>> CreateAsync(CreateMuscleGroupCommand command)
    {
        // Minimal defensive check for null command - fail early with clear error
        if (command == null)
            return ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.RequestCannotBeNull));
            
        // Now we can trust command is not null
        return await ServiceValidate.Build<MuscleGroupDto>()
            .EnsureNotWhiteSpace(command.Name, MuscleGroupErrorMessages.Validation.NameCannotBeEmpty)
            .EnsureMaxLength(command.Name, 100, MuscleGroupErrorMessages.Validation.NameTooLong)
            .EnsureNotEmpty(command.BodyPartId, MuscleGroupErrorMessages.Validation.BodyPartIdRequired)
            .EnsureAsync(
                async () => await BodyPartExistsAsync(command.BodyPartId),
                ServiceError.NotFound("BodyPart"))
            .EnsureIsUniqueAsync(
                async () => await IsMuscleGroupNameUniqueAsync(command.Name),
                "MuscleGroup", 
                command.Name)
            .MatchAsync(
                whenValid: async () => await CreateMuscleGroupAsync(command),
                whenInvalid: errors => ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }
    
    /// <summary>
    /// Updates existing muscle group using command and strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<MuscleGroupDto>> UpdateAsync(MuscleGroupId id, UpdateMuscleGroupCommand command)
    {
        // Minimal defensive check for null command - fail early with clear error
        if (command == null)
            return ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.RequestCannotBeNull));
            
        // Now we can trust command is not null
        return await ServiceValidate.Build<MuscleGroupDto>()
            .EnsureValidId(id, MuscleGroupErrorMessages.Validation.InvalidMuscleGroupId)
            .EnsureNotWhiteSpace(command.Name, MuscleGroupErrorMessages.Validation.NameCannotBeEmpty)
            .EnsureMaxLength(command.Name, 100, MuscleGroupErrorMessages.Validation.NameTooLong)
            .EnsureNotEmpty(command.BodyPartId, MuscleGroupErrorMessages.Validation.BodyPartIdRequired)
            .EnsureExistsAsync(
                async () => await MuscleGroupExistsAsync(id),
                "MuscleGroup")
            .EnsureAsync(
                async () => await BodyPartExistsAsync(command.BodyPartId),
                ServiceError.NotFound("BodyPart"))
            .EnsureIsUniqueAsync(
                async () => await IsMuscleGroupNameUniqueForUpdateAsync(command.Name, id),
                "MuscleGroup", 
                command.Name)
            .MatchAsync(
                whenValid: async () => await UpdateMuscleGroupAsync(id, command),
                whenInvalid: errors => ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }
    
    /// <summary>
    /// Deletes muscle group using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteAsync(MuscleGroupId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureValidId(id, MuscleGroupErrorMessages.Validation.InvalidMuscleGroupId)
            .EnsureExistsAsync(
                async () => await MuscleGroupExistsAsync(id),
                "MuscleGroup")
            .EnsureAsync(
                async () => await CanDeleteMuscleGroupAsync(id),
                ServiceError.DependencyExists("MuscleGroup", "exercises that are in use"))
            .MatchAsync(
                whenValid: async () => await DeleteMuscleGroupAsync(id),
                whenInvalid: errors => ServiceResult<bool>.Failure(false, errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown error"))
            );
    }
    
    /// <summary>
    /// Checks if muscle group exists using strongly-typed ID - returns boolean result
    /// </summary>
    public async Task<ServiceResult<bool>> CheckExistsAsync(MuscleGroupId id)
    {
        // Handle empty ID validation directly to ensure correct error code
        if (id.IsEmpty)
        {
            return ServiceResult<bool>.Failure(
                false, 
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.InvalidMuscleGroupId));
        }
        
        return await CheckMuscleGroupExistenceAsync(id);
    }
    
    /// <summary>
    /// Gets muscle group by name (case-insensitive)
    /// </summary>
    public async Task<ServiceResult<MuscleGroupDto>> GetByNameAsync(string name)
    {
        return await ServiceValidate.For<MuscleGroupDto>()
            .EnsureNotWhiteSpace(name, MuscleGroupErrorMessages.Validation.NameCannotBeEmptyForSearch)
            .Match(
                whenValid: async () => await LoadMuscleGroupByNameAsync(name),
                whenInvalid: errors => ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, errors.ToArray())
            );
    }
    
    
    /// <summary>
    /// Gets muscle groups by body part
    /// </summary>
    public async Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetByBodyPartAsync(BodyPartId bodyPartId)
    {
        return await ServiceValidate.For<IEnumerable<MuscleGroupDto>>()
            .EnsureValidId(bodyPartId, MuscleGroupErrorMessages.Validation.BodyPartIdCannotBeEmptyForSearch)
            .Match(
                whenValid: async () => await LoadMuscleGroupsByBodyPartAsync(bodyPartId),
                whenInvalid: errors => ServiceResult<IEnumerable<MuscleGroupDto>>.Failure([], errors.ToArray())
            );
    }
    
    
    
    
    
    // Positive helper methods - clear intent
    private async Task<bool> MuscleGroupExistsAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.ExistsAsync(id);
    }
    
    private async Task<bool> CanDeleteMuscleGroupAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.CanDeactivateAsync(id);
    }

    private async Task<bool> IsMuscleGroupNameUniqueAsync(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return !await repository.ExistsByNameAsync(name.Trim());
    }
    
    private async Task<bool> IsMuscleGroupNameUniqueForUpdateAsync(string name, MuscleGroupId excludeId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return !await repository.ExistsByNameAsync(name.Trim(), excludeId);
    }

    private async Task<bool> BodyPartExistsAsync(BodyPartId bodyPartId)
    {
        var existsResult = await _bodyPartService.ExistsAsync(bodyPartId);
        return existsResult.IsSuccess && existsResult.Data.Value;
    }
    
    
    
    private async Task<ServiceResult<MuscleGroupDto>> LoadMuscleGroupByIdAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entity = await repository.GetByIdAsync(id);
        
        return entity switch
        {
            null or { IsEmpty: true } => ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, ServiceError.NotFound("MuscleGroup")),
            { IsActive: false } => ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, ServiceError.NotFound("MuscleGroup")),
            _ => ServiceResult<MuscleGroupDto>.Success(MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<MuscleGroupDto>> LoadMuscleGroupByNameAsync(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entity = await repository.GetByNameAsync(name);
        
        return entity switch
        {
            null or { IsEmpty: true } => ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, ServiceError.NotFound("MuscleGroup")),
            { IsActive: false } => ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, ServiceError.NotFound("MuscleGroup")),
            _ => ServiceResult<MuscleGroupDto>.Success(MapToDto(entity))
        };
    }
    
    private async Task<ServiceResult<IEnumerable<MuscleGroupDto>>> LoadMuscleGroupsByBodyPartAsync(BodyPartId bodyPartId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entities = await repository.GetByBodyPartAsync(bodyPartId);
        
        var dtos = entities.Select(MapToDto).ToList();
        return ServiceResult<IEnumerable<MuscleGroupDto>>.Success(dtos);
    }

    private async Task<ServiceResult<MuscleGroupDto>> CreateMuscleGroupAsync(CreateMuscleGroupCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        var muscleGroup = MuscleGroup.Handler.CreateNew(command.Name.Trim(), command.BodyPartId);
        var createdEntity = await repository.CreateAsync(muscleGroup);
        await unitOfWork.CommitAsync();
        
        return ServiceResult<MuscleGroupDto>.Success(MapToDto(createdEntity));
    }
    
    private async Task<ServiceResult<MuscleGroupDto>> UpdateMuscleGroupAsync(MuscleGroupId id, UpdateMuscleGroupCommand command)
    {
        var existingEntity = await LoadEntityForUpdateAsync(id);
        
        // Pattern matching for single exit point
        return existingEntity.IsEmpty switch
        {
            true => ServiceResult<MuscleGroupDto>.Failure(MuscleGroupDto.Empty, ServiceError.NotFound("MuscleGroup")),
            false => await PerformUpdateAsync(existingEntity, command)
        };
    }
    
    private async Task<ServiceResult<MuscleGroupDto>> PerformUpdateAsync(MuscleGroup existingEntity, UpdateMuscleGroupCommand command)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        var updated = MuscleGroup.Handler.Update(existingEntity, command.Name.Trim(), command.BodyPartId);
        var updatedEntity = await repository.UpdateAsync(updated);
        await unitOfWork.CommitAsync();
        
        return ServiceResult<MuscleGroupDto>.Success(MapToDto(updatedEntity));
    }
    
    private async Task<ServiceResult<bool>> CheckMuscleGroupExistenceAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var exists = await repository.ExistsAsync(id);
        return ServiceResult<bool>.Success(exists);
    }
    
    private async Task<ServiceResult<bool>> DeleteMuscleGroupAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        var success = await repository.DeactivateAsync(id);
        await unitOfWork.CommitAsync();
        
        return ServiceResult<bool>.Success(success);
    }
    
    private async Task<MuscleGroup> LoadEntityForUpdateAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entity = await repository.GetByIdAsync(id);
        return entity ?? MuscleGroup.Empty;
    }
    
    private static MuscleGroupDto MapToDto(MuscleGroup entity)
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