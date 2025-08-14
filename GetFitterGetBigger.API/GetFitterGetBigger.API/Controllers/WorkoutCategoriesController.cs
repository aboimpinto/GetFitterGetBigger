using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutCategory;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.DTOs;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving workout category reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/WorkoutCategories")]
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
        _workoutCategoryService = workoutCategoryService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active workout categories
    /// </summary>
    /// <returns>A collection of active workout categories</returns>
    [HttpGet]
    [ProducesResponseType(typeof(WorkoutCategoriesResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkoutCategories()
    {
        _logger.LogInformation("Getting all active workout categories");
        
        var result = await _workoutCategoryService.GetAllAsync();
        
        var response = new WorkoutCategoriesResponseDto
        {
            WorkoutCategories = result.Data.ToList()
        };
        
        return Ok(response);
    }

    /// <summary>
    /// Gets a workout category by ID
    /// </summary>
    /// <param name="id">The ID of the workout category to retrieve in the format "workoutcategory-{guid}"</param>
    /// <returns>The workout category if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWorkoutCategoryById(string id)
    {
        _logger.LogInformation("Getting workout category with ID: {Id}", id);
        
        var result = await _workoutCategoryService.GetByIdAsync(WorkoutCategoryId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets a workout category by value
    /// </summary>
    /// <param name="value">The value of the workout category to retrieve</param>
    /// <returns>The workout category if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value?}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWorkoutCategoryByValue(string value)
    {
        _logger.LogInformation("Getting workout category with value: {Value}", value);
        
        var result = await _workoutCategoryService.GetByValueAsync(value ?? string.Empty);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}