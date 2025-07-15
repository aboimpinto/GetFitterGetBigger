using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving difficulty level reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/DifficultyLevels")]
public class DifficultyLevelsController : ControllerBase
{
    private readonly IDifficultyLevelService _difficultyLevelService;
    private readonly ILogger<DifficultyLevelsController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DifficultyLevelsController"/> class
    /// </summary>
    /// <param name="difficultyLevelService">The difficulty level service</param>
    /// <param name="logger">The logger</param>
    public DifficultyLevelsController(
        IDifficultyLevelService difficultyLevelService,
        ILogger<DifficultyLevelsController> logger)
    {
        _difficultyLevelService = difficultyLevelService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active difficulty levels
    /// </summary>
    /// <returns>A collection of active difficulty levels</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDifficultyLevels()
    {
        _logger.LogInformation("Getting all active difficulty levels");
        
        var result = await _difficultyLevelService.GetAllActiveAsync();
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            _ => Ok(result.Data) // GetAll should always succeed, even if empty
        };
    }

    /// <summary>
    /// Gets a difficulty level by ID
    /// </summary>
    /// <param name="id">The ID of the difficulty level to retrieve in the format "difficultylevel-{guid}"</param>
    /// <returns>The difficulty level if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDifficultyLevelById(string id)
    {
        _logger.LogInformation("Getting difficulty level with ID: {Id}", id);
        
        var result = await _difficultyLevelService.GetByIdAsync(DifficultyLevelId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets a difficulty level by value
    /// </summary>
    /// <param name="value">The value of the difficulty level to retrieve</param>
    /// <returns>The difficulty level if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDifficultyLevelByValue(string value)
    {
        _logger.LogInformation("Getting difficulty level with value: {Value}", value);
        
        var result = await _difficultyLevelService.GetByValueAsync(value);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}
