using System.Text.Json.Serialization;

namespace GetFitterGetBigger.Admin.Models.Dtos;

public class WorkoutCategoryDto
{
    [JsonPropertyName("workoutCategoryId")]
    public required string WorkoutCategoryId { get; set; }
    
    [JsonPropertyName("value")]
    public required string Value { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
    
    [JsonPropertyName("color")]
    public string? Color { get; set; }
    
    [JsonPropertyName("primaryMuscleGroups")]
    public string? PrimaryMuscleGroups { get; set; }
    
    [JsonPropertyName("displayOrder")]
    public int DisplayOrder { get; set; }
    
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}