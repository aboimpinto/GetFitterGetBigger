using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplateExercises;
using GetFitterGetBigger.API.Services.Commands.SetConfigurations;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing workout templates
/// </summary>
[ApiController]
[Route("api/workout-templates")]
[Produces("application/json")]
[Tags("Workout Templates")]
public class WorkoutTemplatesController : ControllerBase
{
    private readonly IWorkoutTemplateService _workoutTemplateService;
    private readonly IWorkoutTemplateExerciseService _workoutTemplateExerciseService;
    private readonly ISetConfigurationService _setConfigurationService;
    private readonly ILogger<WorkoutTemplatesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkoutTemplatesController"/> class
    /// </summary>
    /// <param name="workoutTemplateService">The workout template service</param>
    /// <param name="workoutTemplateExerciseService">The workout template exercise service</param>
    /// <param name="setConfigurationService">The set configuration service</param>
    /// <param name="logger">The logger</param>
    public WorkoutTemplatesController(
        IWorkoutTemplateService workoutTemplateService,
        IWorkoutTemplateExerciseService workoutTemplateExerciseService,
        ISetConfigurationService setConfigurationService,
        ILogger<WorkoutTemplatesController> logger)
    {
        _workoutTemplateService = workoutTemplateService;
        _workoutTemplateExerciseService = workoutTemplateExerciseService;
        _setConfigurationService = setConfigurationService;
        _logger = logger;
    }


    /// <summary>
    /// Gets a paginated list of workout templates with optional filtering
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="namePattern">Search pattern for template names</param>
    /// <param name="categoryId">Filter by workout category</param>
    /// <param name="objectiveId">Filter by workout objective</param>
    /// <param name="difficultyId">Filter by difficulty</param>
    /// <param name="stateId">Filter by workout state</param>
    /// <param name="sortBy">Sort field (name|createdAt|lastModified|duration)</param>
    /// <param name="sortOrder">Sort order (asc|desc)</param>
    /// <returns>A paginated list of workout templates</returns>
    /// <response code="200">Returns the paginated list of workout templates</response>
    /// <response code="400">If the filter parameters are invalid</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<WorkoutTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetWorkoutTemplates(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? namePattern = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] string? objectiveId = null,
        [FromQuery] string? difficultyId = null,
        [FromQuery] string? stateId = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortOrder = "asc")
    {
        // Always log all parameters
        _logger.LogInformation(
            "GetWorkoutTemplates called - Page: {Page}, PageSize: {PageSize}, NamePattern: {NamePattern}, " +
            "CategoryId: {CategoryId}, ObjectiveId: {ObjectiveId}, DifficultyId: {DifficultyId}, " +
            "StateId: {StateId}, SortBy: {SortBy}, SortOrder: {SortOrder}",
            page, pageSize, namePattern, categoryId, objectiveId, difficultyId, 
            stateId, sortBy, sortOrder);

        // Parse specialized IDs - ParseOrEmpty handles null/invalid values
        var parsedCategoryId = WorkoutCategoryId.ParseOrEmpty(categoryId);
        var parsedObjectiveId = WorkoutObjectiveId.ParseOrEmpty(objectiveId);
        var parsedDifficultyId = DifficultyLevelId.ParseOrEmpty(difficultyId);
        var parsedStateId = WorkoutStateId.ParseOrEmpty(stateId);

        // Transform nullable parameters to meaningful values
        var searchNamePattern = namePattern ?? string.Empty;
        var searchSortBy = sortBy ?? "name";
        var searchSortOrder = sortOrder ?? "asc";

        // Call the unified search method in the service
        var result = await _workoutTemplateService.SearchAsync(
            page,
            pageSize,
            searchNamePattern,
            parsedCategoryId,
            parsedObjectiveId,
            parsedDifficultyId,
            parsedStateId,
            searchSortBy,
            searchSortOrder);

        // Single exit point - no business logic, just pass through the result
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }


    /// <summary>
    /// Gets a workout template by ID with full details
    /// </summary>
    /// <param name="id">The ID of the workout template in the format "workouttemplate-{guid}"</param>
    /// <returns>The workout template if found</returns>
    /// <response code="200">Returns the workout template with full details</response>
    /// <response code="404">If the workout template is not found</response>
    /// <response code="403">If accessing a private template not owned by user</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WorkoutTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkoutTemplate(string id)
    {
        _logger.LogInformation("Getting workout template with ID: {Id}", id);

        var result = await _workoutTemplateService.GetByIdAsync(WorkoutTemplateId.ParseOrEmpty(id));

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("access denied")) => Forbid(),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Creates a new workout template
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/workout-templates
    ///     {
    ///         "name": "Upper Body Strength",
    ///         "description": "A comprehensive upper body workout",
    ///         "categoryId": "workoutcategory-550e8400-e29b-41d4-a716-446655440000",
    ///         "difficultyId": "difficultylevel-550e8400-e29b-41d4-a716-446655440000",
    ///         "estimatedDurationMinutes": 60,
    ///         "tags": ["strength", "upper-body"],
    ///         "isPublic": false,
    ///         "objectiveIds": ["workoutobjective-550e8400-e29b-41d4-a716-446655440000"]
    ///     }
    /// </remarks>
    /// <param name="request">The workout template creation request</param>
    /// <returns>The created workout template with its generated ID</returns>
    /// <response code="201">Returns the newly created workout template</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="409">If a template with the same name already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(WorkoutTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateWorkoutTemplate([FromBody] CreateWorkoutTemplateDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating new workout template: {Name}", request.Name);

        var command = new CreateWorkoutTemplateCommand
        {
            Name = request.Name,
            Description = request.Description,
            CategoryId = WorkoutCategoryId.ParseOrEmpty(request.CategoryId),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(request.DifficultyId),
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            Tags = request.Tags,
            IsPublic = request.IsPublic,
            ObjectiveIds = request.ObjectiveIds.Select(WorkoutObjectiveId.ParseOrEmpty).ToList()
        };

        var result = await _workoutTemplateService.CreateAsync(command);

        return result switch
        {
            { IsSuccess: true } => CreatedAtAction(nameof(GetWorkoutTemplate), new { id = result.Data.Id }, result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("already exists")) => Conflict(new { errors }),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Updates an existing workout template
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/workout-templates/workouttemplate-550e8400-e29b-41d4-a716-446655440000
    ///     {
    ///         "name": "Updated Upper Body Strength",
    ///         "description": "An updated comprehensive upper body workout",
    ///         "categoryId": "workoutcategory-550e8400-e29b-41d4-a716-446655440000",
    ///         "difficultyId": "difficultylevel-550e8400-e29b-41d4-a716-446655440000",
    ///         "estimatedDurationMinutes": 70,
    ///         "tags": ["strength", "upper-body", "advanced"],
    ///         "isPublic": true,
    ///         "objectiveIds": ["workoutobjective-550e8400-e29b-41d4-a716-446655440000"]
    ///     }
    /// </remarks>
    /// <param name="id">The ID of the workout template to update</param>
    /// <param name="request">The workout template update request</param>
    /// <returns>The updated workout template</returns>
    /// <response code="200">Returns the updated workout template</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the workout template is not found</response>
    /// <response code="409">If a template with the same name already exists</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(WorkoutTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateWorkoutTemplate(string id, [FromBody] UpdateWorkoutTemplateDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Updating workout template with ID: {Id}", id);

        var command = new UpdateWorkoutTemplateCommand
        {
            Id = WorkoutTemplateId.ParseOrEmpty(id),
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId != null ? WorkoutCategoryId.ParseOrEmpty(request.CategoryId) : null,
            DifficultyId = request.DifficultyId != null ? DifficultyLevelId.ParseOrEmpty(request.DifficultyId) : null,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            Tags = request.Tags,
            IsPublic = request.IsPublic,
            ObjectiveIds = request.ObjectiveIds?.Select(WorkoutObjectiveId.ParseOrEmpty).ToList()
        };

        var result = await _workoutTemplateService.UpdateAsync(command.Id, command);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("already exists")) => Conflict(new { errors }),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Deletes a workout template
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     DELETE /api/workout-templates/workouttemplate-550e8400-e29b-41d4-a716-446655440000
    /// 
    /// Note: The template will be soft deleted if it has execution logs, or permanently deleted if not.
    /// </remarks>
    /// <param name="id">The ID of the workout template to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the workout template was successfully deleted</response>
    /// <response code="404">If the workout template is not found</response>
    /// <response code="403">If not authorized to delete the template</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteWorkoutTemplate(string id)
    {
        _logger.LogInformation("Deleting workout template with ID: {Id}", id);

        var result = await _workoutTemplateService.DeleteAsync(WorkoutTemplateId.ParseOrEmpty(id));

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("access denied")) => Forbid(),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Changes the state of a workout template
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/workout-templates/workouttemplate-550e8400-e29b-41d4-a716-446655440000/state
    ///     {
    ///         "workoutStateId": "workoutstate-02000001-0000-0000-0000-000000000002"
    ///     }
    /// 
    /// Valid state transitions:
    /// - DRAFT → PRODUCTION
    /// - PRODUCTION → ARCHIVED
    /// - ARCHIVED → PRODUCTION (if no execution logs)
    /// </remarks>
    /// <param name="id">The ID of the workout template</param>
    /// <param name="request">The state change request</param>
    /// <returns>The updated workout template</returns>
    /// <response code="200">Returns the workout template with updated state</response>
    /// <response code="400">If the state transition is invalid</response>
    /// <response code="404">If the workout template is not found</response>
    /// <response code="409">If the state transition is blocked (e.g., execution logs exist)</response>
    [HttpPut("{id}/state")]
    [ProducesResponseType(typeof(WorkoutTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeWorkoutTemplateState(string id, [FromBody] ChangeWorkoutStateDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Changing state of workout template {Id} to {StateId}", id, request.WorkoutStateId);

        var result = await _workoutTemplateService.ChangeStateAsync(
            WorkoutTemplateId.ParseOrEmpty(id),
            WorkoutStateId.ParseOrEmpty(request.WorkoutStateId));

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("invalid transition")) => BadRequest(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("blocked")) => Conflict(new { errors }),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Duplicates a workout template
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/workout-templates/workouttemplate-550e8400-e29b-41d4-a716-446655440000/duplicate
    ///     {
    ///         "newName": "Copy of Upper Body Strength"
    ///     }
    /// 
    /// Creates a complete copy of the template including all exercises and set configurations.
    /// The new template will be in DRAFT state.
    /// </remarks>
    /// <param name="id">The ID of the workout template to duplicate</param>
    /// <param name="request">The duplication request with the new name</param>
    /// <returns>The duplicated workout template</returns>
    /// <response code="201">Returns the newly duplicated workout template</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the original workout template is not found</response>
    /// <response code="409">If a template with the new name already exists</response>
    [HttpPost("{id}/duplicate")]
    [ProducesResponseType(typeof(WorkoutTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DuplicateWorkoutTemplate(string id, [FromBody] DuplicateWorkoutTemplateDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Duplicating workout template {Id} with new name: {NewName}", id, request.NewName);

        var result = await _workoutTemplateService.DuplicateAsync(
            WorkoutTemplateId.ParseOrEmpty(id),
            request.NewName);

        return result switch
        {
            { IsSuccess: true } => CreatedAtAction(nameof(GetWorkoutTemplate), new { id = result.Data.Id }, result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("already exists")) => Conflict(new { errors }),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }

    // TODO: Implement GetExerciseSuggestions endpoint when the service method is available
    // This endpoint is planned but the service implementation is not yet complete

    #region WorkoutTemplateExercise Management

    /// <summary>
    /// Gets all exercises for a workout template
    /// </summary>
    /// <param name="id">The ID of the workout template</param>
    /// <returns>List of exercises grouped by zone</returns>
    /// <response code="200">Returns the exercises for the workout template</response>
    /// <response code="404">If the workout template is not found</response>
    /// <response code="403">If not authorized to view the template</response>
    [HttpGet("{id}/exercises")]
    [ProducesResponseType(typeof(WorkoutTemplateExerciseListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkoutTemplateExercises(string id)
    {
        _logger.LogInformation("Getting exercises for workout template: {Id}", id);

        var result = await _workoutTemplateExerciseService.GetByWorkoutTemplateAsync(WorkoutTemplateId.ParseOrEmpty(id));

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("access denied")) => Forbid(),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Gets a specific exercise configuration by ID
    /// </summary>
    /// <param name="id">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <returns>The exercise configuration with set configurations</returns>
    /// <response code="200">Returns the exercise configuration</response>
    /// <response code="404">If the exercise is not found</response>
    /// <response code="403">If not authorized to view the exercise</response>
    [HttpGet("{id}/exercises/{exerciseId}")]
    [ProducesResponseType(typeof(WorkoutTemplateExerciseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkoutTemplateExercise(string id, string exerciseId)
    {
        _logger.LogInformation("Getting exercise {ExerciseId} for workout template: {Id}", exerciseId, id);

        var result = await _workoutTemplateExerciseService.GetByIdAsync(WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId));

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("access denied")) => Forbid(),
            { Errors: var errors } => BadRequest(new { errors })
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
    /// <param name="id">The ID of the workout template</param>
    /// <param name="request">The exercise addition request</param>
    /// <returns>The created exercise configuration</returns>
    /// <response code="201">Returns the newly added exercise configuration</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the workout template is not found</response>
    /// <response code="409">If the exercise already exists in the template</response>
    [HttpPost("{id}/exercises")]
    [ProducesResponseType(typeof(WorkoutTemplateExerciseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddExerciseToTemplate(string id, [FromBody] AddExerciseToTemplateDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Adding exercise {ExerciseId} to workout template {TemplateId} in zone {Zone}", 
            request.ExerciseId, id, request.Zone);

        var command = new AddExerciseToTemplateCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.ParseOrEmpty(id),
            ExerciseId = ExerciseId.ParseOrEmpty(request.ExerciseId),
            Zone = request.Zone,
            Notes = request.Notes,
            SequenceOrder = request.SequenceOrder
        };

        var result = await _workoutTemplateExerciseService.AddExerciseAsync(command);

        return result switch
        {
            { IsSuccess: true } => CreatedAtAction(nameof(GetWorkoutTemplateExercise), 
                new { id, exerciseId = result.Data.Id }, result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("already exists")) => Conflict(new { errors }),
            { Errors: var errors } => BadRequest(new { errors })
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
    /// <param name="id">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="request">The exercise update request</param>
    /// <returns>The updated exercise configuration</returns>
    /// <response code="200">Returns the updated exercise configuration</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpPut("{id}/exercises/{exerciseId}")]
    [ProducesResponseType(typeof(WorkoutTemplateExerciseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTemplateExercise(string id, string exerciseId, [FromBody] UpdateTemplateExerciseDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Updating exercise {ExerciseId} in workout template {TemplateId}", exerciseId, id);

        var command = new UpdateTemplateExerciseCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId),
            Notes = request.Notes
        };

        var result = await _workoutTemplateExerciseService.UpdateExerciseAsync(command);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Removes an exercise from a workout template
    /// </summary>
    /// <param name="id">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the exercise was successfully removed</response>
    /// <response code="404">If the exercise is not found</response>
    /// <response code="403">If not authorized to remove the exercise</response>
    [HttpDelete("{id}/exercises/{exerciseId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveExerciseFromTemplate(string id, string exerciseId)
    {
        _logger.LogInformation("Removing exercise {ExerciseId} from workout template {TemplateId}", exerciseId, id);

        var result = await _workoutTemplateExerciseService.RemoveExerciseAsync(
            WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId));

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("access denied")) => Forbid(),
            { Errors: var errors } => BadRequest(new { errors })
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
    /// <param name="id">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="request">The zone change request</param>
    /// <returns>The updated exercise configuration</returns>
    /// <response code="200">Returns the exercise with updated zone</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpPut("{id}/exercises/{exerciseId}/zone")]
    [ProducesResponseType(typeof(WorkoutTemplateExerciseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeExerciseZone(string id, string exerciseId, [FromBody] ChangeExerciseZoneDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Changing zone of exercise {ExerciseId} in template {TemplateId} to {Zone}", 
            exerciseId, id, request.Zone);

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
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } => BadRequest(new { errors })
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
    /// <param name="id">The ID of the workout template</param>
    /// <param name="request">The reorder request</param>
    /// <returns>Success result</returns>
    /// <response code="200">If the exercises were successfully reordered</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the workout template is not found</response>
    [HttpPut("{id}/exercises/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderExercises(string id, [FromBody] ReorderTemplateExercisesDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Reordering exercises in zone {Zone} for workout template {TemplateId}", 
            request.Zone, id);

        var command = new ReorderTemplateExercisesCommand
        {
            WorkoutTemplateId = WorkoutTemplateId.ParseOrEmpty(id),
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
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }

    #endregion

    #region SetConfiguration Management

    /// <summary>
    /// Gets all set configurations for a workout template exercise
    /// </summary>
    /// <param name="id">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <returns>List of set configurations ordered by set number</returns>
    /// <response code="200">Returns the set configurations for the exercise</response>
    /// <response code="404">If the exercise is not found</response>
    /// <response code="403">If not authorized to view the configurations</response>
    [HttpGet("{id}/exercises/{exerciseId}/sets")]
    [ProducesResponseType(typeof(IEnumerable<SetConfigurationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSetConfigurations(string id, string exerciseId)
    {
        _logger.LogInformation("Getting set configurations for exercise {ExerciseId} in template {TemplateId}", 
            exerciseId, id);

        var result = await _setConfigurationService.GetByWorkoutTemplateExerciseAsync(
            WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId));

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("access denied")) => Forbid(),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Gets a specific set configuration by ID
    /// </summary>
    /// <param name="id">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="setId">The ID of the set configuration</param>
    /// <returns>The set configuration</returns>
    /// <response code="200">Returns the set configuration</response>
    /// <response code="404">If the set configuration is not found</response>
    /// <response code="403">If not authorized to view the configuration</response>
    [HttpGet("{id}/exercises/{exerciseId}/sets/{setId}")]
    [ProducesResponseType(typeof(SetConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSetConfiguration(string id, string exerciseId, string setId)
    {
        _logger.LogInformation("Getting set configuration {SetId} for exercise {ExerciseId} in template {TemplateId}", 
            setId, exerciseId, id);

        var result = await _setConfigurationService.GetByIdAsync(SetConfigurationId.ParseOrEmpty(setId));

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("access denied")) => Forbid(),
            { Errors: var errors } => BadRequest(new { errors })
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
    /// <param name="id">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="request">The set configuration creation request</param>
    /// <returns>The created set configuration</returns>
    /// <response code="201">Returns the newly created set configuration</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    /// <response code="409">If a set with the same number already exists</response>
    [HttpPost("{id}/exercises/{exerciseId}/sets")]
    [ProducesResponseType(typeof(SetConfigurationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateSetConfiguration(string id, string exerciseId, [FromBody] CreateSetConfigurationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating set configuration for exercise {ExerciseId} in template {TemplateId}", 
            exerciseId, id);

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
                new { id, exerciseId, setId = result.Data.Id }, result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("already exists")) => Conflict(new { errors }),
            { Errors: var errors } => BadRequest(new { errors })
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
    /// <param name="id">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="request">The bulk set configuration creation request</param>
    /// <returns>The created set configurations</returns>
    /// <response code="201">Returns the newly created set configurations</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpPost("{id}/exercises/{exerciseId}/sets/bulk")]
    [ProducesResponseType(typeof(IEnumerable<SetConfigurationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBulkSetConfigurations(string id, string exerciseId, [FromBody] CreateBulkSetConfigurationsDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating {Count} set configurations for exercise {ExerciseId} in template {TemplateId}", 
            request.Sets.Count, exerciseId, id);

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
                new { id, exerciseId }, result.Data),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } => BadRequest(new { errors })
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
    /// <param name="id">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="setId">The ID of the set configuration</param>
    /// <param name="request">The set configuration update request</param>
    /// <returns>The updated set configuration</returns>
    /// <response code="200">Returns the updated set configuration</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the set configuration is not found</response>
    [HttpPut("{id}/exercises/{exerciseId}/sets/{setId}")]
    [ProducesResponseType(typeof(SetConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSetConfiguration(string id, string exerciseId, string setId, [FromBody] UpdateSetConfigurationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Updating set configuration {SetId} for exercise {ExerciseId} in template {TemplateId}", 
            setId, exerciseId, id);

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
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }

    /// <summary>
    /// Deletes a set configuration
    /// </summary>
    /// <param name="id">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="setId">The ID of the set configuration</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the set configuration was successfully deleted</response>
    /// <response code="404">If the set configuration is not found</response>
    /// <response code="403">If not authorized to delete the configuration</response>
    [HttpDelete("{id}/exercises/{exerciseId}/sets/{setId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteSetConfiguration(string id, string exerciseId, string setId)
    {
        _logger.LogInformation("Deleting set configuration {SetId} for exercise {ExerciseId} in template {TemplateId}", 
            setId, exerciseId, id);

        var result = await _setConfigurationService.DeleteAsync(
            SetConfigurationId.ParseOrEmpty(setId));

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } when errors.Any(e => e.Contains("access denied")) => Forbid(),
            { Errors: var errors } => BadRequest(new { errors })
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
    /// <param name="id">The ID of the workout template</param>
    /// <param name="exerciseId">The ID of the workout template exercise</param>
    /// <param name="request">The reorder request</param>
    /// <returns>Success result</returns>
    /// <response code="200">If the sets were successfully reordered</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpPut("{id}/exercises/{exerciseId}/sets/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderSetConfigurations(string id, string exerciseId, [FromBody] ReorderSetConfigurationsDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Reordering set configurations for exercise {ExerciseId} in template {TemplateId}", 
            exerciseId, id);

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
            { Errors: var errors } when errors.Any(e => e.Contains("not found")) => NotFound(new { errors }),
            { Errors: var errors } => BadRequest(new { errors })
        };
    }

    #endregion
}

/// <summary>
/// Data transfer object for changing workout template state
/// </summary>
public class ChangeWorkoutStateDto
{
    /// <summary>
    /// The new workout state ID
    /// <example>workoutstate-02000001-0000-0000-0000-000000000002</example>
    /// </summary>
    public required string WorkoutStateId { get; init; }
}

/// <summary>
/// Data transfer object for duplicating a workout template
/// </summary>
public class DuplicateWorkoutTemplateDto
{
    /// <summary>
    /// The name for the duplicated template
    /// <example>Copy of Upper Body Strength</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 3)]
    public required string NewName { get; init; }
}

/// <summary>
/// Data transfer object for adding an exercise to a workout template
/// </summary>
public class AddExerciseToTemplateDto
{
    /// <summary>
    /// The exercise ID to add
    /// <example>exercise-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string ExerciseId { get; init; }

    /// <summary>
    /// The zone to add the exercise to (Warmup, Main, Cooldown)
    /// <example>Main</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string Zone { get; init; }

    /// <summary>
    /// Optional notes for the exercise
    /// <example>Focus on form and control</example>
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Optional sequence order. If not provided, will be added at the end
    /// <example>1</example>
    /// </summary>
    public int? SequenceOrder { get; init; }
}

/// <summary>
/// Data transfer object for updating an exercise in a workout template
/// </summary>
public class UpdateTemplateExerciseDto
{
    /// <summary>
    /// Updated notes for the exercise
    /// <example>Updated: Focus on proper breathing technique</example>
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Data transfer object for changing an exercise zone
/// </summary>
public class ChangeExerciseZoneDto
{
    /// <summary>
    /// The new zone for the exercise (Warmup, Main, Cooldown)
    /// <example>Cooldown</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string Zone { get; init; }

    /// <summary>
    /// The sequence order within the new zone
    /// <example>1</example>
    /// </summary>
    public int? SequenceOrder { get; init; }
}

/// <summary>
/// Data transfer object for reordering exercises within a zone
/// </summary>
public class ReorderTemplateExercisesDto
{
    /// <summary>
    /// The zone to reorder exercises in
    /// <example>Main</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string Zone { get; init; }

    /// <summary>
    /// List of exercise orders
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required List<ExerciseOrderDto> ExerciseOrders { get; init; }
}

/// <summary>
/// Data transfer object for exercise order information
/// </summary>
public class ExerciseOrderDto
{
    /// <summary>
    /// The workout template exercise ID
    /// <example>workouttemplateexercise-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string ExerciseId { get; init; }

    /// <summary>
    /// The new sequence order
    /// <example>1</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required int SequenceOrder { get; init; }
}

/// <summary>
/// Data transfer object for creating a set configuration
/// </summary>
public class CreateSetConfigurationDto
{
    /// <summary>
    /// The set number (optional - will auto-assign if not provided)
    /// <example>1</example>
    /// </summary>
    public int? SetNumber { get; init; }

    /// <summary>
    /// Target reps (can be a range like "8-12")
    /// <example>8-12</example>
    /// </summary>
    public string? TargetReps { get; init; }

    /// <summary>
    /// Target weight in kilograms
    /// <example>80.5</example>
    /// </summary>
    public decimal? TargetWeight { get; init; }

    /// <summary>
    /// Target time in seconds
    /// <example>30</example>
    /// </summary>
    public int? TargetTimeSeconds { get; init; }

    /// <summary>
    /// Rest time in seconds after this set
    /// <example>90</example>
    /// </summary>
    public int RestSeconds { get; init; } = 90;
}

/// <summary>
/// Data transfer object for creating multiple set configurations in bulk
/// </summary>
public class CreateBulkSetConfigurationsDto
{
    /// <summary>
    /// List of set configurations to create
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required List<CreateSetConfigurationDto> Sets { get; init; }
}

/// <summary>
/// Data transfer object for updating a set configuration
/// </summary>
public class UpdateSetConfigurationDto
{
    /// <summary>
    /// Target reps (can be a range like "8-12")
    /// <example>10-15</example>
    /// </summary>
    public string? TargetReps { get; init; }

    /// <summary>
    /// Target weight in kilograms
    /// <example>85.0</example>
    /// </summary>
    public decimal? TargetWeight { get; init; }

    /// <summary>
    /// Target time in seconds
    /// <example>45</example>
    /// </summary>
    public int? TargetTimeSeconds { get; init; }

    /// <summary>
    /// Rest time in seconds after this set
    /// <example>120</example>
    /// </summary>
    public int? RestSeconds { get; init; }
}

/// <summary>
/// Data transfer object for reordering set configurations
/// </summary>
public class ReorderSetConfigurationsDto
{
    /// <summary>
    /// List of set orders
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required List<SetOrderDto> SetOrders { get; init; }
}

/// <summary>
/// Data transfer object for set order information
/// </summary>
public class SetOrderDto
{
    /// <summary>
    /// The set configuration ID
    /// <example>setconfiguration-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string SetId { get; init; }

    /// <summary>
    /// The new set number
    /// <example>1</example>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required int SetNumber { get; init; }
}

