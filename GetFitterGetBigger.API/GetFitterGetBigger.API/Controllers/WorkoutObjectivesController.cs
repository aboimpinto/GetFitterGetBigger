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
    /// Gets all active workout objectives
    /// </summary>
    /// <returns>A collection of active workout objectives</returns>
    /// <response code="200">Returns the collection of workout objectives</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReferenceDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllWorkoutObjectives()
    {
        _logger.LogInformation("Getting all workout objectives");
        
        var objectives = await _workoutObjectiveService.GetAllAsDtosAsync();
        
        _logger.LogInformation("Retrieved {Count} workout objectives", objectives.Count());
        
        return Ok(objectives);
    }

    /// <summary>
    /// Gets a workout objective by ID
    /// </summary>
    /// <param name="id">The ID of the workout objective in the format "workoutobjective-{guid}"</param>
    /// <returns>The workout objective if found</returns>
    /// <response code="200">Returns the workout objective</response>
    /// <response code="400">If the ID format is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the workout objective is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReferenceDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkoutObjectiveById(string id)
    {
        _logger.LogInformation("Getting workout objective by ID: {Id}", id);
        
        var objective = await _workoutObjectiveService.GetByIdAsDtoAsync(id);
        
        if (objective == null)
        {
            _logger.LogWarning("Workout objective not found: {Id}", id);
            return NotFound(new { message = $"Workout objective with ID '{id}' not found" });
        }
        
        return Ok(objective);
    }
}