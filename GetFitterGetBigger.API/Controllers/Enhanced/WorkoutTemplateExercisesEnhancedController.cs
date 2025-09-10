using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Requests;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Responses;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers.Enhanced;

/// <summary>
/// Enhanced controller for managing exercises within workout templates with phase/round support
/// </summary>
[ApiController]
[Route("api/v2/workout-templates/{templateId}/exercises")]
[Authorize]
[Produces("application/json")]
[Tags("Workout Template Exercises - Enhanced API")]
public class WorkoutTemplateExercisesEnhancedController : ControllerBase
{
    private readonly IWorkoutTemplateExerciseService _service;

    public WorkoutTemplateExercisesEnhancedController(IWorkoutTemplateExerciseService service)
    {
        _service = service;
    }

    /// <summary>
    /// Add exercise to workout template with automatic warmup/cooldown linking
    /// </summary>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="request">The exercise addition request with phase/round specification</param>
    /// <returns>Result containing all added exercises (main + auto-linked)</returns>
    /// <response code="201">Returns the result with newly added exercise configurations</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the workout template is not found</response>
    [HttpPost]
    [ProducesResponseType(typeof(AddExerciseResponseDto), 201)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> AddExercise(
        [FromRoute] string templateId,
        [FromBody] AddExerciseToTemplateRequest request)
    {
        var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);
        var exerciseIdParsed = ExerciseId.ParseOrEmpty(request.ExerciseId);

        var dto = new AddExerciseDto(
            exerciseIdParsed,
            request.Phase,
            request.RoundNumber,
            request.Metadata.RootElement.GetRawText());

        var result = await _service.AddExerciseAsync(templateIdParsed, dto);

        return result switch
        {
            { IsSuccess: true } => CreatedAtAction(
                nameof(GetTemplateExercises),
                new { templateId },
                AddExerciseResponseDto.SuccessResponse(result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(ErrorResponseDto.MultipleErrors(errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(ErrorResponseDto.MultipleErrors(errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Remove exercise from workout template with automatic orphan cleanup
    /// </summary>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <returns>Result containing information about removed exercises</returns>
    /// <response code="200">Returns the result with removed exercise information</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpDelete("{exerciseId}")]
    [ProducesResponseType(typeof(RemoveExerciseResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> RemoveExercise(
        [FromRoute] string templateId,
        [FromRoute] string exerciseId)
    {
        var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);
        var exerciseIdParsed = WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId);

        var result = await _service.RemoveExerciseAsync(templateIdParsed, exerciseIdParsed);

        return result switch
        {
            { IsSuccess: true } => Ok(RemoveExerciseResponseDto.SuccessResponse(result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(ErrorResponseDto.MultipleErrors(errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(ErrorResponseDto.MultipleErrors(errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Get all exercises for a workout template organized by phase and round
    /// </summary>
    /// <param name="templateId">The ID of the workout template</param>
    /// <returns>Exercises organized by phases and rounds</returns>
    /// <response code="200">Returns the exercises organized by phases and rounds</response>
    /// <response code="404">If the workout template is not found</response>
    [HttpGet]
    [ProducesResponseType(typeof(WorkoutTemplateExercisesResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> GetTemplateExercises([FromRoute] string templateId)
    {
        var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);

        var result = await _service.GetTemplateExercisesAsync(templateIdParsed);

        return result switch
        {
            { IsSuccess: true } => Ok(WorkoutTemplateExercisesResponseDto.SuccessResponse(result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(ErrorResponseDto.MultipleErrors(errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(ErrorResponseDto.MultipleErrors(errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Update exercise metadata
    /// </summary>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="request">The metadata update request</param>
    /// <returns>Result containing updated exercise information</returns>
    /// <response code="200">Returns the updated exercise metadata result</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpPut("{exerciseId}/metadata")]
    [ProducesResponseType(typeof(UpdateMetadataResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> UpdateExerciseMetadata(
        [FromRoute] string templateId,
        [FromRoute] string exerciseId,
        [FromBody] UpdateExerciseMetadataRequest request)
    {
        var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);
        var exerciseIdParsed = WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId);

        var result = await _service.UpdateExerciseMetadataAsync(
            templateIdParsed, 
            exerciseIdParsed, 
            request.Metadata.RootElement.GetRawText());

        return result switch
        {
            { IsSuccess: true } => Ok(UpdateMetadataResponseDto.SuccessResponse(result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(ErrorResponseDto.MultipleErrors(errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(ErrorResponseDto.MultipleErrors(errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Reorder exercise within its round
    /// </summary>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="request">The reorder request</param>
    /// <returns>Result containing reorder information</returns>
    /// <response code="200">Returns the reorder result</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpPut("{exerciseId}/order")]
    [ProducesResponseType(typeof(ReorderResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> ReorderExercise(
        [FromRoute] string templateId,
        [FromRoute] string exerciseId,
        [FromBody] ReorderExerciseRequest request)
    {
        var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);
        var exerciseIdParsed = WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId);

        var result = await _service.ReorderExerciseAsync(templateIdParsed, exerciseIdParsed, request.NewOrderInRound);

        return result switch
        {
            { IsSuccess: true } => Ok(ReorderResponseDto.SuccessResponse(result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(ErrorResponseDto.MultipleErrors(errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(ErrorResponseDto.MultipleErrors(errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Copy round with all exercises and generate new GUIDs
    /// </summary>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="request">The round copy request</param>
    /// <returns>Result containing copied round information</returns>
    /// <response code="201">Returns the copy round result</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the source round is not found</response>
    [HttpPost("rounds/copy")]
    [ProducesResponseType(typeof(CopyRoundResponseDto), 201)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> CopyRound(
        [FromRoute] string templateId,
        [FromBody] CopyRoundRequest request)
    {
        var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);

        var dto = new CopyRoundDto(
            request.SourcePhase,
            request.SourceRoundNumber,
            request.TargetPhase,
            request.TargetRoundNumber);

        var result = await _service.CopyRoundAsync(templateIdParsed, dto);

        return result switch
        {
            { IsSuccess: true } => CreatedAtAction(
                nameof(GetTemplateExercises),
                new { templateId },
                CopyRoundResponseDto.SuccessResponse(result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(ErrorResponseDto.MultipleErrors(errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(ErrorResponseDto.MultipleErrors(errors.Select(e => e.Message).ToList()))
        };
    }
}