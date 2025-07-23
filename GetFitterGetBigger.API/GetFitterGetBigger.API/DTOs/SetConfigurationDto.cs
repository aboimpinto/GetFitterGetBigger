namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object representing a set configuration within a workout template exercise
/// </summary>
public record SetConfigurationDto
{
    /// <summary>
    /// The unique identifier for the set configuration
    /// <example>setconfiguration-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The workout template exercise ID this configuration belongs to
    /// <example>workouttemplate-exercise-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    public required string WorkoutTemplateExerciseId { get; init; }

    /// <summary>
    /// The set number within the exercise
    /// <example>1</example>
    /// </summary>
    public required int SetNumber { get; init; }

    /// <summary>
    /// The target number of repetitions (can be a range like "8-12")
    /// <example>12</example>
    /// </summary>
    public string? TargetReps { get; init; }

    /// <summary>
    /// The target weight in the preferred unit
    /// <example>45.5</example>
    /// </summary>
    public decimal? TargetWeight { get; init; }

    /// <summary>
    /// The target duration in seconds for time-based exercises
    /// <example>30</example>
    /// </summary>
    public int? TargetDurationSeconds { get; init; }

    /// <summary>
    /// The rest time in seconds after this set
    /// <example>60</example>
    /// </summary>
    public int? RestSeconds { get; init; }

    /// <summary>
    /// Whether this DTO represents an empty/default state
    /// </summary>
    public bool IsEmpty => Id == Empty.Id;

    /// <summary>
    /// Gets an empty SetConfigurationDto instance for the Empty Object Pattern
    /// </summary>
    public static SetConfigurationDto Empty => new()
    {
        Id = string.Empty,
        WorkoutTemplateExerciseId = string.Empty,
        SetNumber = 0,
        TargetReps = null,
        TargetWeight = null,
        TargetDurationSeconds = null,
        RestSeconds = null
    };
}