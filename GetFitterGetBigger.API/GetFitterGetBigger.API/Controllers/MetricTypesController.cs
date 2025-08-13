using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.ReferenceTables.MetricType;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving metric type reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/MetricTypes")]
public class MetricTypesController : ControllerBase
{
    private readonly IMetricTypeService _metricTypeService;
    private readonly ILogger<MetricTypesController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MetricTypesController"/> class
    /// </summary>
    /// <param name="metricTypeService">The metric type service</param>
    /// <param name="logger">The logger</param>
    public MetricTypesController(
        IMetricTypeService metricTypeService,
        ILogger<MetricTypesController> logger)
    {
        _metricTypeService = metricTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active metric types
    /// </summary>
    /// <returns>A collection of active metric types</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetricTypes()
    {
        _logger.LogInformation("Getting all active metric types");
        
        var result = await _metricTypeService.GetAllActiveAsync();
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            _ => Ok(result.Data) // GetAll should always succeed, even if empty
        };
    }

    /// <summary>
    /// Gets a metric type by ID
    /// </summary>
    /// <param name="id">The ID of the metric type to retrieve in the format "metrictype-{guid}"</param>
    /// <returns>The metric type if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMetricTypeById(string id)
    {
        _logger.LogInformation("Getting metric type with ID: {Id}", id);
        
        var result = await _metricTypeService.GetByIdAsync(MetricTypeId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets a metric type by value
    /// </summary>
    /// <param name="value">The value of the metric type to retrieve</param>
    /// <returns>The metric type if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMetricTypeByValue(string value)
    {
        _logger.LogInformation("Getting metric type with value: {Value}", value);
        
        var result = await _metricTypeService.GetByValueAsync(value);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}
