namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// DTO for workout template exercises grouped by zone
/// </summary>
public record WorkoutTemplateExerciseListDto
{
    /// <summary>
    /// The workout template ID
    /// </summary>
    public string WorkoutTemplateId { get; init; } = string.Empty;

    /// <summary>
    /// Warmup zone exercises
    /// </summary>
    public List<WorkoutTemplateExerciseDto> WarmupExercises { get; init; } = new();

    /// <summary>
    /// Main zone exercises
    /// </summary>
    public List<WorkoutTemplateExerciseDto> MainExercises { get; init; } = new();

    /// <summary>
    /// Cooldown zone exercises
    /// </summary>
    public List<WorkoutTemplateExerciseDto> CooldownExercises { get; init; } = new();

    /// <summary>
    /// Total number of exercises
    /// </summary>
    public int TotalExercises => WarmupExercises.Count + MainExercises.Count + CooldownExercises.Count;

    /// <summary>
    /// Total estimated duration in minutes
    /// </summary>
    public int TotalEstimatedDurationMinutes { get; init; }
}