using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing workout categories reference data
/// </summary>
[ApiController]
[Route("api/workout-categories")]
[Produces("application/json")]
[Tags("Workout Reference Data")]
public class WorkoutCategoriesController : ControllerBase
{
    private readonly IWorkoutCategoryService _workoutCategoryService;
    private readonly ILogger<WorkoutCategoriesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkoutCategoriesController"/> class
    /// </summary>
    /// <param name="workoutCategoryService">The workout category service</param>
    /// <param name="logger">The logger</param>
    public WorkoutCategoriesController(
        IWorkoutCategoryService workoutCategoryService,
        ILogger<WorkoutCategoriesController> logger)
    {
        _workoutCategoryService = workoutCategoryService ?? throw new ArgumentNullException(nameof(workoutCategoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all active workout categories
    /// </summary>
    /// <returns>A collection of active workout categories</returns>
    /// <response code="200">Returns the collection of workout categories</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkoutCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllWorkoutCategories()
    {
        _logger.LogInformation("Getting all workout categories");
        
        var categories = await _workoutCategoryService.GetAllAsDtosAsync();
        
        _logger.LogInformation("Retrieved {Count} workout categories", categories.Count());
        
        return Ok(categories);
    }

    /// <summary>
    /// Gets a workout category by ID
    /// </summary>
    /// <param name="id">The ID of the workout category in the format "workoutcategory-{guid}"</param>
    /// <returns>The workout category if found</returns>
    /// <response code="200">Returns the workout category</response>
    /// <response code="400">If the ID format is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the workout category is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WorkoutCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkoutCategoryById(string id)
    {
        _logger.LogInformation("Getting workout category by ID: {Id}", id);
        
        var category = await _workoutCategoryService.GetByIdAsDtoAsync(id);
        
        if (category == null)
        {
            _logger.LogWarning("Workout category not found: {Id}", id);
            return NotFound(new { message = $"Workout category with ID '{id}' not found" });
        }
        
        return Ok(category);
    }
}