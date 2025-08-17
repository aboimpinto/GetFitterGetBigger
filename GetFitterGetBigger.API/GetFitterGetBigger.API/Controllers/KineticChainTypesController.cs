using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.ReferenceTables.KineticChainType;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving kinetic chain type reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/KineticChainTypes")]
public class KineticChainTypesController : ControllerBase
{
    private readonly IKineticChainTypeService _kineticChainTypeService;
    private readonly ILogger<KineticChainTypesController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="KineticChainTypesController"/> class
    /// </summary>
    /// <param name="kineticChainTypeService">The kinetic chain type service</param>
    /// <param name="logger">The logger</param>
    public KineticChainTypesController(
        IKineticChainTypeService kineticChainTypeService,
        ILogger<KineticChainTypesController> logger)
    {
        _kineticChainTypeService = kineticChainTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active kinetic chain types
    /// </summary>
    /// <returns>A collection of active kinetic chain types</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKineticChainTypes()
    {
        _logger.LogInformation("Getting all active kinetic chain types");
        
        var result = await _kineticChainTypeService.GetAllActiveAsync();
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            _ => Ok(result.Data) // GetAll should always succeed, even if empty
        };
    }

    /// <summary>
    /// Gets a kinetic chain type by ID
    /// </summary>
    /// <param name="id">The ID of the kinetic chain type to retrieve in the format "kineticchaintype-{guid}"</param>
    /// <returns>The kinetic chain type if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetKineticChainTypeById(string id)
    {
        _logger.LogInformation("Getting kinetic chain type with ID: {Id}", id);
        
        var result = await _kineticChainTypeService.GetByIdAsync(KineticChainTypeId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets a kinetic chain type by value
    /// </summary>
    /// <param name="value">The value of the kinetic chain type to retrieve</param>
    /// <returns>The kinetic chain type if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetKineticChainTypeByValue(string value)
    {
        _logger.LogInformation("Getting kinetic chain type with value: {Value}", value);
        
        var result = await _kineticChainTypeService.GetByValueAsync(value);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}
