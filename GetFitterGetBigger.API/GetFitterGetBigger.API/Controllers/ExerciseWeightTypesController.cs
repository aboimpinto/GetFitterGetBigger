using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.ReferenceTables.ExerciseWeightType;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing exercise weight types reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/[controller]")]
[Produces("application/json")]
public class ExerciseWeightTypesController : ControllerBase
{
    private readonly IExerciseWeightTypeService _exerciseWeightTypeService;
    private readonly ILogger<ExerciseWeightTypesController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ExerciseWeightTypesController"/> class
    /// </summary>
    /// <param name="exerciseWeightTypeService">The exercise weight type service</param>
    /// <param name="logger">The logger</param>
    public ExerciseWeightTypesController(
        IExerciseWeightTypeService exerciseWeightTypeService,
        ILogger<ExerciseWeightTypesController> logger)
    {
        _exerciseWeightTypeService = exerciseWeightTypeService;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets all exercise weight types
    /// </summary>
    /// <returns>List of all exercise weight types</returns>
    /// <response code="200">Returns the list of exercise weight types</response>
    /// <remarks>
    /// Exercise weight types define how weight is used in different exercises:
    /// - BODYWEIGHT_ONLY: Exercises that cannot have external weight added (e.g., running, planks)
    /// - BODYWEIGHT_OPTIONAL: Exercises that can be performed with or without additional weight (e.g., pull-ups, dips)
    /// - WEIGHT_REQUIRED: Exercises that must have external weight specified (e.g., barbell bench press)
    /// - MACHINE_WEIGHT: Exercises performed on machines with weight stacks (e.g., lat pulldown)
    /// - NO_WEIGHT: Exercises that do not use weight as a metric (e.g., stretching, mobility work)
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all active exercise weight types");
        
        var result = await _exerciseWeightTypeService.GetAllActiveAsync();
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            _ => Ok(result.Data) // GetAll should always succeed, even if empty
        };
    }
    
    /// <summary>
    /// Gets an exercise weight type by ID
    /// </summary>
    /// <param name="id">The exercise weight type ID in format "exerciseweighttype-{guid}"</param>
    /// <returns>The exercise weight type if found</returns>
    /// <response code="200">Returns the exercise weight type</response>
    /// <response code="404">If the exercise weight type is not found</response>
    /// <response code="400">If the ID format is invalid</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(string id)
    {
        _logger.LogInformation("Getting exercise weight type with ID: {Id}", id);
        
        var result = await _exerciseWeightTypeService.GetByIdAsync(ExerciseWeightTypeId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
    
    /// <summary>
    /// Gets an exercise weight type by value
    /// </summary>
    /// <param name="value">The exercise weight type value (e.g., "Bodyweight Only", "Weight Required")</param>
    /// <returns>The exercise weight type if found</returns>
    /// <response code="200">Returns the exercise weight type</response>
    /// <response code="404">If the exercise weight type is not found</response>
    /// <response code="400">If the value is empty</response>
    /// <remarks>
    /// The search is case-insensitive
    /// </remarks>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByValue(string value)
    {
        _logger.LogInformation("Getting exercise weight type with value: {Value}", value);
        
        var result = await _exerciseWeightTypeService.GetByValueAsync(value);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}