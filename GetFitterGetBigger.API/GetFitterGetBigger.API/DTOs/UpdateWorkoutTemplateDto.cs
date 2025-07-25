using System.ComponentModel.DataAnnotations;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object for updating an existing workout template
/// </summary>
public class UpdateWorkoutTemplateDto
{
    /// <summary>
    /// The name of the workout template
    /// <example>Upper Body Strength Training</example>
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
    public required string Name { get; init; }

    /// <summary>
    /// The description of the workout template
    /// <example>A comprehensive upper body workout focusing on compound movements</example>
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; init; }

    /// <summary>
    /// The workout category ID
    /// <example>workoutcategory-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    [Required(ErrorMessage = "Category is required")]
    public required string CategoryId { get; init; }

    /// <summary>
    /// The difficulty level ID
    /// <example>difficultylevel-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    [Required(ErrorMessage = "Difficulty level is required")]
    public required string DifficultyId { get; init; }

    /// <summary>
    /// The estimated duration in minutes
    /// <example>60</example>
    /// </summary>
    [Required(ErrorMessage = "Estimated duration is required")]
    [Range(5, 300, ErrorMessage = "Duration must be between 5 and 300 minutes")]
    public required int EstimatedDurationMinutes { get; init; }

    /// <summary>
    /// Optional tags for the workout template (maximum 10 tags)
    /// <example>["strength", "upper-body", "intermediate"]</example>
    /// </summary>
    [MaxLength(10, ErrorMessage = "Maximum 10 tags allowed")]
    public List<string> Tags { get; init; } = new();

    /// <summary>
    /// Whether the template is publicly visible
    /// <example>false</example>
    /// </summary>
    public bool IsPublic { get; init; } = false;

    /// <summary>
    /// The workout objective IDs associated with this template
    /// <example>["workoutobjective-550e8400-e29b-41d4-a716-446655440000"]</example>
    /// </summary>
    public List<string> ObjectiveIds { get; init; } = new();
}