namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Responses;

/// <summary>
/// Response DTO for workout template exercises operations
/// </summary>
public record WorkoutTemplateExercisesResponseDto(
    bool Success,
    WorkoutTemplateExercisesDto Data,
    string Message = "",
    List<string> Errors = default!)
{
    /// <summary>
    /// Create successful response
    /// </summary>
    public static WorkoutTemplateExercisesResponseDto SuccessResponse(WorkoutTemplateExercisesDto data, string message = "")
        => new(true, data, message, new List<string>());

    /// <summary>
    /// Create error response
    /// </summary>
    public static WorkoutTemplateExercisesResponseDto ErrorResponse(List<string> errors, string message = "")
        => new(false, WorkoutTemplateExercisesDto.Empty, message, errors);
}