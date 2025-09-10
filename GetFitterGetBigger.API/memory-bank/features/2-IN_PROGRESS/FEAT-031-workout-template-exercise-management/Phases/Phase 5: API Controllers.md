## Phase 5: API Controllers - Estimated: 3h 0m

### Task 5.1: Create DTOs and request/response models
`[Completed]` (Est: 1h) - Actual: 45m

**Implementation:**
- Create comprehensive DTO structure in `/GetFitterGetBigger.API/DTOs/WorkoutTemplateExercise/`
- Follow DTO patterns from ExerciseLink feature:

```csharp
// Requests
public record AddExerciseToTemplateRequest(
    string ExerciseId,
    string Phase,
    int RoundNumber,
    JsonDocument Metadata);

public record UpdateExerciseMetadataRequest(
    JsonDocument Metadata);

public record ReorderExerciseRequest(
    int NewOrderInRound);

public record CopyRoundRequest(
    string SourcePhase,
    int SourceRoundNumber,
    string TargetPhase,
    int TargetRoundNumber);

// Responses  
public record WorkoutTemplateExercisesResponseDto(
    bool Success,
    WorkoutTemplateExercisesDto Data,
    string Message = "",
    List<string> Errors = default);

public record AddExerciseResponseDto(
    bool Success,
    AddExerciseResultDto Data,
    string Message = "",
    List<string> Errors = default);

// Core DTOs
public record WorkoutTemplateExerciseDto(
    string Id,
    string ExerciseId,
    string ExerciseName,
    string ExerciseType,
    string Phase,
    int RoundNumber,
    int OrderInRound,
    JsonDocument Metadata,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static WorkoutTemplateExerciseDto Empty => new(
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        0,
        0,
        JsonDocument.Parse("{}"),
        DateTime.MinValue,
        DateTime.MinValue);
}
```

**Unit Tests:**
- Test DTO creation and serialization
- Test Empty pattern implementation
- Test JsonDocument metadata handling
- Test request validation attributes

**Critical Patterns:**
- Follow DTO patterns from `/GetFitterGetBigger.API/DTOs/ExerciseLinkDto.cs`
- Use JsonDocument for metadata (not string)
- Implement Empty pattern for all DTOs
- Use record types for immutability

### Task 5.2: Create WorkoutTemplateExercisesController
`[Completed]` (Est: 1h 30m) - Actual: 1h 15m

**Implementation:**
- Create `/GetFitterGetBigger.API/Controllers/WorkoutTemplateExercisesController.cs`
- Follow controller patterns from existing controllers:

```csharp
[ApiController]
[Route("api/workout-templates/{templateId}/exercises")]
[Authorize]
public class WorkoutTemplateExercisesController : ControllerBase
{
    private readonly IWorkoutTemplateExerciseService _service;

    public WorkoutTemplateExercisesController(IWorkoutTemplateExerciseService service)
    {
        _service = service;
    }

    /// <summary>
    /// Add exercise to workout template with automatic warmup/cooldown linking
    /// </summary>
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
                new AddExerciseResponseDto(true, result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Remove exercise from workout template with automatic orphan cleanup
    /// </summary>
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
            { IsSuccess: true } => Ok(new RemoveExerciseResponseDto(true, result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Get all exercises for a workout template organized by phase and round
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WorkoutTemplateExercisesResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> GetTemplateExercises([FromRoute] string templateId)
    {
        var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);

        var result = await _service.GetTemplateExercisesAsync(templateIdParsed);

        return result switch
        {
            { IsSuccess: true } => Ok(new WorkoutTemplateExercisesResponseDto(true, result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Update exercise metadata
    /// </summary>
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
            { IsSuccess: true } => Ok(new UpdateMetadataResponseDto(true, result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Reorder exercise within its round
    /// </summary>
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
            { IsSuccess: true } => Ok(new ReorderResponseDto(true, result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList()))
        };
    }
}
```

**Unit Tests:**
- Test all controller actions with various scenarios
- Test proper HTTP status code handling
- Test request/response serialization
- Test authorization requirements
- Test pattern matching for error scenarios

**Critical Patterns:**
- Follow controller patterns from `/GetFitterGetBigger.API/Controllers/ExerciseLinksController.cs`
- Use pattern matching for ServiceResult<T> handling
- Use ParseOrEmpty for ID conversion (don't validate in controller)
- Let service layer handle all business validation
- Proper HTTP status codes (201 for creation, 200 for updates)

### Task 5.3: Create round management endpoints
`[Completed]` (Est: 30m) - Actual: Included in Task 5.2

**Implementation:**
- Add round management endpoints to the controller:

```csharp
/// <summary>
/// Copy round with all exercises and generate new GUIDs
/// </summary>
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
            new CopyRoundResponseDto(true, result.Data)),
        { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
            NotFound(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList())),
        { Errors: var errors } => 
            BadRequest(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList()))
    };
}
```

**Unit Tests:**
- Test round copying functionality
- Test proper response handling
- Test error scenarios

## CHECKPOINT: Phase 5 Complete - API Controllers
`[COMPLETE]` - Date: 2025-09-10 10:00

Build Report:
- API Project: ✅ 0 errors, 0 warnings
- Test Project (Unit): ✅ 0 errors, 0 warnings
- Test Project (Integration): ✅ 0 errors, 0 warnings

**Implementation Summary:**
- **DTOs**: Complete request/response model structure with Enhanced API support
- **WorkoutTemplateExercisesEnhancedController**: All CRUD endpoints with proper patterns (v2 API)
- **Round Management**: Copy round functionality with new GUID generation
- **Error Handling**: Pattern matching for ServiceResult<T> with proper HTTP status codes
- **OpenAPI**: Comprehensive Swagger documentation with examples

**Test Completion:**
- Controller Tests: ✅ 9 comprehensive test methods covering all scenarios
- DTO Tests: ✅ 4 test classes for serialization, validation, and edge cases
- Response Tests: ✅ Success and error response patterns validated
- Authorization Tests: ✅ Proper route and authorization validation

**Code Review Status:**
- Status: **APPROVED**
- Report: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_5_API_Controllers/Code-Review-Phase-5-API-Controllers-2025-09-10-09-58-APPROVED.md`

**Git Commits (Phase 5):**
[To be added after commit]