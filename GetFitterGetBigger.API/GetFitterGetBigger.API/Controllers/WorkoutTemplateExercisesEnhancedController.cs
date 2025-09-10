using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Requests;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Responses;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetFitterGetBigger.API.Controllers;

/// <summary>
/// Enhanced controller for managing exercises within workout templates with phase/round support
/// </summary>
[ApiController]
[Route("api/v2/workout-templates/{templateId}/exercises")]
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
    /// <remarks>
    /// Adds an exercise to a workout template with intelligent auto-linking of related exercises.
    /// 
    /// **Metadata Examples by ExecutionProtocol:**
    /// 
    /// REPS_AND_SETS with Weight:
    /// ```json
    /// {
    ///   "reps": 10,
    ///   "weight": {
    ///     "value": 60,
    ///     "unit": "kg"
    ///   }
    /// }
    /// ```
    /// 
    /// REPS_AND_SETS Bodyweight:
    /// ```json
    /// {
    ///   "reps": 15
    /// }
    /// ```
    /// 
    /// Time-based exercise:
    /// ```json
    /// {
    ///   "duration": 30,
    ///   "unit": "seconds"
    /// }
    /// ```
    /// 
    /// REST exercise:
    /// ```json
    /// {
    ///   "duration": 90,
    ///   "unit": "seconds"
    /// }
    /// ```
    /// 
    /// **Auto-Linking Behavior:**
    /// - When adding a WORKOUT exercise, linked WARMUP exercises are automatically added to Warmup phase
    /// - When adding a WORKOUT exercise, linked COOLDOWN exercises are automatically added to Cooldown phase
    /// - Auto-linked exercises are placed in round 1 with appropriate OrderInRound
    /// 
    /// **Sample Request:**
    /// ```json
    /// {
    ///   "exerciseId": "exercise-550e8400-e29b-41d4-a716-446655440000",
    ///   "phase": "Workout",
    ///   "roundNumber": 1,
    ///   "metadata": {
    ///     "reps": 10,
    ///     "weight": {
    ///       "value": 60,
    ///       "unit": "kg"
    ///     }
    ///   }
    /// }
    /// ```
    /// </remarks>
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
            { PrimaryErrorCode: ServiceErrorCode.NotFound, Errors: var errors } => 
                NotFound(ErrorResponseDto.MultipleErrors(errors.ToList())),
            { Errors: var errors } => 
                BadRequest(ErrorResponseDto.MultipleErrors(errors.ToList()))
        };
    }

    /// <summary>
    /// Remove exercise from workout template with automatic orphan cleanup
    /// </summary>
    /// <remarks>
    /// Removes an exercise from a workout template with intelligent orphan cleanup.
    /// 
    /// **Orphan Cleanup Behavior:**
    /// - When removing a WORKOUT exercise, linked WARMUP/COOLDOWN exercises are checked
    /// - If a warmup/cooldown exercise is only used by the removed workout exercise, it's also removed
    /// - If a warmup/cooldown exercise is shared by multiple workout exercises, it remains
    /// - Order is automatically adjusted for remaining exercises in the round
    /// 
    /// **Example Scenarios:**
    /// 
    /// Scenario 1 - Orphan cleanup:
    /// - Template has: Barbell Squat (workout) + High Knees (warmup, auto-linked)
    /// - Remove Barbell Squat → Both exercises removed (High Knees becomes orphan)
    /// 
    /// Scenario 2 - Shared warmup preserved:
    /// - Template has: Barbell Squat (workout) + Leg Press (workout) + High Knees (warmup, linked to both)
    /// - Remove Barbell Squat → Only Barbell Squat removed (High Knees still used by Leg Press)
    /// 
    /// **Response includes:**
    /// - `removedExercises`: List of all exercises that were removed (main + orphans)
    /// - `orphansRemoved`: Number of orphaned exercises that were cleaned up
    /// - `message`: Descriptive success message
    /// </remarks>
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
            { PrimaryErrorCode: ServiceErrorCode.NotFound, Errors: var errors } => 
                NotFound(ErrorResponseDto.MultipleErrors(errors.ToList())),
            { Errors: var errors } => 
                BadRequest(ErrorResponseDto.MultipleErrors(errors.ToList()))
        };
    }

    /// <summary>
    /// Get all exercises for a workout template organized by phase and round
    /// </summary>
    /// <remarks>
    /// Retrieves all exercises in a workout template organized in a hierarchical structure.
    /// 
    /// **Response Structure:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "templateId": "workouttemplate-550e8400-e29b-41d4-a716-446655440000",
    ///     "phases": {
    ///       "Warmup": {
    ///         "rounds": {
    ///           "1": [
    ///             {
    ///               "id": "workouttemplateexercise-guid",
    ///               "exerciseId": "exercise-guid",
    ///               "exerciseName": "High Knees",
    ///               "phase": "Warmup",
    ///               "roundNumber": 1,
    ///               "orderInRound": 1,
    ///               "metadata": {"duration": 30, "unit": "seconds"}
    ///             }
    ///           ]
    ///         }
    ///       },
    ///       "Workout": {
    ///         "rounds": {
    ///           "1": [...],
    ///           "2": [...]
    ///         }
    ///       },
    ///       "Cooldown": {
    ///         "rounds": {
    ///           "1": [...]
    ///         }
    ///       }
    ///     }
    ///   }
    /// }
    /// ```
    /// 
    /// **Features:**
    /// - Hierarchical organization: Phase → Round → Exercises
    /// - Exercises ordered by `orderInRound` within each round
    /// - Complete exercise information including metadata
    /// - Empty phases/rounds are omitted from response
    /// </remarks>
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
            { PrimaryErrorCode: ServiceErrorCode.NotFound, Errors: var errors } => 
                NotFound(ErrorResponseDto.MultipleErrors(errors.ToList())),
            { Errors: var errors } => 
                BadRequest(ErrorResponseDto.MultipleErrors(errors.ToList()))
        };
    }

    /// <summary>
    /// Update exercise metadata
    /// </summary>
    /// <remarks>
    /// Updates the JSON metadata for an exercise while preserving all other properties.
    /// 
    /// **Important Notes:**
    /// - Only the metadata field is updated, all other exercise properties remain unchanged
    /// - Metadata must be valid JSON and appropriate for the exercise's ExecutionProtocol
    /// - The update is atomic - either succeeds completely or fails with no changes
    /// 
    /// **Sample Requests:**
    /// 
    /// Update weight-based exercise:
    /// ```json
    /// {
    ///   "metadata": {
    ///     "reps": 12,
    ///     "weight": {
    ///       "value": 70,
    ///       "unit": "kg"
    ///     }
    ///   }
    /// }
    /// ```
    /// 
    /// Update time-based exercise:
    /// ```json
    /// {
    ///   "metadata": {
    ///     "duration": 45,
    ///     "unit": "seconds"
    ///   }
    /// }
    /// ```
    /// 
    /// **Response includes:**
    /// - `exerciseId`: The ID of the updated exercise
    /// - `updatedMetadata`: The new metadata that was set
    /// - `previousMetadata`: The metadata that was replaced (for rollback if needed)
    /// </remarks>
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
            { PrimaryErrorCode: ServiceErrorCode.NotFound, Errors: var errors } => 
                NotFound(ErrorResponseDto.MultipleErrors(errors.ToList())),
            { Errors: var errors } => 
                BadRequest(ErrorResponseDto.MultipleErrors(errors.ToList()))
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
            { PrimaryErrorCode: ServiceErrorCode.NotFound, Errors: var errors } => 
                NotFound(ErrorResponseDto.MultipleErrors(errors.ToList())),
            { Errors: var errors } => 
                BadRequest(ErrorResponseDto.MultipleErrors(errors.ToList()))
        };
    }

    /// <summary>
    /// Copy round with all exercises and generate new GUIDs
    /// </summary>
    /// <remarks>
    /// Copies all exercises from a source round to a target round with new GUIDs generated.
    /// 
    /// **Key Features:**
    /// - All exercises in the source round are copied with new GUIDs
    /// - All metadata is preserved exactly as-is
    /// - OrderInRound is maintained in the target round
    /// - Can copy within same phase or across different phases
    /// - Can copy within same template (round duplication)
    /// 
    /// **Use Cases:**
    /// 
    /// 1. **Duplicate workout round:**
    /// ```json
    /// {
    ///   "sourcePhase": "Workout",
    ///   "sourceRoundNumber": 1,
    ///   "targetPhase": "Workout",
    ///   "targetRoundNumber": 2
    /// }
    /// ```
    /// 
    /// 2. **Copy workout to cooldown (with modifications):**
    /// ```json
    /// {
    ///   "sourcePhase": "Workout",
    ///   "sourceRoundNumber": 1,
    ///   "targetPhase": "Cooldown",
    ///   "targetRoundNumber": 1
    /// }
    /// ```
    /// 
    /// **Response includes:**
    /// - `copiedExercises`: List of all newly created exercises with their new GUIDs
    /// - `sourceRound`: Information about the source round that was copied
    /// - `targetRound`: Information about the target round that was created
    /// - `exerciseCount`: Number of exercises that were copied
    /// 
    /// **Validation:**
    /// - Source round must exist and contain exercises
    /// - Target round must not already exist
    /// - Cannot copy to the same round (sourcePhase + sourceRoundNumber ≠ targetPhase + targetRoundNumber)
    /// </remarks>
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
            { PrimaryErrorCode: ServiceErrorCode.NotFound, Errors: var errors } => 
                NotFound(ErrorResponseDto.MultipleErrors(errors.ToList())),
            { Errors: var errors } => 
                BadRequest(ErrorResponseDto.MultipleErrors(errors.ToList()))
        };
    }
}