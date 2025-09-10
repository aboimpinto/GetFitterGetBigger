namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Responses;

/// <summary>
/// Response DTO for copying rounds with all exercises
/// </summary>
public record CopyRoundResponseDto(
    bool Success,
    CopyRoundResultDto Data,
    string Message = "",
    List<string> Errors = default!)
{
    /// <summary>
    /// Create successful response
    /// </summary>
    public static CopyRoundResponseDto SuccessResponse(CopyRoundResultDto data, string message = "")
        => new(true, data, message, new List<string>());

    /// <summary>
    /// Create error response
    /// </summary>
    public static CopyRoundResponseDto ErrorResponse(List<string> errors, string message = "")
        => new(false, CopyRoundResultDto.Empty, message, errors);
}