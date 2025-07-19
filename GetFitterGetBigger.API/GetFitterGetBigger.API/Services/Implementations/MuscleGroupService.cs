using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Commands.MuscleGroup;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Microsoft.Extensions.Logging;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for muscle group operations
/// </summary>
public class MuscleGroupService : EnhancedReferenceService<MuscleGroup, MuscleGroupDto, CreateMuscleGroupCommand, UpdateMuscleGroupCommand>, IMuscleGroupService
{
    private readonly IBodyPartService _bodyPartService;
    
    public MuscleGroupService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger<MuscleGroupService> logger,
        IBodyPartService bodyPartService)
        : base(unitOfWorkProvider, cacheService, logger)
    {
        _bodyPartService = bodyPartService;
    }
    
    /// <summary>
    /// Gets muscle group by ID using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<MuscleGroupDto>> GetByIdAsync(MuscleGroupId id) =>
        await base.GetByIdAsync(id);
    
    /// <summary>
    /// Updates existing muscle group using command and strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<MuscleGroupDto>> UpdateAsync(MuscleGroupId id, UpdateMuscleGroupCommand command) =>
        await base.UpdateAsync(id, command);
    
    /// <summary>
    /// Deletes muscle group using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteAsync(MuscleGroupId id)
    {
        var existingResult = await ExistsAsync(id);
        var result = existingResult.IsSuccess switch
        {
            false => ConvertToDeleteResult(existingResult),
            true => await ProcessMuscleGroupDeleteAsync(id)
        };
        
        return result;
    }
    
    private ServiceResult<bool> ConvertToDeleteResult(ServiceResult<MuscleGroupDto> existingResult)
    {
        return existingResult.StructuredErrors.Any() switch
        {
            true => ServiceResult<bool>.Failure(false, existingResult.StructuredErrors.ToArray()),
            false => ServiceResult<bool>.Failure(false, existingResult.Errors)
        };
    }
    
    private async Task<ServiceResult<bool>> ProcessMuscleGroupDeleteAsync(MuscleGroupId id)
    {
        var inUseResult = await CheckIfInUseAsync(id);
        return inUseResult.IsSuccess switch
        {
            false => inUseResult,
            true => await base.DeleteAsync(id)
        };
    }
    
    /// <summary>
    /// Checks if muscle group exists using strongly-typed ID
    /// </summary>
    public async Task<ServiceResult<MuscleGroupDto>> ExistsAsync(MuscleGroupId id) =>
        await base.ExistsAsync(id);
    
    /// <summary>
    /// Gets muscle group by name (case-insensitive)
    /// </summary>
    public async Task<ServiceResult<MuscleGroupDto>> GetByNameAsync(string name)
    {
        var result = string.IsNullOrWhiteSpace(name) switch
        {
            true => ServiceResult<MuscleGroupDto>.Failure(CreateEmptyDto(), "Muscle group name cannot be empty"),
            false => await ProcessGetByNameAsync(name)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<MuscleGroupDto>> ProcessGetByNameAsync(string name)
    {
        var cacheKey = GetCacheKey($"name:{name.ToLowerInvariant()}");
        // Note: The cast is required because base class stores cache service as object to support multiple cache types
        var cacheService = (ICacheService)_cacheService;
        var cached = await cacheService.GetAsync<MuscleGroupDto>(cacheKey);
        
        return cached switch
        {
            not null => ServiceResult<MuscleGroupDto>.Success(cached),
            null => await LoadMuscleGroupByNameAsync(name)
        };
    }
    
    private async Task<ServiceResult<MuscleGroupDto>> LoadMuscleGroupByNameAsync(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entity = await repository.GetByNameAsync(name);
        
        return entity switch
        {
            { IsEmpty: true } or { IsActive: false } => ServiceResult<MuscleGroupDto>.Failure(CreateEmptyDto(), ServiceError.NotFound("MuscleGroup")),
            _ => await MapAndCacheByNameAsync(entity, name)
        };
    }
    
    private async Task<ServiceResult<MuscleGroupDto>> MapAndCacheByNameAsync(MuscleGroup entity, string name)
    {
        var dto = MapToDto(entity);
        var cacheKey = GetCacheKey($"name:{name.ToLowerInvariant()}");
        // Note: The cast is required because base class stores cache service as object to support multiple cache types
        var cacheService = (ICacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dto);
        
        return ServiceResult<MuscleGroupDto>.Success(dto);
    }
    
    /// <summary>
    /// Gets muscle groups by body part
    /// </summary>
    public async Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetByBodyPartAsync(BodyPartId bodyPartId)
    {
        var result = bodyPartId.IsEmpty switch
        {
            true => ServiceResult<IEnumerable<MuscleGroupDto>>.Failure(new List<MuscleGroupDto>(), "Body part ID cannot be empty"),
            false => await ProcessGetByBodyPartAsync(bodyPartId)
        };
        
        return result;
    }
    
    private async Task<ServiceResult<IEnumerable<MuscleGroupDto>>> ProcessGetByBodyPartAsync(BodyPartId bodyPartId)
    {
        var cacheKey = GetCacheKey($"byBodyPart:{bodyPartId}");
        // Note: The cast is required because base class stores cache service as object to support multiple cache types
        var cacheService = (ICacheService)_cacheService;
        var cached = await cacheService.GetAsync<IEnumerable<MuscleGroupDto>>(cacheKey);
        
        return cached switch
        {
            not null => ServiceResult<IEnumerable<MuscleGroupDto>>.Success(cached),
            null => await LoadMuscleGroupsByBodyPartAsync(bodyPartId)
        };
    }
    
    private async Task<ServiceResult<IEnumerable<MuscleGroupDto>>> LoadMuscleGroupsByBodyPartAsync(BodyPartId bodyPartId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entities = await repository.GetByBodyPartAsync(bodyPartId);
        
        var dtos = entities.Select(MapToDto).ToList();
        
        var cacheKey = GetCacheKey($"byBodyPart:{bodyPartId}");
        // Note: The cast is required because base class stores cache service as object to support multiple cache types
        var cacheService = (ICacheService)_cacheService;
        await cacheService.SetAsync(cacheKey, dtos);
        
        return ServiceResult<IEnumerable<MuscleGroupDto>>.Success(dtos);
    }
    
    /// <summary>
    /// Checks if muscle group is being used by any active exercises
    /// </summary>
    private async Task<ServiceResult<bool>> CheckIfInUseAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        var canDeactivate = await repository.CanDeactivateAsync(id);
        
        return canDeactivate switch
        {
            true => ServiceResult<bool>.Success(true),
            false => ServiceResult<bool>.Failure(false, ServiceError.DependencyExists("MuscleGroup", "exercises"))
        };
    }
    
    // Abstract method implementations
    
    /// <summary>
    /// Loads all active muscle groups from the database
    /// </summary>
    protected override async Task<IEnumerable<MuscleGroup>> LoadAllEntitiesAsync()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.GetAllAsync();
    }
    
    /// <summary>
    /// Loads a muscle group by ID from the database
    /// </summary>
    protected override async Task<MuscleGroup> LoadEntityByIdAsync(ISpecializedIdBase id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.GetByIdAsync((MuscleGroupId)id);
    }
    
    /// <summary>
    /// Maps a muscle group entity to a DTO
    /// </summary>
    protected override MuscleGroupDto MapToDto(MuscleGroup entity)
    {
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
    
    /// <summary>
    /// Creates an empty muscle group DTO
    /// </summary>
    protected override MuscleGroupDto CreateEmptyDto()
    {
        return new MuscleGroupDto
        {
            Id = MuscleGroupId.Empty.ToString(),
            Name = string.Empty,
            BodyPartId = BodyPartId.Empty.ToString(),
            BodyPartName = string.Empty,
            IsActive = false,
            CreatedAt = DateTime.MinValue,
            UpdatedAt = null
        };
    }
    
    /// <summary>
    /// Validates a create muscle group command
    /// </summary>
    protected override async Task<ValidationResult> ValidateCreateCommand(CreateMuscleGroupCommand command)
    {
        var basicValidation = ServiceValidate.For()
            .EnsureNotNull(command, ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.RequestCannotBeNull))
            .EnsureNotWhiteSpace(command?.Name, ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.NameCannotBeEmpty));
            
        var result = command switch
        {
            null => basicValidation.ToResult(),
            _ => await ValidateCreateWithChecksAsync(basicValidation, command)
        };
        
        return result;
    }
    
    private async Task<ValidationResult> ValidateCreateWithChecksAsync(
        ServiceValidation validation,
        CreateMuscleGroupCommand command)
    {
        var enhancedValidation = validation
            .Ensure(() => command.Name?.Length <= 100, ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.NameTooLong))
            .Ensure(() => !command.BodyPartId.IsEmpty, ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.BodyPartIdRequired));
            
        // Check if body part exists (only if body part ID is not empty)
        if (!command.BodyPartId.IsEmpty)
        {
            enhancedValidation = await enhancedValidation.EnsureAsync(
                async () => await _bodyPartService.ExistsAsync(command.BodyPartId),
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.InvalidBodyPartId));
        }
            
        // Check for duplicate name (only if name is not empty)
        if (!string.IsNullOrWhiteSpace(command.Name))
        {
            enhancedValidation = await enhancedValidation.EnsureAsync(
                async () => !await CheckDuplicateNameAsync(command.Name),
                ServiceError.AlreadyExists("MuscleGroup", command.Name));
        }
            
        return enhancedValidation.ToResult();
    }
    
    private async Task<bool> CheckDuplicateNameAsync(string name, MuscleGroupId? excludeId = null)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.ExistsByNameAsync(name.Trim(), excludeId);
    }
    
    /// <summary>
    /// Validates an update muscle group command
    /// </summary>
    protected override async Task<ValidationResult> ValidateUpdateCommand(ISpecializedIdBase id, UpdateMuscleGroupCommand command)
    {
        var basicValidation = ServiceValidate.For()
            .EnsureNotNull(command, ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.RequestCannotBeNull))
            .EnsureNotWhiteSpace(command?.Name, ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.NameCannotBeEmpty))
            .Ensure(() => !((MuscleGroupId)id).IsEmpty, ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.InvalidMuscleGroupId));
            
        var result = command switch
        {
            null => basicValidation.ToResult(),
            _ => await ValidateUpdateWithChecksAsync(basicValidation, (MuscleGroupId)id, command)
        };
        
        return result;
    }
    
    private async Task<ValidationResult> ValidateUpdateWithChecksAsync(
        ServiceValidation validation,
        MuscleGroupId muscleGroupId,
        UpdateMuscleGroupCommand command)
    {
        var enhancedValidation = validation
            .Ensure(() => command.Name?.Length <= 100, ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.NameTooLong))
            .Ensure(() => !command.BodyPartId.IsEmpty, ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.BodyPartIdRequired));
            
        // Check if body part exists (only if body part ID is not empty)
        if (!command.BodyPartId.IsEmpty)
        {
            enhancedValidation = await enhancedValidation.EnsureAsync(
                async () => await _bodyPartService.ExistsAsync(command.BodyPartId),
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.InvalidBodyPartId));
        }
            
        // Check for duplicate name (excluding current muscle group) - only if name is not empty
        if (!string.IsNullOrWhiteSpace(command.Name))
        {
            enhancedValidation = await enhancedValidation.EnsureAsync(
                async () => !await CheckDuplicateNameAsync(command.Name, muscleGroupId),
                ServiceError.AlreadyExists("MuscleGroup", command.Name));
        }
            
        return enhancedValidation.ToResult();
    }
    
    /// <summary>
    /// Creates a new muscle group entity
    /// </summary>
    protected override async Task<MuscleGroup> CreateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, CreateMuscleGroupCommand command)
    {
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entity = MuscleGroup.Handler.CreateNew(command.Name, command.BodyPartId);
        return await repository.CreateAsync(entity);
    }
    
    /// <summary>
    /// Updates an existing muscle group entity
    /// </summary>
    protected override async Task<MuscleGroup> UpdateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, MuscleGroup existingEntity, UpdateMuscleGroupCommand command)
    {
        var updatedEntity = MuscleGroup.Handler.Update(existingEntity, command.Name, command.BodyPartId);
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.UpdateAsync(updatedEntity);
    }
    
    /// <summary>
    /// Soft deletes a muscle group entity
    /// </summary>
    protected override async Task<bool> DeleteEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, ISpecializedIdBase id)
    {
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.DeactivateAsync((MuscleGroupId)id);
    }
    
    // Note: Cache invalidation is handled by the base class using pattern-based invalidation.
    // The pattern MuscleGroup:* will invalidate all caches including name-based caches.
    // Body part-specific caches will expire naturally based on cache duration.
}