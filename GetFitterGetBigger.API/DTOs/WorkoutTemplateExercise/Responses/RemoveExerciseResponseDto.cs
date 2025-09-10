namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Responses;

/// <summary>
/// Response DTO for removing exercises from workout template
/// </summary>
public record RemoveExerciseResponseDto(
    bool Success,
    RemoveExerciseResultDto Data,
    string Message = "",
    List<string> Errors = default!)
{
    /// <summary>
    /// Create successful response
    /// </summary>
    public static RemoveExerciseResponseDto SuccessResponse(RemoveExerciseResultDto data, string message = "")
        => new(true, data, message, new List<string>());

    /// <summary>
    /// Create error response
    /// </summary>
    public static RemoveExerciseResponseDto ErrorResponse(List<string> errors, string message = "")
        => new(false, RemoveExerciseResultDto.Empty, message, errors);
}