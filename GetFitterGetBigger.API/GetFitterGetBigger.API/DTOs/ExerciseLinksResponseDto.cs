using System.Collections.Generic;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Response DTO for getting exercise links
/// </summary>
public class ExerciseLinksResponseDto
{
    /// <summary>
    /// The ID of the exercise in the format "exercise-{guid}"
    /// </summary>
    public string ExerciseId { get; set; } = string.Empty;
    
    /// <summary>
    /// The list of linked exercises
    /// </summary>
    public List<ExerciseLinkDto> Links { get; set; } = new();
}