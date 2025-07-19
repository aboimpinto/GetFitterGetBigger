using System.Text.Json.Serialization;

namespace GetFitterGetBigger.Admin.Models.Dtos;

public class WorkoutCategoriesResponseDto
{
    [JsonPropertyName("workoutCategories")]
    public List<WorkoutCategoryDto> WorkoutCategories { get; set; } = new();
}