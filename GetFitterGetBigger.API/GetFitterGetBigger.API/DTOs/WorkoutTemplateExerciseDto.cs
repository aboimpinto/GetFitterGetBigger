using GetFitterGetBigger.API.DTOs.Interfaces;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// DTO for workout template exercise with enhanced phase/round support
/// </summary>
public record WorkoutTemplateExerciseDto : IEmptyDto<WorkoutTemplateExerciseDto>
{
    /// <summary>
    /// The workout template exercise ID (GUID-based)
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The exercise details
    /// </summary>
    public ExerciseDto Exercise { get; init; } = new();

    /// <summary>
    /// The phase this exercise belongs to (Warmup, Workout, Cooldown)
    /// </summary>
    public string Phase { get; init; } = string.Empty;

    /// <summary>
    /// The round number within the phase
    /// </summary>
    public int RoundNumber { get; init; }

    /// <summary>
    /// The order within the round (1-based)
    /// </summary>
    public int OrderInRound { get; init; }

    /// <summary>
    /// JSON metadata containing ExecutionProtocol-specific configuration
    /// </summary>
    public string Metadata { get; init; } = "{}";

    /// <summary>
    /// Optional notes for this exercise
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Indicates if this is an empty instance
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Id);

    /// <summary>
    /// Empty instance for failed operations
    /// </summary>
    public static WorkoutTemplateExerciseDto Empty => new();
}