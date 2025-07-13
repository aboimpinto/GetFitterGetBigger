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
    /// Gets all workout categories
    /// </summary>
    /// <param name="includeInactive">Optional parameter to include inactive categories (default: false)</param>
    /// <returns>A collection of workout categories</returns>
    /// <response code="200">Returns the collection of workout categories</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllWorkoutCategories([FromQuery] bool includeInactive = false)
    {
        _logger.LogInformation("Getting all workout categories (includeInactive: {IncludeInactive})", includeInactive);
        
        var categories = await _workoutCategoryService.GetAllAsWorkoutCategoryDtosAsync(includeInactive);
        var response = new WorkoutCategoriesResponseDto
        {
            WorkoutCategories = categories.ToList()
        };
        
        _logger.LogInformation("Retrieved {Count} workout categories", categories.Count());
        
        // Set cache headers for reference data (1 hour cache)
        if (Response != null)
        {
            Response.Headers.CacheControl = "public, max-age=3600";
        }
        
        return Ok(response);
    }

    /// <summary>
    /// Gets a workout category by ID
    /// </summary>
    /// <param name="id">The ID of the workout category in the format "workoutcategory-{guid}"</param>
    /// <param name="includeInactive">Optional parameter to include inactive categories (default: false)</param>
    /// <returns>The workout category if found</returns>
    /// <response code="200">Returns the workout category</response>
    /// <response code="400">If the ID format is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="404">If the workout category is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkoutCategoryById(string id, [FromQuery] bool includeInactive = false)
    {
        _logger.LogInformation("Getting workout category by ID: {Id} (includeInactive: {IncludeInactive})", id, includeInactive);
        
        try
        {
            var category = await _workoutCategoryService.GetByIdAsWorkoutCategoryDtoAsync(id, includeInactive);
            
            if (category == null)
            {
                _logger.LogWarning("Workout category not found: {Id}", id);
                return NotFound(new { message = "Workout category not found" });
            }
            
            // Set cache headers for reference data (1 hour cache)
            if (Response != null)
            {
                Response.Headers.CacheControl = "public, max-age=3600";
            }
            
            return Ok(category);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid workout category ID format: {Id}. Error: {Error}", id, ex.Message);
            return NotFound(new { message = "Workout category not found" });
        }
    }
}