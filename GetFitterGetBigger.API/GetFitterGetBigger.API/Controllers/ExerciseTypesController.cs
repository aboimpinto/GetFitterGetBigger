using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving exercise type reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/ExerciseTypes")]
public class ExerciseTypesController : ControllerBase
{
    private readonly IExerciseTypeService _exerciseTypeService;
    private readonly ILogger<ExerciseTypesController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ExerciseTypesController"/> class
    /// </summary>
    /// <param name="exerciseTypeService">The exercise type service</param>
    /// <param name="logger">The logger</param>
    public ExerciseTypesController(
        IExerciseTypeService exerciseTypeService,
        ILogger<ExerciseTypesController> logger)
    {
        _exerciseTypeService = exerciseTypeService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active exercise types
    /// </summary>
    /// <returns>A collection of active exercise types</returns>
    /// <response code="200">Returns the list of exercise types</response>
    /// <remarks>
    /// Exercise types include:
    /// - Warmup: Exercises performed to prepare the body for more intense activity
    /// - Workout: Main exercises that form the core of the training session
    /// - Cooldown: Exercises performed to help the body recover after intense activity
    /// - Rest: Periods of rest between exercises or sets
    /// 
    /// Note: The "Rest" type has special business rules - it cannot be combined with other exercise types
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExerciseTypes()
    {
        _logger.LogInformation("Getting all active exercise types");
        
        var result = await _exerciseTypeService.GetAllActiveAsync();
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            _ => Ok(result.Data) // GetAll should always succeed, even if empty
        };
    }

    /// <summary>
    /// Gets an exercise type by ID
    /// </summary>
    /// <param name="id">The ID of the exercise type to retrieve in the format "exercisetype-{guid}"</param>
    /// <returns>The exercise type if found, 404 Not Found otherwise</returns>
    /// <response code="200">Returns the exercise type</response>
    /// <response code="404">If the exercise type is not found</response>
    /// <response code="400">If the exercise type ID format is invalid</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExerciseTypeById(string id)
    {
        _logger.LogInformation("Getting exercise type with ID: {Id}", id);
        
        var result = await _exerciseTypeService.GetByIdAsync(ExerciseTypeId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets an exercise type by value
    /// </summary>
    /// <param name="value">The value of the exercise type to retrieve (e.g., "Warmup", "Workout", "Cooldown", "Rest")</param>
    /// <returns>The exercise type if found, 404 Not Found otherwise</returns>
    /// <response code="200">Returns the exercise type</response>
    /// <response code="404">If the exercise type is not found</response>
    /// <response code="400">If the exercise type value is invalid</response>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExerciseTypeByValue(string value)
    {
        _logger.LogInformation("Getting exercise type with value: {Value}", value);
        
        var result = await _exerciseTypeService.GetByValueAsync(value);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}