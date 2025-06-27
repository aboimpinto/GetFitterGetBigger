using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Controller for managing exercises
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Exercises")]
public class ExercisesController : ControllerBase
{
    private readonly IExerciseService _exerciseService;
    private readonly ILogger<ExercisesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExercisesController"/> class
    /// </summary>
    /// <param name="exerciseService">The exercise service</param>
    /// <param name="logger">The logger</param>
    public ExercisesController(IExerciseService exerciseService, ILogger<ExercisesController> logger)
    {
        _exerciseService = exerciseService;
        _logger = logger;
    }

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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Getting exercises with filters: {@FilterParams}", filterParams);
        
        var result = await _exerciseService.GetPagedAsync(filterParams);
        return Ok(result);
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
        
        var exercise = await _exerciseService.GetByIdAsync(id);
        
        if (exercise == null)
        {
            return NotFound();
        }
        
        return Ok(exercise);
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
    ///         "muscleGroups": [
    ///             {
    ///                 "muscleGroupId": "musclegroup-ccddeeff-0011-2233-4455-667788990011",
    ///                 "muscleRoleId": "musclerole-abcdef12-3456-7890-abcd-ef1234567890"
    ///             }
    ///         ],
    ///         "equipmentIds": ["equipment-33445566-7788-99aa-bbcc-ddeeff001122"],
    ///         "bodyPartIds": ["bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"],
    ///         "movementPatternIds": ["movementpattern-99aabbcc-ddee-ff00-1122-334455667788"]
    ///     }
    /// 
    /// Notes:
    /// - The exercise ID will be generated automatically and returned in the response
    /// - CoachNotes are ordered instruction items that replace the previous single Instructions field
    /// - ExerciseTypeIds specify the types (Warmup, Workout, Cooldown, Rest)
    /// - If "Rest" type is included, it must be the only type (business rule)
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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating new exercise: {Name}", request.Name);
        
        try
        {
            var exercise = await _exerciseService.CreateAsync(request);
            return CreatedAtAction(nameof(GetExercise), new { id = exercise.Id }, exercise);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning("Attempted to create duplicate exercise: {Name}", request.Name);
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Rest"))
        {
            _logger.LogWarning("Rest exclusivity rule violation: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid exercise creation request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
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
    ///         "muscleGroups": [
    ///             {
    ///                 "muscleGroupId": "musclegroup-ccddeeff-0011-2233-4455-667788990011",
    ///                 "muscleRoleId": "musclerole-abcdef12-3456-7890-abcd-ef1234567890"
    ///             }
    ///         ],
    ///         "equipmentIds": ["equipment-33445566-7788-99aa-bbcc-ddeeff001122"],
    ///         "bodyPartIds": ["bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"],
    ///         "movementPatternIds": ["movementpattern-99aabbcc-ddee-ff00-1122-334455667788"]
    ///     }
    /// 
    /// Notes:
    /// - The exercise ID is taken from the URL path, not from the request body
    /// - CoachNotes can include IDs for existing notes (to update) or no ID for new notes
    /// - CoachNotes not included in the request will be deleted
    /// - ExerciseTypeIds completely replace the existing types
    /// - If "Rest" type is included, it must be the only type (business rule)
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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Updating exercise with ID: {Id}", id);
        
        try
        {
            var exercise = await _exerciseService.UpdateAsync(id, request);
            return Ok(exercise);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid exercise update request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning("Attempted to update exercise with duplicate name: {Name}", request.Name);
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Exercise not found for update: {Id}", id);
            return NotFound();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Rest"))
        {
            _logger.LogWarning("Rest exclusivity rule violation: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
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
        
        var result = await _exerciseService.DeleteAsync(id);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}