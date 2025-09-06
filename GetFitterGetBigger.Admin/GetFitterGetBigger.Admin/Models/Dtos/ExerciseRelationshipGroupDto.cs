namespace GetFitterGetBigger.Admin.Models.Dtos;

/// <summary>
/// Represents a grouped collection of exercise relationships for context-based display
/// </summary>
public class ExerciseRelationshipGroupDto
{
    /// <summary>
    /// The group name (e.g., "Warmups", "Cooldowns", "Alternative Workouts")
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// The relationship type for this group
    /// </summary>
    public string RelationshipType { get; set; } = string.Empty;

    /// <summary>
    /// Whether relationships in this group are read-only
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Whether relationships in this group have display order (sequenced)
    /// </summary>
    public bool HasDisplayOrder { get; set; }

    /// <summary>
    /// Maximum number of relationships allowed in this group (null = unlimited)
    /// </summary>
    public int? MaximumCount { get; set; }

    /// <summary>
    /// Current count of relationships in this group
    /// </summary>
    public int CurrentCount { get; set; }

    /// <summary>
    /// Theme color for the group UI
    /// </summary>
    public string ThemeColor { get; set; } = string.Empty;

    /// <summary>
    /// The exercise relationships in this group
    /// </summary>
    public IEnumerable<ExerciseLinkDto> Relationships { get; set; } = new List<ExerciseLinkDto>();

    /// <summary>
    /// Whether the maximum count has been reached
    /// </summary>
    public bool IsAtMaximum => MaximumCount.HasValue && CurrentCount >= MaximumCount.Value;
}