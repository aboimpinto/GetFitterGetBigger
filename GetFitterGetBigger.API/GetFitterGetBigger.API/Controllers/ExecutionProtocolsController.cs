using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.ReferenceTables.ExecutionProtocol;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving execution protocol reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/ExecutionProtocols")]
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
        _executionProtocolService = executionProtocolService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active execution protocols
    /// </summary>
    /// <returns>A collection of active execution protocols</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExecutionProtocols()
    {
        _logger.LogInformation("Getting all active execution protocols");
        
        var result = await _executionProtocolService.GetAllActiveAsync();
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            _ => Ok(result.Data) // GetAll should always succeed, even if empty
        };
    }

    /// <summary>
    /// Gets an execution protocol by ID
    /// </summary>
    /// <param name="id">The ID of the execution protocol to retrieve in the format "executionprotocol-{guid}"</param>
    /// <returns>The execution protocol if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExecutionProtocolById(string id)
    {
        _logger.LogInformation("Getting execution protocol with ID: {Id}", id);
        
        var result = await _executionProtocolService.GetByIdAsync(ExecutionProtocolId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets an execution protocol by value
    /// </summary>
    /// <param name="value">The value of the execution protocol to retrieve</param>
    /// <returns>The execution protocol if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExecutionProtocolByValue(string value)
    {
        _logger.LogInformation("Getting execution protocol with value: {Value}", value);
        
        var result = await _executionProtocolService.GetByValueAsync(value);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets an execution protocol by code
    /// </summary>
    /// <param name="code">The code of the execution protocol to retrieve</param>
    /// <returns>The execution protocol if found, 404 Not Found otherwise</returns>
    [HttpGet("ByCode/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExecutionProtocolByCode(string code)
    {
        _logger.LogInformation("Getting execution protocol with code: {Code}", code);
        
        var result = await _executionProtocolService.GetByCodeAsync(code);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}