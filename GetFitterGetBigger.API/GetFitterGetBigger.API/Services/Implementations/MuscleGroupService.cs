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
/// Service implementation for muscle group operations
/// </summary>
public class MuscleGroupService : ReferenceTableServiceBase<MuscleGroup>, IMuscleGroupService
{
    private readonly ILogger<MuscleGroupService> _specificLogger;
    private readonly IBodyPartService _bodyPartService;
    
    public MuscleGroupService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        ILogger<MuscleGroupService> logger,
        IBodyPartService bodyPartService)
        : base(unitOfWorkProvider, cacheService, logger)
    {
        _specificLogger = logger;
        _bodyPartService = bodyPartService;
    }
    
    protected override string CacheKeyPrefix => "MuscleGroups";
    protected override TimeSpan CacheDuration => TimeSpan.FromHours(1); // Dynamic table
    
    /// <summary>
    /// Gets all muscle groups as DTOs
    /// </summary>
    public async Task<IEnumerable<MuscleGroupDto>> GetAllAsDtosAsync()
    {
        var muscleGroups = await GetAllAsync();
        return muscleGroups.Select(mg => new MuscleGroupDto
        {
            Id = mg.Id.ToString(),
            Name = mg.Name,
            BodyPartId = mg.BodyPartId.ToString(),
            BodyPartName = mg.BodyPart?.Value,
            IsActive = mg.IsActive,
            CreatedAt = mg.CreatedAt,
            UpdatedAt = mg.UpdatedAt
        });
    }
    
    /// <summary>
    /// Gets muscle group by ID as DTO
    /// </summary>
    public async Task<MuscleGroupDto?> GetByIdAsDtoAsync(string id)
    {
        var muscleGroup = await GetByIdAsync(id);
        if (muscleGroup == null || !muscleGroup.IsActive)
            return null;
            
        return new MuscleGroupDto
        {
            Id = muscleGroup.Id.ToString(),
            Name = muscleGroup.Name,
            BodyPartId = muscleGroup.BodyPartId.ToString(),
            BodyPartName = muscleGroup.BodyPart?.Value,
            IsActive = muscleGroup.IsActive,
            CreatedAt = muscleGroup.CreatedAt,
            UpdatedAt = muscleGroup.UpdatedAt
        };
    }
    
    /// <summary>
    /// Gets muscle groups by body part
    /// </summary>
    public async Task<IEnumerable<MuscleGroupDto>> GetByBodyPartAsync(string bodyPartId)
    {
        if (!BodyPartId.TryParse(bodyPartId, out var parsedBodyPartId))
        {
            throw new ArgumentException($"Invalid body part ID format. Expected format: 'bodypart-{{guid}}', got: '{bodyPartId}'");
        }
        
        var cacheKey = $"{CacheKeyPrefix}:byBodyPart:{bodyPartId}";
        
        var cached = await _cacheService.GetAsync<IEnumerable<MuscleGroupDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var muscleGroups = await repository.GetByBodyPartAsync(parsedBodyPartId);
        
        var dtos = muscleGroups.Select(mg => new MuscleGroupDto
        {
            Id = mg.Id.ToString(),
            Name = mg.Name,
            BodyPartId = mg.BodyPartId.ToString(),
            BodyPartName = mg.BodyPart?.Value,
            IsActive = mg.IsActive,
            CreatedAt = mg.CreatedAt,
            UpdatedAt = mg.UpdatedAt
        }).ToList();
        
        await _cacheService.SetAsync(cacheKey, dtos, CacheDuration);
        
        return dtos;
    }
    
    
    protected override async Task<IEnumerable<MuscleGroup>> GetAllEntitiesAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork)
    {
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.GetAllAsync();
    }
    
    protected override async Task<MuscleGroup?> GetEntityByIdAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string id)
    {
        if (!MuscleGroupId.TryParse(id, out var muscleGroupId))
        {
            throw new ArgumentException($"Invalid ID format. Expected format: 'musclegroup-{{guid}}', got: '{id}'");
        }
            
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.GetByIdAsync(muscleGroupId);
    }
    
    protected override async Task<MuscleGroup?> GetEntityByNameAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string name)
    {
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        return await repository.GetByNameAsync(name);
    }
    
    protected override async Task<MuscleGroup?> GetEntityByValueAsync(IReadOnlyUnitOfWork<FitnessDbContext> unitOfWork, string value)
    {
        // For muscle groups, value is the same as name
        return await GetEntityByNameAsync(unitOfWork, value);
    }
    
    protected override Task<MuscleGroup> CreateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, object createDto)
    {
        // Not implemented for MuscleGroup - use specific methods instead
        throw new NotSupportedException("Use specific methods for creating muscle groups");
    }
    
    protected override Task<MuscleGroup> UpdateEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id, object updateDto)
    {
        // Not implemented for MuscleGroup - use specific methods instead
        throw new NotSupportedException("Use specific methods for updating muscle groups");
    }
    
    protected override Task DeleteEntityAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id)
    {
        // Not implemented for MuscleGroup
        throw new NotSupportedException("Deleting muscle groups is not supported");
    }
    
    protected override async Task<bool> CheckEntityExistsAsync(IWritableUnitOfWork<FitnessDbContext> unitOfWork, string id)
    {
        if (!MuscleGroupId.TryParse(id, out var muscleGroupId))
        {
            throw new ArgumentException($"Invalid ID format. Expected format: 'musclegroup-{{guid}}', got: '{id}'");
        }
            
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var entity = await repository.GetByIdAsync(muscleGroupId);
        return entity != null && entity.IsActive;
    }
    
    /// <summary>
    /// Creates a new muscle group
    /// </summary>
    public async Task<MuscleGroupDto> CreateMuscleGroupAsync(CreateMuscleGroupDto request)
    {
        // Validate BodyPart ID format
        if (!BodyPartId.TryParse(request.BodyPartId, out var bodyPartId))
        {
            throw new ArgumentException($"Invalid BodyPart ID format. Expected format: 'bodypart-{{guid}}', got: '{request.BodyPartId}'");
        }
        
        // Check if BodyPart exists using service
        if (!await _bodyPartService.ExistsAsync(bodyPartId))
        {
            throw new ArgumentException("Body part not found or is inactive");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        // Check for duplicate name
        if (await repository.ExistsByNameAsync(request.Name))
        {
            throw new InvalidOperationException($"A muscle group with the name '{request.Name}' already exists");
        }
        
        // Create the muscle group
        var muscleGroup = MuscleGroup.Handler.CreateNew(request.Name.Trim(), bodyPartId);
        var created = await repository.CreateAsync(muscleGroup);
        
        await unitOfWork.CommitAsync();
        
        // Invalidate cache
        await InvalidateCacheAsync();
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}:byBodyPart:{request.BodyPartId}");
        
        // Map to DTO
        return new MuscleGroupDto
        {
            Id = created.Id.ToString(),
            Name = created.Name,
            BodyPartId = created.BodyPartId.ToString(),
            BodyPartName = created.BodyPart?.Value,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };
    }
    
    /// <summary>
    /// Updates an existing muscle group
    /// </summary>
    public async Task<MuscleGroupDto> UpdateMuscleGroupAsync(string id, UpdateMuscleGroupDto request)
    {
        _specificLogger.LogInformation("=== MuscleGroupService.UpdateMuscleGroupAsync START ===");
        _specificLogger.LogInformation("Received ID: {Id}", id);
        _specificLogger.LogInformation("Received Name: {Name}", request.Name);
        _specificLogger.LogInformation("Received BodyPartId: {BodyPartId}", request.BodyPartId);
        
        // Validate muscle group ID format
        if (!MuscleGroupId.TryParse(id, out var muscleGroupId))
        {
            _specificLogger.LogError("Invalid muscle group ID format: {Id}", id);
            throw new ArgumentException($"Invalid ID format. Expected format: 'musclegroup-{{guid}}', got: '{id}'");
        }
        
        _specificLogger.LogInformation("Parsed MuscleGroupId: {MuscleGroupId}", muscleGroupId);
        
        // Validate BodyPart ID format
        if (!BodyPartId.TryParse(request.BodyPartId, out var bodyPartId))
        {
            _specificLogger.LogError("Invalid body part ID format: {BodyPartId}", request.BodyPartId);
            throw new ArgumentException($"Invalid BodyPart ID format. Expected format: 'bodypart-{{guid}}', got: '{request.BodyPartId}'");
        }
        
        _specificLogger.LogInformation("Parsed BodyPartId: {BodyPartId}", bodyPartId);
        
        // Check if BodyPart exists using service
        if (!await _bodyPartService.ExistsAsync(bodyPartId))
        {
            throw new ArgumentException("Body part not found or is inactive");
        }
        
        // Use WritableUnitOfWork only for the update operation
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        // Get existing muscle group
        var existing = await repository.GetByIdAsync(muscleGroupId);
        if (existing == null)
        {
            _specificLogger.LogError("Muscle group not found with ID: {Id}", id);
            throw new InvalidOperationException($"Muscle group with ID '{id}' not found");
        }
        
        _specificLogger.LogInformation("=== EXISTING MuscleGroup BEFORE UPDATE ===");
        _specificLogger.LogInformation("ID: {Id}", existing.Id);
        _specificLogger.LogInformation("Name: {Name}", existing.Name);
        _specificLogger.LogInformation("BodyPartId: {BodyPartId}", existing.BodyPartId);
        _specificLogger.LogInformation("IsActive: {IsActive}", existing.IsActive);
        _specificLogger.LogInformation("CreatedAt: {CreatedAt}", existing.CreatedAt);
        _specificLogger.LogInformation("UpdatedAt: {UpdatedAt}", existing.UpdatedAt);
        _specificLogger.LogInformation("========================================");
        
        // Check for duplicate name (excluding current)
        if (await repository.ExistsByNameAsync(request.Name, muscleGroupId))
        {
            _specificLogger.LogError("Duplicate name found: {Name}", request.Name);
            throw new InvalidOperationException($"A muscle group with the name '{request.Name}' already exists");
        }
        
        _specificLogger.LogInformation("Updating MuscleGroup - New Name: {Name}, New BodyPartId: {BodyPartId}", request.Name.Trim(), bodyPartId);
        
        // Update the muscle group
        var updated = MuscleGroup.Handler.Update(existing, request.Name.Trim(), bodyPartId);
        
        _specificLogger.LogInformation("=== UPDATED MuscleGroup BEFORE SAVE ===");
        _specificLogger.LogInformation("ID: {Id}", updated.Id);
        _specificLogger.LogInformation("Name: {Name}", updated.Name);
        _specificLogger.LogInformation("BodyPartId: {BodyPartId}", updated.BodyPartId);
        _specificLogger.LogInformation("IsActive: {IsActive}", updated.IsActive);
        _specificLogger.LogInformation("CreatedAt: {CreatedAt}", updated.CreatedAt);
        _specificLogger.LogInformation("UpdatedAt: {UpdatedAt}", updated.UpdatedAt);
        _specificLogger.LogInformation("======================================");
        
        var result = await repository.UpdateAsync(updated);
        
        await unitOfWork.CommitAsync();
        
        _specificLogger.LogInformation("=== SAVED MuscleGroup RESULT ===");
        _specificLogger.LogInformation("ID: {Id}", result.Id);
        _specificLogger.LogInformation("Name: {Name}", result.Name);
        _specificLogger.LogInformation("BodyPartId: {BodyPartId}", result.BodyPartId);
        _specificLogger.LogInformation("UpdatedAt: {UpdatedAt}", result.UpdatedAt);
        _specificLogger.LogInformation("================================");
        
        // Invalidate cache
        await InvalidateCacheAsync();
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}:byBodyPart:{existing.BodyPartId}");
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}:byBodyPart:{request.BodyPartId}");
        
        _specificLogger.LogInformation("Cache invalidated for old BodyPartId: {OldBodyPartId} and new BodyPartId: {NewBodyPartId}", 
            existing.BodyPartId, request.BodyPartId);
        
        // Map to DTO
        return new MuscleGroupDto
        {
            Id = result.Id.ToString(),
            Name = result.Name,
            BodyPartId = result.BodyPartId.ToString(),
            BodyPartName = result.BodyPart?.Value,
            IsActive = result.IsActive,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };
    }
    
    /// <summary>
    /// Deactivates a muscle group
    /// </summary>
    public async Task DeactivateMuscleGroupAsync(string id)
    {
        // Validate muscle group ID format
        if (!MuscleGroupId.TryParse(id, out var muscleGroupId))
        {
            throw new ArgumentException($"Invalid ID format. Expected format: 'musclegroup-{{guid}}', got: '{id}'");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        // Check if muscle group exists
        var existing = await repository.GetByIdAsync(muscleGroupId);
        if (existing == null)
        {
            throw new InvalidOperationException($"Muscle group with ID '{id}' not found");
        }
        
        // Check if it can be deactivated
        if (!await repository.CanDeactivateAsync(muscleGroupId))
        {
            throw new InvalidOperationException("Cannot deactivate muscle group as it is being used by active exercises");
        }
        
        // Deactivate the muscle group
        var success = await repository.DeactivateAsync(muscleGroupId);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to deactivate muscle group with ID '{id}'");
        }
        
        await unitOfWork.CommitAsync();
        
        // Invalidate cache
        await InvalidateCacheAsync();
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}:byBodyPart:{existing.BodyPartId}");
    }
}