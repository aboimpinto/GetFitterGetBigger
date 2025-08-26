using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise.Features.Links;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing exercise links (warmup and cooldown associations)
/// </summary>
[ApiController]
[Route("api/exercises/{exerciseId}/links")]
[Produces("application/json")]
[Tags("Exercise Links")]
public class ExerciseLinksController(
    IExerciseLinkService exerciseLinkService,
    ILogger<ExerciseLinksController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new exercise link with enhanced four-way linking support
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="dto">The link creation data</param>
    /// <returns>The created exercise link(s) - may include automatically created reverse link</returns>
    /// <response code="201">Returns the created exercise link(s)</response>
    /// <response code="400">If the request is invalid or business rules are violated</response>
    [HttpPost]
    [ProducesResponseType(typeof(BidirectionalLinkResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ExerciseLinkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLink(string exerciseId, [FromBody] CreateExerciseLinkDto dto)
    {
        var command = dto.ToCommand(exerciseId);
        return await exerciseLinkService.CreateLinkAsync(
            command.SourceExerciseId,
            command.TargetExerciseId,
            command.ActualLinkType) switch
        {
            { IsSuccess: true, Data: var data } => CreatedAtAction(
                nameof(GetLinks), 
                new { exerciseId = exerciseId }, 
                data),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Gets all links for an exercise
    /// </summary>
    /// <param name="exerciseId">The exercise ID</param>
    /// <param name="linkType">Optional filter by link type (supports: Warmup, Cooldown, WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE)</param>
    /// <param name="includeExerciseDetails">Whether to include full exercise details</param>
    /// <returns>The exercise links</returns>
    /// <response code="200">Returns the exercise links</response>
    [HttpGet]
    [ProducesResponseType(typeof(ExerciseLinksResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLinks(
        string exerciseId, 
        [FromQuery] string? linkType = null,
        [FromQuery] bool includeExerciseDetails = false)
    {
        logger.LogInformation("Getting exercise links for {ExerciseId} with type filter: {LinkType}", 
            exerciseId, linkType);
        
        var command = ExerciseLinkRequestMapper.ToGetLinksCommand(exerciseId, linkType, includeExerciseDetails);
        
        var result = await exerciseLinkService.GetLinksAsync(command);
        
        // For GET operations, always return 200 OK per search operation error handling pattern
        return Ok(result.Data);
    }

    /// <summary>
    /// Gets suggested links for an exercise
    /// </summary>
    /// <param name="exerciseId">The exercise ID</param>
    /// <param name="count">Number of suggestions to return (default: 5)</param>
    /// <returns>Suggested exercise links based on usage patterns</returns>
    /// <response code="200">Returns the suggested links</response>
    [HttpGet("suggested")]
    [ProducesResponseType(typeof(List<ExerciseLinkDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuggestedLinks(string exerciseId, [FromQuery] int count = 5)
    {
        logger.LogInformation("Getting suggested links for {ExerciseId}, count: {Count}", 
            exerciseId, count);
        
        var parsedExerciseId = ExerciseId.ParseOrEmpty(exerciseId);
        var result = await exerciseLinkService.GetSuggestedLinksAsync(parsedExerciseId, count);
        
        // For GET operations, always return 200 OK per search operation error handling pattern
        return Ok(result.Data);
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
        logger.LogInformation("Updating exercise link {LinkId} for exercise {ExerciseId}", 
            linkId, exerciseId);
        
        var command = ExerciseLinkRequestMapper.ToUpdateCommand(exerciseId, linkId, dto);
        
        var result = await exerciseLinkService.UpdateLinkAsync(command);
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Deletes an exercise link with bidirectional deletion support
    /// </summary>
    /// <param name="exerciseId">The source exercise ID</param>
    /// <param name="linkId">The link ID to delete</param>
    /// <param name="deleteReverse">Whether to delete the reverse bidirectional link (default: true)</param>
    /// <returns>No content on success</returns>
    /// <response code="204">The link was deleted successfully</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the link is not found</response>
    [HttpDelete("{linkId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLink(
        string exerciseId, 
        string linkId, 
        [FromQuery] bool deleteReverse = true)
    {
        logger.LogInformation("Deleting exercise link {LinkId} for exercise {ExerciseId} with deleteReverse: {DeleteReverse}", 
            linkId, exerciseId, deleteReverse);
        
        var parsedExerciseId = ExerciseId.ParseOrEmpty(exerciseId);
        var parsedLinkId = ExerciseLinkId.ParseOrEmpty(linkId);
        var result = await exerciseLinkService.DeleteLinkAsync(parsedExerciseId, parsedLinkId, deleteReverse);
        
        return result switch
        {
            { IsSuccess: true, Data.Value: true } => NoContent(),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }
}