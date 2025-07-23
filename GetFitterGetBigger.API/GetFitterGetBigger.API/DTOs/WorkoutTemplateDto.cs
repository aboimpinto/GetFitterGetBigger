namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Data transfer object representing a workout template with all related data
/// </summary>
public record WorkoutTemplateDto
{
    /// <summary>
    /// The unique identifier for the workout template
    /// <example>workouttemplate-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The name of the workout template
    /// <example>Upper Body Strength Training</example>
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The description of the workout template
    /// <example>A comprehensive upper body workout focusing on compound movements</example>
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The workout category information
    /// </summary>
    public required ReferenceDataDto Category { get; init; }

    /// <summary>
    /// The difficulty level information
    /// </summary>
    public required ReferenceDataDto Difficulty { get; init; }

    /// <summary>
    /// The estimated duration in minutes
    /// <example>60</example>
    /// </summary>
    public required int EstimatedDurationMinutes { get; init; }

    /// <summary>
    /// The tags associated with this template
    /// <example>["strength", "upper-body", "intermediate"]</example>
    /// </summary>
    public List<string> Tags { get; init; } = new();

    /// <summary>
    /// Whether the template is publicly visible
    /// <example>true</example>
    /// </summary>
    public required bool IsPublic { get; init; }

    /// <summary>
    /// The ID of the user who created this template
    /// <example>user-550e8400-e29b-41d4-a716-446655440000</example>
    /// </summary>
    public required string CreatedBy { get; init; }

    /// <summary>
    /// The current state of the workout template
    /// </summary>
    public required ReferenceDataDto WorkoutState { get; init; }

    /// <summary>
    /// The workout objectives associated with this template
    /// </summary>
    public List<ReferenceDataDto> Objectives { get; init; } = new();

    /// <summary>
    /// The exercises included in this workout template, organized by zones
    /// </summary>
    public List<WorkoutTemplateExerciseDto> Exercises { get; init; } = new();

    /// <summary>
    /// When the template was created
    /// <example>2025-01-15T10:30:00Z</example>
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the template was last updated
    /// <example>2025-01-15T15:45:00Z</example>
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Whether this DTO represents an empty/default state
    /// </summary>
    public bool IsEmpty => Id == Empty.Id;

    /// <summary>
    /// Gets an empty WorkoutTemplateDto instance for the Empty Object Pattern
    /// </summary>
    public static WorkoutTemplateDto Empty => new()
    {
        Id = string.Empty,
        Name = string.Empty,
        Description = null,
        Category = ReferenceDataDto.Empty,
        Difficulty = ReferenceDataDto.Empty,
        EstimatedDurationMinutes = 0,
        Tags = new List<string>(),
        IsPublic = false,
        CreatedBy = string.Empty,
        WorkoutState = ReferenceDataDto.Empty,
        Objectives = new List<ReferenceDataDto>(),
        Exercises = new List<WorkoutTemplateExerciseDto>(),
        CreatedAt = DateTime.MinValue,
        UpdatedAt = DateTime.MinValue
    };
}