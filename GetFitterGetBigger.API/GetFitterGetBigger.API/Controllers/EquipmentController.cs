using GetFitterGetBigger.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.Interfaces;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving equipment data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipmentService;
    private readonly ILogger<EquipmentController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentController"/> class
    /// </summary>
    /// <param name="equipmentService">The equipment service</param>
    /// <param name="logger">The logger</param>
    public EquipmentController(
        IEquipmentService equipmentService,
        ILogger<EquipmentController> logger)
    {
        _equipmentService = equipmentService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all equipment
    /// </summary>
    /// <returns>A collection of equipment</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _equipmentService.GetAllAsDtosAsync();
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
        try
        {
            var equipment = await _equipmentService.GetByIdAsDtoAsync(id);
            if (equipment == null)
                return NotFound();
                
            return Ok(equipment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
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
        var equipment = await _equipmentService.GetByNameAsync(name);
        
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
        var equipment = await _equipmentService.GetByValueAsync(value);
        
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
        try
        {
            var dto = await _equipmentService.CreateEquipmentAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
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
        // Log the incoming request
        _logger.LogInformation("[Equipment Update] Starting update for ID: {Id}", id);
        _logger.LogInformation("[Equipment Update] Incoming JSON: {@Request}", request);
        _logger.LogInformation("[Equipment Update] Request Name: '{Name}'", request?.Name);
        
        try
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }
            
            var dto = await _equipmentService.UpdateEquipmentAsync(id, request);
            
            _logger.LogInformation("[Equipment Update] Successfully updated equipment. Response DTO: {@Dto}", dto);
            
            return Ok(dto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("[Equipment Update] Equipment not found. ID: {Id}", id);
            return NotFound();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning("[Equipment Update] Duplicate name conflict: '{Name}'", request?.Name);
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("[Equipment Update] Invalid argument: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
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
        try
        {
            await _equipmentService.DeactivateAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("in use"))
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}