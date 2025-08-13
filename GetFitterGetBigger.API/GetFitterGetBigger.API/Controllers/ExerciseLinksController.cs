using System;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Exercise.Features.Links;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing exercise links (warmup and cooldown associations)
/// </summary>
[ApiController]
[Route("api/exercises/{exerciseId}/links")]
[Produces("application/json")]
[Tags("Exercise Links")]
public class ExerciseLinksController : ControllerBase
{
    private readonly IExerciseLinkService _exerciseLinkService;
    private readonly ILogger<ExerciseLinksController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExerciseLinksController"/> class
    /// </summary>
    /// <param name="exerciseLinkService">The exercise link service</param>
    /// <param name="logger">The logger</param>
    public ExerciseLinksController(IExerciseLinkService exerciseLinkService, ILogger<ExerciseLinksController> logger)
    {
        _exerciseLinkService = exerciseLinkService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new exercise link
    /// </summary>
    /// <param name="exerciseId">The source exercise ID (must be a Workout type)</param>
    /// <param name="dto">The link creation data</param>
    /// <returns>The created exercise link</returns>
    /// <response code="201">Returns the created exercise link</response>
    /// <response code="400">If the request is invalid or business rules are violated</response>
    [HttpPost]
    [ProducesResponseType(typeof(ExerciseLinkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLink(string exerciseId, [FromBody] CreateExerciseLinkDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Creating exercise link from {SourceId} to {TargetId} as {LinkType}", 
                exerciseId, dto.TargetExerciseId, dto.LinkType);
            
            var result = await _exerciseLinkService.CreateLinkAsync(exerciseId, dto);
            
            return CreatedAtAction(
                nameof(GetLinks), 
                new { exerciseId = exerciseId }, 
                result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request creating exercise link");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all links for an exercise
    /// </summary>
    /// <param name="exerciseId">The exercise ID</param>
    /// <param name="linkType">Optional filter by link type (Warmup or Cooldown)</param>
    /// <param name="includeExerciseDetails">Whether to include full exercise details</param>
    /// <returns>The exercise links</returns>
    /// <response code="200">Returns the exercise links</response>
    /// <response code="400">If the exercise ID is invalid</response>
    [HttpGet]
    [ProducesResponseType(typeof(ExerciseLinksResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLinks(
        string exerciseId, 
        [FromQuery] string? linkType = null,
        [FromQuery] bool includeExerciseDetails = false)
    {
        try
        {
            _logger.LogInformation("Getting exercise links for {ExerciseId} with type filter: {LinkType}", 
                exerciseId, linkType);
            
            var result = await _exerciseLinkService.GetLinksAsync(exerciseId, linkType, includeExerciseDetails);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid exercise ID: {ExerciseId}", exerciseId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets suggested links for an exercise
    /// </summary>
    /// <param name="exerciseId">The exercise ID</param>
    /// <param name="count">Number of suggestions to return (default: 5)</param>
    /// <returns>Suggested exercise links based on usage patterns</returns>
    /// <response code="200">Returns the suggested links</response>
    /// <response code="400">If the exercise ID is invalid</response>
    [HttpGet("suggested")]
    [ProducesResponseType(typeof(ExerciseLinkDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSuggestedLinks(string exerciseId, [FromQuery] int count = 5)
    {
        try
        {
            _logger.LogInformation("Getting suggested links for {ExerciseId}, count: {Count}", 
                exerciseId, count);
            
            var result = await _exerciseLinkService.GetSuggestedLinksAsync(exerciseId, count);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid exercise ID: {ExerciseId}", exerciseId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an exercise link
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="linkId">The link ID to update</param>
    /// <param name="dto">The update data</param>
    /// <returns>The updated exercise link</returns>
    /// <response code="200">Returns the updated exercise link</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the link is not found</response>
    [HttpPut("{linkId}")]
    [ProducesResponseType(typeof(ExerciseLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLink(string exerciseId, string linkId, [FromBody] UpdateExerciseLinkDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Updating exercise link {LinkId} for exercise {ExerciseId}", 
                linkId, exerciseId);
            
            var result = await _exerciseLinkService.UpdateLinkAsync(exerciseId, linkId, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error updating exercise link");
            
            // Check if it's a "not found" error
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new { error = ex.Message });
            }
            
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an exercise link (soft delete)
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="linkId">The link ID to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">The link was deleted successfully</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the link is not found</response>
    [HttpDelete("{linkId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLink(string exerciseId, string linkId)
    {
        try
        {
            _logger.LogInformation("Deleting exercise link {LinkId} for exercise {ExerciseId}", 
                linkId, exerciseId);
            
            var result = await _exerciseLinkService.DeleteLinkAsync(exerciseId, linkId);
            
            if (!result)
            {
                return NotFound();
            }
            
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error deleting exercise link");
            return BadRequest(new { error = ex.Message });
        }
    }
}