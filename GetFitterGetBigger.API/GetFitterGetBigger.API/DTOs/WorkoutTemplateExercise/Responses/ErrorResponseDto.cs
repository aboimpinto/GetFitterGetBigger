namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Responses;

/// <summary>
/// Generic error response DTO
/// </summary>
public record ErrorResponseDto(
    bool Success,
    List<string> Errors,
    string Message = "")
{
    /// <summary>
    /// Create error response with single error message
    /// </summary>
    public static ErrorResponseDto SingleError(string error, string message = "")
        => new(false, new List<string> { error }, message);

    /// <summary>
    /// Create error response with multiple error messages
    /// </summary>
    public static ErrorResponseDto MultipleErrors(List<string> errors, string message = "")
        => new(false, errors, message);
}