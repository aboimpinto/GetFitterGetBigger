using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing execution protocols reference data
/// </summary>
[ApiController]
[Route("api/execution-protocols")]
[Produces("application/json")]
[Tags("Workout Reference Data")]
public class ExecutionProtocolsController : ControllerBase
{
    private readonly IExecutionProtocolService _executionProtocolService;
    private readonly ILogger<ExecutionProtocolsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionProtocolsController"/> class
    /// </summary>
    /// <param name="executionProtocolService">The execution protocol service</param>
    /// <param name="logger">The logger</param>
    public ExecutionProtocolsController(
        IExecutionProtocolService executionProtocolService,
        ILogger<ExecutionProtocolsController> logger)
    {
        _executionProtocolService = executionProtocolService ?? throw new ArgumentNullException(nameof(executionProtocolService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all execution protocols
    /// </summary>
    /// <param name="includeInactive">Optional parameter to include inactive protocols (default: false)</param>
    /// <returns>A collection of execution protocols</returns>
    /// <response code="200">Returns the collection of execution protocols</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllExecutionProtocols([FromQuery] bool includeInactive = false)
    {
        _logger.LogInformation("Getting all execution protocols (includeInactive: {IncludeInactive})", includeInactive);
        
        var protocols = await _executionProtocolService.GetAllAsExecutionProtocolDtosAsync(includeInactive);
        var response = new ExecutionProtocolsResponseDto
        {
            ExecutionProtocols = protocols.ToList()
        };
        
        _logger.LogInformation("Retrieved {Count} execution protocols", protocols.Count());
        
        // Set cache headers for reference data (1 hour cache)
        if (Response != null)
        {
            Response.Headers.CacheControl = "public, max-age=3600";
        }
        
        return Ok(response);
    }

    /// <summary>
    /// Gets an execution protocol by ID
    /// </summary>
    /// <param name="id">The ID of the execution protocol in the format "executionprotocol-{guid}"</param>
    /// <param name="includeInactive">Optional parameter to include inactive protocols (default: false)</param>
    /// <returns>The execution protocol if found</returns>
    /// <response code="200">Returns the execution protocol</response>
    /// <response code="400">If the ID format is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the execution protocol is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecutionProtocolById(string id, [FromQuery] bool includeInactive = false)
    {
        _logger.LogInformation("Getting execution protocol by ID: {Id} (includeInactive: {IncludeInactive})", id, includeInactive);
        
        try
        {
            var protocol = await _executionProtocolService.GetByIdAsExecutionProtocolDtoAsync(id, includeInactive);
            
            if (protocol == null)
            {
                _logger.LogWarning("Execution protocol not found: {Id}", id);
                return NotFound(new { message = "Execution protocol not found" });
            }
            
            // Set cache headers for reference data (1 hour cache)
            if (Response != null)
        {
            Response.Headers.CacheControl = "public, max-age=3600";
        }
            
            return Ok(protocol);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid execution protocol ID format: {Id}. Error: {Error}", id, ex.Message);
            return NotFound(new { message = "Execution protocol not found" });
        }
    }

    /// <summary>
    /// Gets an execution protocol by code
    /// </summary>
    /// <param name="code">The code of the execution protocol (e.g., "STANDARD", "SUPERSET")</param>
    /// <returns>The execution protocol if found</returns>
    /// <response code="200">Returns the execution protocol</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the execution protocol is not found</response>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecutionProtocolByCode(string code)
    {
        _logger.LogInformation("Getting execution protocol by code: {Code}", code);
        
        var protocol = await _executionProtocolService.GetByCodeAsDtoAsync(code);
        
        if (protocol == null)
        {
            _logger.LogWarning("Execution protocol not found with code: {Code}", code);
            return NotFound(new { message = "Execution protocol not found" });
        }
        
        // Set cache headers for reference data (1 hour cache)
        if (Response != null)
        {
            Response.Headers.CacheControl = "public, max-age=3600";
        }
        
        return Ok(protocol);
    }
}