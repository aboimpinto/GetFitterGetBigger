using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving muscle role reference data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/[controller]")]
public class MuscleRolesController : ControllerBase
{
    private readonly IMuscleRoleService _service;
    private readonly ILogger<MuscleRolesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MuscleRolesController"/> class
    /// </summary>
    /// <param name="service">The muscle role service</param>
    /// <param name="logger">The logger</param>
    public MuscleRolesController(
        IMuscleRoleService service,
        ILogger<MuscleRolesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active muscle roles
    /// </summary>
    /// <returns>A collection of active muscle roles</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReferenceDataDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMuscleRoles()
    {
        var result = await _service.GetAllActiveAsync();
        return Ok(result.Data);
    }

    /// <summary>
    /// Gets a muscle role by ID
    /// </summary>
    /// <param name="id">The ID of the muscle role to retrieve in the format "musclerole-{guid}"</param>
    /// <returns>The muscle role if found, 404 Not Found otherwise</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReferenceDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMuscleRoleById(string id)
    {
        var muscleRoleId = MuscleRoleId.ParseOrEmpty(id);
        var result = await _service.GetByIdAsync(muscleRoleId);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }

    /// <summary>
    /// Gets a muscle role by value
    /// </summary>
    /// <param name="value">The value of the muscle role to retrieve</param>
    /// <returns>The muscle role if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(typeof(ReferenceDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMuscleRoleByValue(string value)
    {
        var result = await _service.GetByValueAsync(value);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            _ => BadRequest(new { errors = result.StructuredErrors })
        };
    }
}
