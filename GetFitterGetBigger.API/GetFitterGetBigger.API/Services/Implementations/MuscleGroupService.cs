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
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Services.Implementations;

/// <summary>
/// Service implementation for muscle group operations
/// </summary>
public class MuscleGroupService : ReferenceTableServiceBase<MuscleGroup>, IMuscleGroupService
{
    public MuscleGroupService(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService)
        : base(unitOfWorkProvider, cacheService)
    {
    }
    
    protected override string CacheKeyPrefix => "MuscleGroups";
    protected override TimeSpan CacheDuration => TimeSpan.FromHours(1); // Dynamic table
    
    /// <summary>
    /// Gets all muscle groups as DTOs
    /// </summary>
    public async Task<IEnumerable<ReferenceDataDto>> GetAllAsDtosAsync()
    {
        var muscleGroups = await GetAllAsync();
        return muscleGroups.Select(mg => new ReferenceDataDto
        {
            Id = mg.Id.ToString(),
            Value = mg.Name
        });
    }
    
    /// <summary>
    /// Gets muscle group by ID as DTO
    /// </summary>
    public async Task<ReferenceDataDto?> GetByIdAsDtoAsync(string id)
    {
        var muscleGroup = await GetByIdAsync(id);
        if (muscleGroup == null || !muscleGroup.IsActive)
            return null;
            
        return new ReferenceDataDto
        {
            Id = muscleGroup.Id.ToString(),
            Value = muscleGroup.Name
        };
    }
    
    /// <summary>
    /// Gets muscle groups by body part
    /// </summary>
    public async Task<IEnumerable<ReferenceDataDto>> GetByBodyPartAsync(string bodyPartId)
    {
        if (!BodyPartId.TryParse(bodyPartId, out var parsedBodyPartId))
        {
            throw new ArgumentException($"Invalid body part ID format. Expected format: 'bodypart-{{guid}}', got: '{bodyPartId}'");
        }
        
        var cacheKey = $"{CacheKeyPrefix}:byBodyPart:{bodyPartId}";
        
        var cached = await _cacheService.GetAsync<IEnumerable<ReferenceDataDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var muscleGroups = await repository.GetByBodyPartAsync(parsedBodyPartId);
        
        var dtos = muscleGroups.Select(mg => new ReferenceDataDto
        {
            Id = mg.Id.ToString(),
            Value = mg.Name
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
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var bodyPartRepository = unitOfWork.GetRepository<IBodyPartRepository>();
        
        // Check if BodyPart exists and is active
        var bodyPart = await bodyPartRepository.GetByIdAsync(bodyPartId);
        if (bodyPart == null || !bodyPart.IsActive)
        {
            throw new ArgumentException("Body part not found or is inactive");
        }
        
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
        // Validate muscle group ID format
        if (!MuscleGroupId.TryParse(id, out var muscleGroupId))
        {
            throw new ArgumentException($"Invalid ID format. Expected format: 'musclegroup-{{guid}}', got: '{id}'");
        }
        
        // Validate BodyPart ID format
        if (!BodyPartId.TryParse(request.BodyPartId, out var bodyPartId))
        {
            throw new ArgumentException($"Invalid BodyPart ID format. Expected format: 'bodypart-{{guid}}', got: '{request.BodyPartId}'");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var bodyPartRepository = unitOfWork.GetRepository<IBodyPartRepository>();
        
        // Get existing muscle group
        var existing = await repository.GetByIdAsync(muscleGroupId);
        if (existing == null)
        {
            throw new InvalidOperationException($"Muscle group with ID '{id}' not found");
        }
        
        // Check if BodyPart exists and is active
        var bodyPart = await bodyPartRepository.GetByIdAsync(bodyPartId);
        if (bodyPart == null || !bodyPart.IsActive)
        {
            throw new ArgumentException("Body part not found or is inactive");
        }
        
        // Check for duplicate name (excluding current)
        if (await repository.ExistsByNameAsync(request.Name, muscleGroupId))
        {
            throw new InvalidOperationException($"A muscle group with the name '{request.Name}' already exists");
        }
        
        // Update the muscle group
        var updated = MuscleGroup.Handler.Update(existing, request.Name.Trim(), bodyPartId);
        var result = await repository.UpdateAsync(updated);
        
        await unitOfWork.CommitAsync();
        
        // Invalidate cache
        await InvalidateCacheAsync();
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}:byBodyPart:{existing.BodyPartId}");
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}:byBodyPart:{request.BodyPartId}");
        
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