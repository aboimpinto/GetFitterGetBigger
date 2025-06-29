using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Olimpo.EntityFramework.Persistency;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Configuration;
using GetFitterGetBigger.API.Utilities;
using Microsoft.Extensions.Options;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving muscle group data
/// </summary>
public class MuscleGroupsController : ReferenceTablesBaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MuscleGroupsController"/> class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="cacheConfiguration">The cache configuration</param>
    /// <param name="logger">The logger</param>
    public MuscleGroupsController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        IOptions<CacheConfiguration> cacheConfiguration,
        ILogger<MuscleGroupsController> logger)
        : base(unitOfWorkProvider, cacheService, cacheConfiguration, logger)
    {
    }

    /// <summary>
    /// Gets all muscle groups
    /// </summary>
    /// <returns>A collection of muscle groups</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var muscleGroups = await GetAllWithCacheAsync(async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
            return await repository.GetAllAsync();
        });
        
        // Map to DTOs
        var result = muscleGroups.Select(mg => new ReferenceDataDto
        {
            Id = mg.Id.ToString(),
            Value = mg.Name
        });
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a muscle group by ID
    /// </summary>
    /// <param name="id">The ID of the muscle group to retrieve in the format "musclegroup-{guid}"</param>
    /// <returns>The muscle group if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(string id)
    {
        // Try to parse the ID from the format "musclegroup-{guid}"
        if (!MuscleGroupId.TryParse(id, out var muscleGroupId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'musclegroup-{{guid}}', got: '{id}'");
        }
        
        var muscleGroup = await GetByIdWithCacheAsync(id, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
            return await repository.GetByIdAsync(muscleGroupId);
        });
        
        if (muscleGroup == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = muscleGroup.Id.ToString(),
            Value = muscleGroup.Name
        });
    }

    /// <summary>
    /// Gets a muscle group by name
    /// </summary>
    /// <param name="name">The name of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, 404 Not Found otherwise</returns>
    [HttpGet("ByName/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var muscleGroup = await repository.GetByNameAsync(name);
        
        if (muscleGroup == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = muscleGroup.Id.ToString(),
            Value = muscleGroup.Name
        });
    }
    
    /// <summary>
    /// Gets a muscle group by value (name)
    /// </summary>
    /// <param name="value">The value (name) of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByValue(string value)
    {
        var muscleGroup = await GetByValueWithCacheAsync(value, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
            return await repository.GetByNameAsync(value);
        });
        
        if (muscleGroup == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = muscleGroup.Id.ToString(),
            Value = muscleGroup.Name
        });
    }
    
    /// <summary>
    /// Gets all muscle groups for a specific body part
    /// </summary>
    /// <param name="bodyPartId">The ID of the body part in the format "bodypart-{guid}"</param>
    /// <returns>A collection of muscle groups for the specified body part</returns>
    [HttpGet("ByBodyPart/{bodyPartId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByBodyPart(string bodyPartId)
    {
        // Try to parse the ID from the format "bodypart-{guid}"
        if (!BodyPartId.TryParse(bodyPartId, out var bodyPartIdObj))
        {
            return BadRequest($"Invalid ID format. Expected format: 'bodypart-{{guid}}', got: '{bodyPartId}'");
        }
        
        var cacheKey = $"MuscleGroups:ByBodyPart:{bodyPartId}";
        var muscleGroups = await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
                var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
                return await repository.GetByBodyPartAsync(bodyPartIdObj);
            },
            GetCacheDuration());
        
        // Map to DTOs
        var result = muscleGroups.Select(mg => new ReferenceDataDto
        {
            Id = mg.Id.ToString(),
            Value = mg.Name
        });
        
        return Ok(result);
    }
    
    /// <summary>
    /// Creates a new muscle group
    /// </summary>
    /// <param name="request">The muscle group creation request</param>
    /// <returns>The created muscle group</returns>
    [HttpPost]
    [ProducesResponseType(typeof(MuscleGroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    // TODO: Uncomment when authorization is implemented
    // [Authorize(Policy = "ReferenceData-Management")]
    public async Task<IActionResult> Create([FromBody] CreateMuscleGroupDto request)
    {
        // Validate BodyPart ID format
        if (!BodyPartId.TryParse(request.BodyPartId, out var bodyPartId))
        {
            return BadRequest($"Invalid BodyPart ID format. Expected format: 'bodypart-{{guid}}', got: '{request.BodyPartId}'");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var bodyPartRepository = unitOfWork.GetRepository<IBodyPartRepository>();
        
        // Check if BodyPart exists and is active
        var bodyPart = await bodyPartRepository.GetByIdAsync(bodyPartId);
        if (bodyPart == null || !bodyPart.IsActive)
        {
            return BadRequest("Body part not found or is inactive");
        }
        
        // Check for duplicate name
        if (await repository.ExistsByNameAsync(request.Name))
        {
            return Conflict($"A muscle group with the name '{request.Name}' already exists");
        }
        
        // Create the muscle group
        var muscleGroup = MuscleGroup.Handler.CreateNew(request.Name.Trim(), bodyPartId);
        var created = await repository.CreateAsync(muscleGroup);
        
        await unitOfWork.CommitAsync();
        
        // Invalidate cache
        var tableName = GetTableName();
        await _cacheService.RemoveAsync(CacheKeyGenerator.GetAllKey(tableName));
        await _cacheService.RemoveAsync($"MuscleGroups:ByBodyPart:{request.BodyPartId}");
        
        // Map to DTO
        var dto = new MuscleGroupDto
        {
            Id = created.Id.ToString(),
            Name = created.Name,
            BodyPartId = created.BodyPartId.ToString(),
            BodyPartName = created.BodyPart?.Value,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };
        
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }
    
    /// <summary>
    /// Updates an existing muscle group
    /// </summary>
    /// <param name="id">The ID of the muscle group to update</param>
    /// <param name="request">The muscle group update request</param>
    /// <returns>The updated muscle group</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MuscleGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    // TODO: Uncomment when authorization is implemented
    // [Authorize(Policy = "ReferenceData-Management")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateMuscleGroupDto request)
    {
        // Validate muscle group ID format
        if (!MuscleGroupId.TryParse(id, out var muscleGroupId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'musclegroup-{{guid}}', got: '{id}'");
        }
        
        // Validate BodyPart ID format
        if (!BodyPartId.TryParse(request.BodyPartId, out var bodyPartId))
        {
            return BadRequest($"Invalid BodyPart ID format. Expected format: 'bodypart-{{guid}}', got: '{request.BodyPartId}'");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        var bodyPartRepository = unitOfWork.GetRepository<IBodyPartRepository>();
        
        // Get existing muscle group
        var existing = await repository.GetByIdAsync(muscleGroupId);
        if (existing == null)
        {
            return NotFound();
        }
        
        // Check if BodyPart exists and is active
        var bodyPart = await bodyPartRepository.GetByIdAsync(bodyPartId);
        if (bodyPart == null || !bodyPart.IsActive)
        {
            return BadRequest("Body part not found or is inactive");
        }
        
        // Check for duplicate name (excluding current)
        if (await repository.ExistsByNameAsync(request.Name, muscleGroupId))
        {
            return Conflict($"A muscle group with the name '{request.Name}' already exists");
        }
        
        // Update the muscle group
        var updated = MuscleGroup.Handler.Update(existing, request.Name.Trim(), bodyPartId);
        var result = await repository.UpdateAsync(updated);
        
        await unitOfWork.CommitAsync();
        
        // Invalidate cache
        var tableName = GetTableName();
        await _cacheService.RemoveAsync(CacheKeyGenerator.GetAllKey(tableName));
        await _cacheService.RemoveAsync($"MuscleGroups:ByBodyPart:{existing.BodyPartId}");
        await _cacheService.RemoveAsync($"MuscleGroups:ByBodyPart:{request.BodyPartId}");
        
        // Map to DTO
        var dto = new MuscleGroupDto
        {
            Id = result.Id.ToString(),
            Name = result.Name,
            BodyPartId = result.BodyPartId.ToString(),
            BodyPartName = result.BodyPart?.Value,
            IsActive = result.IsActive,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };
        
        return Ok(dto);
    }
    
    /// <summary>
    /// Deactivates a muscle group
    /// </summary>
    /// <param name="id">The ID of the muscle group to deactivate</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    // TODO: Uncomment when authorization is implemented
    // [Authorize(Policy = "ReferenceData-Management")]
    public async Task<IActionResult> Delete(string id)
    {
        // Validate muscle group ID format
        if (!MuscleGroupId.TryParse(id, out var muscleGroupId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'musclegroup-{{guid}}', got: '{id}'");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IMuscleGroupRepository>();
        
        // Check if muscle group exists
        var existing = await repository.GetByIdAsync(muscleGroupId);
        if (existing == null)
        {
            return NotFound();
        }
        
        // Check if it can be deactivated
        if (!await repository.CanDeactivateAsync(muscleGroupId))
        {
            return Conflict("Cannot deactivate muscle group as it is being used by active exercises");
        }
        
        // Deactivate the muscle group
        var success = await repository.DeactivateAsync(muscleGroupId);
        if (!success)
        {
            return NotFound();
        }
        
        await unitOfWork.CommitAsync();
        
        // Invalidate cache
        var tableName = GetTableName();
        await _cacheService.RemoveAsync(CacheKeyGenerator.GetAllKey(tableName));
        await _cacheService.RemoveAsync($"MuscleGroups:ByBodyPart:{existing.BodyPartId}");
        
        return NoContent();
    }
}
