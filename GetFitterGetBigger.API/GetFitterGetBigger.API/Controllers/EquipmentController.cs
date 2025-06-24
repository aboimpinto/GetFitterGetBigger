using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving equipment data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentController"/> class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    public EquipmentController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    /// <summary>
    /// Gets all equipment
    /// </summary>
    /// <returns>A collection of equipment</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var equipment = await repository.GetAllAsync();
        
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
        
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var equipment = await repository.GetByIdAsync(equipmentId);
        
        if (equipment == null)
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
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var repository = unitOfWork.GetRepository<IEquipmentRepository>();
        var equipment = await repository.GetByNameAsync(value);
        
        if (equipment == null)
            return NotFound();
            
        return Ok(new ReferenceDataDto
        {
            Id = equipment.Id.ToString(),
            Value = equipment.Name
        });
    }
}
