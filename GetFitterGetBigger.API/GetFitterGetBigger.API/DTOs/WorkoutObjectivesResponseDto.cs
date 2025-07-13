using System.Collections.Generic;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Response DTO for getting workout objectives
/// </summary>
public class WorkoutObjectivesResponseDto
{
    /// <summary>
    /// The list of workout objectives
    /// </summary>
    public List<WorkoutObjectiveDto> WorkoutObjectives { get; set; } = new();
}