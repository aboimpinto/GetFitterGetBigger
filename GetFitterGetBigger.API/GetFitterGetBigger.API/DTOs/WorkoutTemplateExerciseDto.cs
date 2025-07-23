namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object representing an exercise within a workout template
/// </summary>
public record WorkoutTemplateExerciseDto
{
    /// <summary>
    /// The unique identifier for the workout template exercise
    /// <example>workouttemplate-exercise-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The workout template ID this exercise belongs to
    /// <example>workouttemplate-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    public required string WorkoutTemplateId { get; init; }

    /// <summary>
    /// The exercise information
    /// </summary>
    public required ReferenceDataDto Exercise { get; init; }

    /// <summary>
    /// The zone where this exercise is performed
    /// <example>Main</example>
    /// </summary>
    public required string Zone { get; init; }

    /// <summary>
    /// The sequence order within the zone
    /// <example>1</example>
    /// </summary>
    public required int SequenceOrder { get; init; }

    /// <summary>
    /// Optional notes for this exercise in the template
    /// <example>Focus on controlled movement and proper form</example>
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// The set configurations for this exercise
    /// </summary>
    public List<SetConfigurationDto> Configurations { get; init; } = new();

    /// <summary>
    /// Whether this DTO represents an empty/default state
    /// </summary>
    public bool IsEmpty => Id == Empty.Id;

    /// <summary>
    /// Gets an empty WorkoutTemplateExerciseDto instance for the Empty Object Pattern
    /// </summary>
    public static WorkoutTemplateExerciseDto Empty => new()
    {
        Id = string.Empty,
        WorkoutTemplateId = string.Empty,
        Exercise = ReferenceDataDto.Empty,
        Zone = string.Empty,
        SequenceOrder = 0,
        Notes = null,
        Configurations = new List<SetConfigurationDto>()
    };
}