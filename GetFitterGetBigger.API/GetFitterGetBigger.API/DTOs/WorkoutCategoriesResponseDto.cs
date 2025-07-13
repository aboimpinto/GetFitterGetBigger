using System.Collections.Generic;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Response DTO for getting workout categories
/// </summary>
public class WorkoutCategoriesResponseDto
{
    /// <summary>
    /// The list of workout categories
    /// </summary>
    public List<WorkoutCategoryDto> WorkoutCategories { get; set; } = new();
}