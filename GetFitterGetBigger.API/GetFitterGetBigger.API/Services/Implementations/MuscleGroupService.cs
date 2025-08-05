using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Cache;
using GetFitterGetBigger.API.Services.Commands.MuscleGroup;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for muscle group operations
/// </summary>
public class MuscleGroupService : EnhancedReferenceService<MuscleGroup, MuscleGroupDto, CreateMuscleGroupCommand, UpdateMuscleGroupCommand>, IMuscleGroupService
{
    private readonly IBodyPartService _bodyPartService;
    private readonly ICacheService _typedCacheService;
    
    public MuscleGroupService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger<MuscleGroupService> logger,
        IBodyPartService bodyPartService)
        : base(unitOfWorkProvider, cacheService, logger)
    {
        _bodyPartService = bodyPartService;
        _typedCacheService = cacheService;
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
        return await Validate<bool>()
            .EnsureValidMuscleGroupId(id, MuscleGroupErrorMessages.Validation.InvalidMuscleGroupId)
            .EnsureMuscleGroupExists(id, MuscleGroupErrorMessages.Operations.NotFound)
            .EnsureCanDeactivate(id, MuscleGroupErrorMessages.BusinessRules.CannotDeleteInUse)
            .DeleteMuscleGroup(id);
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
        return await Validate<MuscleGroupDto>()
            .EnsureValidName(name, MuscleGroupErrorMessages.Validation.NameCannotBeEmptyForSearch)
            .ExecuteAsync(async () => 
            {
                var cacheKey = CacheKeyGenerator.Generate<MuscleGroupDto>("byName", name);
                
                return await CacheLoad.For<MuscleGroupDto>(_typedCacheService, cacheKey)
                    .WithAutoCache(_typedCacheService, cacheKey, async () => 
                    {
                        var entity = await GetEntityByNameFromDatabaseAsync(name);
                        
                        // Return empty DTO for empty/inactive entities (won't be cached)
                        if (entity.IsEmpty || !entity.IsActive)
                        {
                            _logger.LogDebug("MuscleGroup not found or inactive for name: {Name}", name);
                            return MuscleGroupDto.Empty;
                        }
                        
                        _logger.LogDebug("MuscleGroup found for name: {Name}", name);
                        return MapToDto(entity);
                    });
            });
    }
    
    /// <summary>
    /// Gets muscle group entity by name from database (pure DB access)
    /// </summary>
    private async Task<MuscleGroup> GetEntityByNameFromDatabaseAsync(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.GetByNameAsync(name);
    }
    
    /// <summary>
    /// Gets muscle groups by body part
    /// </summary>
    public async Task<ServiceResult<IEnumerable<MuscleGroupDto>>> GetByBodyPartAsync(BodyPartId bodyPartId)
    {
        return await Validate<IEnumerable<MuscleGroupDto>>()
            .EnsureValidBodyPartId(bodyPartId, MuscleGroupErrorMessages.Validation.BodyPartIdCannotBeEmptyForSearch)
            .ExecuteAsync(async () => 
            {
                var cacheKey = CacheKeyGenerator.Generate<MuscleGroupDto>("byBodyPart", bodyPartId);
                
                var result = await CacheLoad.For<List<MuscleGroupDto>>(_typedCacheService, cacheKey)
                    .WithAutoCache(_typedCacheService, cacheKey, async () => 
                    {
                        var entities = await GetEntitiesByBodyPartFromDatabaseAsync(bodyPartId);
                        
                        // Map entities to DTOs - returns empty list if no entities
                        var mappedResult = entities.Select(MapToDto).ToList();
                        _logger.LogDebug("Found {Count} MuscleGroups for BodyPart: {BodyPartId}", mappedResult.Count, bodyPartId);
                        return mappedResult;
                    });
                
                // Convert List<T> result to IEnumerable<T> for interface compatibility
                return result.IsSuccess 
                    ? ServiceResult<IEnumerable<MuscleGroupDto>>.Success(result.Data)
                    : ServiceResult<IEnumerable<MuscleGroupDto>>.Failure([], result.Errors.ToArray());
            });
    }
    
    /// <summary>
    /// Gets muscle group entities by body part from database (pure DB access)
    /// </summary>
    private async Task<IEnumerable<MuscleGroup>> GetEntitiesByBodyPartFromDatabaseAsync(BodyPartId bodyPartId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.GetByBodyPartAsync(bodyPartId);
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
    
    /// <summary>
    /// Creates a validation builder for muscle group operations
    /// </summary>
    private MuscleGroupValidationBuilder<T> Validate<T>()
    {
        return new MuscleGroupValidationBuilder<T>(this);
    }
    
    /// <summary>
    /// Performs the actual delete operation after validation
    /// </summary>
    private async Task<ServiceResult<bool>> PerformDeleteAsync(MuscleGroupId id)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var deleted = await repository.DeactivateAsync(id);
        await unitOfWork.CommitAsync();
        
        await InvalidateCacheAsync();
        _logger.LogInformation("Deleted MuscleGroup with ID: {Id}", id.ToString());
        
        return ServiceResult<bool>.Success(true);
    }
    
    /// <summary>
    /// Private nested validation builder for muscle group-specific validations
    /// </summary>
    private class MuscleGroupValidationBuilder<T>
    {
        private readonly MuscleGroupService _service;
        private readonly ServiceValidationBuilder<T> _innerBuilder;
        
        public MuscleGroupValidationBuilder(MuscleGroupService service)
        {
            _service = service;
            _innerBuilder = ServiceValidate.Build<T>();
        }
        
        public MuscleGroupValidationBuilder<T> EnsureValidMuscleGroupId(MuscleGroupId id, string errorMessage)
        {
            _innerBuilder.EnsureNotEmpty(id, ServiceError.ValidationFailed(errorMessage));
            return this;
        }
        
        public MuscleGroupValidationBuilder<T> EnsureMuscleGroupExists(MuscleGroupId id, string errorMessage)
        {
            _innerBuilder.EnsureAsync(
                async () => (await _service.ExistsAsync(id)).IsSuccess,
                ServiceError.NotFound("MuscleGroup"));
            return this;
        }
        
        public MuscleGroupValidationBuilder<T> EnsureCanDeactivate(MuscleGroupId id, string errorMessage)
        {
            _innerBuilder.EnsureAsync(
                async () =>
                {
                    using var unitOfWork = _service._unitOfWorkProvider.CreateReadOnly();
                    var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
                    return await repository.CanDeactivateAsync(id);
                },
                ServiceError.DependencyExists("MuscleGroup", errorMessage));
            return this;
        }
        
        public MuscleGroupValidationBuilder<T> EnsureValidName(string name, string errorMessage)
        {
            _innerBuilder.EnsureNotWhiteSpace(name, ServiceError.ValidationFailed(errorMessage));
            return this;
        }
        
        public MuscleGroupValidationBuilder<T> EnsureValidBodyPartId(BodyPartId id, string errorMessage)
        {
            _innerBuilder.Ensure(() => !id.IsEmpty, ServiceError.ValidationFailed(errorMessage));
            return this;
        }
        
        public async Task<ServiceResult<bool>> DeleteMuscleGroup(MuscleGroupId id)
        {
            return await _innerBuilder.ExecuteAsync(() => _service.PerformDeleteAsync(id));
        }
        
        public async Task<ServiceResult<T>> ExecuteAsync(Func<Task<ServiceResult<T>>> operation)
        {
            return await _innerBuilder.ExecuteAsync(operation);
        }
    }
}