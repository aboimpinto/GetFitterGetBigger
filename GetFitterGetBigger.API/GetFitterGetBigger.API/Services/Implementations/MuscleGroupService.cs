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
using GetFitterGetBigger.API.Utilities;
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
    /// Checks if muscle group exists using strongly-typed ID - returns boolean result
    /// </summary>
    public async Task<ServiceResult<bool>> CheckExistsAsync(MuscleGroupId id)
    {
        return await ServiceValidate.Build<bool>()
            .EnsureNotEmpty(id, MuscleGroupErrorMessages.Validation.InvalidMuscleGroupId)
            .WhenValidAsync(async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
                var exists = await repository.ExistsAsync(id);
                return ServiceResult<bool>.Success(exists);
            });
    }
    
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
                        
                        // Return empty DTO for null/empty/inactive entities (won't be cached)
                        if (entity == null || entity.IsEmpty || !entity.IsActive)
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
        return await ServiceValidate.Build()
            // Basic validations
            .EnsureNotNull(command, MuscleGroupErrorMessages.Validation.RequestCannotBeNull)
            .EnsureNotWhiteSpace(command?.Name, MuscleGroupErrorMessages.Validation.NameCannotBeEmpty)
            .Ensure(() => command?.Name?.Length <= 100, MuscleGroupErrorMessages.Validation.NameTooLong)
            .Ensure(() => command != null && !command.BodyPartId.IsEmpty, MuscleGroupErrorMessages.Validation.BodyPartIdRequired)
            
            // Async validations - atomic, one validation per aspect
            .EnsureAsync(
                async () => {
                    if (command == null) return false;
                    var existsResult = await _bodyPartService.ExistsAsync(command.BodyPartId);
                    return existsResult.Data?.Value == true;
                },
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.InvalidBodyPartId))
            .EnsureAsync(
                async () => command == null || !await CheckDuplicateNameAsync(command.Name),
                ServiceError.AlreadyExists("MuscleGroup", command?.Name ?? string.Empty))
            .ToValidationResultAsync();
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
        var muscleGroupId = (MuscleGroupId)id;
        
        return await ServiceValidate.Build()
            // Basic validations
            .EnsureNotNull(command, MuscleGroupErrorMessages.Validation.RequestCannotBeNull)
            .EnsureNotWhiteSpace(command?.Name, MuscleGroupErrorMessages.Validation.NameCannotBeEmpty)
            .Ensure(() => !muscleGroupId.IsEmpty, MuscleGroupErrorMessages.Validation.InvalidMuscleGroupId)
            .Ensure(() => command?.Name?.Length <= 100, MuscleGroupErrorMessages.Validation.NameTooLong)
            .Ensure(() => command != null && !command.BodyPartId.IsEmpty, MuscleGroupErrorMessages.Validation.BodyPartIdRequired)
            
            // Async validations - atomic, one validation per aspect
            .EnsureAsync(
                async () => {
                    if (command == null) return false;
                    var existsResult = await _bodyPartService.ExistsAsync(command.BodyPartId);
                    return existsResult.Data?.Value == true;
                },
                ServiceError.ValidationFailed(MuscleGroupErrorMessages.Validation.InvalidBodyPartId))
            .EnsureAsync(
                async () => command == null || !await CheckDuplicateNameAsync(command.Name, muscleGroupId),
                ServiceError.AlreadyExists("MuscleGroup", command?.Name ?? string.Empty))
            .ToValidationResultAsync();
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

    /// <summary>
    /// Checks if a muscle group entity exists and is active
    /// </summary>
    protected override async Task<bool> CheckEntityExistsAsync(ISpecializedIdBase id)
    {
        var muscleGroupId = (MuscleGroupId)id;
        if (muscleGroupId.IsEmpty)
            return false;

        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.ExistsAsync(muscleGroupId);
    }

    /// <summary>
    /// Validates if the muscle group ID is properly formed and not empty
    /// </summary>
    protected override bool IsValidId(ISpecializedIdBase id)
    {
        if (id is not MuscleGroupId muscleGroupId)
            return false;

        return !muscleGroupId.IsEmpty;
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
            _innerBuilder.EnsureNotEmpty(id, errorMessage);
            return this;
        }
        
        public MuscleGroupValidationBuilder<T> EnsureMuscleGroupExists(MuscleGroupId id, string errorMessage)
        {
            _innerBuilder.EnsureAsync(
                async () => (await _service.CheckExistsAsync(id)).Data,
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
            _innerBuilder.EnsureNotWhiteSpace(name, errorMessage);
            return this;
        }
        
        public MuscleGroupValidationBuilder<T> EnsureValidBodyPartId(BodyPartId id, string errorMessage)
        {
            _innerBuilder.Ensure(() => !id.IsEmpty, errorMessage);
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