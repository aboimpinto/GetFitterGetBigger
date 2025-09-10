namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Responses;

/// <summary>
/// Response DTO for adding exercises to workout template
/// </summary>
public record AddExerciseResponseDto(
    bool Success,
    AddExerciseResultDto Data,
    string Message = "",
    List<string> Errors = default!)
{
    /// <summary>
    /// Create successful response
    /// </summary>
    public static AddExerciseResponseDto SuccessResponse(AddExerciseResultDto data, string message = "")
        => new(true, data, message, new List<string>());

    /// <summary>
    /// Create error response
    /// </summary>
    public static AddExerciseResponseDto ErrorResponse(List<string> errors, string message = "")
        => new(false, AddExerciseResultDto.Empty, message, errors);
}