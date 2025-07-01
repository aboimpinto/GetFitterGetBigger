using GetFitterGetBigger.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using GetFitterGetBigger.API.Services.Interfaces;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for retrieving muscle group data
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
    public async Task<IActionResult> GetAll()
    {
        var result = await _muscleGroupService.GetAllAsDtosAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets a muscle group by ID
    /// </summary>
    /// <param name="id">The ID of the muscle group to retrieve in the format "musclegroup-{guid}"</param>
    /// <returns>The muscle group with full details including body part information</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MuscleGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var muscleGroup = await _muscleGroupService.GetByIdAsDtoAsync(id);
            if (muscleGroup == null)
                return NotFound();
                
            return Ok(muscleGroup);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets a muscle group by name
    /// </summary>
    /// <param name="name">The name of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, 404 Not Found otherwise</returns>
    [HttpGet("ByName/{name}")]
    [ProducesResponseType(typeof(MuscleGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name)
    {
        var muscleGroup = await _muscleGroupService.GetByNameAsync(name);
        
        if (muscleGroup == null)
            return NotFound();
            
        return Ok(new MuscleGroupDto
        {
            Id = muscleGroup.Id.ToString(),
            Name = muscleGroup.Name,
            BodyPartId = muscleGroup.BodyPartId.ToString(),
            BodyPartName = muscleGroup.BodyPart?.Value,
            IsActive = muscleGroup.IsActive,
            CreatedAt = muscleGroup.CreatedAt,
            UpdatedAt = muscleGroup.UpdatedAt
        });
    }
    
    /// <summary>
    /// Gets a muscle group by value (name)
    /// </summary>
    /// <param name="value">The value (name) of the muscle group to retrieve</param>
    /// <returns>The muscle group if found, 404 Not Found otherwise</returns>
    [HttpGet("ByValue/{value}")]
    [ProducesResponseType(typeof(MuscleGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByValue(string value)
    {
        var muscleGroup = await _muscleGroupService.GetByValueAsync(value);
        
        if (muscleGroup == null)
            return NotFound();
            
        return Ok(new MuscleGroupDto
        {
            Id = muscleGroup.Id.ToString(),
            Name = muscleGroup.Name,
            BodyPartId = muscleGroup.BodyPartId.ToString(),
            BodyPartName = muscleGroup.BodyPart?.Value,
            IsActive = muscleGroup.IsActive,
            CreatedAt = muscleGroup.CreatedAt,
            UpdatedAt = muscleGroup.UpdatedAt
        });
    }
    
    /// <summary>
    /// Gets all muscle groups for a specific body part
    /// </summary>
    /// <param name="bodyPartId">The ID of the body part in the format "bodypart-{guid}"</param>
    /// <returns>A collection of muscle groups for the specified body part with full details</returns>
    [HttpGet("ByBodyPart/{bodyPartId}")]
    [ProducesResponseType(typeof(IEnumerable<MuscleGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByBodyPart(string bodyPartId)
    {
        try
        {
            var result = await _muscleGroupService.GetByBodyPartAsync(bodyPartId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// Creates a new muscle group
    /// </summary>
    /// <param name="request">The muscle group creation request</param>
    /// <returns>The created muscle group</returns>
    [HttpPost]
    [ProducesResponseType(typeof(MuscleGroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateMuscleGroupDto request)
    {
        try
        {
            var dto = await _muscleGroupService.CreateMuscleGroupAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
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
    public async Task<IActionResult> Update(string id, [FromBody] UpdateMuscleGroupDto request)
    {
        try
        {
            var dto = await _muscleGroupService.UpdateMuscleGroupAsync(id, request);
            return Ok(dto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// Deactivates a muscle group
    /// </summary>
    /// <param name="id">The ID of the muscle group to deactivate</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _muscleGroupService.DeactivateMuscleGroupAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("being used"))
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
