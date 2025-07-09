namespace GetFitterGetBigger.Admin.Models.Dtos;

/// <summary>
/// Represents a link between exercises (warmup or cooldown relationship)
/// </summary>
public class ExerciseLinkDto
{
    /// <summary>
    /// The unique identifier of the exercise link
    /// Format: "exerciselink-{guid}"
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the source exercise (must be a Workout type)
    /// Format: "exercise-{guid}"
    /// </summary>
    public string SourceExerciseId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the target exercise to link to
    /// Format: "exercise-{guid}"
    /// </summary>
    public string TargetExerciseId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the target exercise for display purposes
    /// </summary>
    public string TargetExerciseName { get; set; } = string.Empty;

    /// <summary>
    /// The type of link - either "Warmup" or "Cooldown"
    /// </summary>
    public string LinkType { get; set; } = string.Empty;

    /// <summary>
    /// The display order for this link (1-based, min: 1)
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this link is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When the link was created (ISO 8601 date)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the link was last updated (ISO 8601 date)
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Optional: Full exercise data when includeExerciseDetails=true
    /// </summary>
    public ExerciseDto? TargetExercise { get; set; }
}