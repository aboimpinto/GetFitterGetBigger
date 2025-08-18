using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.SetConfigurations;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.SetConfiguration;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing set configurations within workout template exercises
/// </summary>
[ApiController]
[Route("api/workout-templates/{templateId}/exercises/{exerciseId}/sets")]
[Produces("application/json")]
[Tags("Set Configurations")]
public class SetConfigurationsController(
    ISetConfigurationService setConfigurationService,
    ILogger<SetConfigurationsController> logger) : ControllerBase
{
    private readonly ISetConfigurationService _setConfigurationService = setConfigurationService;
    private readonly ILogger<SetConfigurationsController> _logger = logger;

    /// <summary>
    /// Gets all set configurations for a workout template exercise
    /// </summary>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <returns>List of set configurations ordered by set number</returns>
    /// <response code="200">Returns the set configurations for the exercise</response>
    /// <response code="404">If the exercise is not found</response>
    /// <response code="403">If not authorized to view the configurations</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SetConfigurationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSetConfigurations(string templateId, string exerciseId)
    {
        _logger.LogInformation("Getting set configurations for exercise {ExerciseId} in template {TemplateId}", 
            exerciseId, templateId);

        var result = await _setConfigurationService.GetByWorkoutTemplateExerciseAsync(
            WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId));

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.Unauthorized } => Forbid(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Gets a specific set configuration by ID
    /// </summary>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="setId">The ID of the set configuration</param>
    /// <returns>The set configuration</returns>
    /// <response code="200">Returns the set configuration</response>
    /// <response code="404">If the set configuration is not found</response>
    /// <response code="403">If not authorized to view the configuration</response>
    [HttpGet("{setId}")]
    [ProducesResponseType(typeof(SetConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSetConfiguration(string templateId, string exerciseId, string setId)
    {
        _logger.LogInformation("Getting set configuration {SetId} for exercise {ExerciseId} in template {TemplateId}", 
            setId, exerciseId, templateId);

        var result = await _setConfigurationService.GetByIdAsync(SetConfigurationId.ParseOrEmpty(setId));

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.Unauthorized } => Forbid(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Creates a new set configuration for an exercise
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/workout-templates/workouttemplate-550e8400-e29b-41d4-a716-446655440000/exercises/workouttemplateexercise-550e8400-e29b-41d4-a716-446655440000/sets
    ///     {
    ///         "setNumber": 1,
    ///         "targetReps": "8-12",
    ///         "targetWeight": 80.5,
    ///         "restSeconds": 90
    ///     }
    /// </remarks>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="request">The set configuration creation request</param>
    /// <returns>The created set configuration</returns>
    /// <response code="201">Returns the newly created set configuration</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    /// <response code="409">If a set with the same number already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(SetConfigurationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateSetConfiguration(string templateId, string exerciseId, [FromBody] CreateSetConfigurationDto request)
    {
        _logger.LogInformation("Creating set configuration for exercise {ExerciseId} in template {TemplateId}", 
            exerciseId, templateId);

        var command = new CreateSetConfigurationCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId),
            SetNumber = request.SetNumber,
            TargetReps = request.TargetReps,
            TargetWeight = request.TargetWeight,
            TargetTimeSeconds = request.TargetTimeSeconds,
            RestSeconds = request.RestSeconds
        };

        var result = await _setConfigurationService.CreateAsync(command);

        return result switch
        {
            { IsSuccess: true } => CreatedAtAction(nameof(GetSetConfiguration), 
                new { templateId, exerciseId, setId = result.Data.Id }, result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.AlreadyExists, StructuredErrors: var errors } => Conflict(new { errors }),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Creates multiple set configurations in bulk
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/workout-templates/workouttemplate-550e8400-e29b-41d4-a716-446655440000/exercises/workouttemplateexercise-550e8400-e29b-41d4-a716-446655440000/sets/bulk
    ///     {
    ///         "sets": [
    ///             {
    ///                 "setNumber": 1,
    ///                 "targetReps": "8-12",
    ///                 "targetWeight": 80.0,
    ///                 "restSeconds": 90
    ///             },
    ///             {
    ///                 "setNumber": 2,
    ///                 "targetReps": "6-10",
    ///                 "targetWeight": 85.0,
    ///                 "restSeconds": 120
    ///             }
    ///         ]
    ///     }
    /// </remarks>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="request">The bulk set configuration creation request</param>
    /// <returns>The created set configurations</returns>
    /// <response code="201">Returns the newly created set configurations</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(IEnumerable<SetConfigurationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBulkSetConfigurations(string templateId, string exerciseId, [FromBody] CreateBulkSetConfigurationsDto request)
    {
        _logger.LogInformation("Creating {Count} set configurations for exercise {ExerciseId} in template {TemplateId}", 
            request.Sets.Count, exerciseId, templateId);

        var command = new CreateBulkSetConfigurationsCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId),
            SetConfigurations = request.Sets.Select(s => new SetConfigurationData
            {
                SetNumber = s.SetNumber ?? 1,
                TargetReps = s.TargetReps,
                TargetWeight = s.TargetWeight,
                TargetTimeSeconds = s.TargetTimeSeconds,
                RestSeconds = s.RestSeconds
            }).ToList()
        };

        var result = await _setConfigurationService.CreateBulkAsync(command);

        return result switch
        {
            { IsSuccess: true } => CreatedAtAction(nameof(GetSetConfigurations), 
                new { templateId, exerciseId }, result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { StructuredErrors: var errors } => Conflict(new { errors })
        };
    }

    /// <summary>
    /// Updates a set configuration
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/workout-templates/workouttemplate-550e8400-e29b-41d4-a716-446655440000/exercises/workouttemplateexercise-550e8400-e29b-41d4-a716-446655440000/sets/setconfiguration-550e8400-e29b-41d4-a716-446655440000
    ///     {
    ///         "targetReps": "10-15",
    ///         "targetWeight": 85.0,
    ///         "restSeconds": 120
    ///     }
    /// </remarks>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="setId">The ID of the set configuration</param>
    /// <param name="request">The set configuration update request</param>
    /// <returns>The updated set configuration</returns>
    /// <response code="200">Returns the updated set configuration</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the set configuration is not found</response>
    [HttpPut("{setId}")]
    [ProducesResponseType(typeof(SetConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSetConfiguration(string templateId, string exerciseId, string setId, [FromBody] UpdateSetConfigurationDto request)
    {
        _logger.LogInformation("Updating set configuration {SetId} for exercise {ExerciseId} in template {TemplateId}", 
            setId, exerciseId, templateId);

        var command = new UpdateSetConfigurationCommand
        {
            SetConfigurationId = SetConfigurationId.ParseOrEmpty(setId),
            TargetReps = request.TargetReps,
            TargetWeight = request.TargetWeight,
            TargetTimeSeconds = request.TargetTimeSeconds,
            RestSeconds = request.RestSeconds ?? 90
        };

        var result = await _setConfigurationService.UpdateAsync(command);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Deletes a set configuration
    /// </summary>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="setId">The ID of the set configuration</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the set configuration was successfully deleted</response>
    /// <response code="404">If the set configuration is not found</response>
    /// <response code="403">If not authorized to delete the configuration</response>
    [HttpDelete("{setId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteSetConfiguration(string templateId, string exerciseId, string setId)
    {
        _logger.LogInformation("Deleting set configuration {SetId} for exercise {ExerciseId} in template {TemplateId}", 
            setId, exerciseId, templateId);

        var result = await _setConfigurationService.DeleteAsync(
            SetConfigurationId.ParseOrEmpty(setId));

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.Unauthorized } => Forbid(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Reorders set configurations within an exercise
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/workout-templates/workouttemplate-550e8400-e29b-41d4-a716-446655440000/exercises/workouttemplateexercise-550e8400-e29b-41d4-a716-446655440000/sets/reorder
    ///     {
    ///         "setOrders": [
    ///             {
    ///                 "setId": "setconfiguration-550e8400-e29b-41d4-a716-446655440001",
    ///                 "setNumber": 1
    ///             },
    ///             {
    ///                 "setId": "setconfiguration-550e8400-e29b-41d4-a716-446655440002",
    ///                 "setNumber": 2
    ///             }
    ///         ]
    ///     }
    /// </remarks>
    /// <param name="templateId">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="request">The reorder request</param>
    /// <returns>Success result</returns>
    /// <response code="200">If the sets were successfully reordered</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpPut("reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderSetConfigurations(string templateId, string exerciseId, [FromBody] ReorderSetConfigurationsDto request)
    {
        _logger.LogInformation("Reordering set configurations for exercise {ExerciseId} in template {TemplateId}", 
            exerciseId, templateId);

        var command = new ReorderSetConfigurationsCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId),
            SetReorders = request.SetOrders.ToDictionary(
                o => SetConfigurationId.ParseOrEmpty(o.SetId),
                o => o.SetNumber)
        };

        var result = await _setConfigurationService.ReorderSetsAsync(command);

        return result switch
        {
            { IsSuccess: true } => Ok(new { message = "Set configurations reordered successfully" }),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { StructuredErrors: var errors } => BadRequest(new { errors })
        };
    }
}