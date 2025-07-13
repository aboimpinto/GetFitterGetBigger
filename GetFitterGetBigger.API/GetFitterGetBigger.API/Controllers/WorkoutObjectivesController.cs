using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing workout objectives reference data
/// </summary>
[ApiController]
[Route("api/workout-objectives")]
[Produces("application/json")]
[Tags("Workout Reference Data")]
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
        _workoutObjectiveService = workoutObjectiveService ?? throw new ArgumentNullException(nameof(workoutObjectiveService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all workout objectives
    /// </summary>
    /// <param name="includeInactive">Optional parameter to include inactive objectives (default: false)</param>
    /// <returns>A collection of workout objectives</returns>
    /// <response code="200">Returns the collection of workout objectives</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllWorkoutObjectives([FromQuery] bool includeInactive = false)
    {
        _logger.LogInformation("Getting all workout objectives (includeInactive: {IncludeInactive})", includeInactive);
        
        var objectives = await _workoutObjectiveService.GetAllAsWorkoutObjectiveDtosAsync(includeInactive);
        var response = new WorkoutObjectivesResponseDto
        {
            WorkoutObjectives = objectives.ToList()
        };
        
        _logger.LogInformation("Retrieved {Count} workout objectives", objectives.Count());
        
        // Set cache headers for reference data (1 hour cache)
        if (Response != null)
        {
            Response.Headers.CacheControl = "public, max-age=3600";
        }
        
        return Ok(response);
    }

    /// <summary>
    /// Gets a workout objective by ID
    /// </summary>
    /// <param name="id">The ID of the workout objective in the format "workoutobjective-{guid}"</param>
    /// <param name="includeInactive">Optional parameter to include inactive objectives (default: false)</param>
    /// <returns>The workout objective if found</returns>
    /// <response code="200">Returns the workout objective</response>
    /// <response code="400">If the ID format is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the workout objective is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkoutObjectiveById(string id, [FromQuery] bool includeInactive = false)
    {
        _logger.LogInformation("Getting workout objective by ID: {Id} (includeInactive: {IncludeInactive})", id, includeInactive);
        
        try
        {
            var objective = await _workoutObjectiveService.GetByIdAsWorkoutObjectiveDtoAsync(id, includeInactive);
            
            if (objective == null)
            {
                _logger.LogWarning("Workout objective not found: {Id}", id);
                return NotFound(new { message = "Workout objective not found" });
            }
            
            // Set cache headers for reference data (1 hour cache)
            if (Response != null)
            {
                Response.Headers.CacheControl = "public, max-age=3600";
            }
            
            return Ok(objective);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid workout objective ID format: {Id}. Error: {Error}", id, ex.Message);
            return NotFound(new { message = "Workout objective not found" });
        }
    }
}