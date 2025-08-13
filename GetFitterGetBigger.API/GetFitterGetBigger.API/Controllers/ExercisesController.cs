using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Extensions;
using GetFitterGetBigger.API.Mappers;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Exercise;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing exercises
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Exercises")]
public class ExercisesController(
    IExerciseService exerciseService,
    ILogger<ExercisesController> logger) : ControllerBase
{
    private readonly IExerciseService _exerciseService = exerciseService;
    private readonly ILogger<ExercisesController> _logger = logger;

    /// <summary>
    /// Gets a paginated list of exercises with optional filtering
    /// </summary>
    /// <param name="filterParams">The filter and pagination parameters</param>
    /// <returns>A paginated list of exercises</returns>
    /// <response code="200">Returns the paginated list of exercises</response>
    /// <response code="400">If the filter parameters are invalid</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ExerciseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExercises([FromQuery] ExerciseFilterParams filterParams)
    {
        _logger.LogInformation("Getting exercises with filters: {@FilterParams}", filterParams);
        
        var result = await _exerciseService.GetPagedAsync(filterParams.ToCommand());
        
        return result switch
        {
            { IsSuccess: true, Data: var pagedResponse } => Ok(pagedResponse),
            _ => BadRequest(result.GetCombinedErrorMessage())
        };
    }

    /// <summary>
    /// Gets an exercise by ID
    /// </summary>
    /// <param name="id">The ID of the exercise in the format "exercise-{guid}"</param>
    /// <returns>The exercise if found</returns>
    /// <response code="200">Returns the exercise</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExerciseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExercise(string id)
    {
        _logger.LogInformation("Getting exercise with ID: {Id}", id);
        
        var result = await _exerciseService.GetByIdAsync(ExerciseId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true, Data: var exercise } => Ok(exercise),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(result.GetCombinedErrorMessage()),
            { PrimaryErrorCode: ServiceErrorCode.ValidationFailed } => BadRequest(result.GetCombinedErrorMessage()),
            _ => BadRequest(result.GetCombinedErrorMessage())
        };
    }

    /// <summary>
    /// Creates a new exercise
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/exercises
    ///     {
    ///         "name": "Barbell Back Squat",
    ///         "description": "A compound lower body exercise targeting quads and glutes",
    ///         "coachNotes": [
    ///             {
    ///                 "text": "Keep your chest up and core engaged",
    ///                 "order": 1
    ///             },
    ///             {
    ///                 "text": "Drive through the heels, not the toes",
    ///                 "order": 2
    ///             }
    ///         ],
    ///         "exerciseTypeIds": [
    ///             "exercisetype-11223344-5566-7788-99aa-bbccddeeff00",
    ///             "exercisetype-22334455-6677-8899-aabb-ccddeeff0011"
    ///         ],
    ///         "videoUrl": "https://example.com/squat-video.mp4",
    ///         "imageUrl": "https://example.com/squat-image.jpg",
    ///         "isUnilateral": false,
    ///         "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
    ///         "kineticChainId": "kineticchaintype-12345678-9abc-def0-1234-567890abcdef",
    ///         "muscleGroups": [
    ///             {
    ///                 "muscleGroupId": "musclegroup-ccddeeff-0011-2233-4455-667788990011",
    ///                 "muscleRoleId": "musclerole-abcdef12-3456-7890-abcd-ef1234567890"
    ///             }
    ///         ],
    ///         "equipmentIds": ["equipment-33445566-7788-99aa-bbcc-ddeeff001122"],
    ///         "bodyPartIds": ["bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"],
    ///         "movementPatternIds": ["movementpattern-99aabbcc-ddee-ff00-1122-334455667788"],
    ///         "exerciseWeightTypeId": "exerciseweighttype-12345678-9abc-def0-1234-567890abcdef"
    ///     }
    /// 
    /// Notes:
    /// - The exercise ID will be generated automatically and returned in the response
    /// - CoachNotes are ordered instruction items that replace the previous single Instructions field
    /// - ExerciseTypeIds specify the types (Warmup, Workout, Cooldown, Rest)
    /// - If "Rest" type is included, it must be the only type (business rule)
    /// - KineticChainId is required for non-REST exercises and must be null for REST exercises
    /// - ExerciseWeightTypeId is required for non-REST exercises and must be null for REST exercises
    /// </remarks>
    /// <param name="request">The exercise creation request</param>
    /// <returns>The created exercise with its generated ID</returns>
    /// <response code="201">Returns the newly created exercise</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="409">If an exercise with the same name already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(ExerciseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateExercise([FromBody] CreateExerciseRequest request)
    {
        _logger.LogInformation("Creating new exercise: {Name}", request.Name);
        
        var result = await _exerciseService.CreateAsync(request.ToCommand());
        
        return result switch
        {
            { IsSuccess: true } => CreatedAtAction(nameof(GetExercise), new { id = result.Data.Id }, result.Data),
            { PrimaryErrorCode: ServiceErrorCode.AlreadyExists } => Conflict(result.GetCombinedErrorMessage()),
            { PrimaryErrorCode: ServiceErrorCode.ValidationFailed } => BadRequest(result.GetCombinedErrorMessage()),
            _ => BadRequest(result.GetCombinedErrorMessage())
        };
    }

    /// <summary>
    /// Updates an existing exercise
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/exercises/exercise-12345678-1234-1234-1234-123456789012
    ///     {
    ///         "name": "Updated Barbell Back Squat",
    ///         "description": "A compound lower body exercise targeting quads and glutes",
    ///         "coachNotes": [
    ///             {
    ///                 "id": "coachnote-87654321-4321-4321-4321-210987654321",
    ///                 "text": "Keep your chest up and core engaged (updated)",
    ///                 "order": 1
    ///             },
    ///             {
    ///                 "text": "New note: Watch knee tracking",
    ///                 "order": 2
    ///             }
    ///         ],
    ///         "exerciseTypeIds": [
    ///             "exercisetype-11223344-5566-7788-99aa-bbccddeeff00",
    ///             "exercisetype-22334455-6677-8899-aabb-ccddeeff0011"
    ///         ],
    ///         "videoUrl": "https://example.com/squat-video.mp4",
    ///         "imageUrl": "https://example.com/squat-image.jpg",
    ///         "isUnilateral": false,
    ///         "isActive": true,
    ///         "difficultyId": "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
    ///         "kineticChainId": "kineticchaintype-12345678-9abc-def0-1234-567890abcdef",
    ///         "muscleGroups": [
    ///             {
    ///                 "muscleGroupId": "musclegroup-ccddeeff-0011-2233-4455-667788990011",
    ///                 "muscleRoleId": "musclerole-abcdef12-3456-7890-abcd-ef1234567890"
    ///             }
    ///         ],
    ///         "equipmentIds": ["equipment-33445566-7788-99aa-bbcc-ddeeff001122"],
    ///         "bodyPartIds": ["bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"],
    ///         "movementPatternIds": ["movementpattern-99aabbcc-ddee-ff00-1122-334455667788"],
    ///         "exerciseWeightTypeId": "exerciseweighttype-12345678-9abc-def0-1234-567890abcdef"
    ///     }
    /// 
    /// Notes:
    /// - The exercise ID is taken from the URL path, not from the request body
    /// - CoachNotes can include IDs for existing notes (to update) or no ID for new notes
    /// - CoachNotes not included in the request will be deleted
    /// - ExerciseTypeIds completely replace the existing types
    /// - If "Rest" type is included, it must be the only type (business rule)
    /// - KineticChainId is required for non-REST exercises and must be null for REST exercises
    /// - ExerciseWeightTypeId is required for non-REST exercises and must be null for REST exercises
    /// </remarks>
    /// <param name="id">The ID of the exercise to update (from URL path)</param>
    /// <param name="request">The exercise update request containing all fields to update</param>
    /// <returns>The updated exercise</returns>
    /// <response code="200">Returns the updated exercise</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="404">If the exercise is not found</response>
    /// <response code="409">If an exercise with the same name already exists</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ExerciseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateExercise(string id, [FromBody] UpdateExerciseRequest request)
    {
        _logger.LogInformation("Updating exercise with ID: {Id}", id);
        
        var result = await _exerciseService.UpdateAsync(ExerciseId.ParseOrEmpty(id), request.ToCommand());
        
        return result switch
        {
            { IsSuccess: true } => Ok(result.Data),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(result.GetCombinedErrorMessage()),
            { PrimaryErrorCode: ServiceErrorCode.AlreadyExists } => Conflict(result.GetCombinedErrorMessage()),
            { PrimaryErrorCode: ServiceErrorCode.ValidationFailed } => BadRequest(result.GetCombinedErrorMessage()),
            _ => BadRequest(result.GetCombinedErrorMessage())
        };
    }

    /// <summary>
    /// Deletes an exercise
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     DELETE /api/exercises/exercise-12345678-1234-1234-1234-123456789012
    /// 
    /// Note: The exercise ID is taken from the URL path. No request body is needed.
    /// 
    /// Deletion behavior:
    /// - If the exercise has references (e.g., in workout logs), it will be soft deleted (marked as inactive)
    /// - If the exercise has no references, it will be permanently deleted from the database
    /// </remarks>
    /// <param name="id">The ID of the exercise to delete (from URL path)</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the exercise was successfully deleted</response>
    /// <response code="404">If the exercise is not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExercise(string id)
    {
        _logger.LogInformation("Deleting exercise with ID: {Id}", id);
        
        var result = await _exerciseService.DeleteAsync(ExerciseId.ParseOrEmpty(id));
        
        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { PrimaryErrorCode: ServiceErrorCode.NotFound } => NotFound(result.GetCombinedErrorMessage()),
            { PrimaryErrorCode: ServiceErrorCode.ValidationFailed } => BadRequest(result.GetCombinedErrorMessage()),
            _ => BadRequest(result.GetCombinedErrorMessage())
        };
    }
}