using GetFitterGetBigger.API.Models.SpecializedIds;

namespace GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;

/// <summary>
/// Command for creating a new workout template
/// </summary>
public class CreateWorkoutTemplateCommand
{
    /// <summary>
    /// The name of the workout template
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Optional description of the workout template
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The workout category ID
    /// </summary>
    public required WorkoutCategoryId CategoryId { get; init; }

    /// <summary>
    /// The difficulty level ID
    /// </summary>
    public required DifficultyLevelId DifficultyId { get; init; }

    /// <summary>
    /// The estimated duration in minutes
    /// </summary>
    public required int EstimatedDurationMinutes { get; init; }

    /// <summary>
    /// Optional tags for the workout template
    /// </summary>
    public List<string> Tags { get; init; } = new();

    /// <summary>
    /// Whether the template is publicly visible
    /// </summary>
    public bool IsPublic { get; init; } = false;

    /// <summary>
    /// The ID of the user creating the template
    /// </summary>
    public required UserId CreatedBy { get; init; }

    /// <summary>
    /// The workout objectives associated with this template
    /// </summary>
    public List<WorkoutObjectiveId> ObjectiveIds { get; init; } = new();
}