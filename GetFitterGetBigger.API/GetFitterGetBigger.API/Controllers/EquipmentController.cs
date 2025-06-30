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
/// Controller for retrieving equipment data
/// </summary>
public class EquipmentController : ReferenceTablesBaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentController"/> class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="cacheConfiguration">The cache configuration</param>
    /// <param name="logger">The logger</param>
    public EquipmentController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        IOptions<CacheConfiguration> cacheConfiguration,
        ILogger<EquipmentController> logger)
        : base(unitOfWorkProvider, cacheService, cacheConfiguration, logger)
    {
    }

    /// <summary>
    /// Gets all equipment
    /// </summary>
    /// <returns>A collection of equipment</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var equipment = await GetAllWithCacheAsync(async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IEquipmentRepository>();
            return await repository.GetAllAsync();
        });
        
        // Map to DTOs
        var result = equipment.Select(e => new ReferenceDataDto
        {
            Id = e.Id.ToString(),
            Value = e.Name
        });
        
        return Ok(result);
    }

    /// <summary>
    /// Gets equipment by ID
    /// </summary>
    /// <param name="id">The ID of the equipment to retrieve in the format "equipment-{guid}"</param>
    /// <returns>The equipment if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(string id)
    {
        // Try to parse the ID from the format "equipment-{guid}"
        if (!EquipmentId.TryParse(id, out var equipmentId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'equipment-{{guid}}', got: '{id}'");
        }
        
        var equipment = await GetByIdWithCacheAsync(id, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IEquipmentRepository>();
            return await repository.GetByIdAsync(equipmentId);
        });
        
        if (equipment == null || !equipment.IsActive)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = equipment.Id.ToString(),
            Value = equipment.Name
        });
    }

    /// <summary>
    /// Gets equipment by name
    /// </summary>
    /// <param name="name">The name of the equipment to retrieve</param>
    /// <returns>The equipment if found, 404 Not Found otherwise</returns>
    [HttpGet("ByName/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var equipment = await repository.GetByNameAsync(name);
        
        if (equipment == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = equipment.Id.ToString(),
            Value = equipment.Name
        });
    }
    
    /// <summary>
    /// Gets equipment by value (name)
    /// </summary>
    /// <param name="value">The value (name) of the equipment to retrieve</param>
    /// <returns>The equipment if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByValue(string value)
    {
        var equipment = await GetByValueWithCacheAsync(value, async () =>
        {
            using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
            var repository = unitOfWork.GetRepository<IEquipmentRepository>();
            return await repository.GetByNameAsync(value);
        });
        
        if (equipment == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = equipment.Id.ToString(),
            Value = equipment.Name
        });
    }
    
    /// <summary>
    /// Creates new equipment
    /// </summary>
    /// <param name="request">The equipment creation request</param>
    /// <returns>The created equipment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EquipmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateEquipmentDto request)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        // Check for duplicate name
        if (await repository.ExistsAsync(request.Name))
        {
            return Conflict($"Equipment with the name '{request.Name}' already exists");
        }
        
        // Create the equipment
        var equipment = Equipment.Handler.CreateNew(request.Name.Trim());
        var created = await repository.CreateAsync(equipment);
        
        // Repository already saves, no need to commit again
        
        // Invalidate cache
        var tableName = GetTableName();
        await _cacheService.RemoveAsync(CacheKeyGenerator.GetAllKey(tableName));
        
        // Map to DTO
        var dto = new EquipmentDto
        {
            Id = created.Id.ToString(),
            Name = created.Name,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };
        
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }
    
    /// <summary>
    /// Updates existing equipment
    /// </summary>
    /// <param name="id">The ID of the equipment to update</param>
    /// <param name="request">The equipment update request</param>
    /// <returns>The updated equipment</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EquipmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateEquipmentDto request)
    {
        // Validate equipment ID format
        if (!EquipmentId.TryParse(id, out var equipmentId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'equipment-{{guid}}', got: '{id}'");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        // Get existing equipment
        var existing = await repository.GetByIdAsync(equipmentId);
        if (existing == null || !existing.IsActive)
        {
            return NotFound();
        }
        
        // Check for duplicate name (excluding current)
        if (await repository.ExistsAsync(request.Name, equipmentId))
        {
            return Conflict($"Equipment with the name '{request.Name}' already exists");
        }
        
        // Update the equipment
        var updated = Equipment.Handler.Update(existing, request.Name.Trim());
        var result = await repository.UpdateAsync(updated);
        
        // Repository already saves, no need to commit again
        
        // Invalidate cache
        var tableName = GetTableName();
        await _cacheService.RemoveAsync(CacheKeyGenerator.GetAllKey(tableName));
        await _cacheService.RemoveAsync(CacheKeyGenerator.GetByIdKey(tableName, id));
        await _cacheService.RemoveAsync(CacheKeyGenerator.GetByValueKey(tableName, existing.Name));
        await _cacheService.RemoveAsync(CacheKeyGenerator.GetByValueKey(tableName, request.Name));
        
        // Map to DTO
        var dto = new EquipmentDto
        {
            Id = result.Id.ToString(),
            Name = result.Name,
            IsActive = result.IsActive,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };
        
        return Ok(dto);
    }
    
    /// <summary>
    /// Deactivates equipment
    /// </summary>
    /// <param name="id">The ID of the equipment to deactivate</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string id)
    {
        // Validate equipment ID format
        if (!EquipmentId.TryParse(id, out var equipmentId))
        {
            return BadRequest($"Invalid ID format. Expected format: 'equipment-{{guid}}', got: '{id}'");
        }
        
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        
        // Check if equipment exists
        var existing = await repository.GetByIdAsync(equipmentId);
        if (existing == null || !existing.IsActive)
        {
            return NotFound();
        }
        
        // Check if equipment is in use
        if (await repository.IsInUseAsync(equipmentId))
        {
            return Conflict("Cannot deactivate equipment that is in use by exercises");
        }
        
        // Deactivate the equipment
        var deactivated = await repository.DeactivateAsync(equipmentId);
        if (!deactivated)
        {
            return NotFound();
        }
        
        // Repository already saves, no need to commit again
        
        // Invalidate cache
        var tableName = GetTableName();
        await _cacheService.RemoveAsync(CacheKeyGenerator.GetAllKey(tableName));
        await _cacheService.RemoveAsync(CacheKeyGenerator.GetByIdKey(tableName, id));
        await _cacheService.RemoveAsync(CacheKeyGenerator.GetByValueKey(tableName, existing.Name));
        
        return NoContent();
    }
}
