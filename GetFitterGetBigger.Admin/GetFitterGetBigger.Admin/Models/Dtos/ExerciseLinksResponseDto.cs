namespace GetFitterGetBigger.Admin.Models.Dtos;

/// <summary>
/// Response DTO for getting all links for a specific exercise
/// </summary>
public class ExerciseLinksResponseDto
{
    /// <summary>
    /// The ID of the exercise these links belong to
    /// </summary>
    public string ExerciseId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the exercise for display purposes
    /// </summary>
    public string ExerciseName { get; set; } = string.Empty;

    /// <summary>
    /// Collection of exercise links (warmups and/or cooldowns)
    /// </summary>
    public List<ExerciseLinkDto> Links { get; set; } = new();

    /// <summary>
    /// Total count of links returned
    /// </summary>
    public int TotalCount { get; set; }
}