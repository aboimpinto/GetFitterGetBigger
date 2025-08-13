using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.ReferenceTables.BodyPart;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving body part reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/BodyParts")]
public class BodyPartsController : ControllerBase
{
    private readonly IBodyPartService _bodyPartService;
    private readonly ILogger<BodyPartsController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BodyPartsController"/> class
    /// </summary>
    /// <param name="bodyPartService">The body part service</param>
    /// <param name="logger">The logger</param>
    public BodyPartsController(
        IBodyPartService bodyPartService,
        ILogger<BodyPartsController> logger)
    {
        _bodyPartService = bodyPartService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active body parts
    /// </summary>
    /// <returns>A collection of active body parts</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBodyParts()
    {
        _logger.LogInformation("Getting all active body parts");
        
        var result = await _bodyPartService.GetAllActiveAsync();
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            _ => Ok(result.Data) // GetAll should always succeed, even if empty
        };
    }

    /// <summary>
    /// Gets a body part by ID
    /// </summary>
    /// <param name="id">The ID of the body part to retrieve in the format "bodypart-{guid}"</param>
    /// <returns>The body part if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBodyPartById(string id)
    {
        _logger.LogInformation("Getting body part with ID: {Id}", id);
        
        var result = await _bodyPartService.GetByIdAsync(BodyPartId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets a body part by value
    /// </summary>
    /// <param name="value">The value of the body part to retrieve</param>
    /// <returns>The body part if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBodyPartByValue(string value)
    {
        _logger.LogInformation("Getting body part with value: {Value}", value);
        
        var result = await _bodyPartService.GetByValueAsync(value);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}
