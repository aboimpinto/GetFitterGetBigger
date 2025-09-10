namespace GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Responses;

/// <summary>
/// Response DTO for updating exercise metadata
/// </summary>
public record UpdateMetadataResponseDto(
    bool Success,
    UpdateMetadataResultDto Data,
    string Message = "",
    List<string> Errors = default!)
{
    /// <summary>
    /// Create successful response
    /// </summary>
    public static UpdateMetadataResponseDto SuccessResponse(UpdateMetadataResultDto data, string message = "")
        => new(true, data, message, new List<string>());

    /// <summary>
    /// Create error response
    /// </summary>
    public static UpdateMetadataResponseDto ErrorResponse(List<string> errors, string message = "")
        => new(false, UpdateMetadataResultDto.Empty, message, errors);
}