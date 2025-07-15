using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving movement pattern data
/// Uses service layer for all operations
/// </summary>
[ApiController]
[Route("api/ReferenceTables/[controller]")]
[Tags("ReferenceTables")]
public class MovementPatternsController : ControllerBase
{
    private readonly IMovementPatternService _movementPatternService;
    private readonly ILogger<MovementPatternsController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MovementPatternsController"/> class
    /// </summary>
    /// <param name="movementPatternService">The movement pattern service</param>
    /// <param name="logger">The logger</param>
    public MovementPatternsController(
        IMovementPatternService movementPatternService,
        ILogger<MovementPatternsController> logger)
    {
        _movementPatternService = movementPatternService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active movement patterns
    /// </summary>
    /// <returns>A collection of movement patterns</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _movementPatternService.GetAllActiveAsync();
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            _ => Ok(result.Data) // GetAll should always succeed, even if empty
        };
    }

    /// <summary>
    /// Gets a movement pattern by ID
    /// </summary>
    /// <param name="id">The ID of the movement pattern to retrieve in the format "movementpattern-{guid}"</param>
    /// <returns>The movement pattern if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(string id)
    {
        _logger.LogInformation("Getting movement pattern with ID: {Id}", id);
        
        var result = await _movementPatternService.GetByIdAsync(MovementPatternId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets a movement pattern by name
    /// </summary>
    /// <param name="name">The name of the movement pattern to retrieve</param>
    /// <returns>The movement pattern if found, 404 Not Found otherwise</returns>
    [HttpGet("ByName/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByName(string name)
    {
        var result = await _movementPatternService.GetByValueAsync(name);
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
    
    /// <summary>
    /// Gets a movement pattern by value (name)
    /// </summary>
    /// <param name="value">The value (name) of the movement pattern to retrieve</param>
    /// <returns>The movement pattern if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByValue(string value)
    {
        var result = await _movementPatternService.GetByValueAsync(value);
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}
