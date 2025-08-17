using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing workout templates
/// </summary>
[ApiController]
[Route("api/workout-templates")]
[Produces("application/json")]
[Tags("Workout Templates")]
public class WorkoutTemplatesController(
    IWorkoutTemplateService workoutTemplateService,
    ILogger<WorkoutTemplatesController> logger) : ControllerBase
{
    private readonly IWorkoutTemplateService _workoutTemplateService = workoutTemplateService;
    private readonly ILogger<WorkoutTemplatesController> _logger = logger;


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
        // Parse specialized IDs - ParseOrEmpty handles null/invalid values
        var parsedCategoryId = WorkoutCategoryId.ParseOrEmpty(categoryId);
        var parsedObjectiveId = WorkoutObjectiveId.ParseOrEmpty(objectiveId);
        var parsedDifficultyId = DifficultyLevelId.ParseOrEmpty(difficultyId);
        var parsedStateId = WorkoutStateId.ParseOrEmpty(stateId);

        // Call the unified search method in the service
        var result = await _workoutTemplateService.SearchAsync(
            page,
            pageSize,
            namePattern ?? string.Empty,
            parsedCategoryId,
            parsedObjectiveId,
            parsedDifficultyId,
            parsedStateId,
            sortBy,
            sortOrder);

        // For search operations, always return the data (empty list on any failure)
        // This prevents information leakage about system state
        return Ok(result.Data);
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
            _ => NotFound()
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
            { StructuredErrors: var errors } => Conflict(new { errors })
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
        _logger.LogInformation("Updating workout template with ID: {Id}", id);

        var templateId = WorkoutTemplateId.ParseOrEmpty(id);
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = request.Name,
            Description = request.Description,
            CategoryId = WorkoutCategoryId.ParseOrEmpty(request.CategoryId),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(request.DifficultyId),
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            Tags = request.Tags,
            IsPublic = request.IsPublic,
            ObjectiveIds = request.ObjectiveIds?.Select(WorkoutObjectiveId.ParseOrEmpty).ToList()
        };

        var result = await _workoutTemplateService.UpdateAsync(templateId, command);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { StructuredErrors: var errors } => Conflict(new { errors })
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
    /// <response code="404">If the workout template is not found or cannot be deleted</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkoutTemplate(string id)
    {
        _logger.LogInformation("Deleting workout template with ID: {Id}", id);

        var result = await _workoutTemplateService.DeleteAsync(WorkoutTemplateId.ParseOrEmpty(id));

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            _ => NotFound()
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
        var parsedTemplateId = WorkoutTemplateId.ParseOrEmpty(id);
        var parsedStateId = WorkoutStateId.ParseOrEmpty(request.WorkoutStateId);

        var result = await _workoutTemplateService.ChangeStateAsync(
            parsedTemplateId,
            parsedStateId);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { PrimaryErrorCode: ServiceErrorCode.DependencyExists, StructuredErrors: var errors } => Conflict(new { errors }),
            { StructuredErrors: var errors } => BadRequest(new { errors })  // Handles ALL 400 errors including ValidationFailed
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
        _logger.LogInformation("Duplicating workout template {Id} with new name: {NewName}", id, request.NewName);

        var result = await _workoutTemplateService.DuplicateAsync(
            WorkoutTemplateId.ParseOrEmpty(id),
            request.NewName);

        return result switch
        {
            { IsSuccess: true } => CreatedAtAction(nameof(GetWorkoutTemplate), new { id = result.Data.Id }, result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(),
            { StructuredErrors: var errors } => Conflict(new { errors })
        };
    }

    // TODO: Implement GetExerciseSuggestions endpoint when the service method is available
    // This endpoint is planned but the service implementation is not yet complete
}
