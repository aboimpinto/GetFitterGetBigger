using GetFitterGetBigger.API.Configuration;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing exercise weight types reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/[controller]")]
[Produces("application/json")]
[Tags("Reference Tables")]
public class ExerciseWeightTypesController : ReferenceTablesBaseController
{
    private readonly IExerciseWeightTypeService _exerciseWeightTypeService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ExerciseWeightTypesController"/> class
    /// </summary>
    /// <param name="unitOfWorkProvider">The unit of work provider</param>
    /// <param name="cacheService">The cache service</param>
    /// <param name="cacheConfiguration">The cache configuration</param>
    /// <param name="exerciseWeightTypeService">The exercise weight type service</param>
    /// <param name="logger">The logger</param>
    public ExerciseWeightTypesController(
        IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
        ICacheService cacheService,
        IOptions<CacheConfiguration> cacheConfiguration,
        IExerciseWeightTypeService exerciseWeightTypeService,
        ILogger<ExerciseWeightTypesController> logger)
        : base(unitOfWorkProvider, cacheService, cacheConfiguration, logger)
    {
        _exerciseWeightTypeService = exerciseWeightTypeService;
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
        var dtos = await _exerciseWeightTypeService.GetAllAsDtosAsync();
        return Ok(dtos);
    }
    
    /// <summary>
    /// Gets an exercise weight type by ID
    /// </summary>
    /// <param name="id">The exercise weight type ID in format "exerciseweighttype-{guid}"</param>
    /// <returns>The exercise weight type if found</returns>
    /// <response code="200">Returns the exercise weight type</response>
    /// <response code="404">If the exercise weight type is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var dto = await _exerciseWeightTypeService.GetByIdAsDtoAsync(id);
        
        if (dto == null)
        {
            return NotFound();
        }
        
        return Ok(dto);
    }
    
    /// <summary>
    /// Gets an exercise weight type by value
    /// </summary>
    /// <param name="value">The exercise weight type value (e.g., "Bodyweight Only", "Weight Required")</param>
    /// <returns>The exercise weight type if found</returns>
    /// <response code="200">Returns the exercise weight type</response>
    /// <response code="404">If the exercise weight type is not found</response>
    /// <remarks>
    /// The search is case-insensitive
    /// </remarks>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByValue(string value)
    {
        var dto = await _exerciseWeightTypeService.GetByValueAsDtoAsync(value);
        
        if (dto == null)
        {
            return NotFound();
        }
        
        return Ok(dto);
    }
    
    /// <summary>
    /// Gets an exercise weight type by code
    /// </summary>
    /// <param name="code">The exercise weight type code (e.g., "BODYWEIGHT_ONLY", "WEIGHT_REQUIRED")</param>
    /// <returns>The exercise weight type if found</returns>
    /// <response code="200">Returns the exercise weight type</response>
    /// <response code="404">If the exercise weight type is not found</response>
    /// <remarks>
    /// The code search is case-sensitive. Valid codes are:
    /// - BODYWEIGHT_ONLY
    /// - BODYWEIGHT_OPTIONAL
    /// - WEIGHT_REQUIRED
    /// - MACHINE_WEIGHT
    /// - NO_WEIGHT
    /// </remarks>
    [HttpGet("ByCode/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var dto = await _exerciseWeightTypeService.GetByCodeAsDtoAsync(code);
        
        if (dto == null)
        {
            return NotFound();
        }
        
        return Ok(dto);
    }
}