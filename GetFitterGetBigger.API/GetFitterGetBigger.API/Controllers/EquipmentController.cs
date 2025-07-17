using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing equipment data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/Equipment")]
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
    [ProducesResponseType(typeof(IEnumerable<EquipmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _equipmentService.GetAllAsync();
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            _ => Ok(result.Data) // GetAll should always succeed, even if empty
        };
    }

    /// <summary>
    /// Gets equipment by ID
    /// </summary>
    /// <param name="id">The ID of the equipment to retrieve in the format "equipment-{guid}"</param>
    /// <returns>The equipment if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EquipmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _equipmentService.GetByIdAsync(EquipmentId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets equipment by name
    /// </summary>
    /// <param name="name">The name of the equipment to retrieve</param>
    /// <returns>The equipment if found, 404 Not Found otherwise</returns>
    [HttpGet("ByName/{name}")]
    [ProducesResponseType(typeof(EquipmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByName(string name)
    {
        var result = await _equipmentService.GetByNameAsync(name);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
    
    /// <summary>
    /// Creates new equipment
    /// </summary>
    /// <param name="request">The equipment creation request</param>
    /// <returns>The created equipment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EquipmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEquipmentDto request)
    {
        var result = await _equipmentService.CreateAsync(request.ToCommand());
        
        return result switch
        {
            { IsSuccess: true } => CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
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
    public async Task<IActionResult> Update(string id, [FromBody] UpdateEquipmentDto request)
    {
        var result = await _equipmentService.UpdateAsync(EquipmentId.ParseOrEmpty(id), request.ToCommand());
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
    
    /// <summary>
    /// Deletes equipment (soft delete)
    /// </summary>
    /// <param name="id">The ID of the equipment to delete</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _equipmentService.DeleteAsync(EquipmentId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.DependencyExists } => Conflict(new { errors = result.StructuredErrors }),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}