using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing exercises within workout templates
/// </summary>
[ApiController]
[Route("api/workout-templates/{templateId}/exercises")]
[Produces("application/json")]
[Tags("Workout Template Exercises")]
public class WorkoutTemplateExercisesController(
    IWorkoutTemplateExerciseService workoutTemplateExerciseService,
    ILogger<WorkoutTemplateExercisesController> logger) : ControllerBase
{
    private readonly IWorkoutTemplateExerciseService _workoutTemplateExerciseService = workoutTemplateExerciseService;
    private readonly ILogger<WorkoutTemplateExercisesController> _logger = logger;

    /// <summary>
    /// Gets all exercises for a workout template
    /// </summary>
    /// <param name="templateId">The ID of the workout template</param>
    /// <returns>List of exercises grouped by zone</returns>
    /// <response code="200">Returns the exercises for the workout template</response>
    /// <response code="404">If the workout template is not found</response>
    /// <response code="403">If not authorized to view the template</response>
    [HttpGet]
    [ProducesResponseType(typeof(WorkoutTemplateExerciseListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkoutTemplateExercises(string templateId)
    {
        _logger.LogInformation("Getting exercises for workout template: {Id}", templateId);

        var result = await _workoutTemplateExerciseService.GetByWorkoutTemplateAsync(WorkoutTemplateId.ParseOrEmpty(templateId));

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.Unauthorized } => Forbid(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Gets a specific exercise configuration by ID
    /// </summary>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <returns>The exercise configuration with set configurations</returns>
    /// <response code="200">Returns the exercise configuration</response>
    /// <response code="404">If the exercise is not found</response>
    /// <response code="403">If not authorized to view the exercise</response>
    [HttpGet("{exerciseId}")]
    [ProducesResponseType(typeof(WorkoutTemplateExerciseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkoutTemplateExercise(string templateId, string exerciseId)
    {
        _logger.LogInformation("Getting exercise {ExerciseId} for workout template: {Id}", exerciseId, templateId);

        var result = await _workoutTemplateExerciseService.GetByIdAsync(WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId));

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.Unauthorized } => Forbid(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Adds an exercise to a workout template
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/workout-templates/workouttemplate-550e8400-e29b-41d4-a716-446655440000/exercises
    ///     {
    ///         "exerciseId": "exercise-550e8400-e29b-41d4-a716-446655440000",
    ///         "zone": "Main",
    ///         "notes": "Focus on form and control",
    ///         "sequenceOrder": 1
    ///     }
    /// </remarks>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="request">The exercise addition request</param>
    /// <returns>The created exercise configuration</returns>
    /// <response code="201">Returns the newly added exercise configuration</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the workout template is not found</response>
    /// <response code="409">If the exercise already exists in the template</response>
    [HttpPost]
    [ProducesResponseType(typeof(WorkoutTemplateExerciseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddExerciseToTemplate(string templateId, [FromBody] AddExerciseToTemplateDto request)
    {
        _logger.LogInformation("Adding exercise {ExerciseId} to workout template {TemplateId} in zone {Zone}", 
            request.ExerciseId, templateId, request.Zone);

        var command = request.ToCommand(WorkoutTemplateId.ParseOrEmpty(templateId));
        var result = await _workoutTemplateExerciseService.AddExerciseAsync(command);

        return result switch
        {
            { IsSuccess: true } => CreatedAtAction(nameof(GetWorkoutTemplateExercise), 
                new { templateId, exerciseId = result.Data.Id }, result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.AlreadyExists, StructuredErrors: var errors } => Conflict(new { errors }),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Updates an exercise in a workout template
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/workout-templates/workouttemplate-550e8400-e29b-41d4-a716-446655440000/exercises/workouttemplateexercise-550e8400-e29b-41d4-a716-446655440000
    ///     {
    ///         "notes": "Updated: Focus on proper breathing technique"
    ///     }
    /// </remarks>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="request">The exercise update request</param>
    /// <returns>The updated exercise configuration</returns>
    /// <response code="200">Returns the updated exercise configuration</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpPut("{exerciseId}")]
    [ProducesResponseType(typeof(WorkoutTemplateExerciseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTemplateExercise(string templateId, string exerciseId, [FromBody] UpdateTemplateExerciseDto request)
    {
        _logger.LogInformation("Updating exercise {ExerciseId} in workout template {TemplateId}", exerciseId, templateId);

        var command = new UpdateTemplateExerciseCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId),
            Notes = request.Notes
        };

        var result = await _workoutTemplateExerciseService.UpdateExerciseAsync(command);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Removes an exercise from a workout template
    /// </summary>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the exercise was successfully removed</response>
    /// <response code="404">If the exercise is not found</response>
    /// <response code="403">If not authorized to remove the exercise</response>
    [HttpDelete("{exerciseId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveExerciseFromTemplate(string templateId, string exerciseId)
    {
        _logger.LogInformation("Removing exercise {ExerciseId} from workout template {TemplateId}", exerciseId, templateId);

        var result = await _workoutTemplateExerciseService.RemoveExerciseAsync(
            WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId));

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.Unauthorized } => Forbid(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Changes the zone of an exercise within a workout template
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/workout-templates/workouttemplate-550e8400-e29b-41d4-a716-446655440000/exercises/workouttemplateexercise-550e8400-e29b-41d4-a716-446655440000/zone
    ///     {
    ///         "zone": "Cooldown",
    ///         "sequenceOrder": 1
    ///     }
    /// </remarks>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="request">The zone change request</param>
    /// <returns>The updated exercise configuration</returns>
    /// <response code="200">Returns the exercise with updated zone</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpPut("{exerciseId}/zone")]
    [ProducesResponseType(typeof(WorkoutTemplateExerciseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeExerciseZone(string templateId, string exerciseId, [FromBody] ChangeExerciseZoneDto request)
    {
        _logger.LogInformation("Changing zone of exercise {ExerciseId} in template {TemplateId} to {Zone}", 
            exerciseId, templateId, request.Zone);

        var command = new ChangeExerciseZoneCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId),
            NewZone = request.Zone,
            NewSequenceOrder = request.SequenceOrder
        };

        var result = await _workoutTemplateExerciseService.ChangeExerciseZoneAsync(command);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Reorders exercises within a zone
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/workout-templates/workouttemplate-550e8400-e29b-41d4-a716-446655440000/exercises/reorder
    ///     {
    ///         "zone": "Main",
    ///         "exerciseOrders": [
    ///             {
    ///                 "exerciseId": "workouttemplateexercise-550e8400-e29b-41d4-a716-446655440001",
    ///                 "sequenceOrder": 1
    ///             },
    ///             {
    ///                 "exerciseId": "workouttemplateexercise-550e8400-e29b-41d4-a716-446655440002",
    ///                 "sequenceOrder": 2
    ///             }
    ///         ]
    ///     }
    /// </remarks>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="request">The reorder request</param>
    /// <returns>Success result</returns>
    /// <response code="200">If the exercises were successfully reordered</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the workout template is not found</response>
    [HttpPut("reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderExercises(string templateId, [FromBody] ReorderTemplateExercisesDto request)
    {
        _logger.LogInformation("Reordering exercises in zone {Zone} for workout template {TemplateId}", 
            request.Zone, templateId);

        var command = new ReorderTemplateExercisesCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.ParseOrEmpty(templateId),
            Zone = request.Zone,
            ExerciseIds = request.ExerciseOrders
                .OrderBy(o => o.SequenceOrder)
                .Select(o => WorkoutTemplateExerciseId.ParseOrEmpty(o.ExerciseId))
                .ToList()
        };

        var result = await _workoutTemplateExerciseService.ReorderExercisesAsync(command);

        return result switch
        {
            { IsSuccess: true } => Ok(new { message = "Exercises reordered successfully" }),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }
}