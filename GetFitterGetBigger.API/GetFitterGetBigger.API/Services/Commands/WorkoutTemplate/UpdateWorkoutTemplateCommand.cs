using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;

/// <summary>
/// Command for updating an existing workout template
/// </summary>
public class UpdateWorkoutTemplateCommand
{
    /// <summary>
    /// The ID of the workout template to update
    /// </summary>
    public required WorkoutTemplateId Id { get; init; }

    /// <summary>
    /// The name of the workout template
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Optional description of the workout template
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The workout category ID
    /// </summary>
    public WorkoutCategoryId? CategoryId { get; init; }

    /// <summary>
    /// The difficulty level ID
    /// </summary>
    public DifficultyLevelId? DifficultyId { get; init; }

    /// <summary>
    /// The estimated duration in minutes
    /// </summary>
    public int? EstimatedDurationMinutes { get; init; }

    /// <summary>
    /// Optional tags for the workout template
    /// </summary>
    public List<string>? Tags { get; init; }

    /// <summary>
    /// Whether the template is publicly visible
    /// </summary>
    public bool? IsPublic { get; init; }

    /// <summary>
    /// The workout objectives associated with this template
    /// </summary>
    public List<WorkoutObjectiveId>? ObjectiveIds { get; init; }
}