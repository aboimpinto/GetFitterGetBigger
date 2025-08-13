using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup;
using GetFitterGetBigger.API.Services.Results;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing muscle group data
/// </summary>
[ApiController]
[Route("api/ReferenceTables/[controller]")]
public class MuscleGroupsController : ControllerBase
{
    private readonly IMuscleGroupService _muscleGroupService;
    private readonly ILogger<MuscleGroupsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MuscleGroupsController"/> class
    /// </summary>
    /// <param name="muscleGroupService">The muscle group service</param>
    /// <param name="logger">The logger</param>
    public MuscleGroupsController(
        IMuscleGroupService muscleGroupService,
        ILogger<MuscleGroupsController> logger)
    {
        _muscleGroupService = muscleGroupService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all muscle groups
    /// </summary>
    /// <returns>A collection of muscle groups with full details including body part information</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MuscleGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        await _muscleGroupService.GetAllAsync() switch
        {
            { Data: var data } => Ok(data) // GetAll should always succeed, even if empty
        };

    /// <summary>
    /// Gets a muscle group by ID
    /// </summary>
    /// <param name="id">The ID of the muscle group to retrieve in the format "musclegroup-{guid}"</param>
    /// <returns>The muscle group with full details including body part information</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MuscleGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(string id) =>
        await _muscleGroupService.GetByIdAsync(MuscleGroupId.ParseOrEmpty(id)) switch
        {
            { IsSuccess: true, Data: var data } => Ok(data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.InvalidFormat, StructuredErrors: var errors } => BadRequest(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.ValidationFailed, StructuredErrors: var errors } => BadRequest(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.InternalError, StructuredErrors: var errors } => StatusCode(StatusCodes.Status500InternalServerError, new { errors }),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };

    /// <summary>
    /// Gets a muscle group by value
    /// </summary>
    /// <param name="value">The value of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(typeof(MuscleGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByValue(string value) =>
        await _muscleGroupService.GetByNameAsync(value) switch
        {
            { IsSuccess: true, Data: var data } => Ok(data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.InvalidFormat, StructuredErrors: var errors } => BadRequest(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.ValidationFailed, StructuredErrors: var errors } => BadRequest(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.InternalError, StructuredErrors: var errors } => StatusCode(StatusCodes.Status500InternalServerError, new { errors }),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    
    
    /// <summary>
    /// Gets all muscle groups for a specific body part
    /// </summary>
    /// <param name="bodyPartId">The ID of the body part in the format "bodypart-{guid}"</param>
    /// <returns>A collection of muscle groups for the specified body part with full details</returns>
    [HttpGet("ByBodyPart/{bodyPartId}")]
    [ProducesResponseType(typeof(IEnumerable<MuscleGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByBodyPart(string bodyPartId) =>
        await _muscleGroupService.GetByBodyPartAsync(BodyPartId.ParseOrEmpty(bodyPartId)) switch
        {
            { IsSuccess: true, Data: var data } => Ok(data),
            { PrimaryErrorCode: ServiceErrorCode.InvalidFormat, StructuredErrors: var errors } => BadRequest(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.ValidationFailed, StructuredErrors: var errors } => BadRequest(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.InternalError, StructuredErrors: var errors } => StatusCode(StatusCodes.Status500InternalServerError, new { errors }),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    
    /// <summary>
    /// Creates a new muscle group
    /// </summary>
    /// <param name="request">The muscle group creation request</param>
    /// <returns>The created muscle group</returns>
    [HttpPost]
    [ProducesResponseType(typeof(MuscleGroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateMuscleGroupDto request) =>
        await _muscleGroupService.CreateAsync(request.ToCommand()) switch
        {
            { IsSuccess: true, Data: var data } => CreatedAtAction(nameof(GetById), new { id = data.Id }, data),
            { PrimaryErrorCode: ServiceErrorCode.AlreadyExists, StructuredErrors: var errors } => Conflict(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.InvalidFormat, StructuredErrors: var errors } => BadRequest(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.ValidationFailed, StructuredErrors: var errors } => BadRequest(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.NotFound, StructuredErrors: var errors } => BadRequest(new { errors }), // Body part not found
            { PrimaryErrorCode: ServiceErrorCode.InternalError, StructuredErrors: var errors } => StatusCode(StatusCodes.Status500InternalServerError, new { errors }),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    
    /// <summary>
    /// Updates an existing muscle group
    /// </summary>
    /// <param name="id">The ID of the muscle group to update</param>
    /// <param name="request">The muscle group update request</param>
    /// <returns>The updated muscle group</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MuscleGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateMuscleGroupDto request) =>
        await _muscleGroupService.UpdateAsync(MuscleGroupId.ParseOrEmpty(id), request.ToCommand()) switch
        {
            { IsSuccess: true, Data: var data } => Ok(data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.AlreadyExists, StructuredErrors: var errors } => Conflict(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.InvalidFormat, StructuredErrors: var errors } => BadRequest(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.ValidationFailed, StructuredErrors: var errors } => BadRequest(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.ConcurrencyConflict, StructuredErrors: var errors } => Conflict(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.InternalError, StructuredErrors: var errors } => StatusCode(StatusCodes.Status500InternalServerError, new { errors }),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    
    /// <summary>
    /// Deletes muscle group (soft delete)
    /// </summary>
    /// <param name="id">The ID of the muscle group to delete</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(string id) =>
        await _muscleGroupService.DeleteAsync(MuscleGroupId.ParseOrEmpty(id)) switch
        {
            { IsSuccess: true } => NoContent(),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.InvalidFormat, StructuredErrors: var errors } => BadRequest(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.ValidationFailed, StructuredErrors: var errors } => BadRequest(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.ConcurrencyConflict, StructuredErrors: var errors } => Conflict(new { errors }),
            { PrimaryErrorCode: ServiceErrorCode.InternalError, StructuredErrors: var errors } => StatusCode(StatusCodes.Status500InternalServerError, new { errors }),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
}