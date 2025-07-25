namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// DTO for workout template exercise
/// </summary>
public record WorkoutTemplateExerciseDto
{
    /// <summary>
    /// The workout template exercise ID
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The exercise details
    /// </summary>
    public ExerciseDto Exercise { get; init; } = new();

    /// <summary>
    /// The zone this exercise belongs to
    /// </summary>
    public string Zone { get; init; } = string.Empty;

    /// <summary>
    /// The sequence order within the zone
    /// </summary>
    public int SequenceOrder { get; init; }

    /// <summary>
    /// Optional notes for this exercise
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Set configurations for this exercise
    /// </summary>
    public List<SetConfigurationDto> SetConfigurations { get; init; } = new();

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}