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
    /// Gets all active execution protocols
    /// </summary>
    /// <returns>A collection of active execution protocols</returns>
    /// <response code="200">Returns the collection of execution protocols</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExecutionProtocolDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllExecutionProtocols()
    {
        _logger.LogInformation("Getting all execution protocols");
        
        var protocols = await _executionProtocolService.GetAllAsDtosAsync();
        
        _logger.LogInformation("Retrieved {Count} execution protocols", protocols.Count());
        
        return Ok(protocols);
    }

    /// <summary>
    /// Gets an execution protocol by ID
    /// </summary>
    /// <param name="id">The ID of the execution protocol in the format "executionprotocol-{guid}"</param>
    /// <returns>The execution protocol if found</returns>
    /// <response code="200">Returns the execution protocol</response>
    /// <response code="400">If the ID format is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the execution protocol is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExecutionProtocolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecutionProtocolById(string id)
    {
        _logger.LogInformation("Getting execution protocol by ID: {Id}", id);
        
        var protocol = await _executionProtocolService.GetByIdAsDtoAsync(id);
        
        if (protocol == null)
        {
            _logger.LogWarning("Execution protocol not found: {Id}", id);
            return NotFound(new { message = $"Execution protocol with ID '{id}' not found" });
        }
        
        return Ok(protocol);
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
    [ProducesResponseType(typeof(ExecutionProtocolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecutionProtocolByCode(string code)
    {
        _logger.LogInformation("Getting execution protocol by code: {Code}", code);
        
        var protocol = await _executionProtocolService.GetByCodeAsDtoAsync(code);
        
        if (protocol == null)
        {
            _logger.LogWarning("Execution protocol not found with code: {Code}", code);
            return NotFound(new { message = $"Execution protocol with code '{code}' not found" });
        }
        
        return Ok(protocol);
    }
}