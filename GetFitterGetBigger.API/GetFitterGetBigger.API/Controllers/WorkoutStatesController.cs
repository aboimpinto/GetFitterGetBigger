using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving workout state reference data
/// </summary>
[ApiController]
[Route("api/workout-states")]
public class WorkoutStatesController : ControllerBase
{
    private readonly IWorkoutStateService _workoutStateService;
    private readonly ILogger<WorkoutStatesController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkoutStatesController"/> class
    /// </summary>
    /// <param name="workoutStateService">The workout state service</param>
    /// <param name="logger">The logger</param>
    public WorkoutStatesController(
        IWorkoutStateService workoutStateService,
        ILogger<WorkoutStatesController> logger)
    {
        _workoutStateService = workoutStateService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active workout states
    /// </summary>
    /// <returns>A collection of active workout states</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkoutStates()
    {
        _logger.LogInformation("Getting all active workout states");
        
        var result = await _workoutStateService.GetAllActiveAsync();
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            _ => Ok(result.Data) // GetAll should always succeed, even if empty
        };
    }

    /// <summary>
    /// Gets a workout state by ID
    /// </summary>
    /// <param name="id">The ID of the workout state to retrieve in the format "workoutstate-{guid}"</param>
    /// <returns>The workout state if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWorkoutStateById(string id)
    {
        _logger.LogInformation("Getting workout state with ID: {Id}", id);
        
        var result = await _workoutStateService.GetByIdAsync(WorkoutStateId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets a workout state by value
    /// </summary>
    /// <param name="value">The value of the workout state to retrieve</param>
    /// <returns>The workout state if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWorkoutStateByValue(string value)
    {
        _logger.LogInformation("Getting workout state with value: {Value}", value);
        
        var result = await _workoutStateService.GetByValueAsync(value);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}