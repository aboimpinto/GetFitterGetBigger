using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutObjective;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving workout objective reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/WorkoutObjectives")]
public class WorkoutObjectivesController : ControllerBase
{
    private readonly IWorkoutObjectiveService _workoutObjectiveService;
    private readonly ILogger<WorkoutObjectivesController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkoutObjectivesController"/> class
    /// </summary>
    /// <param name="workoutObjectiveService">The workout objective service</param>
    /// <param name="logger">The logger</param>
    public WorkoutObjectivesController(
        IWorkoutObjectiveService workoutObjectiveService,
        ILogger<WorkoutObjectivesController> logger)
    {
        _workoutObjectiveService = workoutObjectiveService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active workout objectives
    /// </summary>
    /// <returns>A collection of active workout objectives</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkoutObjectives()
    {
        _logger.LogInformation("Getting all active workout objectives");
        
        var result = await _workoutObjectiveService.GetAllActiveAsync();
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            _ => Ok(result.Data) // GetAll should always succeed, even if empty
        };
    }

    /// <summary>
    /// Gets a workout objective by ID
    /// </summary>
    /// <param name="id">The ID of the workout objective to retrieve in the format "workoutobjective-{guid}"</param>
    /// <returns>The workout objective if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWorkoutObjectiveById(string id)
    {
        _logger.LogInformation("Getting workout objective with ID: {Id}", id);
        
        var result = await _workoutObjectiveService.GetByIdAsync(WorkoutObjectiveId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets a workout objective by value
    /// </summary>
    /// <param name="value">The value of the workout objective to retrieve</param>
    /// <returns>The workout objective if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWorkoutObjectiveByValue(string value)
    {
        _logger.LogInformation("Getting workout objective with value: {Value}", value);
        
        var result = await _workoutObjectiveService.GetByValueAsync(value);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}