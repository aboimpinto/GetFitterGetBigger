namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Responses;

/// <summary>
/// Response DTO for reordering exercises within rounds
/// </summary>
public record ReorderResponseDto(
    bool Success,
    ReorderResultDto Data,
    string Message = "",
    List<string> Errors = default!)
{
    /// <summary>
    /// Create successful response
    /// </summary>
    public static ReorderResponseDto SuccessResponse(ReorderResultDto data, string message = "")
        => new(true, data, message, new List<string>());

    /// <summary>
    /// Create error response
    /// </summary>
    public static ReorderResponseDto ErrorResponse(List<string> errors, string message = "")
        => new(false, ReorderResultDto.Empty, message, errors);
}